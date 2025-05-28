using FKRemoteDesktop.Enums;
using FKRemoteDesktop.Utilities;
using System;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Structs
{
    public class SFileTransfer : IEquatable<SFileTransfer>
    {
        private static readonly SafeRandom Random = new SafeRandom();

        public int Id { get; set; }
        public ETransferType Type { get; set; }
        public long Size { get; set; }
        public long TransferredSize { get; set; }
        public string LocalPath { get; set; }
        public string RemotePath { get; set; }
        public string Status { get; set; }
        public FileSplit FileSplit { get; set; }

        public bool Equals(SFileTransfer other)
        {
            if (ReferenceEquals(null, other)) 
                return false;
            if (ReferenceEquals(this, other)) 
                return true;
            return Id == other.Id && Type == other.Type && Size == other.Size &&
                   TransferredSize == other.TransferredSize && string.Equals(LocalPath, other.LocalPath) &&
                   string.Equals(RemotePath, other.RemotePath) && string.Equals(Status, other.Status);
        }

        public SFileTransfer Clone()
        {
            return new SFileTransfer()
            {
                Id = Id,
                Type = Type,
                Size = Size,
                TransferredSize = TransferredSize,
                LocalPath = LocalPath,
                RemotePath = RemotePath,
                Status = Status,
                FileSplit = FileSplit
            };
        }

        public static bool operator ==(SFileTransfer f1, SFileTransfer f2)
        {
            if (ReferenceEquals(f1, null))
                return ReferenceEquals(f2, null);

            return f1.Equals(f2);
        }

        public static bool operator !=(SFileTransfer f1, SFileTransfer f2)
        {
            return !(f1 == f2);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SFileTransfer);
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public static int GetRandomTransferId()
        {
            return Random.Next(0, int.MaxValue);
        }
    }
}
