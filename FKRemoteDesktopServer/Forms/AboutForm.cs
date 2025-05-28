using System;
using System.Diagnostics;
using System.Windows.Forms;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Forms
{
    public partial class AboutForm : Form
    {
        private const string GITHUB_URL = @"https://github.com/duzhi5368/FKRemoteDesktop";

        public AboutForm()
        {
            InitializeComponent();

            rtxtContent.Text = Properties.Resources.License;
            lblVersion.Text = $"v{Application.ProductVersion}";
            linkLabelGithubPage.Links.Add(new LinkLabel.Link { LinkData = GITHUB_URL });
        }

        private void btnOkay_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void linkLabelGithubPage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabelGithubPage.LinkVisited = true;
            Process.Start(e.Link.LinkData.ToString());
        }
    }
}
