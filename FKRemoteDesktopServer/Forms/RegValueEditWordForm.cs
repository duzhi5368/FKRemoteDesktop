using FKRemoteDesktop.Enums;
using FKRemoteDesktop.Message.MessageStructs;
using FKRemoteDesktop.Utilities;
using Microsoft.Win32;
using System.Windows.Forms;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Forms
{
    public partial class RegValueEditWordForm : Form
    {
        private readonly RegValueData _value;

        private const string DWORD_WARNING = "输入的十进制值大于32位DWORD的最大值，是否进行截断？";
        private const string QWORD_WARNING = "输入的十进制值大于64位QWORD的最大值，是否进行截断？";

        public RegValueEditWordForm(RegValueData value)
        {
            _value = value;

            InitializeComponent();

            this.valueNameTxtBox.Text = value.Name;
            if (value.Kind == RegistryValueKind.DWord)
            {
                this.Text = "FK远控服务器端 - 编辑32位DWORD值";
                this.valueDataTxtBox.Type = EWordType.eWordType_DWORD;
                this.valueDataTxtBox.Text = ByteConverter.ToUInt32(value.Data).ToString("x");
            }
            else
            {
                this.Text = "FK远控服务器端 - 编辑64位QWORD值";
                this.valueDataTxtBox.Type = EWordType.eWordType_QWORD;
                this.valueDataTxtBox.Text = ByteConverter.ToUInt64(value.Data).ToString("x");
            }
        }

        private void radioHex_CheckedChanged(object sender, System.EventArgs e)
        {
            if (valueDataTxtBox.IsHexNumber == radioHex.Checked)
                return;

            if (valueDataTxtBox.IsConversionValid() || IsOverridePossible())
                valueDataTxtBox.IsHexNumber = radioHex.Checked;
            else
                radioDecimal.Checked = true;
        }

        private void okButton_Click(object sender, System.EventArgs e)
        {
            if (valueDataTxtBox.IsConversionValid() || IsOverridePossible())
            {
                _value.Data = _value.Kind == RegistryValueKind.DWord
                    ? ByteConverter.GetBytes(valueDataTxtBox.UIntValue)
                    : ByteConverter.GetBytes(valueDataTxtBox.ULongValue);
                this.Tag = _value;
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                this.DialogResult = DialogResult.None;
            }

            this.Close();
        }

        private bool IsOverridePossible()
        {
            string message = _value.Kind == RegistryValueKind.DWord ? DWORD_WARNING : QWORD_WARNING;
            return MessageBox.Show(message, "数值溢出", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
        }
    }
}
