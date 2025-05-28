using System.IO;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Extensions
{
    public static class DriveTypeExtensions
    {
        public static string ToFriendlyString(this DriveType type)
        {
            switch (type)
            {
                case DriveType.Fixed:
                    return "本地硬盘";

                case DriveType.Network:
                    return "网络硬盘";

                case DriveType.Removable:
                    return "可移动磁盘";

                default:
                    return type.ToString();
            }
        }
    }
}