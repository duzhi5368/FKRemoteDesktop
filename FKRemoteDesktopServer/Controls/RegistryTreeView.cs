using System.Windows.Forms;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Controls
{
    public class RegistryTreeView : TreeView
    {
        public RegistryTreeView()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        }
    }
}
