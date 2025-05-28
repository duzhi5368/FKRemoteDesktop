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
    public partial class ConnectionsForm : Form
    {
        private readonly Client _connectClient;
        private readonly TcpConnectionsHandler _connectionsHandler;
        private readonly Dictionary<string, ListViewGroup> _groups = new Dictionary<string, ListViewGroup>();
        private static readonly Dictionary<Client, ConnectionsForm> OpenedForms = new Dictionary<Client, ConnectionsForm>();

        public ConnectionsForm(Client client)
        {
            _connectClient = client;
            _connectionsHandler = new TcpConnectionsHandler(client);

            RegisterMessageHandler();
            InitializeComponent();
        }

        public static ConnectionsForm CreateNewOrGetExisting(Client client)
        {
            if (OpenedForms.ContainsKey(client))
            {
                return OpenedForms[client];
            }
            ConnectionsForm f = new ConnectionsForm(client);
            f.Disposed += (sender, args) => OpenedForms.Remove(client);
            OpenedForms.Add(client, f);
            return f;
        }

        private void RegisterMessageHandler()
        {
            _connectClient.ClientState += ClientDisconnected;
            _connectionsHandler.ProgressChanged += TcpConnectionsChanged;
            MessageHandler.Register(_connectionsHandler);
        }

        private void UnregisterMessageHandler()
        {
            MessageHandler.Unregister(_connectionsHandler);
            _connectionsHandler.ProgressChanged -= TcpConnectionsChanged;
            _connectClient.ClientState -= ClientDisconnected;
        }

        private void ClientDisconnected(Client client, bool connected)
        {
            if (!connected)
            {
                this.Invoke((MethodInvoker)this.Close);
            }
        }

        private void TcpConnectionsChanged(object sender, TcpConnection[] connections)
        {
            lstConnections.Items.Clear();
            foreach (var con in connections)
            {
                string state = ToString(con.State);
                ListViewItem lvi = new ListViewItem(new[]
                {
                    con.ProcessName, con.LocalAddress, con.LocalPort.ToString(),
                    con.RemoteAddress, con.RemotePort.ToString(), state
                });
                if (!_groups.ContainsKey(state))
                {
                    ListViewGroup g = new ListViewGroup(state, state);
                    lstConnections.Groups.Add(g);
                    _groups.Add(state, g);
                }
                lvi.Group = lstConnections.Groups[state];
                lstConnections.Items.Add(lvi);
            }
        }

        private string ToString(EConnectionState eState)
        {
            switch (eState)
            {
                case EConnectionState.eConnetionState_Closed: return "连接已关闭"; // 连接已关闭
                case EConnectionState.eConnetionState_Listening: return "正在监听中..."; // 服务器正在监听连接请求
                case EConnectionState.eConnetionState_Syn_Sent: return "SYN 已发送，等待确认"; // 已发送 SYN，等待对方确认
                case EConnectionState.eConnetionState_Syn_Recieved: return "SYN 已接收，等待连接"; // 已接收 SYN，等待建立连接
                case EConnectionState.eConnetionState_Established: return "已连接"; // 连接已建立，数据传输状态
                case EConnectionState.eConnetionState_Finish_Wait_1: return "FIN 等待1，等待确认"; // 主动关闭，发送 FIN，等待对方确认
                case EConnectionState.eConnetionState_Finish_Wait_2: return "FIN 等待2，等待确认"; // 收到对方 ACK，等待对方 FIN
                case EConnectionState.eConnetionState_Closed_Wait: return "等待被关闭"; // 被动关闭，收到 FIN，等待应用层关闭
                case EConnectionState.eConnetionState_Closing: return "关闭连接中，等待确认"; // 双方同时发起关闭，等待确认
                case EConnectionState.eConnetionState_Last_ACK: return "等待被关闭，最后确认"; // 被动关闭，发送 ACK，等待确认
                case EConnectionState.eConnetionState_Time_Wait: return "等待连接"; // 等待 2MSL，确保对方收到 ACK
                case EConnectionState.eConnetionState_Delete_TCB: return "删除 TCB，连接终止"; // 连接控制块被删除，连接完全终止
                default: return "未知状态"; // 未知或未定义状态
            }
        }

        private void ConnectionsForm_Load(object sender, System.EventArgs e)
        {
            this.Text = WindowHelper.GetWindowTitle("FK远控服务器端 - 网络连接查看器", _connectClient);
            _connectionsHandler.RefreshTcpConnections();
        }

        private void ConnectionsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            UnregisterMessageHandler();
        }

        // 刷新按钮
        private void refreshToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            _connectionsHandler.RefreshTcpConnections();
        }

        // 关闭连接
        private void closeConnectionToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            bool modified = false;
            foreach (ListViewItem lvi in lstConnections.SelectedItems)
            {
                _connectionsHandler.CloseTcpConnection(lvi.SubItems[1].Text, ushort.Parse(lvi.SubItems[2].Text),
                    lvi.SubItems[3].Text, ushort.Parse(lvi.SubItems[4].Text));
                modified = true;
            }
            if (modified)
            {
                _connectionsHandler.RefreshTcpConnections();
            }
        }

        // 点击列，进行排序
        private void lstConnections_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            lstConnections.ListViewColumnSorter.NeedNumberCompare = (e.Column == 2 || e.Column == 4);
        }
    }
}
