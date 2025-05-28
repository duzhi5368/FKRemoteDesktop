using FKRemoteDesktop.Enums;
using ProtoBuf;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.MessageStructs
{
    [ProtoContract]
    public class StartupItem
    {
        [ProtoMember(1)]
        public string Name { get; set; }

        [ProtoMember(2)]
        public string Path { get; set; }

        [ProtoMember(3)]
        public EStartupType Type { get; set; }
    }
}