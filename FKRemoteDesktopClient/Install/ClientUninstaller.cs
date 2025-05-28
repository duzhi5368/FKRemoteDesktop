using FKRemoteDesktop.Configs;
using FKRemoteDesktop.Helpers;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Install
{
    public class ClientUninstaller : ClientSetupBase
    {
        public void Uninstall()
        {
            if (SettingsFromServer.STARTUP)
            {
                var clientStartup = new ClientStartup();
                clientStartup.RemoveFromStartup(SettingsFromClient.STARTUPKEY);
            }

            if (SettingsFromServer.ENABLELOGGER && Directory.Exists(SettingsFromClient.LOGSPATH))
            {
                Regex reg = new Regex(@"^\d{4}\-(0[1-9]|1[012])\-(0[1-9]|[12][0-9]|3[01])$");
                foreach (var logFile in Directory.GetFiles(SettingsFromClient.LOGSPATH, "*", SearchOption.TopDirectoryOnly)
                    .Where(path => reg.IsMatch(Path.GetFileName(path))).ToList())
                {
                    try
                    {
                        File.Delete(logFile);
                    }
                    catch (Exception) { }
                }
            }

            string batchFile = BatchFileHelper.CreateUninstallBatch(Application.ExecutablePath);
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = true,
                FileName = batchFile
            };
            ProcessHelper.StartProcess(startInfo);
        }
    }
}