namespace FKRemoteDesktop.Forms
{
    partial class RemoteExecutionForm
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
            FKRemoteDesktop.Controls.ListViewColumnSorter listViewColumnSorter1 = new FKRemoteDesktop.Controls.ListViewColumnSorter();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RemoteExecutionForm));
            this.btnExecute = new System.Windows.Forms.Button();
            this.txtURL = new System.Windows.Forms.TextBox();
            this.lblURL = new System.Windows.Forms.Label();
            this.groupLocalFile = new System.Windows.Forms.GroupBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.hClient = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.hStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.radioURL = new System.Windows.Forms.RadioButton();
            this.radioLocalFile = new System.Windows.Forms.RadioButton();
            this.groupURL = new System.Windows.Forms.GroupBox();
            this.chkUpdate = new System.Windows.Forms.CheckBox();
            this.lstTransfers = new FKRemoteDesktop.Controls.FKListView();
            this.colClient = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupLocalFile.SuspendLayout();
            this.groupURL.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnExecute
            // 
            this.btnExecute.Location = new System.Drawing.Point(351, 393);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(138, 23);
            this.btnExecute.TabIndex = 12;
            this.btnExecute.Text = "开始远程执行";
            this.btnExecute.UseVisualStyleBackColor = true;
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            // 
            // txtURL
            // 
            this.txtURL.Location = new System.Drawing.Point(73, 15);
            this.txtURL.Name = "txtURL";
            this.txtURL.Size = new System.Drawing.Size(396, 20);
            this.txtURL.TabIndex = 1;
            // 
            // lblURL
            // 
            this.lblURL.AutoSize = true;
            this.lblURL.Location = new System.Drawing.Point(13, 18);
            this.lblURL.Name = "lblURL";
            this.lblURL.Size = new System.Drawing.Size(56, 13);
            this.lblURL.TabIndex = 0;
            this.lblURL.Text = "URL路径:";
            // 
            // groupLocalFile
            // 
            this.groupLocalFile.Controls.Add(this.btnBrowse);
            this.groupLocalFile.Controls.Add(this.txtPath);
            this.groupLocalFile.Controls.Add(this.label1);
            this.groupLocalFile.Location = new System.Drawing.Point(10, 29);
            this.groupLocalFile.Name = "groupLocalFile";
            this.groupLocalFile.Size = new System.Drawing.Size(479, 49);
            this.groupLocalFile.TabIndex = 8;
            this.groupLocalFile.TabStop = false;
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(394, 16);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.TabIndex = 2;
            this.btnBrowse.Text = "打开文件...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // txtPath
            // 
            this.txtPath.Location = new System.Drawing.Point(73, 18);
            this.txtPath.Name = "txtPath";
            this.txtPath.ReadOnly = true;
            this.txtPath.Size = new System.Drawing.Size(317, 20);
            this.txtPath.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "文件路径:";
            // 
            // hClient
            // 
            this.hClient.Text = "Client";
            this.hClient.Width = 302;
            // 
            // hStatus
            // 
            this.hStatus.Text = "Status";
            this.hStatus.Width = 173;
            // 
            // radioURL
            // 
            this.radioURL.AutoSize = true;
            this.radioURL.Location = new System.Drawing.Point(10, 84);
            this.radioURL.Name = "radioURL";
            this.radioURL.Size = new System.Drawing.Size(97, 17);
            this.radioURL.TabIndex = 9;
            this.radioURL.Text = "执行网络文件";
            this.radioURL.UseVisualStyleBackColor = true;
            this.radioURL.CheckedChanged += new System.EventHandler(this.radioURL_CheckedChanged);
            // 
            // radioLocalFile
            // 
            this.radioLocalFile.AutoSize = true;
            this.radioLocalFile.Checked = true;
            this.radioLocalFile.Location = new System.Drawing.Point(10, 6);
            this.radioLocalFile.Name = "radioLocalFile";
            this.radioLocalFile.Size = new System.Drawing.Size(97, 17);
            this.radioLocalFile.TabIndex = 7;
            this.radioLocalFile.TabStop = true;
            this.radioLocalFile.Text = "执行本地文件";
            this.radioLocalFile.UseVisualStyleBackColor = true;
            this.radioLocalFile.CheckedChanged += new System.EventHandler(this.radioLocalFile_CheckedChanged);
            // 
            // groupURL
            // 
            this.groupURL.Controls.Add(this.txtURL);
            this.groupURL.Controls.Add(this.lblURL);
            this.groupURL.Enabled = false;
            this.groupURL.Location = new System.Drawing.Point(10, 107);
            this.groupURL.Name = "groupURL";
            this.groupURL.Size = new System.Drawing.Size(479, 49);
            this.groupURL.TabIndex = 10;
            this.groupURL.TabStop = false;
            // 
            // chkUpdate
            // 
            this.chkUpdate.AutoSize = true;
            this.chkUpdate.Location = new System.Drawing.Point(149, 397);
            this.chkUpdate.Name = "chkUpdate";
            this.chkUpdate.Size = new System.Drawing.Size(182, 17);
            this.chkUpdate.TabIndex = 11;
            this.chkUpdate.Text = "使用该文件进行客户端自更新";
            this.chkUpdate.UseVisualStyleBackColor = true;
            this.chkUpdate.CheckedChanged += new System.EventHandler(this.chkUpdate_CheckedChanged);
            // 
            // lstTransfers
            // 
            this.lstTransfers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstTransfers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colClient,
            this.colStatus});
            this.lstTransfers.FullRowSelect = true;
            this.lstTransfers.GridLines = true;
            this.lstTransfers.HideSelection = false;
            listViewColumnSorter1.NeedNumberCompare = false;
            listViewColumnSorter1.Order = System.Windows.Forms.SortOrder.None;
            listViewColumnSorter1.SortColumn = 0;
            this.lstTransfers.ListViewColumnSorter = listViewColumnSorter1;
            this.lstTransfers.Location = new System.Drawing.Point(10, 162);
            this.lstTransfers.Name = "lstTransfers";
            this.lstTransfers.Size = new System.Drawing.Size(479, 226);
            this.lstTransfers.TabIndex = 4;
            this.lstTransfers.UseCompatibleStateImageBehavior = false;
            this.lstTransfers.View = System.Windows.Forms.View.Details;
            // 
            // colClient
            // 
            this.colClient.Text = "客户端";
            this.colClient.Width = 300;
            // 
            // colStatus
            // 
            this.colStatus.Text = "状态";
            this.colStatus.Width = 170;
            // 
            // RemoteExecutionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(498, 420);
            this.Controls.Add(this.lstTransfers);
            this.Controls.Add(this.btnExecute);
            this.Controls.Add(this.groupLocalFile);
            this.Controls.Add(this.radioURL);
            this.Controls.Add(this.radioLocalFile);
            this.Controls.Add(this.groupURL);
            this.Controls.Add(this.chkUpdate);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RemoteExecutionForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "FK远控服务器端 - 远程执行";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RemoteExecutionForm_FormClosing);
            this.Load += new System.EventHandler(this.RemoteExecutionForm_Load);
            this.groupLocalFile.ResumeLayout(false);
            this.groupLocalFile.PerformLayout();
            this.groupURL.ResumeLayout(false);
            this.groupURL.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnExecute;
        private System.Windows.Forms.TextBox txtURL;
        private System.Windows.Forms.Label lblURL;
        private System.Windows.Forms.GroupBox groupLocalFile;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ColumnHeader hClient;
        private System.Windows.Forms.ColumnHeader hStatus;
        private System.Windows.Forms.RadioButton radioURL;
        private System.Windows.Forms.RadioButton radioLocalFile;
        private System.Windows.Forms.GroupBox groupURL;
        private System.Windows.Forms.CheckBox chkUpdate;
        private Controls.FKListView lstTransfers;
        private System.Windows.Forms.ColumnHeader colClient;
        private System.Windows.Forms.ColumnHeader colStatus;
    }
}