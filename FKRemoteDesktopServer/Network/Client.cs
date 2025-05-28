using FKRemoteDesktop.Enums;
using FKRemoteDesktop.Message;
using FKRemoteDesktop.Structs;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Threading;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Network
{
    public class Client : IEquatable<Client>, ISender
    {
        #region 消息事件
        /// <summary>
        /// 客户端状态更变事件
        /// </summary>
        /// <param name="s">状态发生更变的客户端</param>
        /// <param name="connected">客户端新的连接状态</param>
        public delegate void ClientStateEventHandler(Client s, bool connected);
        public event ClientStateEventHandler ClientState;

        /// <summary>
        /// 客户端收到消息事件
        /// </summary>
        /// <param name="s">收到消息的客户端</param>
        /// <param name="message">客户端收到的消息</param>
        /// <param name="messageLength">消息的长度</param>
        public delegate void ClientReadEventHandler(Client s, IMessage message, int messageLength);
        public event ClientReadEventHandler ClientRead;

        /// <summary>
        /// 客户端发送消息事件
        /// </summary>
        /// <param name="s">发送消息的客户端</param>
        /// <param name="message">客户端发送的消息</param>
        /// <param name="messageLength">消息的长度</param>
        public delegate void ClientWriteEventHandler(Client s, IMessage message, int messageLength);
        public event ClientWriteEventHandler ClientWrite;
        #endregion

        #region 变量
        private const int HEADER_SIZE = 4;                                      // 消息头长度
        private const int MAX_MESSAGE_SIZE = (1024 * 1024) * 5;                 // 消息最大长度

        public DateTime ConnectedTime { get; private set; }                     // 客户端连接时间
        public bool Connected { get; private set; }                             // 客户端当前连接状态
        public bool Identified { get; set; }                                    // 客户端是否已被服务器识别
        public IPEndPoint EndPoint { get; }                                     // 客户端自身的IP和Port
        public SUserInfo UserInfo { get; set; }                                 // 客户端信息

        private readonly SslStream _stream;                                     // 通信流
        private readonly BufferPool _bufferPool;                                // 接受到的消息缓冲池

        private readonly Queue<IMessage> _sendBuffers = new Queue<IMessage>();  // 发送消息队列
        private bool _sendingMessages;                                          // 客户端当前是否正在发送消息
        private readonly object _sendingMessagesLock = new object();            // 是否正在发送消息变量的锁

        private readonly Queue<byte[]> _readBuffers = new Queue<byte[]>();      // 接受消息队列
        private bool _readingMessages;                                          // 客户端是否正在读取消息
        private readonly object _readingMessagesLock = new object();            // 是否正在读取消息变量的锁

        private readonly byte[] _readBuffer;                                    // 客户端收到的消息缓冲
        private byte[] _payloadBuffer;                                          // 客户端收到的payload缓冲

        private int _readOffset;
        private int _writeOffset;
        private int _readableDataLen;
        private int _payloadLen;
        private EMessageType _receiveState = EMessageType.eMessageType_Header;

        private readonly Mutex _singleWriteMutex = new Mutex();
        #endregion

        #region 消息处理函数
        // 客户端状态更变处理函数
        private void OnClientState(bool connected)
        {
            if (Connected == connected) 
                return;

            Connected = connected;

            var handler = ClientState;
            handler?.Invoke(this, connected);
        }

        // 客户端收到消息处理函数
        private void OnClientRead(IMessage message, int messageLength)
        {
            var handler = ClientRead;
            handler?.Invoke(this, message, messageLength);
        }

        // 客户端发送消息处理函数
        private void OnClientWrite(IMessage message, int messageLength)
        {
            var handler = ClientWrite;
            handler?.Invoke(this, message, messageLength);
        }
        #endregion

        #region 继承辅助函数
        public static bool operator ==(Client c1, Client c2)
        {
            if (ReferenceEquals(c1, null))
                return ReferenceEquals(c2, null);

            return c1.Equals(c2);
        }

        public static bool operator !=(Client c1, Client c2)
        {
            return !(c1 == c2);
        }

        // 检查两个客户端是否为同一客户端
        public bool Equals(Client other)
        {
            if (ReferenceEquals(null, other)) 
                return false;
            if (ReferenceEquals(this, other)) 
                return true;

            try
            {
                // 对每个客户端而言，其 IP和Port 一定唯一
                return this.EndPoint.Port.Equals(other.EndPoint.Port);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as Client);
        }

        // 本实例的唯一HashCode
        public override int GetHashCode()
        {
            return this.EndPoint.GetHashCode();
        }
        #endregion

        #region 核心功能函数
        public Client(BufferPool bufferPool, SslStream stream, IPEndPoint endPoint)
        {
            try
            {
                Identified = false;
                UserInfo = new SUserInfo();
                EndPoint = endPoint;
                ConnectedTime = DateTime.UtcNow;
                _stream = stream;
                _bufferPool = bufferPool;
                _readBuffer = _bufferPool.GetBuffer();
                _stream.BeginRead(_readBuffer, 0, _readBuffer.Length, AsyncReceive, null);
                OnClientState(true);
            }
            catch (Exception)
            {
                Disconnect();
            }
        }

        private void AsyncReceive(IAsyncResult result)
        {
            int bytesTransferred;
            try
            {
                bytesTransferred = _stream.EndRead(result);
                if (bytesTransferred <= 0)
                    throw new Exception("no bytes transferred");
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
            catch (Exception)
            {
                Disconnect();
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
            catch (Exception)
            {
                Disconnect();
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
                                    // 完整接受消息头
                                    int headerLength = HEADER_SIZE - _writeOffset;
                                    try
                                    {
                                        Array.Copy(readBuffer, _readOffset, _payloadBuffer, _writeOffset, headerLength);
                                        _payloadLen = BitConverter.ToInt32(_payloadBuffer, _readOffset);
                                        if (_payloadLen <= 0 || _payloadLen > MAX_MESSAGE_SIZE)
                                            throw new Exception("invalid header");
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
                                    // 仅仅收到部分消息头
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
                                    // 完整接收到Payload
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

        // 发送消息，将消息放入发送消息队列
        public void Send<T>(T message) where T : IMessage
        {
            if (!Connected || message == null) 
                return;
            lock (_sendBuffers)
            {
                _sendBuffers.Enqueue(message);
                lock (_sendingMessagesLock)
                {
                    if (_sendingMessages) 
                        return;
                    _sendingMessages = true;
                    ThreadPool.QueueUserWorkItem(ProcessSendBuffers);
                }
            }
        }

        // 消息发送（会阻塞线程）
        public void SendBlocking<T>(T message) where T : IMessage
        {
            if (!Connected || message == null) 
                return;
            SafeSendMessage(message);
        }

        // 真正的安全发送消息（线程安全）
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
                try
                {
                    _singleWriteMutex.ReleaseMutex();
                }catch { }
            }
        }

        // 处理消息发送队列
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

        // 断开网络连接
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
                _singleWriteMutex.Dispose();

                _bufferPool.ReturnBuffer(_readBuffer);
            }

            OnClientState(false);
        }
        #endregion
    }
}
