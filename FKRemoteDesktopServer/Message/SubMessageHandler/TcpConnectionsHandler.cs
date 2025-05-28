using FKRemoteDesktop.Message.MessageStructs;
using FKRemoteDesktop.Message.SubMessages;
using FKRemoteDesktop.Network;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.SubMessageHandler
{
    public class TcpConnectionsHandler : MessageProcessorBase<TcpConnection[]>
    {
        private readonly Client _client;

        public TcpConnectionsHandler(Client client) : base(true)
        {
            _client = client;
        }

        public override bool CanExecute(IMessage message) => message is GetConnectionsResponse;

        public override bool CanExecuteFrom(ISender sender) => _client.Equals(sender);

        public override void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case GetConnectionsResponse con:
                    Execute(sender, con);
                    break;
            }
        }

        public void RefreshTcpConnections()
        {
            _client.Send(new GetConnections());
        }

        public void CloseTcpConnection(string localAddress, ushort localPort, string remoteAddress, ushort remotePort)
        {
            _client.Send(new DoCloseConnection
            {
                LocalAddress = localAddress,
                LocalPort = localPort,
                RemoteAddress = remoteAddress,
                RemotePort = remotePort
            });
        }

        private void Execute(ISender client, GetConnectionsResponse message)
        {
            OnReport(message.Connections);
        }

    }
}
