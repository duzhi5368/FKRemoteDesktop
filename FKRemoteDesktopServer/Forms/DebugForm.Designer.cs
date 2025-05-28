namespace FKRemoteDesktop.Forms
{
    partial class DebugForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DebugForm));
            this.btnSendEmptyMessage = new System.Windows.Forms.Button();
            this.btnTestMessage = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnSendEmptyMessage
            // 
            this.btnSendEmptyMessage.Location = new System.Drawing.Point(12, 12);
            this.btnSendEmptyMessage.Name = "btnSendEmptyMessage";
            this.btnSendEmptyMessage.Size = new System.Drawing.Size(161, 23);
            this.btnSendEmptyMessage.TabIndex = 0;
            this.btnSendEmptyMessage.Text = "测试 TestEmptyMessage";
            this.btnSendEmptyMessage.UseVisualStyleBackColor = true;
            this.btnSendEmptyMessage.Click += new System.EventHandler(this.btnSendEmptyMessage_Click);
            // 
            // btnTestMessage
            // 
            this.btnTestMessage.Location = new System.Drawing.Point(192, 12);
            this.btnTestMessage.Name = "btnTestMessage";
            this.btnTestMessage.Size = new System.Drawing.Size(161, 23);
            this.btnTestMessage.TabIndex = 1;
            this.btnTestMessage.Text = "测试 TestMessage";
            this.btnTestMessage.UseVisualStyleBackColor = true;
            this.btnTestMessage.Click += new System.EventHandler(this.btnTestMessage_Click);
            // 
            // DebugForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(363, 346);
            this.Controls.Add(this.btnTestMessage);
            this.Controls.Add(this.btnSendEmptyMessage);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DebugForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "FK远控服务器端 - DEBUG面板";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnSendEmptyMessage;
        private System.Windows.Forms.Button btnTestMessage;
    }
}