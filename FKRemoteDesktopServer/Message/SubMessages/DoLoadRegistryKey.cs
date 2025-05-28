using ProtoBuf;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.SubMessages
{
    [ProtoContract]
    public class DoLoadRegistryKey : IMessage
    {
        [ProtoMember(1)]
        public string RootKeyName { get; set; }
    }
}
