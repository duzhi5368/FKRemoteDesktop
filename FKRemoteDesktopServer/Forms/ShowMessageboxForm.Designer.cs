namespace FKRemoteDesktop.Forms
{
    partial class ShowMessageboxForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShowMessageboxForm));
            this.groupMsgSettings = new System.Windows.Forms.GroupBox();
            this.cmbMsgIcon = new System.Windows.Forms.ComboBox();
            this.lblMsgIcon = new System.Windows.Forms.Label();
            this.cmbMsgButtons = new System.Windows.Forms.ComboBox();
            this.lblMsgButtons = new System.Windows.Forms.Label();
            this.txtText = new System.Windows.Forms.TextBox();
            this.txtCaption = new System.Windows.Forms.TextBox();
            this.lblText = new System.Windows.Forms.Label();
            this.lblCaption = new System.Windows.Forms.Label();
            this.btnSend = new System.Windows.Forms.Button();
            this.btnPreview = new System.Windows.Forms.Button();
            this.groupMsgSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupMsgSettings
            // 
            this.groupMsgSettings.Controls.Add(this.cmbMsgIcon);
            this.groupMsgSettings.Controls.Add(this.lblMsgIcon);
            this.groupMsgSettings.Controls.Add(this.cmbMsgButtons);
            this.groupMsgSettings.Controls.Add(this.lblMsgButtons);
            this.groupMsgSettings.Controls.Add(this.txtText);
            this.groupMsgSettings.Controls.Add(this.txtCaption);
            this.groupMsgSettings.Controls.Add(this.lblText);
            this.groupMsgSettings.Controls.Add(this.lblCaption);
            this.groupMsgSettings.Location = new System.Drawing.Point(12, 12);
            this.groupMsgSettings.Name = "groupMsgSettings";
            this.groupMsgSettings.Size = new System.Drawing.Size(325, 146);
            this.groupMsgSettings.TabIndex = 3;
            this.groupMsgSettings.TabStop = false;
            this.groupMsgSettings.Text = "设置";
            // 
            // cmbMsgIcon
            // 
            this.cmbMsgIcon.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMsgIcon.FormattingEnabled = true;
            this.cmbMsgIcon.Location = new System.Drawing.Point(147, 107);
            this.cmbMsgIcon.Name = "cmbMsgIcon";
            this.cmbMsgIcon.Size = new System.Drawing.Size(162, 21);
            this.cmbMsgIcon.TabIndex = 8;
            // 
            // lblMsgIcon
            // 
            this.lblMsgIcon.AutoSize = true;
            this.lblMsgIcon.Location = new System.Drawing.Point(6, 110);
            this.lblMsgIcon.Name = "lblMsgIcon";
            this.lblMsgIcon.Size = new System.Drawing.Size(58, 13);
            this.lblMsgIcon.TabIndex = 7;
            this.lblMsgIcon.Text = "窗口 Icon:";
            // 
            // cmbMsgButtons
            // 
            this.cmbMsgButtons.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMsgButtons.FormattingEnabled = true;
            this.cmbMsgButtons.Location = new System.Drawing.Point(147, 80);
            this.cmbMsgButtons.Name = "cmbMsgButtons";
            this.cmbMsgButtons.Size = new System.Drawing.Size(162, 21);
            this.cmbMsgButtons.TabIndex = 6;
            // 
            // lblMsgButtons
            // 
            this.lblMsgButtons.AutoSize = true;
            this.lblMsgButtons.Location = new System.Drawing.Point(6, 83);
            this.lblMsgButtons.Name = "lblMsgButtons";
            this.lblMsgButtons.Size = new System.Drawing.Size(82, 13);
            this.lblMsgButtons.TabIndex = 5;
            this.lblMsgButtons.Text = "窗口按钮类型:";
            // 
            // txtText
            // 
            this.txtText.Location = new System.Drawing.Point(94, 49);
            this.txtText.MaxLength = 256;
            this.txtText.Name = "txtText";
            this.txtText.Size = new System.Drawing.Size(215, 20);
            this.txtText.TabIndex = 4;
            this.txtText.Text = "你正在使用 FK 远控。";
            // 
            // txtCaption
            // 
            this.txtCaption.Location = new System.Drawing.Point(94, 21);
            this.txtCaption.MaxLength = 256;
            this.txtCaption.Name = "txtCaption";
            this.txtCaption.Size = new System.Drawing.Size(215, 20);
            this.txtCaption.TabIndex = 2;
            this.txtCaption.Text = "提示";
            // 
            // lblText
            // 
            this.lblText.AutoSize = true;
            this.lblText.Location = new System.Drawing.Point(6, 52);
            this.lblText.Name = "lblText";
            this.lblText.Size = new System.Drawing.Size(82, 13);
            this.lblText.TabIndex = 3;
            this.lblText.Text = "弹出窗口内容:";
            // 
            // lblCaption
            // 
            this.lblCaption.AutoSize = true;
            this.lblCaption.Location = new System.Drawing.Point(6, 24);
            this.lblCaption.Name = "lblCaption";
            this.lblCaption.Size = new System.Drawing.Size(82, 13);
            this.lblCaption.TabIndex = 1;
            this.lblCaption.Text = "弹出窗口标题:";
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(250, 164);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(87, 23);
            this.btnSend.TabIndex = 5;
            this.btnSend.Text = "发送至客户端";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // btnPreview
            // 
            this.btnPreview.Location = new System.Drawing.Point(159, 164);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Size = new System.Drawing.Size(75, 23);
            this.btnPreview.TabIndex = 4;
            this.btnPreview.Text = "本地预览";
            this.btnPreview.UseVisualStyleBackColor = true;
            this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
            // 
            // ShowMessageboxForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(353, 193);
            this.Controls.Add(this.groupMsgSettings);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.btnPreview);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ShowMessageboxForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "FK远控服务器端 - 消息弹出框";
            this.Load += new System.EventHandler(this.ShowMessageboxForm_Load);
            this.groupMsgSettings.ResumeLayout(false);
            this.groupMsgSettings.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupMsgSettings;
        private System.Windows.Forms.ComboBox cmbMsgIcon;
        private System.Windows.Forms.Label lblMsgIcon;
        private System.Windows.Forms.ComboBox cmbMsgButtons;
        private System.Windows.Forms.Label lblMsgButtons;
        private System.Windows.Forms.TextBox txtText;
        private System.Windows.Forms.TextBox txtCaption;
        private System.Windows.Forms.Label lblText;
        private System.Windows.Forms.Label lblCaption;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Button btnPreview;
    }
}