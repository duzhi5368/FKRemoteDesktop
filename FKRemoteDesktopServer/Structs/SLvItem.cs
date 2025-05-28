using System;
using System.Runtime.InteropServices;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Structs
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class SLvItem
    {
        public uint mask;
        public int iItem;
        public int iSubItem;
        public int state;
        public int stateMask;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string pszText;
        public int cchTextMax;
        public int iImage;
        public IntPtr lParam;
        public int iIndent;
        public int iGroupId;
        public uint cColumns;
        public IntPtr puColumns;
        public IntPtr piColFmt;
        public int iGroup;
    }
}
