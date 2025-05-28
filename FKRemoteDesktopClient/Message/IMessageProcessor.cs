//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message
{
    // 提供处理消息基本功能的消息处理器
    public interface IMessageProcessor
    {
        // 检查本消息处理器是否可以处理指定的消息
        bool CanExecute(IMessage message);

        // 决定此消息处理器是否可以执行从指定发送者接收的消息
        bool CanExecuteFrom(ISender sender);

        // 执行收到的消息
        void Execute(ISender sender, IMessage message);
    }
}