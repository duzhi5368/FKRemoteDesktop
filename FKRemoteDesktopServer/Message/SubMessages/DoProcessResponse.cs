using FKRemoteDesktop.Enums;
using ProtoBuf;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.SubMessages
{
    [ProtoContract]
    public class DoProcessResponse : IMessage
    {
        [ProtoMember(1)]
        public EProcessAction Action { get; set; }

        [ProtoMember(2)]
        public bool Result { get; set; }
    }
}
