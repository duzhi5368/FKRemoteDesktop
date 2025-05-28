using System;
using System.Threading;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message
{
    // 保存 MessageProcessorBase 的静态值，以避免每个类型T都一个静态实例
    internal static class ProgressStatics
    {
        internal static readonly SynchronizationContext DefaultContext = new SynchronizationContext();
    }

    // 提供进度报告的 MessageProcessor
    public abstract class MessageProcessorBase<T> : IMessageProcessor, IProgress<T>
    {
        // 多线程上下文
        protected readonly SynchronizationContext SynchronizationContext;
        // 缓存委托，用于同步上下文
        private readonly SendOrPostCallback _invokeReportProgressHandlers;

        /// <summary>
        /// 处理进度发生更变的消息
        /// </summary>
        /// <param name="sender">发送进度更新的消息处理器</param>
        /// <param name="value">新的进度</param>
        public delegate void ReportProgressEventHandler(object sender, T value);
        public event ReportProgressEventHandler ProgressChanged;

        protected virtual void OnReport(T value)
        {
            var handler = ProgressChanged;
            if (handler != null)
            {
                SynchronizationContext.Post(_invokeReportProgressHandlers, value);
            }
        }

        protected MessageProcessorBase(bool useCurrentContext)
        {
            _invokeReportProgressHandlers = InvokeReportProgressHandlers;
            SynchronizationContext = useCurrentContext ? SynchronizationContext.Current : ProgressStatics.DefaultContext;
        }

        // 进度更变事件回调函数
        private void InvokeReportProgressHandlers(object state)
        {
            var handler = ProgressChanged;
            handler?.Invoke(this, (T)state);
        }

        public abstract bool CanExecute(IMessage message);
        public abstract bool CanExecuteFrom(ISender sender);
        public abstract void Execute(ISender sender, IMessage message);

        void IProgress<T>.Report(T value) => OnReport(value);
    }
}
