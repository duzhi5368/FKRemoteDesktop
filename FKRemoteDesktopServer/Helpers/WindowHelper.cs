using FKRemoteDesktop.Network;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Helpers
{
    public static class WindowHelper
    {
        public static string GetWindowTitle(string title, Client c)
        {
            return string.Format("{0} - {1}@{2} [{3}:{4}]", title, 
                c.UserInfo.Username, c.UserInfo.PcName, c.EndPoint.Address.ToString(), c.EndPoint.Port.ToString());
        }

        public static string GetWindowTitle(string title, int count)
        {
            return string.Format("{0} [已选中: {1}]", title, count);
        }
    }
}
