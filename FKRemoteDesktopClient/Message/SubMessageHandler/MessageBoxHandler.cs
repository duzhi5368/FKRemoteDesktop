using FKRemoteDesktop.Debugger;
using FKRemoteDesktop.Enums;
using FKRemoteDesktop.Message.SubMessages;
using System;
using System.Threading;
using System.Windows.Forms;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.SubMessageHandler
{
    public class MessageBoxHandler : IMessageProcessor
    {
        public bool CanExecute(IMessage message) => message is DoShowMessageBox;

        public bool CanExecuteFrom(ISender sender) => true;

        public void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case DoShowMessageBox msg:
                    Logger.Log(ELogType.eLogType_Debug, "收到服务器消息: DoShowMessageBox");
                    Execute(sender, msg);
                    break;
            }
        }

        private void Execute(ISender client, DoShowMessageBox message)
        {
            new Thread(() =>
            {
                MessageBox.Show(message.Text, message.Caption,
                    (MessageBoxButtons)Enum.Parse(typeof(MessageBoxButtons), message.Button),
                    (MessageBoxIcon)Enum.Parse(typeof(MessageBoxIcon), message.Icon),
                    MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            })
            { IsBackground = true }.Start();

            client.Send(new SetStatus { Message = "MessageBox 成功展示." });
        }
    }
}