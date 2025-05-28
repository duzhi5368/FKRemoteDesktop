using FKRemoteDesktop.Helpers;
using FKRemoteDesktop.Message;
using FKRemoteDesktop.Message.SubMessageHandler;
using FKRemoteDesktop.Network;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Forms
{
    public partial class RemoteShellForm : Form
    {
        private readonly Client _connectClient;                 // 本shell连接的客户端
        public readonly RemoteShellHandler RemoteShellHandler;  // 处理与客户端的消息
        private static readonly Dictionary<Client, RemoteShellForm> OpenedForms = new Dictionary<Client, RemoteShellForm>();

        public RemoteShellForm(Client client)
        {
            _connectClient = client;
            RemoteShellHandler = new RemoteShellHandler(client);
            RegisterMessageHandler();

            InitializeComponent();

            txtConsoleOutput.AppendText(">> 输入 'exit' 关闭本连接" + Environment.NewLine);
        }

        public static RemoteShellForm CreateNewOrGetExisting(Client client)
        {
            if (OpenedForms.ContainsKey(client))
            {
                return OpenedForms[client];
            }
            RemoteShellForm f = new RemoteShellForm(client);
            f.Disposed += (sender, args) => OpenedForms.Remove(client);
            OpenedForms.Add(client, f);
            return f;
        }

        private void RegisterMessageHandler()
        {
            _connectClient.ClientState += ClientDisconnected;
            RemoteShellHandler.ProgressChanged += CommandOutput;
            RemoteShellHandler.CommandError += CommandError;
            MessageHandler.Register(RemoteShellHandler);
        }

        private void UnregisterMessageHandler()
        {
            MessageHandler.Unregister(RemoteShellHandler);
            RemoteShellHandler.ProgressChanged -= CommandOutput;
            RemoteShellHandler.CommandError -= CommandError;
            _connectClient.ClientState -= ClientDisconnected;
        }

        private void CommandOutput(object sender, string output)
        {
            txtConsoleOutput.SelectionColor = Color.White;
            txtConsoleOutput.AppendText(output);
        }

        private void CommandError(object sender, string output)
        {
            txtConsoleOutput.SelectionColor = Color.Red;
            txtConsoleOutput.AppendText(output);
        }

        private void ClientDisconnected(Client client, bool connected)
        {
            if (!connected)
            {
                this.Invoke((MethodInvoker)this.Close);
            }
        }

        private void RemoteShellForm_Load(object sender, EventArgs e)
        {
            this.DoubleBuffered = true;
            this.Text = WindowHelper.GetWindowTitle("FK远控服务器端 - 远程命令控制窗口", _connectClient);
        }

        private void RemoteShellForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            UnregisterMessageHandler();
            if (_connectClient.Connected)
                RemoteShellHandler.SendCommand("exit");
        }

        private void txtConsoleOutput_TextChanged(object sender, EventArgs e)
        {
            NativeMethodsHelper.ScrollToBottom(txtConsoleOutput.Handle);
        }

        private void txtConsoleInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !string.IsNullOrEmpty(txtConsoleInput.Text.Trim()))
            {
                string input = txtConsoleInput.Text.TrimStart(' ', ' ').TrimEnd(' ', ' ');
                txtConsoleInput.Text = string.Empty;

                string[] splitSpaceInput = input.Split(' ');
                string[] splitNullInput = input.Split(' ');
                if (input == "exit" ||
                    ((splitSpaceInput.Length > 0) && splitSpaceInput[0] == "exit") ||
                    ((splitNullInput.Length > 0) && splitNullInput[0] == "exit"))
                {
                    this.Close();
                }
                else
                {
                    switch (input)
                    {
                        case "cls":
                            txtConsoleOutput.Text = string.Empty;
                            break;
                        default:
                            RemoteShellHandler.SendCommand(input);
                            break;
                    }
                }
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void txtConsoleOutput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != (char)2)
            {
                txtConsoleInput.Text += e.KeyChar.ToString();
                txtConsoleInput.Focus();
                txtConsoleInput.SelectionStart = txtConsoleOutput.TextLength;
                txtConsoleInput.ScrollToCaret();
            }
        }
    }
}
