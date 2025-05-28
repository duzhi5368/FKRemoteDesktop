using FKRemoteDesktop.Debugger;
using FKRemoteDesktop.Enums;
using FKRemoteDesktop.Helpers;
using FKRemoteDesktop.Message.MessageStructs;
using FKRemoteDesktop.Message.SubMessages;
using FKRemoteDesktop.Utilities;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.SubMessageHandler
{
    public class RemoteDesktopHandler : NotificationMessageProcessor, IDisposable
    {
        private UnsafeStreamCodec _streamCodec;

        public override bool CanExecute(IMessage message) => message is GetDesktop ||
                                                             message is DoMouseEvent ||
                                                             message is DoKeyboardEvent ||
                                                             message is GetMonitors;

        public override bool CanExecuteFrom(ISender sender) => true;

        public override void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case GetDesktop msg:
                    Logger.Log(ELogType.eLogType_Debug, "收到服务器消息: GetDesktop");
                    Execute(sender, msg);
                    break;

                case DoMouseEvent msg:
                    Logger.Log(ELogType.eLogType_Debug, "收到服务器消息: DoMouseEvent");
                    Execute(sender, msg);
                    break;

                case DoKeyboardEvent msg:
                    Logger.Log(ELogType.eLogType_Debug, "收到服务器消息: DoKeyboardEvent");
                    Execute(sender, msg);
                    break;

                case GetMonitors msg:
                    Logger.Log(ELogType.eLogType_Debug, "收到服务器消息: GetMonitors");
                    Execute(sender, msg);
                    break;
            }
        }

        private void Execute(ISender client, GetDesktop message)
        {
            var monitorBounds = ScreenHelper.GetBounds((message.DisplayIndex));
            var resolution = new Resolution { Height = monitorBounds.Height, Width = monitorBounds.Width };

            if (_streamCodec == null)
                _streamCodec = new UnsafeStreamCodec(message.Quality, message.DisplayIndex, resolution);

            if (message.CreateNew)
            {
                _streamCodec?.Dispose();
                _streamCodec = new UnsafeStreamCodec(message.Quality, message.DisplayIndex, resolution);
                OnReport("远程桌面任务开始");
            }

            if (_streamCodec.ImageQuality != message.Quality || _streamCodec.Monitor != message.DisplayIndex || _streamCodec.Resolution != resolution)
            {
                _streamCodec?.Dispose();
                _streamCodec = new UnsafeStreamCodec(message.Quality, message.DisplayIndex, resolution);
            }

            BitmapData desktopData = null;
            Bitmap desktop = null;
            try
            {
                desktop = ScreenHelper.CaptureScreen(message.DisplayIndex);
                desktopData = desktop.LockBits(new Rectangle(0, 0, desktop.Width, desktop.Height),
                    ImageLockMode.ReadWrite, desktop.PixelFormat);

                using (MemoryStream stream = new MemoryStream())
                {
                    if (_streamCodec == null) throw new Exception("StreamCodec 不可为空.");
                    _streamCodec.CodeImage(desktopData.Scan0,
                        new Rectangle(0, 0, desktop.Width, desktop.Height),
                        new Size(desktop.Width, desktop.Height),
                        desktop.PixelFormat, stream);
                    client.Send(new GetDesktopResponse
                    {
                        Image = stream.ToArray(),
                        Quality = _streamCodec.ImageQuality,
                        Monitor = _streamCodec.Monitor,
                        Resolution = _streamCodec.Resolution
                    });
                }
            }
            catch (Exception)
            {
                if (_streamCodec != null)
                {
                    client.Send(new GetDesktopResponse
                    {
                        Image = null,
                        Quality = _streamCodec.ImageQuality,
                        Monitor = _streamCodec.Monitor,
                        Resolution = _streamCodec.Resolution
                    });
                }
                _streamCodec = null;
            }
            finally
            {
                if (desktop != null)
                {
                    if (desktopData != null)
                    {
                        try
                        {
                            desktop.UnlockBits(desktopData);
                        }
                        catch { }
                    }
                    desktop.Dispose();
                }
            }
        }

        private void Execute(ISender sender, DoMouseEvent message)
        {
            try
            {
                Screen[] allScreens = Screen.AllScreens;
                int offsetX = allScreens[message.MonitorIndex].Bounds.X;
                int offsetY = allScreens[message.MonitorIndex].Bounds.Y;
                Point p = new Point(message.X + offsetX, message.Y + offsetY);

                switch (message.Action)
                {
                    case EMouseAction.eMouseAction_LeftDown:
                    case EMouseAction.eMouseAction_LeftUp:
                    case EMouseAction.eMouseAction_RightDown:
                    case EMouseAction.eMouseAction_RightUp:
                    case EMouseAction.eMouseAction_MoveCursor:
                        if (NativeMethodsHelper.IsScreensaverActive())
                            NativeMethodsHelper.DisableScreensaver();
                        break;
                }

                switch (message.Action)
                {
                    case EMouseAction.eMouseAction_LeftDown:
                    case EMouseAction.eMouseAction_LeftUp:
                        NativeMethodsHelper.DoMouseLeftClick(p, message.IsMouseDown);
                        break;

                    case EMouseAction.eMouseAction_RightDown:
                    case EMouseAction.eMouseAction_RightUp:
                        NativeMethodsHelper.DoMouseRightClick(p, message.IsMouseDown);
                        break;

                    case EMouseAction.eMouseAction_MoveCursor:
                        NativeMethodsHelper.DoMouseMove(p);
                        break;

                    case EMouseAction.eMouseAction_ScrollDown:
                        NativeMethodsHelper.DoMouseScroll(p, true);
                        break;

                    case EMouseAction.eMouseAction_ScrollUp:
                        NativeMethodsHelper.DoMouseScroll(p, false);
                        break;
                }
            }
            catch { }
        }

        private void Execute(ISender sender, DoKeyboardEvent message)
        {
            if (NativeMethodsHelper.IsScreensaverActive())
                NativeMethodsHelper.DisableScreensaver();
            NativeMethodsHelper.DoKeyPress(message.Key, message.KeyDown);
        }

        private void Execute(ISender client, GetMonitors message)
        {
            client.Send(new GetMonitorsResponse { Number = Screen.AllScreens.Length });
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
                _streamCodec?.Dispose();
            }
        }
    }
}