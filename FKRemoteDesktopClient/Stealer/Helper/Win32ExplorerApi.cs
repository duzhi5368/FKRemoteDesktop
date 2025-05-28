using System;
using System.Runtime.InteropServices;
using System.Text;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Stealer.Helper
{
    public class Win32api
    {
        [Flags]
        public enum shlwapi_URL : uint
        {
            URL_DONT_SIMPLIFY = 0x08000000,
            URL_ESCAPE_PERCENT = 0x00001000,
            URL_ESCAPE_SPACES_ONLY = 0x04000000,
            URL_ESCAPE_UNSAFE = 0x20000000,
            URL_PLUGGABLE_PROTOCOL = 0x40000000,
            URL_UNESCAPE = 0x10000000
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SHFILEINFO
        {
            public IntPtr hIcon;
            public IntPtr iIcon;
            public uint dwAttributes;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        public struct SYSTEMTIME
        {
            public Int16 Day;
            public Int16 DayOfWeek;
            public Int16 Hour;
            public Int16 Milliseconds;
            public Int16 Minute;
            public Int16 Month;
            public Int16 Second;
            public Int16 Year;
        }

        public const uint SHGFI_ATTR_SPECIFIED = 0x20000;
        public const uint SHGFI_ATTRIBUTES = 0x800;
        public const uint SHGFI_PIDL = 0x8;
        public const uint SHGFI_DISPLAYNAME = 0x200;
        public const uint SHGFI_USEFILEATTRIBUTES = 0x10;
        public const uint FILE_ATTRIBUTRE_NORMAL = 0x4000;
        public const uint SHGFI_EXETYPE = 0x2000;
        public const uint SHGFI_SYSICONINDEX = 0x4000;
        public const uint ILC_COLORDDB = 0x1;
        public const uint ILC_MASK = 0x0;
        public const uint ILD_TRANSPARENT = 0x1;
        public const uint SHGFI_ICON = 0x100;
        public const uint SHGFI_LARGEICON = 0x0;
        public const uint SHGFI_SHELLICONSIZE = 0x4;
        public const uint SHGFI_SMALLICON = 0x1;
        public const uint SHGFI_TYPENAME = 0x400;
        public const uint SHGFI_ICONLOCATION = 0x1000;

        [DllImport("shlwapi.dll")]
        public static extern int UrlCanonicalize(
            string pszUrl,
            StringBuilder pszCanonicalized,
            ref int pcchCanonicalized,
            shlwapi_URL dwFlags
        );

        public static string CannonializeURL(string pszUrl, shlwapi_URL dwFlags)
        {
            var buff = new StringBuilder(260);
            int s = buff.Capacity;
            int c = UrlCanonicalize(pszUrl, buff, ref s, dwFlags);
            if (c == 0)
                return buff.ToString();
            else
            {
                buff.Capacity = s;
                c = UrlCanonicalize(pszUrl, buff, ref s, dwFlags);
                return buff.ToString();
            }
        }

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool FileTimeToSystemTime
        (ref System.Runtime.InteropServices.ComTypes.FILETIME FileTime, ref SYSTEMTIME SystemTime);

        public static DateTime FileTimeToDateTime(System.Runtime.InteropServices.ComTypes.FILETIME filetime)
        {
            var st = new SYSTEMTIME();
            FileTimeToSystemTime(ref filetime, ref st);
            try
            {
                return new DateTime(st.Year, st.Month, st.Day, st.Hour, st.Minute, st.Second, st.Milliseconds);
            }
            catch (Exception)
            {
                return DateTime.Now;
            }
        }

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool SystemTimeToFileTime([In] ref SYSTEMTIME lpSystemTime,
                                                                out System.Runtime.InteropServices.ComTypes.FILETIME lpFileTime);

        public static System.Runtime.InteropServices.ComTypes.FILETIME DateTimeToFileTime(DateTime datetime)
        {
            var st = new SYSTEMTIME();
            st.Year = (short)datetime.Year;
            st.Month = (short)datetime.Month;
            st.Day = (short)datetime.Day;
            st.Hour = (short)datetime.Hour;
            st.Minute = (short)datetime.Minute;
            st.Second = (short)datetime.Second;
            st.Milliseconds = (short)datetime.Millisecond;
            System.Runtime.InteropServices.ComTypes.FILETIME filetime;
            SystemTimeToFileTime(ref st, out filetime);
            return filetime;
        }

        [DllImport("Kernel32.dll")]
        public static extern int CompareFileTime([In] ref System.Runtime.InteropServices.ComTypes.FILETIME lpFileTime1,
            [In] ref System.Runtime.InteropServices.ComTypes.FILETIME lpFileTime2);

        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi,
          uint cbSizeFileInfo, uint uFlags);
    }
}