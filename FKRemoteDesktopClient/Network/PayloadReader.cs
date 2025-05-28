using FKRemoteDesktop.Message;
using ProtoBuf;
using System;
using System.IO;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Network
{
    public class PayloadReader : MemoryStream
    {
        private readonly Stream _innerStream;
        public bool LeaveInnerStreamOpen { get; }

        public PayloadReader(byte[] payload, int length, bool leaveInnerStreamOpen)
        {
            _innerStream = new MemoryStream(payload, 0, length, false, true);
            LeaveInnerStreamOpen = leaveInnerStreamOpen;
        }

        public PayloadReader(Stream stream, bool leaveInnerStreamOpen)
        {
            _innerStream = stream;
            LeaveInnerStreamOpen = leaveInnerStreamOpen;
        }

        public int ReadInteger()
        {
            return BitConverter.ToInt32(ReadBytes(4), 0);
        }

        public byte[] ReadBytes(int length)
        {
            if (_innerStream.Position + length <= _innerStream.Length)
            {
                byte[] result = new byte[length];
                _innerStream.Read(result, 0, result.Length);
                return result;
            }
            throw new OverflowException($"Unable to read {length} bytes from stream");
        }

        // 读取payload并进行反序列化
        public IMessage ReadMessage()
        {
            /*
            int payloadLength = ReadInteger();
            byte[] data = ReadBytes(payloadLength);
            Logger.Log(Enums.ELogType.eLogType_Debug, $"Received: Length={payloadLength}, Data={BitConverter.ToString(data)}");
            IMessage message = null;
            using (var ms = new MemoryStream(data))
            {
                // 使用 RuntimeTypeModel 反序列化
                message = ProtoBuf.Meta.RuntimeTypeModel.Default.Deserialize(ms, null, typeof(IMessage)) as IMessage;
            }
            return message;
            */
            ReadInteger();

            // 这里忽略了 Length 前缀，交给Client类进行处理
            IMessage message = Serializer.Deserialize<IMessage>(_innerStream);
            return message;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (LeaveInnerStreamOpen)
                {
                    _innerStream.Flush();
                }
                else
                {
                    _innerStream.Close();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}