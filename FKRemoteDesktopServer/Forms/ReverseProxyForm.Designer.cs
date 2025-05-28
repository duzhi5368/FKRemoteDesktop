namespace FKRemoteDesktop.Forms
{
    partial class ReverseProxyForm
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
            this.components = new System.ComponentModel.Container();
            FKRemoteDesktop.Controls.ListViewColumnSorter listViewColumnSorter1 = new FKRemoteDesktop.Controls.ListViewColumnSorter();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReverseProxyForm));
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.killConnectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnStop = new System.Windows.Forms.Button();
            this.nudServerPort = new System.Windows.Forms.NumericUpDown();
            this.lblLocalServerPort = new System.Windows.Forms.Label();
            this.btnStart = new System.Windows.Forms.Button();
            this.rtbLog = new System.Windows.Forms.RichTextBox();
            this.tabCtrl = new System.Windows.Forms.TabControl();
            this.tabPageConnections = new System.Windows.Forms.TabPage();
            this.lstConnections = new FKRemoteDesktop.Controls.FKListView();
            this.colClientIP = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colClientCountry = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colServerIP = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colServerPort = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colTotalReceived = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colTotalSent = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colProxyType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudServerPort)).BeginInit();
            this.tabCtrl.SuspendLayout();
            this.tabPageConnections.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.killConnectionToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(181, 48);
            // 
            // killConnectionToolStripMenuItem
            // 
            this.killConnectionToolStripMenuItem.Name = "killConnectionToolStripMenuItem";
            this.killConnectionToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.killConnectionToolStripMenuItem.Text = "断开连接";
            this.killConnectionToolStripMenuItem.Click += new System.EventHandler(this.killConnectionToolStripMenuItem_Click);
            // 
            // btnStop
            // 
            this.btnStop.Enabled = false;
            this.btnStop.Location = new System.Drawing.Point(420, 9);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(100, 23);
            this.btnStop.TabIndex = 8;
            this.btnStop.Text = "停止监听";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // nudServerPort
            // 
            this.nudServerPort.Location = new System.Drawing.Point(190, 12);
            this.nudServerPort.Maximum = new decimal(new int[] {
            65534,
            0,
            0,
            0});
            this.nudServerPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudServerPort.Name = "nudServerPort";
            this.nudServerPort.Size = new System.Drawing.Size(69, 20);
            this.nudServerPort.TabIndex = 7;
            this.nudServerPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.nudServerPort.Value = new decimal(new int[] {
            3128,
            0,
            0,
            0});
            this.nudServerPort.ValueChanged += new System.EventHandler(this.nudServerPort_ValueChanged);
            // 
            // lblLocalServerPort
            // 
            this.lblLocalServerPort.AutoSize = true;
            this.lblLocalServerPort.Location = new System.Drawing.Point(93, 14);
            this.lblLocalServerPort.Name = "lblLocalServerPort";
            this.lblLocalServerPort.Size = new System.Drawing.Size(91, 13);
            this.lblLocalServerPort.TabIndex = 6;
            this.lblLocalServerPort.Text = "本地服务器端口";
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(314, 9);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(100, 23);
            this.btnStart.TabIndex = 5;
            this.btnStart.Text = "开启监听";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // rtbLog
            // 
            this.rtbLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbLog.Location = new System.Drawing.Point(12, 44);
            this.rtbLog.Name = "rtbLog";
            this.rtbLog.ReadOnly = true;
            this.rtbLog.Size = new System.Drawing.Size(605, 83);
            this.rtbLog.TabIndex = 9;
            this.rtbLog.Text = "";
            // 
            // tabCtrl
            // 
            this.tabCtrl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabCtrl.Controls.Add(this.tabPageConnections);
            this.tabCtrl.Location = new System.Drawing.Point(12, 143);
            this.tabCtrl.Name = "tabCtrl";
            this.tabCtrl.SelectedIndex = 0;
            this.tabCtrl.Size = new System.Drawing.Size(605, 256);
            this.tabCtrl.TabIndex = 10;
            // 
            // tabPageConnections
            // 
            this.tabPageConnections.Controls.Add(this.lstConnections);
            this.tabPageConnections.Location = new System.Drawing.Point(4, 22);
            this.tabPageConnections.Name = "tabPageConnections";
            this.tabPageConnections.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageConnections.Size = new System.Drawing.Size(597, 230);
            this.tabPageConnections.TabIndex = 0;
            this.tabPageConnections.Text = "连接列表";
            this.tabPageConnections.UseVisualStyleBackColor = true;
            // 
            // lstConnections
            // 
            this.lstConnections.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colClientIP,
            this.colClientCountry,
            this.colServerIP,
            this.colServerPort,
            this.colTotalReceived,
            this.colTotalSent,
            this.colProxyType});
            this.lstConnections.ContextMenuStrip = this.contextMenuStrip;
            this.lstConnections.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstConnections.FullRowSelect = true;
            this.lstConnections.GridLines = true;
            this.lstConnections.HideSelection = false;
            listViewColumnSorter1.NeedNumberCompare = false;
            listViewColumnSorter1.Order = System.Windows.Forms.SortOrder.None;
            listViewColumnSorter1.SortColumn = 0;
            this.lstConnections.ListViewColumnSorter = listViewColumnSorter1;
            this.lstConnections.Location = new System.Drawing.Point(3, 3);
            this.lstConnections.Name = "lstConnections";
            this.lstConnections.Size = new System.Drawing.Size(591, 224);
            this.lstConnections.TabIndex = 0;
            this.lstConnections.UseCompatibleStateImageBehavior = false;
            this.lstConnections.View = System.Windows.Forms.View.Details;
            this.lstConnections.VirtualMode = true;
            this.lstConnections.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.lstConnections_RetrieveVirtualItem);
            // 
            // colClientIP
            // 
            this.colClientIP.Text = "客户所在IP";
            this.colClientIP.Width = 95;
            // 
            // colClientCountry
            // 
            this.colClientCountry.Text = "客户所属国家";
            this.colClientCountry.Width = 85;
            // 
            // colServerIP
            // 
            this.colServerIP.Text = "目标服务器IP";
            this.colServerIP.Width = 95;
            // 
            // colServerPort
            // 
            this.colServerPort.Text = "服务器端口";
            this.colServerPort.Width = 75;
            // 
            // colTotalReceived
            // 
            this.colTotalReceived.Text = "总接收量";
            this.colTotalReceived.Width = 80;
            // 
            // colTotalSent
            // 
            this.colTotalSent.Text = "总发送量";
            this.colTotalSent.Width = 80;
            // 
            // colProxyType
            // 
            this.colProxyType.Text = "代理模式";
            // 
            // ReverseProxyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(629, 411);
            this.Controls.Add(this.tabCtrl);
            this.Controls.Add(this.rtbLog);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.nudServerPort);
            this.Controls.Add(this.lblLocalServerPort);
            this.Controls.Add(this.btnStart);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ReverseProxyForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "FK远控服务器端 - 反向代理窗口";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ReverseProxyForm_FormClosing);
            this.Load += new System.EventHandler(this.ReverseProxyForm_Load);
            this.contextMenuStrip.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nudServerPort)).EndInit();
            this.tabCtrl.ResumeLayout(false);
            this.tabPageConnections.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.NumericUpDown nudServerPort;
        private System.Windows.Forms.Label lblLocalServerPort;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.RichTextBox rtbLog;
        private System.Windows.Forms.TabControl tabCtrl;
        private System.Windows.Forms.TabPage tabPageConnections;
        private Controls.FKListView lstConnections;
        private System.Windows.Forms.ColumnHeader colClientIP;
        private System.Windows.Forms.ColumnHeader colClientCountry;
        private System.Windows.Forms.ColumnHeader colServerIP;
        private System.Windows.Forms.ColumnHeader colServerPort;
        private System.Windows.Forms.ColumnHeader colTotalReceived;
        private System.Windows.Forms.ColumnHeader colTotalSent;
        private System.Windows.Forms.ColumnHeader colProxyType;
        private System.Windows.Forms.ToolStripMenuItem killConnectionToolStripMenuItem;
    }
}