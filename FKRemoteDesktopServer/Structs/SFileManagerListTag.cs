using FKRemoteDesktop.Enums;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Structs
{
    public class SFileManagerListTag
    {
        public EFileType Type { get; set; }
        public long FileSize { get; set; }

        public SFileManagerListTag(EFileType type, long fileSize)
        {
            this.Type = type;
            this.FileSize = fileSize;
        }
    }
}
