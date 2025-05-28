using FKRemoteDesktop.Controls;
using FKRemoteDesktop.Enums;
using FKRemoteDesktop.Helpers;
using FKRemoteDesktop.Message;
using FKRemoteDesktop.Message.MessageStructs;
using FKRemoteDesktop.Message.SubMessageHandler;
using FKRemoteDesktop.Network;
using FKRemoteDesktop.Structs;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Forms
{
    public partial class FileManagerForm : Form
    {
        private enum ETransferColumn
        {
            eTransferColumn_ID,
            eTransferColumn_Type, 
            eTransferColumn_Status,
        }

        private string _currentDir;                             // 文件管理器显示的当前远程目录
        private readonly Client _connectClient;                 // 当前处理的客户端
        private readonly FileManagerHandler _fileManagerHandler;// 用于与客户端通信的消息处理程序
        // 打开的文件管理器面板清单
        private static readonly Dictionary<Client, FileManagerForm> OpenedForms = new Dictionary<Client, FileManagerForm>();

        public FileManagerForm(Client client)
        {
            _connectClient = client;
            _fileManagerHandler = new FileManagerHandler(client);
            RegisterMessageHandler();

            InitializeComponent();
        }

        public static FileManagerForm CreateNewOrGetExisting(Client client)
        {
            if (OpenedForms.ContainsKey(client))
            {
                return OpenedForms[client];
            }
            FileManagerForm f = new FileManagerForm(client);
            f.Disposed += (sender, args) => OpenedForms.Remove(client);
            OpenedForms.Add(client, f);
            return f;
        }

        private void RegisterMessageHandler()
        {
            _connectClient.ClientState += ClientDisconnected;
            _fileManagerHandler.ProgressChanged += SetStatusMessage;
            _fileManagerHandler.DrivesChanged += DrivesChanged;
            _fileManagerHandler.DirectoryChanged += DirectoryChanged;
            _fileManagerHandler.FileTransferUpdated += FileTransferUpdated;
            MessageHandler.Register(_fileManagerHandler);
        }

        private void UnregisterMessageHandler()
        {
            MessageHandler.Unregister(_fileManagerHandler);
            _fileManagerHandler.ProgressChanged -= SetStatusMessage;
            _fileManagerHandler.DrivesChanged -= DrivesChanged;
            _fileManagerHandler.DirectoryChanged -= DirectoryChanged;
            _fileManagerHandler.FileTransferUpdated -= FileTransferUpdated;
            _connectClient.ClientState -= ClientDisconnected;
        }

        #region 回调函数

        // 客户端断开连接时调用
        private void ClientDisconnected(Client client, bool connected)
        {
            if (!connected)
            {
                this.Invoke((MethodInvoker)this.Close);
            }
        }

        // 当驱动器发生更变时调用
        private void DrivesChanged(object sender, Drive[] drives)
        {
            cmbDrives.Items.Clear();
            cmbDrives.DisplayMember = "DisplayName";
            cmbDrives.ValueMember = "RootDirectory";
            cmbDrives.DataSource = new BindingSource(drives, null);
            SetStatusMessage(this, "完成");
        }

        // 当目录发生更变时调用
        private void DirectoryChanged(object sender, string remotePath, FileSystemEntry[] items)
        {
            txtPath.Text = remotePath;
            _currentDir = remotePath;
            lstDirectory.Items.Clear();
            AddItemToFileBrowser("..", 0, EFileType.eFileType_Back, 0);
            foreach (var item in items)
            {
                switch (item.EntryType)
                {
                    case EFileType.eFileType_Directory:
                        AddItemToFileBrowser(item.Name, 0, item.EntryType, 1);
                        break;
                    case EFileType.eFileType_File:
                        int imageIndex = item.ContentType == null ? 2 : (int)item.ContentType;
                        AddItemToFileBrowser(item.Name, item.Size, item.EntryType, imageIndex);
                        break;
                }
            }
            SetStatusMessage(this, "处理完成");
        }

        // 每当文件传输发生更新时调用
        private void FileTransferUpdated(object sender, SFileTransfer transfer)
        {
            for (var i = 0; i < lstTransfers.Items.Count; i++)
            {
                if (lstTransfers.Items[i].SubItems[(int)ETransferColumn.eTransferColumn_ID].Text == transfer.Id.ToString())
                {
                    lstTransfers.Items[i].SubItems[(int)ETransferColumn.eTransferColumn_Status].Text = transfer.Status;
                    return;
                }
            }
            var lvi = new ListViewItem(new[]
                    {transfer.Id.ToString(), ToString(transfer.Type), transfer.Status, transfer.RemotePath})
            { Tag = transfer };
            lstTransfers.Items.Add(lvi);
        }

        private string ToString(ETransferType type)
        {
            switch (type)
            {
                case ETransferType.eTransferType_Upload: return "上传";
                case ETransferType.eTransferType_Download: return "下载";
                default: return string.Empty;
            }
        }

        // 更新状态信息
        private void SetStatusMessage(object sender, string message)
        {
            stripLblStatus.Text = $"状态：{message}";
        }

        #endregion

        #region 核心函数

        // 合并当前路径和新路径
        private string GetAbsolutePath(string path)
        {
            if (!string.IsNullOrEmpty(_currentDir) && _currentDir[0] == '/')
            {
                if (_currentDir.Length == 1)
                    return Path.Combine(_currentDir, path);
                else
                    return Path.Combine(_currentDir + '/', path);
            }
            return Path.GetFullPath(Path.Combine(_currentDir, path));
        }

        // 在分层目录树中，向上导航一个目录
        private string NavigateUp()
        {
            if (!string.IsNullOrEmpty(_currentDir) && _currentDir[0] == '/')
            {
                if (_currentDir.LastIndexOf('/') > 0)
                {
                    _currentDir = _currentDir.Remove(_currentDir.LastIndexOf('/') + 1);
                    _currentDir = _currentDir.TrimEnd('/');
                }
                else
                    _currentDir = "/";

                return _currentDir;
            }
            else
                return GetAbsolutePath(@"..\");
        }

        // 添加一个元素到文件管理器中
        private void AddItemToFileBrowser(string name, long size, EFileType type, int imageIndex)
        {
            ListViewItem lvi = new ListViewItem(new string[]
            {
                name,
                (type == EFileType.eFileType_File) ? StringHelper.GetHumanReadableFileSize(size) : string.Empty,
                (type != EFileType.eFileType_Back) ? ToString(type) : string.Empty
            })
            {
                Tag = new SFileManagerListTag(type, size),
                ImageIndex = imageIndex
            };
            lstDirectory.Items.Add(lvi);
        }

        private string ToString(EFileType type)
        {
            switch (type)
            {
                case EFileType.eFileType_File: return "文件";
                case EFileType.eFileType_Back: return "上一层";
                case EFileType.eFileType_Directory: return "文件夹";
                default: return string.Empty;
            }
        }

        // 刷新文件夹
        private void RefreshDirectory()
        {
            SwitchDirectory(_currentDir);
        }

        // 切换到新目录并获取其内容
        private void SwitchDirectory(string remotePath)
        {
            _fileManagerHandler.GetDirectoryContents(remotePath);
            SetStatusMessage(this, "加载文件夹信息...");
        }

        #endregion

        #region UI函数

        // 加载 Form
        private void FileManagerForm_Load(object sender, System.EventArgs e)
        {
            this.Text = WindowHelper.GetWindowTitle("FK远控服务器端 - 文件传输管理", _connectClient);
            _fileManagerHandler.RefreshDrives();
        }

        // 关闭 Form
        private void FileManagerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            UnregisterMessageHandler();
            _fileManagerHandler.Dispose();
        }

        // 更变磁盘驱动器
        private void cmbDrives_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            SwitchDirectory(cmbDrives.SelectedValue.ToString());
        }

        // 双击某文件夹
        private void lstDirectory_DoubleClick(object sender, System.EventArgs e)
        {
            if (lstDirectory.SelectedItems.Count > 0)
            {
                SFileManagerListTag tag = (SFileManagerListTag)lstDirectory.SelectedItems[0].Tag;
                switch (tag.Type)
                {
                    case EFileType.eFileType_Back:
                        SwitchDirectory(NavigateUp());
                        break;
                    case EFileType.eFileType_Directory:
                        SwitchDirectory(GetAbsolutePath(lstDirectory.SelectedItems[0].SubItems[0].Text));
                        break;
                }
            }
        }

        // 选择某文件夹列（排序）
        private void lstDirectory_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            lstDirectory.ListViewColumnSorter.NeedNumberCompare = (e.Column == 1);
        }

        // 下载文件
        private void downloadToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            foreach (ListViewItem files in lstDirectory.SelectedItems)
            {
                SFileManagerListTag tag = (SFileManagerListTag)files.Tag;
                if (tag.Type == EFileType.eFileType_File)
                {
                    string remotePath = GetAbsolutePath(files.SubItems[0].Text);
                    _fileManagerHandler.BeginDownloadFile(remotePath);
                }
            }
        }

        // 上传文件
        private void uploadToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "选择文件上传";
                ofd.Filter = "全部文件 (*.*)|*.*";
                ofd.Multiselect = true;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    foreach (var localFilePath in ofd.FileNames)
                    {
                        if (!File.Exists(localFilePath)) 
                            continue;
                        string remotePath = GetAbsolutePath(Path.GetFileName(localFilePath));
                        _fileManagerHandler.BeginUploadFile(localFilePath, remotePath);
                    }
                }
            }
        }

        // 执行文件
        private void executeToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            foreach (ListViewItem files in lstDirectory.SelectedItems)
            {
                SFileManagerListTag tag = (SFileManagerListTag)files.Tag;
                if (tag.Type == EFileType.eFileType_File)
                {
                    string remotePath = GetAbsolutePath(files.SubItems[0].Text);
                    _fileManagerHandler.StartProcess(remotePath);
                }
            }
        }

        // 重命名文件
        private void renameToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            foreach (ListViewItem files in lstDirectory.SelectedItems)
            {
                SFileManagerListTag tag = (SFileManagerListTag)files.Tag;
                switch (tag.Type)
                {
                    case EFileType.eFileType_Directory:
                    case EFileType.eFileType_File:
                        string path = GetAbsolutePath(files.SubItems[0].Text);
                        string newName = files.SubItems[0].Text;
                        if (FKInputBox.Show("重命名", "请输入新名称:", ref newName) == DialogResult.OK)
                        {
                            newName = GetAbsolutePath(newName);
                            _fileManagerHandler.RenameFile(path, newName, tag.Type);
                        }
                        break;
                }
            }
        }

        // 删除文件
        private void deleteToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            int count = lstDirectory.SelectedItems.Count;
            if (count == 0) 
                return;
            if (MessageBox.Show(string.Format("确定要删除 {0} 个文件吗？", count),
                "删除文件确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                foreach (ListViewItem files in lstDirectory.SelectedItems)
                {
                    SFileManagerListTag tag = (SFileManagerListTag)files.Tag;
                    switch (tag.Type)
                    {
                        case EFileType.eFileType_Directory:
                        case EFileType.eFileType_File:
                            string path = GetAbsolutePath(files.SubItems[0].Text);
                            _fileManagerHandler.DeleteFile(path, tag.Type);
                            break;
                    }
                }
            }
        }

        // 添加到启动项
        private void addToStartupToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            foreach (ListViewItem files in lstDirectory.SelectedItems)
            {
                SFileManagerListTag tag = (SFileManagerListTag)files.Tag;
                if (tag.Type == EFileType.eFileType_File)
                {
                    string path = GetAbsolutePath(files.SubItems[0].Text);
                    using (var frm = new StartupAddForm(path))
                    {
                        if (frm.ShowDialog() == DialogResult.OK)
                        {
                            _fileManagerHandler.AddToStartup(frm.StartupItem);
                        }
                    }
                }
            }
        }

        // 刷新
        private void refreshToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            RefreshDirectory();
        }

        // 在Shell中打开文件夹
        private void openDirectoryInShellToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            string path = _currentDir;
            if (lstDirectory.SelectedItems.Count == 1)
            {
                var item = lstDirectory.SelectedItems[0];
                SFileManagerListTag tag = (SFileManagerListTag)item.Tag;
                if (tag.Type == EFileType.eFileType_Directory)
                {
                    path = GetAbsolutePath(item.SubItems[0].Text);
                }
            }

            RemoteShellForm frmRs = RemoteShellForm.CreateNewOrGetExisting(_connectClient);
            frmRs.Show();
            frmRs.Focus();
            var driveLetter = Path.GetPathRoot(path);
            frmRs.RemoteShellHandler.SendCommand($"{driveLetter.Remove(driveLetter.Length - 1)} && cd \"{path}\"");
        }

        // 按钮 打开下载文件夹
        private void btnOpenDLFolder_Click(object sender, System.EventArgs e)
        {
            if (!Directory.Exists(_connectClient.UserInfo.DownloadDirectory))
                Directory.CreateDirectory(_connectClient.UserInfo.DownloadDirectory);
            System.Diagnostics.Process.Start(_connectClient.UserInfo.DownloadDirectory);
        }

        // 取消传输
        private void cancelToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            foreach (ListViewItem transfer in lstTransfers.SelectedItems)
            {
                if (!transfer.SubItems[(int)ETransferColumn.eTransferColumn_Status].Text.StartsWith("下载中") &&
                    !transfer.SubItems[(int)ETransferColumn.eTransferColumn_Status].Text.StartsWith("上传中") &&
                    !transfer.SubItems[(int)ETransferColumn.eTransferColumn_Status].Text.StartsWith("等待中")) 
                    continue;
                int id = int.Parse(transfer.SubItems[(int)ETransferColumn.eTransferColumn_ID].Text);
                _fileManagerHandler.CancelFileTransfer(id);
            }
        }

        // 取消全部传输
        private void clearToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            foreach (ListViewItem transfer in lstTransfers.Items)
            {
                if (transfer.SubItems[(int)ETransferColumn.eTransferColumn_Status].Text.StartsWith("下载中") ||
                    transfer.SubItems[(int)ETransferColumn.eTransferColumn_Status].Text.StartsWith("上传中") ||
                    transfer.SubItems[(int)ETransferColumn.eTransferColumn_Status].Text.StartsWith("等待中")) 
                    continue;
                transfer.Remove();
            }
        }

        // 向列表中拖入文件
        private void lstDirectory_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        // 向列表中拖入文件
        private void lstDirectory_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string localFilePath in files)
                {
                    if (!File.Exists(localFilePath)) 
                        continue;
                    string remotePath = GetAbsolutePath(Path.GetFileName(localFilePath));
                    _fileManagerHandler.BeginUploadFile(localFilePath, remotePath);
                }
            }
        }

        // 刷新 按钮
        private void btnRefresh_Click(object sender, System.EventArgs e)
        {
            RefreshDirectory();
        }

        // 按键消息
        private void FileManagerForm_KeyDown(object sender, KeyEventArgs e)
        {
            // F5按钮
            if (e.KeyCode == Keys.F5 && !string.IsNullOrEmpty(_currentDir) && TabControlFileManager.SelectedIndex == 0)
            {
                RefreshDirectory();
                e.Handled = true;
            }
        }

        #endregion

    }
}
