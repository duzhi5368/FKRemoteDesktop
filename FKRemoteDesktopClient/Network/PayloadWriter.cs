using FKRemoteDesktop.Message;
using ProtoBuf;
using System;
using System.IO;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Network
{
    public class PayloadWriter : MemoryStream
    {
        private readonly Stream _innerStream;
        public bool LeaveInnerStreamOpen { get; }

        public PayloadWriter(Stream stream, bool leaveInnerStreamOpen)
        {
            _innerStream = stream;
            LeaveInnerStreamOpen = leaveInnerStreamOpen;
        }

        public void WriteBytes(byte[] value)
        {
            _innerStream.Write(value, 0, value.Length);
        }

        public void WriteInteger(int value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        public int WriteMessage(IMessage message)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Serializer.Serialize(ms, message);
                byte[] payload = ms.ToArray();
                WriteInteger(payload.Length);
                WriteBytes(payload);
                return sizeof(int) + payload.Length;
            }
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