using FKRemoteDesktop.DllHook;
using FKRemoteDesktop.Helpers;
using System;
using System.Windows.Forms;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Extensions
{
    public static class ListViewExtensions
    {
        private const uint SET_COLUMN_WIDTH = 4126;
        private static readonly IntPtr AUTOSIZE_USEHEADER = new IntPtr(-2);

        public static void AutosizeColumns(this ListView targetListView)
        {
            if (PlatformHelper.RunningOnMono) return;
            for (int lngColumn = 0; lngColumn <= (targetListView.Columns.Count - 1); lngColumn++)
            {
                NativeMethods.SendMessage(targetListView.Handle, SET_COLUMN_WIDTH, new IntPtr(lngColumn), AUTOSIZE_USEHEADER);
            }
        }

        public static void SelectAllItems(this ListView targetListView)
        {
            NativeMethodsHelper.SetItemState(targetListView.Handle, -1, 2, 2);
        }
    }
}
