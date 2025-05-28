using System;
using System.Threading;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Utilities
{
    // 互斥锁，确保每次只运行一个实例
    public class SingleInstanceMutex
    {
        private readonly Mutex _appMutex;
        public bool CreatedNew { get; }                 // 该互斥锁是新创建还是已经存在
        public bool IsDisposed { get; private set; }    // 该实例是否已被释放且不应再被使用

        public SingleInstanceMutex(string name)
        {
            _appMutex = new Mutex(false, $"Local\\{name}", out var createdNew);
            CreatedNew = createdNew;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            if (disposing)
            {
                _appMutex?.Dispose();
            }

            IsDisposed = true;
        }
    }
}