using FKRemoteDesktop.Enums;
using System;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Extensions
{
    public static class StringExtensions
    {
        public static T ForceTo<T>(this object @this)
        {
            return (T)Convert.ChangeType(@this, typeof(T));
        }

        public static EContentType ToContentType(this string fileExtension)
        {
            switch (fileExtension.ToLower())
            {
                default:
                    return EContentType.eContentType_Blob;

                case ".exe":
                    return EContentType.eContentType_Application;

                case ".txt":
                case ".log":
                case ".conf":
                case ".cfg":
                case ".asc":
                    return EContentType.eContentType_Text;

                case ".rar":
                case ".zip":
                case ".zipx":
                case ".tar":
                case ".tgz":
                case ".gz":
                case ".s7z":
                case ".7z":
                case ".bz2":
                case ".cab":
                case ".zz":
                case ".apk":
                    return EContentType.eContentType_Archive;

                case ".doc":
                case ".docx":
                case ".odt":
                    return EContentType.eContentType_Word;

                case ".pdf":
                    return EContentType.eContentType_Pdf;

                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".bmp":
                case ".gif":
                case ".ico":
                    return EContentType.eContentType_Image;

                case ".mp4":
                case ".mov":
                case ".avi":
                case ".wmv":
                case ".mkv":
                case ".m4v":
                case ".flv":
                    return EContentType.eContentType_Video;

                case ".mp3":
                case ".wav":
                case ".pls":
                case ".m3u":
                case ".m4a":
                    return EContentType.eContentType_Audio;
            }
        }
    }
}