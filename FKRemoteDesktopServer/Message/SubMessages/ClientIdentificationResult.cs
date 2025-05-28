using ProtoBuf;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.SubMessages
{
    [ProtoContract]
    public class ClientIdentificationResult : IMessage
    {
        [ProtoMember(1)]
        public bool Result { get; set; }
    }
}
