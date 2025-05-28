using ProtoBuf;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.SubMessages
{
    [ProtoContract]
    public class ReverseProxyDisconnect : IMessage
    {
        [ProtoMember(1)]
        public int ConnectionId { get; set; }
    }
}
