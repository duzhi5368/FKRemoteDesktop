using System;
using System.Management;
using System.Text.RegularExpressions;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Helpers
{
    public static class PlatformHelper
    {
        public static string FullName { get; }                  // 操作系统完整名（包括架构）
        public static string Name { get; }                      // 操作系统名
        public static bool Is64Bit { get; }                     // 是否是64位操作系统
        public static bool RunningOnMono { get; }               // 系统是否运行 Mono
        public static bool Win32NT { get; }                     // 操作系统是否基于Windows32 NT
        public static bool XpOrHigher { get; }                  // 操作系统是否基于Windows XP 或更高版本
        public static bool VistaOrHigher { get; }               // 操作系统是否是 Windows Vista 或更高版本
        public static bool SevenOrHigher { get; }               // 操作系统是否是 Windows 7 或更高版本
        public static bool EightOrHigher { get; }               // 操作系统是否是 Windows 8 或更高版本
        public static bool EightPointOneOrHigher { get; }       // 操作系统是否是 Windows 8.1 或更高版本
        public static bool TenOrHigher { get; }                 // 操作系统是否是 Windows 10 或更高版本

        static PlatformHelper()
        {
            Win32NT = Environment.OSVersion.Platform == PlatformID.Win32NT;
            XpOrHigher = Win32NT && Environment.OSVersion.Version.Major >= 5;
            VistaOrHigher = Win32NT && Environment.OSVersion.Version.Major >= 6;
            SevenOrHigher = Win32NT && (Environment.OSVersion.Version >= new Version(6, 1));
            EightOrHigher = Win32NT && (Environment.OSVersion.Version >= new Version(6, 2, 9200));
            EightPointOneOrHigher = Win32NT && (Environment.OSVersion.Version >= new Version(6, 3));
            TenOrHigher = Win32NT && (Environment.OSVersion.Version >= new Version(10, 0));
            RunningOnMono = Type.GetType("Mono.Runtime") != null;

            Name = "Unknown OS";
            using (var searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem"))
            {
                foreach (ManagementObject os in searcher.Get())
                {
                    Name = os["Caption"].ToString();
                    break;
                }
            }
            Name = Regex.Replace(Name, "^.*(?=Windows)", "").TrimEnd().TrimStart();

            Is64Bit = Environment.Is64BitOperatingSystem;
            FullName = $"{Name} {(Is64Bit ? 64 : 32)} Bit";
        }
    }
}