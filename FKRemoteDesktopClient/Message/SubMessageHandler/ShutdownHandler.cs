using FKRemoteDesktop.Debugger;
using FKRemoteDesktop.Enums;
using FKRemoteDesktop.Message.SubMessages;
using System;
using System.Diagnostics;
using System.Windows.Forms;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.SubMessageHandler
{
    public class ShutdownHandler : IMessageProcessor
    {
        public bool CanExecute(IMessage message) => message is DoShutdownAction;

        public bool CanExecuteFrom(ISender sender) => true;

        public void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case DoShutdownAction msg:
                    Logger.Log(ELogType.eLogType_Debug, "收到服务器消息: DoShutdownAction");
                    Execute(sender, msg);
                    break;
            }
        }

        private void Execute(ISender client, DoShutdownAction message)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                switch (message.Action)
                {
                    case EShutdownAction.eShutdownAction_Shutdown:
                        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        startInfo.UseShellExecute = true;
                        startInfo.Arguments = "/s /t 0";
                        startInfo.FileName = "shutdown";
                        Process.Start(startInfo);
                        break;

                    case EShutdownAction.eShutdownAction_Restart:
                        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        startInfo.UseShellExecute = true;
                        startInfo.Arguments = "/r /t 0";
                        startInfo.FileName = "shutdown";
                        Process.Start(startInfo);
                        break;

                    case EShutdownAction.eShutdownAction_Standby:
                        Application.SetSuspendState(PowerState.Suspend, true, true);
                        break;
                }
            }
            catch (Exception ex)
            {
                client.Send(new SetStatus { Message = $"关机/重启失败: {ex.Message}" });
            }
        }
    }
}