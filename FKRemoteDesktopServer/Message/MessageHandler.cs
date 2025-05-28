using System.Collections.Generic;
using System.Linq;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message
{
    // 负责注册 IMessageProcessor  和处理 IMessage 的处理类
    public static class MessageHandler
    {
        // 注册的消息处理器
        private static readonly List<IMessageProcessor> Processors = new List<IMessageProcessor>();
        // 处理跨线程时消息处理器锁
        private static readonly object SyncLock = new object();

        // 注册处理器
        public static void Register(IMessageProcessor proc)
        {
            lock (SyncLock)
            {
                if (Processors.Contains(proc)) 
                    return;
                Processors.Add(proc);
            }
        }

        // 解除处理器注册
        public static void Unregister(IMessageProcessor proc)
        {
            lock (SyncLock)
            {
                Processors.Remove(proc);
            }
        }

        /// <summary>
        /// 将接收到的IMessage进行转发给对应的IMessageProcessor进行处理
        /// </summary>
        /// <param name="sender">消息发送者</param>
        /// <param name="msg">接收到的消息</param>
        public static void Process(ISender sender, IMessage msg)
        {
            IEnumerable<IMessageProcessor> availableProcessors;
            lock (SyncLock)
            {
                // 选择合适的消息处理器进行消息处理
                availableProcessors = Processors.Where(x => x.CanExecute(msg) && x.CanExecuteFrom(sender)).ToList();
            }

            foreach (var executor in availableProcessors)
                executor.Execute(sender, msg);
        }
    }
}
