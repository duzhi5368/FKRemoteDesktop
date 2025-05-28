using ProtoBuf;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.SubMessages
{
    [ProtoContract]
    public class DoProcessEnd : IMessage
    {
        [ProtoMember(1)]
        public int Pid { get; set; }
    }
}