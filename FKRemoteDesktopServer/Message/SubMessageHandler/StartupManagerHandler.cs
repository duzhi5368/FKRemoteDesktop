using FKRemoteDesktop.Message.MessageStructs;
using FKRemoteDesktop.Message.SubMessages;
using FKRemoteDesktop.Network;
using System.Collections.Generic;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.SubMessageHandler
{
    public class StartupManagerHandler : MessageProcessorBase<List<StartupItem>>
    {
        private readonly Client _client;

        public StartupManagerHandler(Client client) : base(true)
        {
            _client = client;
        }

        public override bool CanExecute(IMessage message) => message is GetStartupItemsResponse;

        public override bool CanExecuteFrom(ISender sender) => _client.Equals(sender);

        public override void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case GetStartupItemsResponse items:
                    Execute(sender, items);
                    break;
            }
        }

        public void RefreshStartupItems()
        {
            _client.Send(new GetStartupItems());
        }

        public void RemoveStartupItem(StartupItem item)
        {
            _client.Send(new DoStartupItemRemove { StartupItem = item });
        }

        public void AddStartupItem(StartupItem item)
        {
            _client.Send(new DoStartupItemAdd { StartupItem = item });
        }

        private void Execute(ISender client, GetStartupItemsResponse message)
        {
            OnReport(message.StartupItems);
        }
    }
}
