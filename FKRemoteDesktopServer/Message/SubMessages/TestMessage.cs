using ProtoBuf;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.SubMessages
{
    [ProtoContract]
    public class TestMessage : IMessage
    {
        [ProtoMember(1)]
        public string String1 { get; set; }

        [ProtoMember(2)]
        public string String2 { get; set; }
    }
}
