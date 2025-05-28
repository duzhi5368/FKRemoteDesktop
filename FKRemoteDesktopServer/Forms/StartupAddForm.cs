using FKRemoteDesktop.Enums;
using FKRemoteDesktop.Helpers;
using FKRemoteDesktop.Message.MessageStructs;
using System.IO;
using System.Windows.Forms;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Forms
{
    public partial class StartupAddForm : Form
    {
        public StartupItem StartupItem { get; set; }

        public StartupAddForm()
        {
            InitializeComponent();
            AddTypes();
        }

        public StartupAddForm(string startupPath)
        {
            InitializeComponent();
            AddTypes();

            txtName.Text = Path.GetFileNameWithoutExtension(startupPath);
            txtPath.Text = startupPath;
        }

        private void AddTypes()
        {
            cmbType.Items.Add("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
            cmbType.Items.Add("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce");
            cmbType.Items.Add("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
            cmbType.Items.Add("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce");
            cmbType.Items.Add("%APPDATA%\\Microsoft\\Windows\\Start Menu\\Programs\\Startup");
            cmbType.SelectedIndex = 0;
        }

        // 确定 按钮
        private void btnAdd_Click(object sender, System.EventArgs e)
        {
            StartupItem = new StartupItem
            { Name = txtName.Text, Path = txtPath.Text, Type = (EStartupType)cmbType.SelectedIndex };

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // 取消 按钮
        private void btnCancel_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void txtName_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = ((e.KeyChar == '\\' || FileHelper.HasIllegalCharacters(e.KeyChar.ToString())) &&
                         !char.IsControl(e.KeyChar));
            bool b = e.Handled;
        }

        private void txtPath_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = (FileHelper.HasIllegalCharacters(e.KeyChar.ToString()) &&
                !char.IsControl(e.KeyChar) && (e.KeyChar != '\\') && (e.KeyChar != '"'));
            bool b = e.Handled;
        }
    }
}
