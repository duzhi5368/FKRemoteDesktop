using FKRemoteDesktop.Helpers;
using FKRemoteDesktop.Message.MessageStructs;
using System.Windows.Forms;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Forms
{
    public partial class RegValueEditBinaryForm : Form
    {
        private readonly RegValueData _value;
        private const string INVALID_BINARY_ERROR = "二进制值无效，无法正确转换.";

        public RegValueEditBinaryForm(RegValueData value)
        {
            _value = value;

            InitializeComponent();

            this.valueNameTxtBox.Text = RegValueHelper.GetName(value.Name);
            hexEditor.HexTable = value.Data;
        }

        private void okButton_Click(object sender, System.EventArgs e)
        {
            byte[] bytes = hexEditor.HexTable;
            if (bytes != null)
            {
                try
                {
                    _value.Data = bytes;
                    this.DialogResult = DialogResult.OK;
                    this.Tag = _value;
                }
                catch
                {
                    MessageBox.Show(INVALID_BINARY_ERROR, "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.DialogResult = DialogResult.None;
                }
            }

            this.Close();
        }
    }
}
