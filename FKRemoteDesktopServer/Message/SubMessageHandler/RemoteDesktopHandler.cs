using FKRemoteDesktop.Enums;
using FKRemoteDesktop.Message.SubMessages;
using FKRemoteDesktop.Network;
using FKRemoteDesktop.Utilities;
using System;
using System.Drawing;
using System.IO;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.SubMessageHandler
{
    public class RemoteDesktopHandler : MessageProcessorBase<Bitmap>, IDisposable
    {
        public bool IsStarted { get; set; }
        private readonly object _syncLock = new object();
        private readonly object _sizeLock = new object();
        private Size _localResolution;
        private readonly Client _client;
        private UnsafeStreamCodec _codec;

        public Size LocalResolution
        {
            get
            {
                lock (_sizeLock)
                {
                    return _localResolution;
                }
            }
            set
            {
                lock (_sizeLock)
                {
                    _localResolution = value;
                }
            }
        }

        public delegate void DisplaysChangedEventHandler(object sender, int value);
        public event DisplaysChangedEventHandler DisplaysChanged;
        private void OnDisplaysChanged(int value)
        {
            SynchronizationContext.Post(val =>
            {
                var handler = DisplaysChanged;
                handler?.Invoke(this, (int)val);
            }, value);
        }

        public RemoteDesktopHandler(Client client) : base(true)
        {
            _client = client;
        }

        public override bool CanExecute(IMessage message) => message is GetDesktopResponse 
                                                        || message is GetMonitorsResponse;

        public override bool CanExecuteFrom(ISender sender) => _client.Equals(sender);

        public override void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case GetDesktopResponse d:
                    Execute(sender, d);
                    break;
                case GetMonitorsResponse m:
                    Execute(sender, m);
                    break;
            }
        }

        public void BeginReceiveFrames(int quality, int display)
        {
            lock (_syncLock)
            {
                IsStarted = true;
                _codec?.Dispose();
                _codec = null;
                _client.Send(new GetDesktop { CreateNew = true, Quality = quality, DisplayIndex = display });
            }
        }

        public void EndReceiveFrames()
        {
            lock (_syncLock)
            {
                IsStarted = false;
            }
        }

        public void RefreshDisplays()
        {
            _client.Send(new GetMonitors());
        }

        public void SendMouseEvent(EMouseAction mouseAction, bool isMouseDown, int x, int y, int displayIndex)
        {
            lock (_syncLock)
            {
                _client.Send(new DoMouseEvent
                {
                    Action = mouseAction,
                    IsMouseDown = isMouseDown,
                    X = x * _codec.Resolution.Width / LocalResolution.Width,
                    Y = y * _codec.Resolution.Height / LocalResolution.Height,
                    MonitorIndex = displayIndex
                });
            }
        }

        public void SendKeyboardEvent(byte keyCode, bool keyDown)
        {
            _client.Send(new DoKeyboardEvent { Key = keyCode, KeyDown = keyDown });
        }

        private void Execute(ISender client, GetDesktopResponse message)
        {
            lock (_syncLock)
            {
                if (!IsStarted)
                    return;

                if (_codec == null || _codec.ImageQuality != message.Quality 
                    || _codec.Monitor != message.Monitor || _codec.Resolution != message.Resolution)
                {
                    _codec?.Dispose();
                    _codec = new UnsafeStreamCodec(message.Quality, message.Monitor, message.Resolution);
                }
                using (MemoryStream ms = new MemoryStream(message.Image))
                {
                    OnReport(new Bitmap(_codec.DecodeData(ms), LocalResolution));
                }

                message.Image = null;
                client.Send(new GetDesktop { Quality = message.Quality, DisplayIndex = message.Monitor });
            }
        }

        private void Execute(ISender client, GetMonitorsResponse message)
        {
            OnDisplaysChanged(message.Number);
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
                    _codec?.Dispose();
                    IsStarted = false;
                }
            }
        }
    }
}
