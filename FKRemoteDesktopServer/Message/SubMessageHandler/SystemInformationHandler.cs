using FKRemoteDesktop.Message.SubMessages;
using FKRemoteDesktop.Network;
using System;
using System.Collections.Generic;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.SubMessageHandler
{
    // 处理与远程系统信息交互的消息
    public class SystemInformationHandler : MessageProcessorBase<List<Tuple<string, string>>>
    {
        private readonly Client _client;

        public SystemInformationHandler(Client client) : base(true)
        {
            _client = client;
        }

        public override bool CanExecute(IMessage message) => message is GetSystemInfoResponse;

        public override bool CanExecuteFrom(ISender client) => _client.Equals(client);

        public override void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case GetSystemInfoResponse info:
                    Execute(sender, info);
                    break;
            }
        }

        // 从客户端获取更多的细节信息
        public void RefreshSystemInformation()
        {
            _client.Send(new GetSystemInfo());
        }

        private void Execute(ISender client, GetSystemInfoResponse message)
        {
            OnReport(message.SystemInfos);
        }
    }
}
