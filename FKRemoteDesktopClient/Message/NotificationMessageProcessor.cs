//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message
{
    public abstract class NotificationMessageProcessor : MessageProcessorBase<string>
    {
        protected NotificationMessageProcessor() : base(true)
        {
        }
    }
}