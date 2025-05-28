using FKRemoteDesktop.Debugger;
using FKRemoteDesktop.Enums;
using FKRemoteDesktop.Extensions;
using FKRemoteDesktop.Message;
using FKRemoteDesktop.Message.SubMessages;
using FKRemoteDesktop.ReverseProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Network
{
    public class Client : ISender
    {
        #region 消息事件

        /// <summary>
        /// 处理客户端失败事件
        /// </summary>
        /// <param name="s">失败的客户端</param>
        /// <param name="ex">客户端失败的异常信息</param>
        public delegate void ClientFailEventHandler(Client s, Exception ex);

        public event ClientFailEventHandler ClientFail;

        private void OnClientFail(Exception ex)
        {
            var handler = ClientFail;
            handler?.Invoke(this, ex);
        }

        /// <summary>
        /// 客户端状态改变事件
        /// </summary>
        /// <param name="s">改变状态的客户端</param>
        /// <param name="connected">客户端新连接状态</param>
        public delegate void ClientStateEventHandler(Client s, bool connected);

        public event ClientStateEventHandler ClientState;

        private void OnClientState(bool connected)
        {
            if (Connected == connected)
                return;
            Connected = connected;
            var handler = ClientState;
            handler?.Invoke(this, connected);
        }

        /// <summary>
        /// 处理来自服务器消息的事件
        /// </summary>
        /// <param name="s">收到消息的客户端</param>
        /// <param name="message">收到的服务器消息</param>
        /// <param name="messageLength">消息的长度</param>
        public delegate void ClientReadEventHandler(Client s, IMessage message, int messageLength);

        public event ClientReadEventHandler ClientRead;

        private void OnClientRead(IMessage message, int messageLength)
        {
            var handler = ClientRead;
            handler?.Invoke(this, message, messageLength);
        }

        /// <summary>
        /// 客户端发送消息的事件
        /// </summary>
        /// <param name="s">发送消息的客户端</param>
        /// <param name="message">客户端发送的消息</param>
        /// <param name="messageLength">消息长度</param>
        public delegate void ClientWriteEventHandler(Client s, IMessage message, int messageLength);

        public event ClientWriteEventHandler ClientWrite;

        private void OnClientWrite(IMessage message, int messageLength)
        {
            var handler = ClientWrite;
            handler?.Invoke(this, message, messageLength);
        }

        #endregion 消息事件

        #region 变量

        // 接收数据的缓冲区大小（字节为单位）
        public int BUFFER_SIZE
        { get { return 1024 * 16; } } // 16KB

        // 保持连接时间（毫秒）
        public uint KEEP_ALIVE_TIME
        { get { return 29000; } }

        // 保持连接间隔时间（毫秒）
        public uint KEEP_ALIVE_INTERVAL
        { get { return 29000; } }

        // 消息头部大小（字节）
        public int HEADER_SIZE
        { get { return 4; } } // 4B

        // 消息体最大大小（字节）
        public int MAX_MESSAGE_SIZE
        { get { return (1024 * 1024) * 5; } } // 5MB

        public bool Connected { get; private set; }                                     // 当前客户端是否已经连接到服务器
        private SslStream _stream;                                                      // 用于通信连接的流
        private readonly Mutex _singleWriteMutex = new Mutex();                         // 防止在stream同时写入的锁
        private readonly X509Certificate2 _serverCertificate;                           // 服务器证书
        private List<ReverseProxyClient> _proxyClients = new List<ReverseProxyClient>();// 本客户端持有的代理客户端列表
        private readonly object _proxyClientsLock = new object();                       // 代理客户端列表的锁

        private byte[] _readBuffer;                                                     // 收到的消息缓冲区
        private byte[] _payloadBuffer;                                                  // 收到的Payload缓冲区
        private readonly Queue<IMessage> _sendBuffers = new Queue<IMessage>();          // 发送消息队列
        private bool _sendingMessages;                                                  // 标记当前客户端是否正在发送消息
        private readonly object _sendingMessagesLock = new object();                    // 发送消息锁
        private readonly Queue<byte[]> _readBuffers = new Queue<byte[]>();              // 保存等待读取的缓冲区队列
        private bool _readingMessages;                                                  // 标记当前客户端是否正在读取消息
        private readonly object _readingMessagesLock = new object();                    // 读取消息锁

        // 收到的消息信息
        private int _readOffset;

        private int _writeOffset;
        private int _readableDataLen;
        private int _payloadLen;
        private EMessageType _receiveState = EMessageType.eMessageType_Header;

        #endregion 变量

        // 本客户端的所有代理客户端的数组
        public ReverseProxyClient[] ProxyClients
        {
            get
            {
                lock (_proxyClientsLock)
                {
                    return _proxyClients.ToArray();
                }
            }
        }

        // 构造函数，初始化序列化器类型
        protected Client(X509Certificate2 serverCertificate)
        {
            _serverCertificate = serverCertificate;
            _readBuffer = new byte[BUFFER_SIZE];
            TypeRegistry.AddTypesToSerializer(typeof(IMessage), TypeRegistry.GetPacketTypes(typeof(IMessage)).ToArray());
        }

        // 尝试连接指定IP和指定端口
        protected void Connect(IPAddress ip, ushort port)
        {
            Socket handle = null;
            try
            {
                Disconnect();

                handle = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                handle.SetKeepAliveEx(KEEP_ALIVE_INTERVAL, KEEP_ALIVE_TIME);
                handle.Connect(ip, port);

                if (handle.Connected)
                {
                    _stream = new SslStream(new NetworkStream(handle, true), false, ValidateServerCertificate);
                    _stream.AuthenticateAsClient(ip.ToString(), null, SslProtocols.Tls12, false);
                    _stream.BeginRead(_readBuffer, 0, _readBuffer.Length, AsyncReceive, null);
                    OnClientState(true);
                }
                else
                {
                    handle.Dispose();
                }
            }
            catch (Exception ex)
            {
                handle?.Dispose();
                OnClientFail(ex);
            }
        }

        /// <summary>
        /// 检查服务器证书是否正确
        /// </summary>
        /// <param name="sender">回调对象</param>
        /// <param name="certificate">等待验证的服务器证书</param>
        /// <param name="chain">X.509链</param>
        /// <param name="sslPolicyErrors">SSL策略错误</param>
        /// <returns>如果合法服务器证书，则返回true；否则返回false</returns>
        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
#if DEBUG
            Logger.Log(ELogType.eLogType_Warning, "当前是DEBUG模式，未启用服务器证书验证");
            return true;
#else
            var serverCsp = (RSACryptoServiceProvider)_serverCertificate.PublicKey.Key;
            var connectedCsp = (RSACryptoServiceProvider)new X509Certificate2(certificate).PublicKey.Key;
            // 将收到的服务器证书和本客户端包含的证书进行比较，以验证是否连接的是正确的服务器
            return _serverCertificate.Equals(certificate);
#endif
        }

        private void AsyncReceive(IAsyncResult result)
        {
            int bytesTransferred;
            try
            {
                bytesTransferred = _stream.EndRead(result);
                if (bytesTransferred <= 0)
                    throw new Exception("没有传输任何信息");
            }
            catch (NullReferenceException)
            {
                return;
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (Exception)
            {
                Disconnect();
                return;
            }

            byte[] received = new byte[bytesTransferred];
            try
            {
                Array.Copy(_readBuffer, received, received.Length);
            }
            catch (Exception ex)
            {
                OnClientFail(ex);
                return;
            }

            lock (_readBuffers)
            {
                _readBuffers.Enqueue(received);
            }

            lock (_readingMessagesLock)
            {
                if (!_readingMessages)
                {
                    _readingMessages = true;
                    ThreadPool.QueueUserWorkItem(AsyncReceive);
                }
            }

            try
            {
                _stream.BeginRead(_readBuffer, 0, _readBuffer.Length, AsyncReceive, null);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                OnClientFail(ex);
            }
        }

        private void AsyncReceive(object state)
        {
            while (true)
            {
                byte[] readBuffer;
                lock (_readBuffers)
                {
                    if (_readBuffers.Count == 0)
                    {
                        lock (_readingMessagesLock)
                        {
                            _readingMessages = false;
                        }
                        return;
                    }
                    readBuffer = _readBuffers.Dequeue();
                }

                _readableDataLen += readBuffer.Length;
                bool process = true;
                while (process)
                {
                    switch (_receiveState)
                    {
                        case EMessageType.eMessageType_Header:
                            {
                                if (_payloadBuffer == null)
                                    _payloadBuffer = new byte[HEADER_SIZE];

                                if (_readableDataLen + _writeOffset >= HEADER_SIZE)
                                {
                                    // 收到完整的头部信息
                                    int headerLength = HEADER_SIZE - _writeOffset;
                                    try
                                    {
                                        Array.Copy(readBuffer, _readOffset, _payloadBuffer, _writeOffset, headerLength);
                                        _payloadLen = BitConverter.ToInt32(_payloadBuffer, _readOffset);
                                        if (_payloadLen <= 0 || _payloadLen > MAX_MESSAGE_SIZE)
                                            throw new Exception("无效的消息头");

                                        // 尝试复用老的payload缓冲区
                                        if (_payloadBuffer.Length <= _payloadLen + HEADER_SIZE)
                                            Array.Resize(ref _payloadBuffer, _payloadLen + HEADER_SIZE);
                                    }
                                    catch (Exception)
                                    {
                                        process = false;
                                        Disconnect();
                                        break;
                                    }

                                    _readableDataLen -= headerLength;
                                    _writeOffset += headerLength;
                                    _readOffset += headerLength;
                                    _receiveState = EMessageType.eMessageType_Payload;
                                }
                                else // _readableDataLen + _writeOffset < HeaderSize
                                {
                                    // 仅仅收到部分头文件
                                    try
                                    {
                                        Array.Copy(readBuffer, _readOffset, _payloadBuffer, _writeOffset, _readableDataLen);
                                    }
                                    catch (Exception)
                                    {
                                        process = false;
                                        Disconnect();
                                        break;
                                    }
                                    _readOffset += _readableDataLen;
                                    _writeOffset += _readableDataLen;
                                    process = false;
                                }
                                break;
                            }
                        case EMessageType.eMessageType_Payload:
                            {
                                int length = (_writeOffset - HEADER_SIZE + _readableDataLen) >= _payloadLen
                                    ? _payloadLen - (_writeOffset - HEADER_SIZE)
                                    : _readableDataLen;

                                try
                                {
                                    Array.Copy(readBuffer, _readOffset, _payloadBuffer, _writeOffset, length);
                                }
                                catch (Exception)
                                {
                                    process = false;
                                    Disconnect();
                                    break;
                                }

                                _writeOffset += length;
                                _readOffset += length;
                                _readableDataLen -= length;

                                if (_writeOffset - HEADER_SIZE == _payloadLen)
                                {
                                    // 收到完整的Payload信息
                                    try
                                    {
                                        using (PayloadReader pr = new PayloadReader(_payloadBuffer, _payloadLen + HEADER_SIZE, false))
                                        {
                                            IMessage message = pr.ReadMessage();
                                            OnClientRead(message, _payloadBuffer.Length);
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        process = false;
                                        Disconnect();
                                        break;
                                    }

                                    _receiveState = EMessageType.eMessageType_Header;
                                    _payloadLen = 0;
                                    _writeOffset = 0;
                                }

                                if (_readableDataLen == 0)
                                    process = false;
                                break;
                            }
                    }
                }

                _readOffset = 0;
                _readableDataLen = 0;
            }
        }

        // 向连接的服务器发送消息
        public void Send<T>(T message) where T : IMessage
        {
            if (!Connected || message == null)
                return;

            lock (_sendBuffers)
            {
                _sendBuffers.Enqueue(message);

                lock (_sendingMessagesLock)
                {
                    if (_sendingMessages) return;

                    _sendingMessages = true;
                    ThreadPool.QueueUserWorkItem(ProcessSendBuffers);
                }
            }
        }

        // 向服务器发送消息（阻塞线程，直到消息发送完毕）
        public void SendBlocking<T>(T message) where T : IMessage
        {
            if (!Connected || message == null)
                return;

            SafeSendMessage(message);
        }

        // 安全发送消息（不会同时执行多个在 stream 上的写入操作）
        private void SafeSendMessage(IMessage message)
        {
            try
            {
                _singleWriteMutex.WaitOne();
                using (PayloadWriter pw = new PayloadWriter(_stream, true))
                {
                    OnClientWrite(message, pw.WriteMessage(message));
                }
            }
            catch (Exception)
            {
                Disconnect();
                SendCleanup(true);
            }
            finally
            {
                _singleWriteMutex.ReleaseMutex();
            }
        }

        private void ProcessSendBuffers(object state)
        {
            while (true)
            {
                if (!Connected)
                {
                    SendCleanup(true);
                    return;
                }

                IMessage message;
                lock (_sendBuffers)
                {
                    if (_sendBuffers.Count == 0)
                    {
                        SendCleanup();
                        return;
                    }
                    message = _sendBuffers.Dequeue();
                }

                SafeSendMessage(message);
            }
        }

        private void SendCleanup(bool clear = false)
        {
            lock (_sendingMessagesLock)
            {
                _sendingMessages = false;
            }

            if (!clear)
                return;

            lock (_sendBuffers)
            {
                _sendBuffers.Clear();
            }
        }

        public void Disconnect()
        {
            if (_stream != null)
            {
                _stream.Close();
                _readOffset = 0;
                _writeOffset = 0;
                _readableDataLen = 0;
                _payloadLen = 0;
                _payloadBuffer = null;
                _receiveState = EMessageType.eMessageType_Header;
                //_singleWriteMutex.Dispose(); TODO: 通过在断开连接时，创建新的客户端，来修复socket重用问题

                if (_proxyClients != null)
                {
                    lock (_proxyClientsLock)
                    {
                        try
                        {
                            foreach (ReverseProxyClient proxy in _proxyClients)
                                proxy.Disconnect();
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }

            OnClientState(false);
        }

        public void ConnectReverseProxy(ReverseProxyConnect command)
        {
            lock (_proxyClientsLock)
            {
                _proxyClients.Add(new ReverseProxyClient(command, this));
            }
        }

        public ReverseProxyClient GetReverseProxyByConnectionId(int connectionId)
        {
            lock (_proxyClientsLock)
            {
                return _proxyClients.FirstOrDefault(t => t.ConnectionId == connectionId);
            }
        }

        public void RemoveProxyClient(int connectionId)
        {
            try
            {
                lock (_proxyClientsLock)
                {
                    for (int i = 0; i < _proxyClients.Count; i++)
                    {
                        if (_proxyClients[i].ConnectionId == connectionId)
                        {
                            _proxyClients.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
            catch { }
        }
    }
}