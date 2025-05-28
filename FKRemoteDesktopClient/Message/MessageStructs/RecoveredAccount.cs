using ProtoBuf;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.MessageStructs
{
    [ProtoContract]
    public class RecoveredAccount
    {
        [ProtoMember(1)]
        public string KeyName { get; set; }

        [ProtoMember(2)]
        public string Username { get; set; }

        [ProtoMember(3)]
        public string Password { get; set; }

        [ProtoMember(4)]
        public string Url { get; set; }

        [ProtoMember(5)]
        public string Application { get; set; }
    }
}