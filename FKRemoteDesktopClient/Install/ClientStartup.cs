using FKRemoteDesktop.Enums;
using FKRemoteDesktop.Helpers;
using Microsoft.Win32;
using System.Diagnostics;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Install
{
    public class ClientStartup : ClientSetupBase
    {
        public void AddToStartup(string executablePath, string startupName)
        {
            if (UserAccount.Type == EAccountType.eAccountType_Admin)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo("schtasks")
                {
                    Arguments = "/create /tn \"" + startupName + "\" /sc ONLOGON /tr \"" + executablePath +
                                "\" /rl HIGHEST /f",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process p = ProcessHelper.StartProcess(startInfo);
                if (p != null)
                {
                    p.WaitForExit(1000);
                    if (p.ExitCode == 0)
                        return;
                }
            }
            RegistryKeyHelper.AddRegistryKeyValue(RegistryHive.CurrentUser,
                "Software\\Microsoft\\Windows\\CurrentVersion\\Run", startupName, executablePath,
                true);
        }

        public void RemoveFromStartup(string startupName)
        {
            if (UserAccount.Type == EAccountType.eAccountType_Admin)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo("schtasks")
                {
                    Arguments = "/delete /tn \"" + startupName + "\" /f",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process p = ProcessHelper.StartProcess(startInfo);
                if (p != null)
                {
                    p.WaitForExit(1000);
                    if (p.ExitCode == 0)
                        return;
                }
            }
            RegistryKeyHelper.DeleteRegistryKeyValue(RegistryHive.CurrentUser,
                "Software\\Microsoft\\Windows\\CurrentVersion\\Run", startupName);
        }
    }
}