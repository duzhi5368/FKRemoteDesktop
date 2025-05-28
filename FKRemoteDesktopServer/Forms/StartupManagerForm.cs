using FKRemoteDesktop.Enums;
using FKRemoteDesktop.Helpers;
using FKRemoteDesktop.Message;
using FKRemoteDesktop.Message.MessageStructs;
using FKRemoteDesktop.Message.SubMessageHandler;
using FKRemoteDesktop.Network;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Forms
{
    public partial class StartupManagerForm : Form
    {
        private readonly Client _connectClient;
        private readonly StartupManagerHandler _startupManagerHandler;
        private static readonly Dictionary<Client, StartupManagerForm> OpenedForms = new Dictionary<Client, StartupManagerForm>();

        public StartupManagerForm(Client client)
        {
            _connectClient = client;
            _startupManagerHandler = new StartupManagerHandler(client);

            RegisterMessageHandler();
            InitializeComponent();
        }

        public static StartupManagerForm CreateNewOrGetExisting(Client client)
        {
            if (OpenedForms.ContainsKey(client))
            {
                return OpenedForms[client];
            }
            StartupManagerForm f = new StartupManagerForm(client);
            f.Disposed += (sender, args) => OpenedForms.Remove(client);
            OpenedForms.Add(client, f);
            return f;
        }

        private void RegisterMessageHandler()
        {
            _connectClient.ClientState += ClientDisconnected;
            _startupManagerHandler.ProgressChanged += StartupItemsChanged;
            MessageHandler.Register(_startupManagerHandler);
        }

        private void UnregisterMessageHandler()
        {
            MessageHandler.Unregister(_startupManagerHandler);
            _startupManagerHandler.ProgressChanged -= StartupItemsChanged;
            _connectClient.ClientState -= ClientDisconnected;
        }

        private void ClientDisconnected(Client client, bool connected)
        {
            if (!connected)
            {
                this.Invoke((MethodInvoker)this.Close);
            }
        }

        private void StartupItemsChanged(object sender, List<StartupItem> startupItems)
        {
            lstStartupItems.Items.Clear();

            foreach (var item in startupItems)
            {
                var i = lstStartupItems.Groups.Cast<ListViewGroup>().First(x => (EStartupType)x.Tag == item.Type);
                ListViewItem lvi = new ListViewItem(new[] { item.Name, item.Path }) { Group = i, Tag = item };
                lstStartupItems.Items.Add(lvi);
            }
        }

        private void StartupManagerForm_Load(object sender, System.EventArgs e)
        {
            this.Text = WindowHelper.GetWindowTitle("FK远控服务器端 - 启动项管理器", _connectClient);

            AddGroups();
            _startupManagerHandler.RefreshStartupItems();
        }

        private void StartupManagerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            UnregisterMessageHandler();
        }

        private void AddGroups()
        {
            lstStartupItems.Groups.Add(
                new ListViewGroup("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run")
                { Tag = EStartupType.eStartupType_CurrentUserRun });
            lstStartupItems.Groups.Add(
                new ListViewGroup("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce")
                { Tag = EStartupType.eStartupType_CurrentUserRunOnce });
            lstStartupItems.Groups.Add(
                new ListViewGroup("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run")
                { Tag = EStartupType.eStartupType_LocalMachineRun });
            lstStartupItems.Groups.Add(
                new ListViewGroup("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce")
                { Tag = EStartupType.eStartupType_LocalMachineRunOnce });
            lstStartupItems.Groups.Add(
                new ListViewGroup("HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Run")
                { Tag = EStartupType.eStartupType_LocalMachineRunX86 });
            lstStartupItems.Groups.Add(
                new ListViewGroup("HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\RunOnce")
                { Tag = EStartupType.eStartupType_LocalMachineRunOnceX86 });
            lstStartupItems.Groups.Add(
                new ListViewGroup("%APPDATA%\\Microsoft\\Windows\\Start Menu\\Programs\\Startup")
                { Tag = EStartupType.eStartupType_StartMenu });
        }

        // 添加启动项
        private void addEntryToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            using (var frm = new StartupAddForm())
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    _startupManagerHandler.AddStartupItem(frm.StartupItem);
                    _startupManagerHandler.RefreshStartupItems();
                }
            }
        }

        // 删除启动项
        private void removeEntryToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            bool modified = false;

            foreach (ListViewItem item in lstStartupItems.SelectedItems)
            {
                _startupManagerHandler.RemoveStartupItem((StartupItem)item.Tag);
                modified = true;
            }

            if (modified)
            {
                _startupManagerHandler.RefreshStartupItems();
            }
        }
    }
}
