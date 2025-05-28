using System;
using System.Collections;
using System.Runtime.InteropServices;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Stealer.Helper
{
    public class SortFileTimeAscendingHelper : IComparer
    {
        public enum STATURLFLAGS : uint
        {
            STATURLFLAG_ISCACHED = 0x00000001,
            STATURLFLAG_ISTOPLEVEL = 0x00000002,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct STATURL
        {
            public int cbSize;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string pwcsUrl;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string pwcsTitle;

            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastVisited;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastUpdated;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftExpires;
            public STATURLFLAGS dwFlags;

            public string URL
            {
                get
                {
                    return pwcsUrl;
                }
            }

            public string UrlString
            {
                get
                {
                    int index = pwcsUrl.IndexOf('?');
                    return index < 0 ? pwcsUrl : pwcsUrl.Substring(0, index);
                }
            }

            public string Title
            {
                get
                {
                    if (pwcsUrl.StartsWith("file:"))
                        return Win32api.CannonializeURL(pwcsUrl, Win32api.shlwapi_URL.URL_UNESCAPE).Substring(8).Replace(
                            '/', '\\');
                    return pwcsTitle;
                }
            }

            public DateTime LastVisited
            {
                get
                {
                    return Win32api.FileTimeToDateTime(ftLastVisited).ToLocalTime();
                }
            }

            public DateTime LastUpdated
            {
                get
                {
                    return Win32api.FileTimeToDateTime(ftLastUpdated).ToLocalTime();
                }
            }

            public DateTime Expires
            {
                get
                {
                    try
                    {
                        return Win32api.FileTimeToDateTime(ftExpires).ToLocalTime();
                    }
                    catch (Exception)
                    {
                        return DateTime.Now;
                    }
                }
            }

            public override string ToString()
            {
                return pwcsUrl;
            }
        }

        int IComparer.Compare(object a, object b)
        {
            var c1 = (STATURL)a;
            var c2 = (STATURL)b;
            return (CompareFileTime(ref c1.ftLastVisited, ref c2.ftLastVisited));
        }

        [DllImport("Kernel32.dll")]
        private static extern int CompareFileTime([In] ref System.Runtime.InteropServices.ComTypes.FILETIME lpFileTime1,
            [In] ref System.Runtime.InteropServices.ComTypes.FILETIME lpFileTime2);

        public static IComparer SortFileTimeAscending()
        {
            return new SortFileTimeAscendingHelper();
        }
    }
}