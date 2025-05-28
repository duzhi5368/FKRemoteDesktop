using FKRemoteDesktop.Configs;
using FKRemoteDesktop.Helpers;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Install
{
    public class ClientUpdater : ClientSetupBase
    {
        public void Update(string newFilePath)
        {
            FileHelper.DeleteZoneIdentifier(newFilePath);

            var bytes = File.ReadAllBytes(newFilePath);
            if (!FileHelper.HasExecutableIdentifier(bytes))
                throw new Exception("没有可执行文件.");

            string batchFile = BatchFileHelper.CreateUpdateBatch(Application.ExecutablePath, newFilePath);

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = true,
                FileName = batchFile
            };
            Process.Start(startInfo);

            if (SettingsFromServer.STARTUP)
            {
                var clientStartup = new ClientStartup();
                clientStartup.RemoveFromStartup(SettingsFromClient.STARTUPKEY);
            }
        }
    }
}