using FKRemoteDesktop.Controls;
using FKRemoteDesktop.Enums;
using FKRemoteDesktop.Helpers;
using FKRemoteDesktop.Message;
using FKRemoteDesktop.Message.MessageStructs;
using FKRemoteDesktop.Message.SubMessageHandler;
using FKRemoteDesktop.Network;
using System.Collections.Generic;
using System.Windows.Forms;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Forms
{
    public partial class TaskManagerForm : Form
    {
        private readonly Client _connectClient;
        private readonly TaskManagerHandler _taskManagerHandler;
        private static readonly Dictionary<Client, TaskManagerForm> OpenedForms = new Dictionary<Client, TaskManagerForm>();

        public TaskManagerForm(Client client)
        {
            _connectClient = client;
            _taskManagerHandler = new TaskManagerHandler(client);

            RegisterMessageHandler();
            InitializeComponent();
        }

        public static TaskManagerForm CreateNewOrGetExisting(Client client)
        {
            if (OpenedForms.ContainsKey(client))
            {
                return OpenedForms[client];
            }
            TaskManagerForm f = new TaskManagerForm(client);
            f.Disposed += (sender, args) => OpenedForms.Remove(client);
            OpenedForms.Add(client, f);
            return f;
        }

        private void RegisterMessageHandler()
        {
            _connectClient.ClientState += ClientDisconnected;
            _taskManagerHandler.ProgressChanged += TasksChanged;
            _taskManagerHandler.ProcessActionPerformed += ProcessActionPerformed;
            MessageHandler.Register(_taskManagerHandler);
        }

        private void UnregisterMessageHandler()
        {
            MessageHandler.Unregister(_taskManagerHandler);
            _taskManagerHandler.ProcessActionPerformed -= ProcessActionPerformed;
            _taskManagerHandler.ProgressChanged -= TasksChanged;
            _connectClient.ClientState -= ClientDisconnected;
        }

        private void ClientDisconnected(Client client, bool connected)
        {
            if (!connected)
            {
                this.Invoke((MethodInvoker)this.Close);
            }
        }

        private void TasksChanged(object sender, Process[] processes)
        {
            lstTasks.Items.Clear();
            foreach (var process in processes)
            {
                ListViewItem lvi =
                    new ListViewItem(new[] { process.Name, process.Id.ToString(), process.MainWindowTitle });
                lstTasks.Items.Add(lvi);
            }
            processesToolStripStatusLabel.Text = $"进程数：{processes.Length}";
        }

        private void ProcessActionPerformed(object sender, EProcessAction action, bool result)
        {
            string text = string.Empty;
            switch (action)
            {
                case EProcessAction.eProcessAction_Start:
                    text = result ? "启动进程成功" : "启动进程失败";
                    break;
                case EProcessAction.eProcessAction_End:
                    text = result ? "结束进程成功" : "结束进程失败";
                    break;
            }
            processesToolStripStatusLabel.Text = text;
        }

        private void TaskManagerForm_Load(object sender, System.EventArgs e)
        {
            this.Text = WindowHelper.GetWindowTitle("FK远控服务器端 - 任务管理器", _connectClient);
            _taskManagerHandler.RefreshProcesses();
        }

        private void TaskManagerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            UnregisterMessageHandler();
        }

        // 结束进程
        private void killProcessToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            foreach (ListViewItem lvi in lstTasks.SelectedItems)
            {
                _taskManagerHandler.EndProcess(int.Parse(lvi.SubItems[1].Text));
            }
        }

        // 开始进程
        private void startProcessToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            string processName = string.Empty;
            if (FKInputBox.Show("进程名", "启动进程名:", ref processName) == DialogResult.OK)
            {
                _taskManagerHandler.StartProcess(processName);
            }
        }

        // 刷新
        private void refreshToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            _taskManagerHandler.RefreshProcesses();
        }

        // 点选标题，进行排序
        private void lstTasks_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            lstTasks.ListViewColumnSorter.NeedNumberCompare = (e.Column == 1);
        }
    }
}
