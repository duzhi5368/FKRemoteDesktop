using FKRemoteDesktop.Extensions;
using FKRemoteDesktop.Helpers;
using FKRemoteDesktop.Message;
using FKRemoteDesktop.Message.SubMessageHandler;
using FKRemoteDesktop.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Forms
{
    public partial class SystemInformationForm : Form
    {
        private readonly Client _connectClient;
        private readonly SystemInformationHandler _sysInfoHandler;
        private static readonly Dictionary<Client, SystemInformationForm> OpenedForms = new Dictionary<Client, SystemInformationForm>();

        public SystemInformationForm(Client client)
        {
            _connectClient = client;
            _sysInfoHandler = new SystemInformationHandler(client);

            RegisterMessageHandler();

            InitializeComponent();
        }

        public static SystemInformationForm CreateNewOrGetExisting(Client client)
        {
            if (OpenedForms.ContainsKey(client))
            {
                return OpenedForms[client];
            }
            SystemInformationForm f = new SystemInformationForm(client);
            f.Disposed += (sender, args) => OpenedForms.Remove(client);
            OpenedForms.Add(client, f);
            return f;
        }

        private void RegisterMessageHandler()
        {
            _connectClient.ClientState += ClientDisconnected;
            _sysInfoHandler.ProgressChanged += SystemInformationChanged;
            MessageHandler.Register(_sysInfoHandler);
        }

        private void UnregisterMessageHandler()
        {
            MessageHandler.Unregister(_sysInfoHandler);
            _sysInfoHandler.ProgressChanged -= SystemInformationChanged;
            _connectClient.ClientState -= ClientDisconnected;
        }

        private void ClientDisconnected(Client client, bool connected)
        {
            if (!connected)
            {
                this.Invoke((MethodInvoker)this.Close);
            }
        }

        private void SystemInformationChanged(object sender, List<Tuple<string, string>> infos)
        {
            lstSystem.Items.RemoveAt(2);
            foreach (var info in infos)
            {
                var lvi = new ListViewItem(new[] { info.Item1, info.Item2 });
                lstSystem.Items.Add(lvi);
            }
            lstSystem.AutosizeColumns();
        }

        private void SystemInformationForm_Load(object sender, EventArgs e)
        {
            this.Text = WindowHelper.GetWindowTitle("FK远控服务器端 - 信息查看器", _connectClient);
            _sysInfoHandler.RefreshSystemInformation();
            AddBasicSystemInformation();
        }

        private void SystemInformationForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            UnregisterMessageHandler();
        }

        // 复制全部信息
        private void copyAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lstSystem.Items.Count == 0) 
                return;

            string output = string.Empty;
            foreach (ListViewItem lvi in lstSystem.Items)
            {
                output = lvi.SubItems.Cast<ListViewItem.ListViewSubItem>().Aggregate(output, (current, lvs) => current + (lvs.Text + " : "));
                output = output.Remove(output.Length - 3);
                output = output + "\r\n";
            }
            ClipboardHelper.SetClipboardTextSafe(output);
        }

        // 复制当前所选项
        private void copySelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lstSystem.SelectedItems.Count == 0) 
                return;

            string output = string.Empty;
            foreach (ListViewItem lvi in lstSystem.SelectedItems)
            {
                output = lvi.SubItems.Cast<ListViewItem.ListViewSubItem>().Aggregate(output, (current, lvs) => current + (lvs.Text + " : "));
                output = output.Remove(output.Length - 3);
                output = output + "\r\n";
            }
            ClipboardHelper.SetClipboardTextSafe(output);
        }

        // 刷新信息
        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lstSystem.Items.Clear();
            _sysInfoHandler.RefreshSystemInformation();
            AddBasicSystemInformation();
        }

        private void AddBasicSystemInformation()
        {
            ListViewItem lvi =
                new ListViewItem(new[] { "操作系统", _connectClient.UserInfo.OperatingSystem });
            lstSystem.Items.Add(lvi);
            lvi =
                new ListViewItem(new[]
                {
                    "架构",
                    (_connectClient.UserInfo.OperatingSystem.Contains("32 Bit")) ? "x86 (32 Bit)" : "x64 (64 Bit)"
                });
            lstSystem.Items.Add(lvi);
            lvi = new ListViewItem(new[] { "", "获取更多信息..." });
            lstSystem.Items.Add(lvi);
        }
    }
}
