using ProtoBuf;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.SubMessages
{
    [ProtoContract]
    public class DoCreateRegistryKey : IMessage
    {
        [ProtoMember(1)]
        public string ParentPath { get; set; }
    }
}