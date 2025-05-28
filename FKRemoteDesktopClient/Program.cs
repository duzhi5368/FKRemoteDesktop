using FKRemoteDesktop.Helpers;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Windows.Forms;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop
{
    public static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        public static void Main()
        {
            // 开启 TLS 1.2
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            // 强制全部 Windows 窗口错误通过我们的程序进行处理
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            // 处理UI线程的错误
            Application.ThreadException += HandleThreadException;
            // 处理非UI线程的错误
            AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ClientForm());
        }

        private static void HandleThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Debug.WriteLine(e);
            try
            {
                // 出现异常退出，就立刻重启
                string batchFile = BatchFileHelper.CreateRestartBatch(Application.ExecutablePath);
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = true,
                    FileName = batchFile
                };
                ProcessHelper.StartProcess(startInfo);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
            }
            finally
            {
                Environment.Exit(0);
            }
        }

        private static void HandleUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.IsTerminating)
            {
                Debug.WriteLine(e);
                try
                {
                    // 出现异常退出，就立刻重启
                    string batchFile = BatchFileHelper.CreateRestartBatch(Application.ExecutablePath);
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = true,
                        FileName = batchFile
                    };
                    ProcessHelper.StartProcess(startInfo);
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception);
                }
                finally
                {
                    Environment.Exit(0);
                }
            }
        }
    }
}