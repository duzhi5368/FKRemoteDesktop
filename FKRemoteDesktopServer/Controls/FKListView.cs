using FKRemoteDesktop.DllHook;
using FKRemoteDesktop.Helpers;
using System;
using System.Windows.Forms;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Controls
{
    internal class FKListView : ListView
    {
        private const uint WM_CHANGE_UI_STATE = 0x127;
        private const short UIS_SET = 1;
        private const short UISF_HIDE_FOCUS = 0x1;
        private const int SB_VERT = 1; // 垂直滚动条

        private readonly IntPtr _removeDots = new IntPtr(NativeMethodsHelper.MakeWin32Long(UIS_SET, UISF_HIDE_FOCUS));

        public ListViewColumnSorter ListViewColumnSorter { get; set; }

        public FKListView()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            this.ListViewColumnSorter = new ListViewColumnSorter();
            this.ListViewItemSorter = ListViewColumnSorter;
            this.View = View.Details;
            this.FullRowSelect = true;

            // 启用双缓冲以减少闪烁
            this.DoubleBuffered = true;
            // 初始化时强制显示垂直滚动条
            ShowVerticalScrollBar();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (PlatformHelper.RunningOnMono) 
                return;
            if (PlatformHelper.VistaOrHigher)
            {
                // 设置窗口主题为资源管理器风格
                NativeMethods.SetWindowTheme(this.Handle, "explorer", null);
            }
            if (PlatformHelper.XpOrHigher)
            {
                // 删除虚线显示
                NativeMethods.SendMessage(this.Handle, WM_CHANGE_UI_STATE, _removeDots, IntPtr.Zero);
            }

            // 控件句柄创建后，显示垂直滚动条
            ShowVerticalScrollBar();
        }

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            base.WndProc(ref m);
            // 每次窗口消息处理后，确保垂直滚动条可见
            ShowVerticalScrollBar();
        }

        private void ShowVerticalScrollBar()
        {
            if (this.IsHandleCreated)
            {
                // 调用 Windows API 强制显示垂直滚动条
                NativeMethods.ShowScrollBar(this.Handle, SB_VERT, true);
            }
        }

        protected override void OnColumnClick(ColumnClickEventArgs e)
        {
            base.OnColumnClick(e);

            if (e.Column == this.ListViewColumnSorter.SortColumn)
            {
                this.ListViewColumnSorter.Order = (this.ListViewColumnSorter.Order == SortOrder.Ascending)
                    ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                this.ListViewColumnSorter.SortColumn = e.Column;
                this.ListViewColumnSorter.Order = SortOrder.Ascending;
            }

            // 重新排序
            if (!this.VirtualMode)
                this.Sort();
        }
    }
}
