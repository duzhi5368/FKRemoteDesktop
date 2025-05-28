using System.Runtime.InteropServices;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Donut
{
    public struct DSCrypt
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] mk;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] ctr;
    }
}
