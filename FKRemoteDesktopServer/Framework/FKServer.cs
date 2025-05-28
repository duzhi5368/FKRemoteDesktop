using FKRemoteDesktop.Cryptography;
using FKRemoteDesktop.Message;
using FKRemoteDesktop.Message.SubMessages;
using FKRemoteDesktop.Network;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Framework
{
    public class FKServer : Server
    {
        #region 消息事件
        // 客户端连接事件消息
        public delegate void ClientConnectedEventHandler(Client client);
        public event ClientConnectedEventHandler ClientConnected;

        // 客户端断开连接事件消息
        public delegate void ClientDisconnectedEventHandler(Client client);
        public event ClientDisconnectedEventHandler ClientDisconnected;

        // 客户端连接事件
        private void OnClientConnected(Client client)
        {
            if (ProcessingDisconnect || !Listening) 
                return;
            var handler = ClientConnected;
            handler?.Invoke(client);
        }

        // 客户端断开连接事件
        private void OnClientDisconnected(Client client)
        {
            if (ProcessingDisconnect || !Listening) 
                return;
            var handler = ClientDisconnected;
            handler?.Invoke(client);
        }
        #endregion

        // 构造函数
        public FKServer(X509Certificate2 serverCertificate) : base(serverCertificate)
        {
            base.ClientState += OnClientState;
            base.ClientRead += OnClientRead;
        }

        // 客户端状态更变事件
        private void OnClientState(Server server, Client client, bool connected)
        {
            if (!connected)
            {
                if (client.Identified)
                {
                    OnClientDisconnected(client);
                }
            }
        }

        // 将客户端收到的消息转发到 MessageHandler
        private void OnClientRead(Server server, Client client, IMessage message)
        {
            if (!client.Identified)
            {
                if (message.GetType() == typeof(ClientIdentification))
                {
                    client.Identified = IdentifyClient(client, (ClientIdentification)message);
                    if (client.Identified)
                    {
                        client.Send(new ClientIdentificationResult { Result = true }); // 结束握手
                        OnClientConnected(client);
                    }
                    else
                    {
                        // 验证失败
                        client.Disconnect();
                    }
                }
                else
                {
                    // 只要客户端处于未验证状态，就不允许发送其他消息
                    client.Disconnect();
                }
                return;
            }
            MessageHandler.Process(client, message);
        }

        // 验证客户端是否合法
        private bool IdentifyClient(Client client, ClientIdentification packet)
        {
            if (packet.Id.Length != 64)
                return false;

            client.UserInfo.Version = packet.Version;
            client.UserInfo.OperatingSystem = packet.OperatingSystem;
            client.UserInfo.AccountType = packet.AccountType;
            client.UserInfo.Country = packet.Country;
            client.UserInfo.CountryCode = packet.CountryCode;
            client.UserInfo.Id = packet.Id;
            client.UserInfo.Username = packet.Username;
            client.UserInfo.PcName = packet.PcName;
            client.UserInfo.Tag = packet.Tag;
            client.UserInfo.ImageIndex = packet.ImageIndex;
            client.UserInfo.EncryptionKey = packet.EncryptionKey;
            try
            {
#if DEBUG
                // debug 模式下，暂时允许客户端连接
                return true;
#else
                var csp = (RSACryptoServiceProvider)ServerCertificate.PublicKey.Key;
                return csp.VerifyHash(Sha256.ComputeHash(Encoding.UTF8.GetBytes(packet.EncryptionKey)),
                    CryptoConfig.MapNameToOID("SHA256"), packet.Signature);
#endif
            }
            catch (Exception)
            {
                return false;
            }
        }

        // 获取当前服务器中已连接和已被识别的客户端列表
        public Client[] ConnectedClients
        {
            get { return Clients.Where(c => c != null && c.Identified).ToArray(); }
        }
    }
}
