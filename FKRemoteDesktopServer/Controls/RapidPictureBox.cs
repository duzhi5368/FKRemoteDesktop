using FKRemoteDesktop.Utilities;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Controls
{
    public class RapidPictureBox : PictureBox
    {
        public bool Running { get; set; }
        public int ScreenWidth { get; private set; }
        public int ScreenHeight { get; private set; }
        private readonly object _imageLock = new object();
        private Stopwatch _sWatch;
        private FrameCounter _frameCounter;

        public Image GetImageSafe
        {
            get
            {
                return Image;
            }
            set
            {
                lock (_imageLock)
                {
                    Image = value;
                }
            }
        }

        public RapidPictureBox()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            lock (_imageLock)
            {
                if (GetImageSafe != null)
                {
                    pe.Graphics.DrawImage(GetImageSafe, Location);
                }
            }
        }

        private void UpdateScreenSize(int newWidth, int newHeight)
        {
            ScreenWidth = newWidth;
            ScreenHeight = newHeight;
        }

        private void CountFps()
        {
            var deltaTime = (float)_sWatch.Elapsed.TotalSeconds;
            _sWatch = Stopwatch.StartNew();

            _frameCounter.Update(deltaTime);
        }

        public void SetFrameUpdatedEvent(FrameUpdatedEventHandler e)
        {
            _frameCounter.FrameUpdated += e;
        }

        public void UnsetFrameUpdatedEvent(FrameUpdatedEventHandler e)
        {
            _frameCounter.FrameUpdated -= e;
        }

        public void Start()
        {
            _frameCounter = new FrameCounter();
            _sWatch = Stopwatch.StartNew();
            Running = true;
        }

        public void Stop()
        {
            _sWatch?.Stop();
            Running = false;
        }

        public void UpdateImage(Bitmap bmp, bool cloneBitmap)
        {
            try
            {
                CountFps();
                if ((ScreenWidth != bmp.Width) && (ScreenHeight != bmp.Height))
                    UpdateScreenSize(bmp.Width, bmp.Height);

                lock (_imageLock)
                {
                    var oldImage = GetImageSafe;
                    SuspendLayout();
                    GetImageSafe = cloneBitmap ? new Bitmap(bmp, Width, Height) /*resize bitmap*/ : bmp;
                    ResumeLayout();
                    oldImage?.Dispose();
                }
            }
            catch (InvalidOperationException) {}
            catch (Exception) {}
        }
    }
}
