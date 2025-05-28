using System.Windows.Forms;
using System;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Helpers
{
    public static class ClipboardHelper
    {
        public static void SetClipboardTextSafe(string text)
        {
            try
            {
                Clipboard.SetText(text);
            }
            catch (Exception)
            {
            }
        }
    }
}
