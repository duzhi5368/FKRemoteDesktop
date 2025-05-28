namespace FKRemoteDesktop.Forms
{
    partial class VisitWebsiteForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VisitWebsiteForm));
            this.chkVisitHidden = new System.Windows.Forms.CheckBox();
            this.lblURL = new System.Windows.Forms.Label();
            this.txtURL = new System.Windows.Forms.TextBox();
            this.btnVisitWebsite = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // chkVisitHidden
            // 
            this.chkVisitHidden.AutoSize = true;
            this.chkVisitHidden.Checked = true;
            this.chkVisitHidden.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkVisitHidden.Location = new System.Drawing.Point(21, 40);
            this.chkVisitHidden.Name = "chkVisitHidden";
            this.chkVisitHidden.Size = new System.Drawing.Size(122, 17);
            this.chkVisitHidden.TabIndex = 6;
            this.chkVisitHidden.Text = "使用后台隐藏访问";
            this.chkVisitHidden.UseVisualStyleBackColor = true;
            // 
            // lblURL
            // 
            this.lblURL.AutoSize = true;
            this.lblURL.Location = new System.Drawing.Point(18, 15);
            this.lblURL.Name = "lblURL";
            this.lblURL.Size = new System.Drawing.Size(79, 13);
            this.lblURL.TabIndex = 4;
            this.lblURL.Text = "打开远程网页";
            // 
            // txtURL
            // 
            this.txtURL.Location = new System.Drawing.Point(104, 12);
            this.txtURL.Name = "txtURL";
            this.txtURL.Size = new System.Drawing.Size(429, 20);
            this.txtURL.TabIndex = 5;
            this.txtURL.Text = "https://github.com/duzhi5368";
            // 
            // btnVisitWebsite
            // 
            this.btnVisitWebsite.Location = new System.Drawing.Point(395, 38);
            this.btnVisitWebsite.Name = "btnVisitWebsite";
            this.btnVisitWebsite.Size = new System.Drawing.Size(138, 23);
            this.btnVisitWebsite.TabIndex = 7;
            this.btnVisitWebsite.Text = "开始访问";
            this.btnVisitWebsite.UseVisualStyleBackColor = true;
            this.btnVisitWebsite.Click += new System.EventHandler(this.btnVisitWebsite_Click);
            // 
            // VisitWebsiteForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(545, 72);
            this.Controls.Add(this.chkVisitHidden);
            this.Controls.Add(this.lblURL);
            this.Controls.Add(this.txtURL);
            this.Controls.Add(this.btnVisitWebsite);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VisitWebsiteForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "FK远控服务器端 - 打开远程网页";
            this.Load += new System.EventHandler(this.VisitWebsiteForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkVisitHidden;
        private System.Windows.Forms.Label lblURL;
        private System.Windows.Forms.TextBox txtURL;
        private System.Windows.Forms.Button btnVisitWebsite;
    }
}