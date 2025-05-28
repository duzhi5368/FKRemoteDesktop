using FKRemoteDesktop.Enums;
using FKRemoteDesktop.Helpers;
using FKRemoteDesktop.Message;
using FKRemoteDesktop.Message.SubMessageHandler;
using FKRemoteDesktop.Network;
using FKRemoteDesktop.Utilities;
using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Forms
{
    public partial class RemoteDesktopForm : Form
    {
        private bool _enableMouseInput;
        private bool _enableKeyboardInput;
        private readonly List<Keys> _keysPressed;
        private readonly Client _connectClient;
        private readonly RemoteDesktopHandler _remoteDesktopHandler;
        private IKeyboardMouseEvents _keyboardHook;
        private IKeyboardMouseEvents _mouseHook;
        private static readonly Dictionary<Client, RemoteDesktopForm> OpenedForms = new Dictionary<Client, RemoteDesktopForm>();

        public RemoteDesktopForm(Client client)
        {
            _connectClient = client;
            _remoteDesktopHandler = new RemoteDesktopHandler(client);
            _keysPressed = new List<Keys>();

            RegisterMessageHandler();
            InitializeComponent();
        }

        public static RemoteDesktopForm CreateNewOrGetExisting(Client client)
        {
            if (OpenedForms.ContainsKey(client))
            {
                return OpenedForms[client];
            }
            RemoteDesktopForm r = new RemoteDesktopForm(client);
            r.Disposed += (sender, args) => OpenedForms.Remove(client);
            OpenedForms.Add(client, r);
            return r;
        }

        private void ClientDisconnected(Client client, bool connected)
        {
            if (!connected)
            {
                this.Invoke((MethodInvoker)this.Close);
            }
        }

        private void RegisterMessageHandler()
        {
            _connectClient.ClientState += ClientDisconnected;
            _remoteDesktopHandler.DisplaysChanged += DisplaysChanged;
            _remoteDesktopHandler.ProgressChanged += UpdateImage;
            MessageHandler.Register(_remoteDesktopHandler);
        }

        private void UnregisterMessageHandler()
        {
            MessageHandler.Unregister(_remoteDesktopHandler);
            _remoteDesktopHandler.DisplaysChanged -= DisplaysChanged;
            _remoteDesktopHandler.ProgressChanged -= UpdateImage;
            _connectClient.ClientState -= ClientDisconnected;
        }

        private void SubscribeEvents()
        {
            if (PlatformHelper.RunningOnMono) // Mono/Linux
            {
                this.KeyDown += OnKeyDown;
                this.KeyUp += OnKeyUp;
            }
            else // Windows
            {
                _keyboardHook = Hook.GlobalEvents();
                _keyboardHook.KeyDown += OnKeyDown;
                _keyboardHook.KeyUp += OnKeyUp;

                _mouseHook = Hook.AppEvents();
                _mouseHook.MouseWheel += OnMouseWheelMove;
            }
        }

        private void UnsubscribeEvents()
        {
            if (PlatformHelper.RunningOnMono) // Mono/Linux
            {
                this.KeyDown -= OnKeyDown;
                this.KeyUp -= OnKeyUp;
            }
            else // Windows
            {
                if (_keyboardHook != null)
                {
                    _keyboardHook.KeyDown -= OnKeyDown;
                    _keyboardHook.KeyUp -= OnKeyUp;
                    _keyboardHook.Dispose();
                }
                if (_mouseHook != null)
                {
                    _mouseHook.MouseWheel -= OnMouseWheelMove;
                    _mouseHook.Dispose();
                }
            }
        }

        private void StartStream()
        {
            ToggleConfigurationControls(true);
            picDesktop.Start();
            picDesktop.SetFrameUpdatedEvent(frameCounter_FrameUpdated);
            this.ActiveControl = picDesktop;
            _remoteDesktopHandler.BeginReceiveFrames(barQuality.Value, cbMonitors.SelectedIndex);
        }

        private void StopStream()
        {
            ToggleConfigurationControls(false);
            picDesktop.Stop();
            picDesktop.UnsetFrameUpdatedEvent(frameCounter_FrameUpdated);
            this.ActiveControl = picDesktop;
            _remoteDesktopHandler.EndReceiveFrames();
        }

        private void frameCounter_FrameUpdated(FrameUpdatedEventArgs e)
        {
            this.Text = string.Format("{0} - FPS: {1}", 
                WindowHelper.GetWindowTitle("FK远控服务器端 - 桌面监控", _connectClient), 
                e.CurrentFramesPerSecond.ToString("0.00"));
        }

        private void ToggleConfigurationControls(bool started)
        {
            btnStart.Enabled = !started;
            btnStop.Enabled = started;
            barQuality.Enabled = !started;
            cbMonitors.Enabled = !started;
        }

        private void TogglePanelVisibility(bool visible)
        {
            panelTop.Visible = visible;
            btnShow.Visible = !visible;
            this.ActiveControl = picDesktop;
        }

        private void DisplaysChanged(object sender, int displays)
        {
            cbMonitors.Items.Clear();
            for (int i = 0; i < displays; i++)
                cbMonitors.Items.Add($"Display {i + 1}");
            cbMonitors.SelectedIndex = 0;
        }

        private void UpdateImage(object sender, Bitmap bmp)
        {
            picDesktop.UpdateImage(bmp, false);
        }

        #region FormUI信息

        private void RemoteDesktopForm_Load(object sender, System.EventArgs e)
        {
            this.Text = WindowHelper.GetWindowTitle("FK远控服务器端 - 桌面监控", _connectClient);
            OnResize(EventArgs.Empty);
            _remoteDesktopHandler.RefreshDisplays();
        }

        private void RemoteDesktopForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            UnsubscribeEvents();
            if (_remoteDesktopHandler.IsStarted) StopStream();
            UnregisterMessageHandler();
            _remoteDesktopHandler.Dispose();
            picDesktop.Image?.Dispose();
        }

        private void RemoteDesktopForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
                return;

            _remoteDesktopHandler.LocalResolution = picDesktop.Size;
            panelTop.Left = (this.Width - panelTop.Width) / 2;
            btnShow.Left = (this.Width - btnShow.Width) / 2;
            btnHide.Left = (panelTop.Width - btnHide.Width) / 2;
        }

        #endregion

        #region UI事件信息

        // 开始 按钮
        private void btnStart_Click(object sender, EventArgs e)
        {
            if (cbMonitors.Items.Count == 0)
            {
                MessageBox.Show("未检测到远程显示器。\n请等待客户端发送可用显示器列表。",
                    "启动失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SubscribeEvents();
            StartStream();
        }

        // 停止 按钮
        private void btnStop_Click(object sender, EventArgs e)
        {
            UnsubscribeEvents();
            StopStream();
        }

        // 显示控制台 按钮
        private void btnShow_Click(object sender, EventArgs e)
        {
            TogglePanelVisibility(true);
        }

        // 隐藏控制台 按钮
        private void btnHide_Click(object sender, EventArgs e)
        {
            TogglePanelVisibility(false);
        }

        // 调整清晰度
        private void barQuality_Scroll(object sender, EventArgs e)
        {
            int value = barQuality.Value;
            lblQualityShow.Text = value.ToString();

            if (value < 25)
                lblQualityShow.Text += " (低清晰度)";
            else if (value >= 85)
                lblQualityShow.Text += " (最佳清晰度)";
            else if (value >= 75)
                lblQualityShow.Text += " (高清晰度)";
            else if (value >= 25)
                lblQualityShow.Text += " (中等清晰度)";

            this.ActiveControl = picDesktop;
        }

        // 鼠标按钮
        private void btnMouse_Click(object sender, EventArgs e)
        {
            if (_enableMouseInput)
            {
                this.picDesktop.Cursor = Cursors.Default;
                toolTipButtons.SetToolTip(btnMouse, "开启鼠标输入功能");
                _enableMouseInput = false;
            }
            else
            {
                this.picDesktop.Cursor = Cursors.Hand;
                toolTipButtons.SetToolTip(btnMouse, "关闭鼠标输入功能");
                _enableMouseInput = true;
            }
            this.ActiveControl = picDesktop;
        }

        // 键盘按钮
        private void btnKeyboard_Click(object sender, EventArgs e)
        {
            if (_enableKeyboardInput)
            {
                this.picDesktop.Cursor = Cursors.Default;
                toolTipButtons.SetToolTip(btnKeyboard, "开启按键输入功能");
                _enableKeyboardInput = false;
            }
            else
            {
                this.picDesktop.Cursor = Cursors.Hand;
                toolTipButtons.SetToolTip(btnKeyboard, "关闭按键输入功能");
                _enableKeyboardInput = true;
            }
            this.ActiveControl = picDesktop;
        }

        #endregion

        #region 屏幕信息

        private void picDesktop_MouseDown(object sender, MouseEventArgs e)
        {
            if (picDesktop.Image != null && _enableMouseInput && this.ContainsFocus)
            {
                EMouseAction action = EMouseAction.eMouseAction_None;
                if (e.Button == MouseButtons.Left)
                    action = EMouseAction.eMouseAction_LeftDown;
                if (e.Button == MouseButtons.Right)
                    action = EMouseAction.eMouseAction_RightDown;
                int selectedDisplayIndex = cbMonitors.SelectedIndex;
                _remoteDesktopHandler.SendMouseEvent(action, true, e.X, e.Y, selectedDisplayIndex);
            }
        }

        private void picDesktop_MouseUp(object sender, MouseEventArgs e)
        {
            if (picDesktop.Image != null && _enableMouseInput && this.ContainsFocus)
            {
                EMouseAction action = EMouseAction.eMouseAction_None;
                if (e.Button == MouseButtons.Left)
                    action = EMouseAction.eMouseAction_LeftUp;
                if (e.Button == MouseButtons.Right)
                    action = EMouseAction.eMouseAction_RightUp;
                int selectedDisplayIndex = cbMonitors.SelectedIndex;
                _remoteDesktopHandler.SendMouseEvent(action, false, e.X, e.Y, selectedDisplayIndex);
            }
        }

        private void picDesktop_MouseMove(object sender, MouseEventArgs e)
        {
            if (picDesktop.Image != null && _enableMouseInput && this.ContainsFocus)
            {
                int selectedDisplayIndex = cbMonitors.SelectedIndex;
                _remoteDesktopHandler.SendMouseEvent(EMouseAction.eMouseAction_MoveCursor, false, e.X, e.Y, selectedDisplayIndex);
            }
        }

        #endregion

        #region 鼠标键盘回调信息

        private void OnMouseWheelMove(object sender, MouseEventArgs e)
        {
            if (picDesktop.Image != null && _enableMouseInput && this.ContainsFocus)
            {
                _remoteDesktopHandler.SendMouseEvent(e.Delta == 120 ? EMouseAction.eMouseAction_ScrollUp : EMouseAction.eMouseAction_ScrollDown,
                    false, 0, 0, cbMonitors.SelectedIndex);
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (picDesktop.Image != null && _enableKeyboardInput && this.ContainsFocus)
            {
                if (!IsLockKey(e.KeyCode))
                    e.Handled = true;
                if (_keysPressed.Contains(e.KeyCode))
                    return;
                _keysPressed.Add(e.KeyCode);
                _remoteDesktopHandler.SendKeyboardEvent((byte)e.KeyCode, true);
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (picDesktop.Image != null && _enableKeyboardInput && this.ContainsFocus)
            {
                if (!IsLockKey(e.KeyCode))
                    e.Handled = true;
                _keysPressed.Remove(e.KeyCode);
                _remoteDesktopHandler.SendKeyboardEvent((byte)e.KeyCode, false);
            }
        }

        private bool IsLockKey(Keys key)
        {
            return ((key & Keys.CapsLock) == Keys.CapsLock)
                   || ((key & Keys.NumLock) == Keys.NumLock)
                   || ((key & Keys.Scroll) == Keys.Scroll);
        }


        #endregion

    }
}
