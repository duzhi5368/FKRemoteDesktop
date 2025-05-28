using FKRemoteDesktop.Enums;
using FKRemoteDesktop.Framework;
using FKRemoteDesktop.Helpers;
using FKRemoteDesktop.Message.SubMessages;
using FKRemoteDesktop.Network;
using System;
using System.Threading;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.UserActivity
{
    public class ActivityDetection : IDisposable
    {
        private EUserStatus _lastUserStatus;                    // 用户状态
        private readonly FKClient _client;                      // 用于与服务器通讯的客户端
        private readonly CancellationTokenSource _tokenSource;  // 任务取消token管理器
        private readonly CancellationToken _token;              // 任务取消token

        public ActivityDetection(FKClient client)
        {
            _client = client;
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
            client.ClientState += OnClientStateChange;
        }

        private void OnClientStateChange(Client s, bool connected)
        {
            if (connected)
                _lastUserStatus = EUserStatus.eUserStatus_Active;
        }

        // 开始用户行为检测
        public void Start()
        {
            new Thread(UserActivityThread).Start();
        }

        private void UserActivityThread()
        {
            try
            {
                if (IsUserIdle())
                {
                    if (_lastUserStatus != EUserStatus.eUserStatus_Idle)
                    {
                        _lastUserStatus = EUserStatus.eUserStatus_Idle;
                        _client.Send(new SetUserStatus { Message = _lastUserStatus });
                    }
                }
                else
                {
                    if (_lastUserStatus != EUserStatus.eUserStatus_Active)
                    {
                        _lastUserStatus = EUserStatus.eUserStatus_Active;
                        _client.Send(new SetUserStatus { Message = _lastUserStatus });
                    }
                }
            }
            catch (Exception e) when (e is NullReferenceException || e is ObjectDisposedException)
            {
            }
        }

        private bool IsUserIdle()
        {
            var ticks = Environment.TickCount;
            var idleTime = ticks - NativeMethodsHelper.GetLastInputInfoTickCount();
            idleTime = ((idleTime > 0) ? (idleTime / 1000) : 0);
            return (idleTime > 600); // 超过10分钟就视为Idle了
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
            }
        }
    }
}