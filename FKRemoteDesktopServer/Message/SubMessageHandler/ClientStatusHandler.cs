using FKRemoteDesktop.Enums;
using FKRemoteDesktop.Message.SubMessages;
using FKRemoteDesktop.Network;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.SubMessageHandler
{
    // 处理和远程客户端状态交互的消息
    public class ClientStatusHandler : MessageProcessorBase<object>
    {
        #region 消息事件
        /// <summary>
        /// 客户端更新状态事件
        /// </summary>
        /// <param name="sender">引发事件的消息处理程序</param>
        /// <param name="client">更新状态的客户端</param>
        /// <param name="statusMessage">新状态</param>
        public delegate void StatusUpdatedEventHandler(object sender, Client client, string statusMessage);
        public event StatusUpdatedEventHandler StatusUpdated;

        /// <summary>
        /// 客户端状态更新事件
        /// </summary>
        /// <param name="sender">引发事件的消息处理程序</param>
        /// <param name="client">更新状态的客户端</param>
        /// <param name="userStatusMessage">新状态</param>
        public delegate void UserStatusUpdatedEventHandler(object sender, Client client, EUserStatus userStatusMessage);
        public event UserStatusUpdatedEventHandler UserStatusUpdated;

        private void OnStatusUpdated(Client client, string statusMessage)
        {
            SynchronizationContext.Post(c =>
            {
                var handler = StatusUpdated;
                handler?.Invoke(this, (Client)c, statusMessage);
            }, client);
        }

        private void OnUserStatusUpdated(Client client, EUserStatus userStatusMessage)
        {
            SynchronizationContext.Post(c =>
            {
                var handler = UserStatusUpdated;
                handler?.Invoke(this, (Client)c, userStatusMessage);
            }, client);
        }
        #endregion

        public ClientStatusHandler() : base(true)
        {

        }

        public override bool CanExecute(IMessage message) => message is SetStatus || message is SetUserStatus;

        public override bool CanExecuteFrom(ISender sender) => true;

        public override void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case SetStatus status:
                    Execute((Client)sender, status);
                    break;
                case SetUserStatus userStatus:
                    Execute((Client)sender, userStatus);
                    break;
            }
        }

        private void Execute(Client client, SetStatus message)
        {
            OnStatusUpdated(client, message.Message);
        }

        private void Execute(Client client, SetUserStatus message)
        {
            OnUserStatusUpdated(client, message.Message);
        }
    }
}
