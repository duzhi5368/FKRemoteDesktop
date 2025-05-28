using FKRemoteDesktop.Message.MessageStructs;
using ProtoBuf;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.SubMessages
{
    [ProtoContract]
    public class GetConnectionsResponse : IMessage
    {
        [ProtoMember(1)]
        public TcpConnection[] Connections { get; set; }
    }
}