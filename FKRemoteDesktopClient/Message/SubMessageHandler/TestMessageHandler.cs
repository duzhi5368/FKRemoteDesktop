using FKRemoteDesktop.Debugger;
using FKRemoteDesktop.Enums;
using FKRemoteDesktop.Message.SubMessages;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.SubMessageHandler
{
    public class TestMessageHandler : IMessageProcessor
    {
        public bool CanExecute(IMessage message) => message is TestMessage ||
                                                    message is TestEmptyMessage;

        public bool CanExecuteFrom(ISender sender) => true;

        public void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case TestMessage msg:
                    Logger.Log(ELogType.eLogType_Debug, "收到服务器消息: TestMessage");
                    Execute(sender, msg);
                    break;

                case TestEmptyMessage msg:
                    Logger.Log(ELogType.eLogType_Debug, "收到服务器消息: TestEmptyMessage");
                    Execute(sender, msg);
                    break;
            }
        }

        private void Execute(ISender client, TestMessage message)
        {
            Logger.Log(ELogType.eLogType_Debug, "收到 TestMessage 消息 : String1 = " +
                message.String1 + " ; String2 = " + message.String2);

            client.Send(new SetStatus
            {
                Message = "TestMessage 消息收到 : String1 =" +
                message.String1 + " ; String2 = Client Pong"
            });
        }

        private void Execute(ISender client, TestEmptyMessage message)
        {
            Logger.Log(ELogType.eLogType_Debug, "收到 TestEmptyMessage 消息");

            client.Send(new SetStatus { Message = "TestEmptyMessage 消息已收到." });
        }
    }
}