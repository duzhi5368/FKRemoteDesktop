using System.Diagnostics;
using System.Windows.Forms;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Helpers
{
    public static class ProcessHelper
    {
        public static Process StartProcess(ProcessStartInfo psi)
        {
#if DEBUG
            var answer = MessageBox.Show("确定重启启动新进程吗？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (answer == DialogResult.Yes)
            {
                return Process.Start(psi);
            }
            return null;
#else
            return Process.Start(psi);
#endif
        }
    }
}