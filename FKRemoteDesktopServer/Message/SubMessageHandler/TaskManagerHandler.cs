using FKRemoteDesktop.Enums;
using FKRemoteDesktop.Message.MessageStructs;
using FKRemoteDesktop.Message.SubMessages;
using FKRemoteDesktop.Network;
using System.Threading;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.SubMessageHandler
{
    // 远程任务交互的消息
    public class TaskManagerHandler : MessageProcessorBase<Process[]>
    {
        /// <summary>
        /// 处理任务操作结果
        /// </summary>
        /// <param name="sender">引发事件的对象</param>
        /// <param name="action">已执行的任务操作</param>
        /// <param name="result">执行任务操作的结果</param>
        public delegate void ProcessActionPerformedEventHandler(object sender, EProcessAction action, bool result);
        public event ProcessActionPerformedEventHandler ProcessActionPerformed;
        private void OnProcessActionPerformed(EProcessAction action, bool result)
        {
            SynchronizationContext.Post(r =>
            {
                var handler = ProcessActionPerformed;
                handler?.Invoke(this, action, (bool)r);
            }, result);
        }

        private readonly Client _client;    // 与此远程任务执行关联的客户端

        public TaskManagerHandler(Client client) : base(true)
        {
            _client = client;
        }

        public override bool CanExecute(IMessage message) => message is DoProcessResponse ||
                                                             message is GetProcessesResponse;

        public override bool CanExecuteFrom(ISender sender) => _client.Equals(sender);

        public override void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case DoProcessResponse execResp:
                    Execute(sender, execResp);
                    break;
                case GetProcessesResponse procResp:
                    Execute(sender, procResp);
                    break;
            }
        }

        // 执行一个本地进程
        public void StartProcess(string remotePath, bool isUpdate = false)
        {
            _client.Send(new DoProcessStart { FilePath = remotePath, IsUpdate = isUpdate });
        }

        // 执行一个网络URL进程（先下载后执行）
        public void StartProcessFromWeb(string url, bool isUpdate = false)
        {
            _client.Send(new DoProcessStart { DownloadUrl = url, IsUpdate = isUpdate });
        }

        // 刷新一个进程状态
        public void RefreshProcesses()
        {
            _client.Send(new GetProcesses());
        }

        // 停止一个进程
        public void EndProcess(int pid)
        {
            _client.Send(new DoProcessEnd { Pid = pid });
        }

        private void Execute(ISender client, DoProcessResponse message)
        {
            OnProcessActionPerformed(message.Action, message.Result);
        }

        private void Execute(ISender client, GetProcessesResponse message)
        {
            OnReport(message.Processes);
        }
    }
}
