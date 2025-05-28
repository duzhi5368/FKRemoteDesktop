using FKRemoteDesktop.Message.SubMessages;
using FKRemoteDesktop.Network;
using FKRemoteDesktop.ReverseProxy;
using System;
using System.Linq;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.SubMessageHandler
{
    public class ReverseProxyHandler : MessageProcessorBase<ReverseProxyClient[]>
    {
        private readonly Client[] _clients;
        private readonly ReverseProxyServer _socksServer;

        public ReverseProxyHandler(Client[] clients) : base(true)
        {
            _socksServer = new ReverseProxyServer();
            _clients = clients;
        }

        public override bool CanExecute(IMessage message) => message is ReverseProxyConnectResponse ||
                                                     message is ReverseProxyData ||
                                                     message is ReverseProxyDisconnect;

        public override bool CanExecuteFrom(ISender sender) => _clients.Any(c => c.Equals(sender));

        public override void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case ReverseProxyConnectResponse con:
                    Execute(sender, con);
                    break;
                case ReverseProxyData data:
                    Execute(sender, data);
                    break;
                case ReverseProxyDisconnect disc:
                    Execute(sender, disc);
                    break;
            }
        }

        public void StartReverseProxyServer(ushort port)
        {
            _socksServer.OnConnectionEstablished += socksServer_onConnectionEstablished;
            _socksServer.OnUpdateConnection += socksServer_onUpdateConnection;
            _socksServer.StartServer(_clients, "0.0.0.0", port);
        }

        public void StopReverseProxyServer()
        {
            _socksServer.Stop();
            _socksServer.OnConnectionEstablished -= socksServer_onConnectionEstablished;
            _socksServer.OnUpdateConnection -= socksServer_onUpdateConnection;
        }

        private void Execute(ISender client, ReverseProxyConnectResponse message)
        {
            ReverseProxyClient socksClient = _socksServer.GetClientByConnectionId(message.ConnectionId);
            socksClient?.HandleCommandResponse(message);
        }

        private void Execute(ISender client, ReverseProxyData message)
        {
            ReverseProxyClient socksClient = _socksServer.GetClientByConnectionId(message.ConnectionId);
            socksClient?.SendToClient(message.Data);
        }

        private void Execute(ISender client, ReverseProxyDisconnect message)
        {
            ReverseProxyClient socksClient = _socksServer.GetClientByConnectionId(message.ConnectionId);
            socksClient?.Disconnect();
        }

        void socksServer_onUpdateConnection(ReverseProxyClient proxyClient)
        {
            OnReport(_socksServer.OpenConnections);
        }

        void socksServer_onConnectionEstablished(ReverseProxyClient proxyClient)
        {
            OnReport(_socksServer.OpenConnections);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopReverseProxyServer();
            }
        }
    }
}
