using FKRemoteDesktop.Enums;
using ProtoBuf;
using System;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.MessageStructs
{
    [ProtoContract]
    public class FileSystemEntry
    {
        [ProtoMember(1)]
        public EFileType EntryType { get; set; }

        [ProtoMember(2)]
        public string Name { get; set; }

        [ProtoMember(3)]
        public long Size { get; set; }

        [ProtoMember(4)]
        public DateTime LastAccessTimeUtc { get; set; }

        [ProtoMember(5)]
        public EContentType? ContentType { get; set; }
    }
}