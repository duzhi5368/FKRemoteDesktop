using FKRemoteDesktop.Extensions;
using FKRemoteDesktop.Message;
using FKRemoteDesktop.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Network
{
    public class Server
    {
        #region 事件
        /// <summary>
        /// 服务器状态发生变化事件
        /// </summary>
        /// <param name="s">发生变化事件的服务器</param>
        /// <param name="listening">服务器新的监听状态</param>
        /// <param name="port">服务器监听的端口</param>
        public delegate void ServerStateEventHandler(Server s, bool listening, ushort port);
        public event ServerStateEventHandler ServerState;

        /// <summary>
        /// 客户端状态发生变化事件
        /// </summary>
        /// <param name="s">客户端连接的服务器</param>
        /// <param name="c">状态发生更变的客户端</param>
        /// <param name="connected">客户端的新连接状态</param>
        public delegate void ClientStateEventHandler(Server s, Client c, bool connected);
        public event ClientStateEventHandler ClientState;

        /// <summary>
        /// 客户端收到消息的事件
        /// </summary>
        /// <param name="s">客户端连接的服务器</param>
        /// <param name="c">收到消息的客户端</param>
        /// <param name="message">客户端收到的消息</param>
        public delegate void ClientReadEventHandler(Server s, Client c, IMessage message);
        public event ClientReadEventHandler ClientRead;

        /// <summary>
        /// 客户端发送消息事件
        /// </summary>
        /// <param name="s">客户端连接的服务器</param>
        /// <param name="c">发送消息的客户端</param>
        /// <param name="message">客户端发送的消息</param>
        public delegate void ClientWriteEventHandler(Server s, Client c, IMessage message);
        public event ClientWriteEventHandler ClientWrite;

        #endregion

        #region 变量
        public bool Listening { get; private set; }     // 服务器是否正处于监听状态
        public ushort Port { get; private set; }        // 服务器正在监听的端口
        public long BytesReceived { get; set; }         // 总接收的消息字节数
        public long BytesSent { get; set; }             // 总发送的消息字节数

        private const int BUFFER_SIZE = 1024 * 16;      // 接收数据的缓冲区大小（字节）
        private const uint KEEP_ALIVE_TIME = 29000;     // 保持连接时间 29s
        private const uint KEEP_ALIVE_INTERVAL = 29000; // 保持活动间隔 29s
        private readonly BufferPool _bufferPool = new BufferPool(BUFFER_SIZE, 1) { ClearOnReturn = false };

        private Socket _handle;                                     // 服务器Socket
        protected readonly X509Certificate2 ServerCertificate;      // 服务器证书
        private SocketAsyncEventArgs _item;                         // 异步接到新连接消息

        private readonly List<Client> _clients = new List<Client>();// 连接到服务器的客户端列表
        private readonly object _clientsLock = new object();        // 客户端列表锁
        private UPnPService _UPnPService;                           // UPnP服务
        protected bool ProcessingDisconnect { get; set; }           // 服务器是否正在处理Disconnect事件

        #endregion

        #region 消息事件
        private void OnServerState(bool listening)
        {
            if (Listening == listening) 
                return;

            Listening = listening;

            var handler = ServerState;
            handler?.Invoke(this, listening, Port);
        }

        private void OnClientState(Client c, bool connected)
        {
            if (!connected)
                RemoveClient(c);

            var handler = ClientState;
            handler?.Invoke(this, c, connected);
        }

        private void OnClientRead(Client c, IMessage message, int messageLength)
        {
            BytesReceived += messageLength;
            var handler = ClientRead;
            handler?.Invoke(this, c, message);
        }

        private void OnClientWrite(Client c, IMessage message, int messageLength)
        {
            BytesSent += messageLength;
            var handler = ClientWrite;
            handler?.Invoke(this, c, message);
        }
        #endregion

        #region 核心函数
        protected Client[] Clients
        {
            get
            {
                lock (_clientsLock)
                {
                    return _clients.ToArray();
                }
            }
        }

        /// <summary>
        /// 构造函数，初始化序列化器类型
        /// </summary>
        /// <param name="serverCertificate">服务器证书</param>
        protected Server(X509Certificate2 serverCertificate)
        {
            ServerCertificate = serverCertificate;
            TypeRegistry.AddTypesToSerializer(typeof(IMessage), TypeRegistry.GetPacketTypes(typeof(IMessage)).ToArray());
        }

        /// <summary>
        /// 开始监听客户端
        /// </summary>
        /// <param name="port">监听的端口</param>
        /// <param name="ipv6">是否支持IPv6连接。如果该值为false，则仅使用IPv4的Socket</param>
        /// <param name="enableUPnP">是否开启UPnP端口转发</param>
        public void Listen(ushort port, bool ipv6, bool enableUPnP)
        {
            if (Listening) 
                return;
            this.Port = port;

            if (enableUPnP)
            {
                _UPnPService = new UPnPService();
                _UPnPService.CreatePortMapAsync(port);
            }

            if (Socket.OSSupportsIPv6 && ipv6)
            {
                _handle = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                _handle.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, 0);
                _handle.Bind(new IPEndPoint(IPAddress.IPv6Any, port));
            }
            else
            {
                _handle = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _handle.Bind(new IPEndPoint(IPAddress.Any, port));
            }
            _handle.Listen(1000);

            OnServerState(true);

            _item = new SocketAsyncEventArgs();
            _item.Completed += AcceptClient;

            if (!_handle.AcceptAsync(_item))
                AcceptClient(this, _item);
        }

        /// <summary>
        /// 接受客户端并进行身份验证
        /// </summary>
        /// <param name="s">发送方</param>
        /// <param name="e">异步socket事件</param>
        private void AcceptClient(object s, SocketAsyncEventArgs e)
        {
            try
            {
                do
                {
                    switch (e.SocketError)
                    {
                        case SocketError.Success:
                            SslStream sslStream = null;
                            try
                            {
                                Socket clientSocket = e.AcceptSocket;
                                clientSocket.SetKeepAliveEx(KEEP_ALIVE_INTERVAL, KEEP_ALIVE_TIME);
                                sslStream = new SslStream(new NetworkStream(clientSocket, true), false);
                                // SSLStream拥有socket，开始处理NetworkStream和Scoket
                                sslStream.BeginAuthenticateAsServer(ServerCertificate, false, SslProtocols.Tls12, false, EndAuthenticateClient,
                                    new SPendingClient { Stream = sslStream, EndPoint = (IPEndPoint)clientSocket.RemoteEndPoint });
                            }
                            catch (Exception)
                            {
                                sslStream?.Close();
                            }
                            break;
                        case SocketError.ConnectionReset:
                            break;
                        default:
                            throw new SocketException((int)e.SocketError);
                    }

                    e.AcceptSocket = null; // 可以重用
                } while (!_handle.AcceptAsync(e));
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception)
            {
                Disconnect();
            }
        }

        /// <summary>
        /// 结束新客户端的身份验证过程
        /// </summary>
        /// <param name="ar">异步操作的结果</param>
        private void EndAuthenticateClient(IAsyncResult ar)
        {
            var con = (SPendingClient)ar.AsyncState;
            try
            {
                con.Stream.EndAuthenticateAsServer(ar);

                Client client = new Client(_bufferPool, con.Stream, con.EndPoint);
                AddClient(client);
                OnClientState(client, true);
            }
            catch (Exception)
            {
                con.Stream.Close();
            }
        }

        // 将已连接成功的客户端添加到客户端列表
        private void AddClient(Client client)
        {
            lock (_clientsLock)
            {
                client.ClientState += OnClientState;
                client.ClientRead += OnClientRead;
                client.ClientWrite += OnClientWrite;
                _clients.Add(client);
            }
        }

        // 移除一个客户端
        private void RemoveClient(Client client)
        {
            if (ProcessingDisconnect) 
                return;
            lock (_clientsLock)
            {
                client.ClientState -= OnClientState;
                client.ClientRead -= OnClientRead;
                client.ClientWrite -= OnClientWrite;
                _clients.Remove(client);
            }
        }

        // 断开服务器和所有客户端的连接并停止监听
        public void Disconnect()
        {
            if (ProcessingDisconnect) return;
            ProcessingDisconnect = true;

            if (_handle != null)
            {
                _handle.Close();
                _handle = null;
            }

            if (_item != null)
            {
                _item.Dispose();
                _item = null;
            }

            if (_UPnPService != null)
            {
                _UPnPService.DeletePortMapAsync(Port);
                _UPnPService = null;
            }

            lock (_clientsLock)
            {
                while (_clients.Count != 0)
                {
                    try
                    {
                        _clients[0].Disconnect();
                        _clients[0].ClientState -= OnClientState;
                        _clients[0].ClientRead -= OnClientRead;
                        _clients[0].ClientWrite -= OnClientWrite;
                        _clients.RemoveAt(0);
                    }
                    catch
                    {
                    }
                }
            }

            ProcessingDisconnect = false;
            OnServerState(false);
        }
        #endregion
    }
}
