using FKRemoteDesktop.Message.SubMessages;
using FKRemoteDesktop.Network;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.SubMessageHandler
{
    public class RemoteShellHandler : MessageProcessorBase<string>
    {
        /// <summary>
        /// 发出错误指令事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="errorMessage"></param>
        public delegate void CommandErrorEventHandler(object sender, string errorMessage);
        public event CommandErrorEventHandler CommandError;
        private void OnCommandError(string errorMessage)
        {
            SynchronizationContext.Post(val =>
            {
                var handler = CommandError;
                handler?.Invoke(this, (string)val);
            }, errorMessage);
        }

        private readonly Client _client;

        public RemoteShellHandler(Client client) : base(true)
        {
            _client = client;
        }

        public override bool CanExecute(IMessage message) => message is DoShellExecuteResponse;

        public override bool CanExecuteFrom(ISender sender) => _client.Equals(sender);

        public override void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case DoShellExecuteResponse resp:
                    Execute(sender, resp);
                    break;
            }
        }

        public void SendCommand(string command)
        {
            _client.Send(new DoShellExecute { Command = command });
        }

        private void Execute(ISender client, DoShellExecuteResponse message)
        {
            if (message.IsError)
                OnCommandError(message.Output);
            else
                OnReport(message.Output);
        }
    }
}
