namespace FKRemoteDesktop.Forms
{
    partial class SettingForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingForm));
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.settingTabs = new FKRemoteDesktop.Controls.FKBarTabControl();
            this.serverPage = new System.Windows.Forms.TabPage();
            this.chkUseUpnp = new System.Windows.Forms.CheckBox();
            this.chkAutoListen = new System.Windows.Forms.CheckBox();
            this.chkIPv6Support = new System.Windows.Forms.CheckBox();
            this.btnListen = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.ncPort = new System.Windows.Forms.NumericUpDown();
            this.clientPage = new System.Windows.Forms.TabPage();
            this.chkHideFileAndRandomName = new System.Windows.Forms.CheckBox();
            this.chkStartup = new System.Windows.Forms.CheckBox();
            this.chkInstall = new System.Windows.Forms.CheckBox();
            this.chkKeylogger = new System.Windows.Forms.CheckBox();
            this.numericUpDownDelay = new System.Windows.Forms.NumericUpDown();
            this.lblMS = new System.Windows.Forms.Label();
            this.lblDelay = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lstHosts = new System.Windows.Forms.ListBox();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.removeHostToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.numericUpDownPort = new System.Windows.Forms.NumericUpDown();
            this.txtMutex = new System.Windows.Forms.TextBox();
            this.btnAddHost = new System.Windows.Forms.Button();
            this.btnMutex = new System.Windows.Forms.Button();
            this.lblHost = new System.Windows.Forms.Label();
            this.lblMutex = new System.Windows.Forms.Label();
            this.txtHost = new System.Windows.Forms.TextBox();
            this.lblPort = new System.Windows.Forms.Label();
            this.txtTag = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.assemblyPage = new System.Windows.Forms.TabPage();
            this.txtIconPath = new System.Windows.Forms.TextBox();
            this.txtCopyIconInfoPath = new System.Windows.Forms.TextBox();
            this.txtCopyAssemblyInfoPath = new System.Windows.Forms.TextBox();
            this.txtCopySignaturePath = new System.Windows.Forms.TextBox();
            this.btnBrowseAssemblyIcon = new System.Windows.Forms.Button();
            this.btnBrowseAssemblyInfo = new System.Windows.Forms.Button();
            this.btnBrowseSignature = new System.Windows.Forms.Button();
            this.chkCopySignature = new System.Windows.Forms.CheckBox();
            this.txtMofifyTime = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.iconPreview = new System.Windows.Forms.PictureBox();
            this.btnBrowseIcon = new System.Windows.Forms.Button();
            this.chkChangeAsmInfo = new System.Windows.Forms.CheckBox();
            this.txtFileVersion = new System.Windows.Forms.TextBox();
            this.lblProductName = new System.Windows.Forms.Label();
            this.chkChangeIcon = new System.Windows.Forms.CheckBox();
            this.lblFileVersion = new System.Windows.Forms.Label();
            this.txtProductName = new System.Windows.Forms.TextBox();
            this.txtProductVersion = new System.Windows.Forms.TextBox();
            this.lblDescription = new System.Windows.Forms.Label();
            this.lblProductVersion = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.txtOriginalFilename = new System.Windows.Forms.TextBox();
            this.lblCompanyName = new System.Windows.Forms.Label();
            this.lblOriginalFilename = new System.Windows.Forms.Label();
            this.txtCompanyName = new System.Windows.Forms.TextBox();
            this.txtTrademarks = new System.Windows.Forms.TextBox();
            this.lblCopyright = new System.Windows.Forms.Label();
            this.lblTrademarks = new System.Windows.Forms.Label();
            this.txtCopyright = new System.Windows.Forms.TextBox();
            this.settingTabs.SuspendLayout();
            this.serverPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ncPort)).BeginInit();
            this.clientPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDelay)).BeginInit();
            this.contextMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPort)).BeginInit();
            this.assemblyPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.iconPreview)).BeginInit();
            this.SuspendLayout();
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(505, 334);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 1;
            this.btnSave.Text = "保存(&S)";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(415, 334);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "取消(&C)";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // settingTabs
            // 
            this.settingTabs.Alignment = System.Windows.Forms.TabAlignment.Left;
            this.settingTabs.Controls.Add(this.serverPage);
            this.settingTabs.Controls.Add(this.clientPage);
            this.settingTabs.Controls.Add(this.assemblyPage);
            this.settingTabs.Dock = System.Windows.Forms.DockStyle.Top;
            this.settingTabs.ItemSize = new System.Drawing.Size(44, 136);
            this.settingTabs.Location = new System.Drawing.Point(0, 0);
            this.settingTabs.Multiline = true;
            this.settingTabs.Name = "settingTabs";
            this.settingTabs.SelectedIndex = 0;
            this.settingTabs.Size = new System.Drawing.Size(584, 329);
            this.settingTabs.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.settingTabs.TabIndex = 0;
            // 
            // serverPage
            // 
            this.serverPage.Controls.Add(this.chkUseUpnp);
            this.serverPage.Controls.Add(this.chkAutoListen);
            this.serverPage.Controls.Add(this.chkIPv6Support);
            this.serverPage.Controls.Add(this.btnListen);
            this.serverPage.Controls.Add(this.label1);
            this.serverPage.Controls.Add(this.ncPort);
            this.serverPage.Location = new System.Drawing.Point(140, 4);
            this.serverPage.Name = "serverPage";
            this.serverPage.Padding = new System.Windows.Forms.Padding(3);
            this.serverPage.Size = new System.Drawing.Size(440, 321);
            this.serverPage.TabIndex = 0;
            this.serverPage.Text = "服务器配置";
            this.serverPage.UseVisualStyleBackColor = true;
            // 
            // chkUseUpnp
            // 
            this.chkUseUpnp.AutoSize = true;
            this.chkUseUpnp.Location = new System.Drawing.Point(23, 93);
            this.chkUseUpnp.Name = "chkUseUpnp";
            this.chkUseUpnp.Size = new System.Drawing.Size(180, 17);
            this.chkUseUpnp.TabIndex = 9;
            this.chkUseUpnp.Text = "尝试开启 UPnP 自动转发端口";
            this.chkUseUpnp.UseVisualStyleBackColor = true;
            // 
            // chkAutoListen
            // 
            this.chkAutoListen.AutoSize = true;
            this.chkAutoListen.Location = new System.Drawing.Point(23, 70);
            this.chkAutoListen.Name = "chkAutoListen";
            this.chkAutoListen.Size = new System.Drawing.Size(134, 17);
            this.chkAutoListen.TabIndex = 7;
            this.chkAutoListen.Text = "启动时自动开启监听";
            this.chkAutoListen.UseVisualStyleBackColor = true;
            // 
            // chkIPv6Support
            // 
            this.chkIPv6Support.AutoSize = true;
            this.chkIPv6Support.Location = new System.Drawing.Point(23, 47);
            this.chkIPv6Support.Name = "chkIPv6Support";
            this.chkIPv6Support.Size = new System.Drawing.Size(102, 17);
            this.chkIPv6Support.TabIndex = 6;
            this.chkIPv6Support.Text = "开启 IPv6 支持";
            this.chkIPv6Support.UseVisualStyleBackColor = true;
            // 
            // btnListen
            // 
            this.btnListen.Location = new System.Drawing.Point(213, 10);
            this.btnListen.Name = "btnListen";
            this.btnListen.Size = new System.Drawing.Size(110, 23);
            this.btnListen.TabIndex = 4;
            this.btnListen.Text = "开启服务器监听";
            this.btnListen.UseVisualStyleBackColor = true;
            this.btnListen.Click += new System.EventHandler(this.btnListen_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "服务器监听端口";
            // 
            // ncPort
            // 
            this.ncPort.Location = new System.Drawing.Point(117, 12);
            this.ncPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.ncPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.ncPort.Name = "ncPort";
            this.ncPort.Size = new System.Drawing.Size(75, 20);
            this.ncPort.TabIndex = 2;
            this.ncPort.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // clientPage
            // 
            this.clientPage.Controls.Add(this.chkHideFileAndRandomName);
            this.clientPage.Controls.Add(this.chkStartup);
            this.clientPage.Controls.Add(this.chkInstall);
            this.clientPage.Controls.Add(this.chkKeylogger);
            this.clientPage.Controls.Add(this.numericUpDownDelay);
            this.clientPage.Controls.Add(this.lblMS);
            this.clientPage.Controls.Add(this.lblDelay);
            this.clientPage.Controls.Add(this.label2);
            this.clientPage.Controls.Add(this.lstHosts);
            this.clientPage.Controls.Add(this.numericUpDownPort);
            this.clientPage.Controls.Add(this.txtMutex);
            this.clientPage.Controls.Add(this.btnAddHost);
            this.clientPage.Controls.Add(this.btnMutex);
            this.clientPage.Controls.Add(this.lblHost);
            this.clientPage.Controls.Add(this.lblMutex);
            this.clientPage.Controls.Add(this.txtHost);
            this.clientPage.Controls.Add(this.lblPort);
            this.clientPage.Controls.Add(this.txtTag);
            this.clientPage.Controls.Add(this.label7);
            this.clientPage.Location = new System.Drawing.Point(140, 4);
            this.clientPage.Name = "clientPage";
            this.clientPage.Padding = new System.Windows.Forms.Padding(3);
            this.clientPage.Size = new System.Drawing.Size(440, 321);
            this.clientPage.TabIndex = 1;
            this.clientPage.Text = "客户端基本配置";
            this.clientPage.UseVisualStyleBackColor = true;
            // 
            // chkHideFileAndRandomName
            // 
            this.chkHideFileAndRandomName.AutoSize = true;
            this.chkHideFileAndRandomName.Location = new System.Drawing.Point(14, 275);
            this.chkHideFileAndRandomName.Name = "chkHideFileAndRandomName";
            this.chkHideFileAndRandomName.Size = new System.Drawing.Size(287, 17);
            this.chkHideFileAndRandomName.TabIndex = 20;
            this.chkHideFileAndRandomName.Text = "客户端伪装模式 (随机文件夹名，文件名和进程名)";
            this.chkHideFileAndRandomName.UseVisualStyleBackColor = true;
            this.chkHideFileAndRandomName.CheckedChanged += new System.EventHandler(this.chkHideFileAndRandomName_CheckedChanged);
            // 
            // chkStartup
            // 
            this.chkStartup.AutoSize = true;
            this.chkStartup.Location = new System.Drawing.Point(14, 229);
            this.chkStartup.Name = "chkStartup";
            this.chkStartup.Size = new System.Drawing.Size(134, 17);
            this.chkStartup.TabIndex = 19;
            this.chkStartup.Text = "客户端开机自动启动";
            this.chkStartup.UseVisualStyleBackColor = true;
            this.chkStartup.CheckedChanged += new System.EventHandler(this.chkStartup_CheckedChanged);
            // 
            // chkInstall
            // 
            this.chkInstall.AutoSize = true;
            this.chkInstall.Location = new System.Drawing.Point(14, 252);
            this.chkInstall.Name = "chkInstall";
            this.chkInstall.Size = new System.Drawing.Size(299, 17);
            this.chkInstall.TabIndex = 18;
            this.chkInstall.Text = "客户端自动备份 (自动在用户文件夹内进行拷贝备份)";
            this.chkInstall.UseVisualStyleBackColor = true;
            this.chkInstall.CheckedChanged += new System.EventHandler(this.chkInstall_CheckedChanged);
            // 
            // chkKeylogger
            // 
            this.chkKeylogger.AutoSize = true;
            this.chkKeylogger.Location = new System.Drawing.Point(14, 206);
            this.chkKeylogger.Name = "chkKeylogger";
            this.chkKeylogger.Size = new System.Drawing.Size(134, 17);
            this.chkKeylogger.TabIndex = 17;
            this.chkKeylogger.Text = "客户端进行键盘记录";
            this.chkKeylogger.UseVisualStyleBackColor = true;
            this.chkKeylogger.CheckedChanged += new System.EventHandler(this.chkKeylogger_CheckedChanged);
            // 
            // numericUpDownDelay
            // 
            this.numericUpDownDelay.Location = new System.Drawing.Point(215, 96);
            this.numericUpDownDelay.Maximum = new decimal(new int[] {
            600,
            0,
            0,
            0});
            this.numericUpDownDelay.Name = "numericUpDownDelay";
            this.numericUpDownDelay.Size = new System.Drawing.Size(52, 20);
            this.numericUpDownDelay.TabIndex = 15;
            this.numericUpDownDelay.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDownDelay.ValueChanged += new System.EventHandler(this.numericUpDownDelay_ValueChanged);
            // 
            // lblMS
            // 
            this.lblMS.AutoSize = true;
            this.lblMS.Location = new System.Drawing.Point(273, 98);
            this.lblMS.Name = "lblMS";
            this.lblMS.Size = new System.Drawing.Size(19, 13);
            this.lblMS.TabIndex = 16;
            this.lblMS.Text = "秒";
            // 
            // lblDelay
            // 
            this.lblDelay.AutoSize = true;
            this.lblDelay.Location = new System.Drawing.Point(11, 98);
            this.lblDelay.Name = "lblDelay";
            this.lblDelay.Size = new System.Drawing.Size(187, 13);
            this.lblDelay.TabIndex = 14;
            this.lblDelay.Text = "客户端断开后，自动重连间隔时间";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(215, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "服务器列表";
            // 
            // lstHosts
            // 
            this.lstHosts.ContextMenuStrip = this.contextMenuStrip;
            this.lstHosts.FormattingEnabled = true;
            this.lstHosts.Location = new System.Drawing.Point(215, 29);
            this.lstHosts.Name = "lstHosts";
            this.lstHosts.Size = new System.Drawing.Size(219, 56);
            this.lstHosts.TabIndex = 12;
            this.lstHosts.TabStop = false;
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.removeHostToolStripMenuItem,
            this.clearToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(161, 48);
            // 
            // removeHostToolStripMenuItem
            // 
            this.removeHostToolStripMenuItem.Name = "removeHostToolStripMenuItem";
            this.removeHostToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.removeHostToolStripMenuItem.Text = "移除指定服务器";
            this.removeHostToolStripMenuItem.Click += new System.EventHandler(this.removeHostToolStripMenuItem_Click);
            // 
            // clearToolStripMenuItem
            // 
            this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            this.clearToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.clearToolStripMenuItem.Text = "清除全部服务器";
            this.clearToolStripMenuItem.Click += new System.EventHandler(this.clearToolStripMenuItem_Click);
            // 
            // numericUpDownPort
            // 
            this.numericUpDownPort.Location = new System.Drawing.Point(111, 33);
            this.numericUpDownPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numericUpDownPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownPort.Name = "numericUpDownPort";
            this.numericUpDownPort.Size = new System.Drawing.Size(98, 20);
            this.numericUpDownPort.TabIndex = 8;
            this.numericUpDownPort.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // txtMutex
            // 
            this.txtMutex.Location = new System.Drawing.Point(215, 151);
            this.txtMutex.MaxLength = 64;
            this.txtMutex.Name = "txtMutex";
            this.txtMutex.Size = new System.Drawing.Size(219, 20);
            this.txtMutex.TabIndex = 10;
            this.txtMutex.TextChanged += new System.EventHandler(this.txtMutex_TextChanged);
            // 
            // btnAddHost
            // 
            this.btnAddHost.Location = new System.Drawing.Point(80, 63);
            this.btnAddHost.Name = "btnAddHost";
            this.btnAddHost.Size = new System.Drawing.Size(129, 22);
            this.btnAddHost.TabIndex = 9;
            this.btnAddHost.Text = "添加服务器信息";
            this.btnAddHost.UseVisualStyleBackColor = true;
            this.btnAddHost.Click += new System.EventHandler(this.btnAddHost_Click);
            // 
            // btnMutex
            // 
            this.btnMutex.Location = new System.Drawing.Point(318, 179);
            this.btnMutex.Name = "btnMutex";
            this.btnMutex.Size = new System.Drawing.Size(114, 23);
            this.btnMutex.TabIndex = 11;
            this.btnMutex.Text = "生成随机互斥锁";
            this.btnMutex.UseVisualStyleBackColor = true;
            this.btnMutex.Click += new System.EventHandler(this.btnMutex_Click);
            // 
            // lblHost
            // 
            this.lblHost.AutoSize = true;
            this.lblHost.Location = new System.Drawing.Point(11, 10);
            this.lblHost.Name = "lblHost";
            this.lblHost.Size = new System.Drawing.Size(94, 13);
            this.lblHost.TabIndex = 5;
            this.lblHost.Text = "服务器IP/主机名";
            // 
            // lblMutex
            // 
            this.lblMutex.AutoSize = true;
            this.lblMutex.Location = new System.Drawing.Point(11, 154);
            this.lblMutex.Name = "lblMutex";
            this.lblMutex.Size = new System.Drawing.Size(198, 13);
            this.lblMutex.TabIndex = 9;
            this.lblMutex.Text = "互斥锁 (确保PC上仅运行一个客户端)";
            // 
            // txtHost
            // 
            this.txtHost.Location = new System.Drawing.Point(111, 7);
            this.txtHost.Name = "txtHost";
            this.txtHost.Size = new System.Drawing.Size(98, 20);
            this.txtHost.TabIndex = 6;
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(11, 38);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(91, 13);
            this.lblPort.TabIndex = 7;
            this.lblPort.Text = "服务器监听端口";
            // 
            // txtTag
            // 
            this.txtTag.Location = new System.Drawing.Point(215, 125);
            this.txtTag.Name = "txtTag";
            this.txtTag.Size = new System.Drawing.Size(219, 20);
            this.txtTag.TabIndex = 5;
            this.txtTag.TextChanged += new System.EventHandler(this.txtTag_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(11, 128);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(175, 13);
            this.label7.TabIndex = 4;
            this.label7.Text = "客户端分组标签  (人工助记理解)";
            // 
            // assemblyPage
            // 
            this.assemblyPage.Controls.Add(this.txtIconPath);
            this.assemblyPage.Controls.Add(this.txtCopyIconInfoPath);
            this.assemblyPage.Controls.Add(this.txtCopyAssemblyInfoPath);
            this.assemblyPage.Controls.Add(this.txtCopySignaturePath);
            this.assemblyPage.Controls.Add(this.btnBrowseAssemblyIcon);
            this.assemblyPage.Controls.Add(this.btnBrowseAssemblyInfo);
            this.assemblyPage.Controls.Add(this.btnBrowseSignature);
            this.assemblyPage.Controls.Add(this.chkCopySignature);
            this.assemblyPage.Controls.Add(this.txtMofifyTime);
            this.assemblyPage.Controls.Add(this.label3);
            this.assemblyPage.Controls.Add(this.iconPreview);
            this.assemblyPage.Controls.Add(this.btnBrowseIcon);
            this.assemblyPage.Controls.Add(this.chkChangeAsmInfo);
            this.assemblyPage.Controls.Add(this.txtFileVersion);
            this.assemblyPage.Controls.Add(this.lblProductName);
            this.assemblyPage.Controls.Add(this.chkChangeIcon);
            this.assemblyPage.Controls.Add(this.lblFileVersion);
            this.assemblyPage.Controls.Add(this.txtProductName);
            this.assemblyPage.Controls.Add(this.txtProductVersion);
            this.assemblyPage.Controls.Add(this.lblDescription);
            this.assemblyPage.Controls.Add(this.lblProductVersion);
            this.assemblyPage.Controls.Add(this.txtDescription);
            this.assemblyPage.Controls.Add(this.txtOriginalFilename);
            this.assemblyPage.Controls.Add(this.lblCompanyName);
            this.assemblyPage.Controls.Add(this.lblOriginalFilename);
            this.assemblyPage.Controls.Add(this.txtCompanyName);
            this.assemblyPage.Controls.Add(this.txtTrademarks);
            this.assemblyPage.Controls.Add(this.lblCopyright);
            this.assemblyPage.Controls.Add(this.lblTrademarks);
            this.assemblyPage.Controls.Add(this.txtCopyright);
            this.assemblyPage.Location = new System.Drawing.Point(140, 4);
            this.assemblyPage.Name = "assemblyPage";
            this.assemblyPage.Size = new System.Drawing.Size(440, 321);
            this.assemblyPage.TabIndex = 2;
            this.assemblyPage.Text = "客户端进阶配置";
            this.assemblyPage.UseVisualStyleBackColor = true;
            // 
            // txtIconPath
            // 
            this.txtIconPath.Location = new System.Drawing.Point(79, 292);
            this.txtIconPath.Name = "txtIconPath";
            this.txtIconPath.Size = new System.Drawing.Size(213, 20);
            this.txtIconPath.TabIndex = 74;
            this.txtIconPath.TextChanged += new System.EventHandler(this.txtIconPath_TextChanged);
            // 
            // txtCopyIconInfoPath
            // 
            this.txtCopyIconInfoPath.Location = new System.Drawing.Point(79, 266);
            this.txtCopyIconInfoPath.Name = "txtCopyIconInfoPath";
            this.txtCopyIconInfoPath.Size = new System.Drawing.Size(213, 20);
            this.txtCopyIconInfoPath.TabIndex = 73;
            this.txtCopyIconInfoPath.TextChanged += new System.EventHandler(this.txtCopyIconInfoPath_TextChanged);
            // 
            // txtCopyAssemblyInfoPath
            // 
            this.txtCopyAssemblyInfoPath.Location = new System.Drawing.Point(16, 73);
            this.txtCopyAssemblyInfoPath.Name = "txtCopyAssemblyInfoPath";
            this.txtCopyAssemblyInfoPath.Size = new System.Drawing.Size(276, 20);
            this.txtCopyAssemblyInfoPath.TabIndex = 72;
            this.txtCopyAssemblyInfoPath.TextChanged += new System.EventHandler(this.txtCopyAssemblyInfoPath_TextChanged);
            // 
            // txtCopySignaturePath
            // 
            this.txtCopySignaturePath.Location = new System.Drawing.Point(16, 26);
            this.txtCopySignaturePath.Name = "txtCopySignaturePath";
            this.txtCopySignaturePath.Size = new System.Drawing.Size(276, 20);
            this.txtCopySignaturePath.TabIndex = 71;
            this.txtCopySignaturePath.TextChanged += new System.EventHandler(this.txtCopySignaturePath_TextChanged);
            // 
            // btnBrowseAssemblyIcon
            // 
            this.btnBrowseAssemblyIcon.Location = new System.Drawing.Point(310, 263);
            this.btnBrowseAssemblyIcon.Name = "btnBrowseAssemblyIcon";
            this.btnBrowseAssemblyIcon.Size = new System.Drawing.Size(111, 23);
            this.btnBrowseAssemblyIcon.TabIndex = 70;
            this.btnBrowseAssemblyIcon.Text = "窃取目标程序图标";
            this.btnBrowseAssemblyIcon.UseVisualStyleBackColor = true;
            this.btnBrowseAssemblyIcon.Click += new System.EventHandler(this.btnBrowseAssemblyIcon_Click);
            // 
            // btnBrowseAssemblyInfo
            // 
            this.btnBrowseAssemblyInfo.Location = new System.Drawing.Point(310, 70);
            this.btnBrowseAssemblyInfo.Name = "btnBrowseAssemblyInfo";
            this.btnBrowseAssemblyInfo.Size = new System.Drawing.Size(111, 23);
            this.btnBrowseAssemblyInfo.TabIndex = 69;
            this.btnBrowseAssemblyInfo.Text = "窃取目标程序信息";
            this.btnBrowseAssemblyInfo.UseVisualStyleBackColor = true;
            this.btnBrowseAssemblyInfo.Click += new System.EventHandler(this.btnBrowseAssemblyInfo_Click);
            // 
            // btnBrowseSignature
            // 
            this.btnBrowseSignature.Location = new System.Drawing.Point(310, 23);
            this.btnBrowseSignature.Name = "btnBrowseSignature";
            this.btnBrowseSignature.Size = new System.Drawing.Size(111, 23);
            this.btnBrowseSignature.TabIndex = 68;
            this.btnBrowseSignature.Text = "窃取目标签名文件";
            this.btnBrowseSignature.UseVisualStyleBackColor = true;
            this.btnBrowseSignature.Click += new System.EventHandler(this.btnBrowseSignature_Click);
            // 
            // chkCopySignature
            // 
            this.chkCopySignature.AutoSize = true;
            this.chkCopySignature.Location = new System.Drawing.Point(16, 6);
            this.chkCopySignature.Name = "chkCopySignature";
            this.chkCopySignature.Size = new System.Drawing.Size(110, 17);
            this.chkCopySignature.TabIndex = 67;
            this.chkCopySignature.Text = "修改客户端签名";
            this.chkCopySignature.UseVisualStyleBackColor = true;
            this.chkCopySignature.CheckedChanged += new System.EventHandler(this.chkCopySignature_CheckedChanged);
            // 
            // txtMofifyTime
            // 
            this.txtMofifyTime.Location = new System.Drawing.Point(295, 177);
            this.txtMofifyTime.Name = "txtMofifyTime";
            this.txtMofifyTime.Size = new System.Drawing.Size(132, 20);
            this.txtMofifyTime.TabIndex = 66;
            this.txtMofifyTime.TextChanged += new System.EventHandler(this.txtAssemblyInfo_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(221, 180);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 13);
            this.label3.TabIndex = 65;
            this.label3.Text = "修改时间";
            // 
            // iconPreview
            // 
            this.iconPreview.Location = new System.Drawing.Point(16, 264);
            this.iconPreview.Name = "iconPreview";
            this.iconPreview.Size = new System.Drawing.Size(48, 48);
            this.iconPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.iconPreview.TabIndex = 64;
            this.iconPreview.TabStop = false;
            // 
            // btnBrowseIcon
            // 
            this.btnBrowseIcon.Location = new System.Drawing.Point(310, 289);
            this.btnBrowseIcon.Name = "btnBrowseIcon";
            this.btnBrowseIcon.Size = new System.Drawing.Size(111, 23);
            this.btnBrowseIcon.TabIndex = 63;
            this.btnBrowseIcon.Text = "选择 Icon 图标";
            this.btnBrowseIcon.UseVisualStyleBackColor = true;
            this.btnBrowseIcon.Click += new System.EventHandler(this.btnBrowseIcon_Click);
            // 
            // chkChangeAsmInfo
            // 
            this.chkChangeAsmInfo.AutoSize = true;
            this.chkChangeAsmInfo.Location = new System.Drawing.Point(17, 52);
            this.chkChangeAsmInfo.Name = "chkChangeAsmInfo";
            this.chkChangeAsmInfo.Size = new System.Drawing.Size(124, 17);
            this.chkChangeAsmInfo.TabIndex = 43;
            this.chkChangeAsmInfo.Text = "修改 Assembly 信息";
            this.chkChangeAsmInfo.UseVisualStyleBackColor = true;
            this.chkChangeAsmInfo.CheckedChanged += new System.EventHandler(this.chkChangeAsmInfo_CheckedChanged);
            // 
            // txtFileVersion
            // 
            this.txtFileVersion.Location = new System.Drawing.Point(295, 152);
            this.txtFileVersion.Name = "txtFileVersion";
            this.txtFileVersion.Size = new System.Drawing.Size(132, 20);
            this.txtFileVersion.TabIndex = 61;
            this.txtFileVersion.TextChanged += new System.EventHandler(this.txtAssemblyInfo_TextChanged);
            // 
            // lblProductName
            // 
            this.lblProductName.AutoSize = true;
            this.lblProductName.Location = new System.Drawing.Point(13, 108);
            this.lblProductName.Name = "lblProductName";
            this.lblProductName.Size = new System.Drawing.Size(55, 13);
            this.lblProductName.TabIndex = 45;
            this.lblProductName.Text = "产品名称";
            // 
            // chkChangeIcon
            // 
            this.chkChangeIcon.AutoSize = true;
            this.chkChangeIcon.Location = new System.Drawing.Point(17, 241);
            this.chkChangeIcon.Name = "chkChangeIcon";
            this.chkChangeIcon.Size = new System.Drawing.Size(101, 17);
            this.chkChangeIcon.TabIndex = 46;
            this.chkChangeIcon.Text = "修改 Icon 信息";
            this.chkChangeIcon.UseVisualStyleBackColor = true;
            this.chkChangeIcon.CheckedChanged += new System.EventHandler(this.chkChangeIcon_CheckedChanged);
            // 
            // lblFileVersion
            // 
            this.lblFileVersion.AutoSize = true;
            this.lblFileVersion.Location = new System.Drawing.Point(221, 155);
            this.lblFileVersion.Name = "lblFileVersion";
            this.lblFileVersion.Size = new System.Drawing.Size(67, 13);
            this.lblFileVersion.TabIndex = 60;
            this.lblFileVersion.Text = "文件版本号";
            // 
            // txtProductName
            // 
            this.txtProductName.Location = new System.Drawing.Point(79, 105);
            this.txtProductName.Name = "txtProductName";
            this.txtProductName.Size = new System.Drawing.Size(132, 20);
            this.txtProductName.TabIndex = 47;
            this.txtProductName.TextChanged += new System.EventHandler(this.txtAssemblyInfo_TextChanged);
            // 
            // txtProductVersion
            // 
            this.txtProductVersion.Location = new System.Drawing.Point(295, 127);
            this.txtProductVersion.Name = "txtProductVersion";
            this.txtProductVersion.Size = new System.Drawing.Size(132, 20);
            this.txtProductVersion.TabIndex = 59;
            this.txtProductVersion.TextChanged += new System.EventHandler(this.txtAssemblyInfo_TextChanged);
            // 
            // lblDescription
            // 
            this.lblDescription.AutoSize = true;
            this.lblDescription.Location = new System.Drawing.Point(13, 132);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(55, 13);
            this.lblDescription.TabIndex = 48;
            this.lblDescription.Text = "产品描述";
            // 
            // lblProductVersion
            // 
            this.lblProductVersion.AutoSize = true;
            this.lblProductVersion.Location = new System.Drawing.Point(221, 130);
            this.lblProductVersion.Name = "lblProductVersion";
            this.lblProductVersion.Size = new System.Drawing.Size(67, 13);
            this.lblProductVersion.TabIndex = 58;
            this.lblProductVersion.Text = "产品版本号";
            // 
            // txtDescription
            // 
            this.txtDescription.Location = new System.Drawing.Point(79, 129);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(132, 20);
            this.txtDescription.TabIndex = 49;
            this.txtDescription.TextChanged += new System.EventHandler(this.txtAssemblyInfo_TextChanged);
            // 
            // txtOriginalFilename
            // 
            this.txtOriginalFilename.Location = new System.Drawing.Point(295, 102);
            this.txtOriginalFilename.Name = "txtOriginalFilename";
            this.txtOriginalFilename.Size = new System.Drawing.Size(132, 20);
            this.txtOriginalFilename.TabIndex = 57;
            this.txtOriginalFilename.TextChanged += new System.EventHandler(this.txtAssemblyInfo_TextChanged);
            // 
            // lblCompanyName
            // 
            this.lblCompanyName.AutoSize = true;
            this.lblCompanyName.Location = new System.Drawing.Point(13, 157);
            this.lblCompanyName.Name = "lblCompanyName";
            this.lblCompanyName.Size = new System.Drawing.Size(55, 13);
            this.lblCompanyName.TabIndex = 50;
            this.lblCompanyName.Text = "公司名字";
            // 
            // lblOriginalFilename
            // 
            this.lblOriginalFilename.AutoSize = true;
            this.lblOriginalFilename.Location = new System.Drawing.Point(221, 105);
            this.lblOriginalFilename.Name = "lblOriginalFilename";
            this.lblOriginalFilename.Size = new System.Drawing.Size(67, 13);
            this.lblOriginalFilename.TabIndex = 56;
            this.lblOriginalFilename.Text = "原始产品名";
            // 
            // txtCompanyName
            // 
            this.txtCompanyName.Location = new System.Drawing.Point(79, 154);
            this.txtCompanyName.Name = "txtCompanyName";
            this.txtCompanyName.Size = new System.Drawing.Size(132, 20);
            this.txtCompanyName.TabIndex = 51;
            this.txtCompanyName.TextChanged += new System.EventHandler(this.txtAssemblyInfo_TextChanged);
            // 
            // txtTrademarks
            // 
            this.txtTrademarks.Location = new System.Drawing.Point(79, 204);
            this.txtTrademarks.Name = "txtTrademarks";
            this.txtTrademarks.Size = new System.Drawing.Size(132, 20);
            this.txtTrademarks.TabIndex = 55;
            this.txtTrademarks.TextChanged += new System.EventHandler(this.txtAssemblyInfo_TextChanged);
            // 
            // lblCopyright
            // 
            this.lblCopyright.AutoSize = true;
            this.lblCopyright.Location = new System.Drawing.Point(13, 182);
            this.lblCopyright.Name = "lblCopyright";
            this.lblCopyright.Size = new System.Drawing.Size(55, 13);
            this.lblCopyright.TabIndex = 52;
            this.lblCopyright.Text = "版权信息";
            // 
            // lblTrademarks
            // 
            this.lblTrademarks.AutoSize = true;
            this.lblTrademarks.Location = new System.Drawing.Point(13, 207);
            this.lblTrademarks.Name = "lblTrademarks";
            this.lblTrademarks.Size = new System.Drawing.Size(55, 13);
            this.lblTrademarks.TabIndex = 54;
            this.lblTrademarks.Text = "商标信息";
            // 
            // txtCopyright
            // 
            this.txtCopyright.Location = new System.Drawing.Point(79, 179);
            this.txtCopyright.Name = "txtCopyright";
            this.txtCopyright.Size = new System.Drawing.Size(132, 20);
            this.txtCopyright.TabIndex = 53;
            this.txtCopyright.TextChanged += new System.EventHandler(this.txtAssemblyInfo_TextChanged);
            // 
            // SettingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 361);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.settingTabs);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "FK远控服务器端 - 设置";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingForm_FormClosing);
            this.Load += new System.EventHandler(this.SettingForm_Load);
            this.settingTabs.ResumeLayout(false);
            this.serverPage.ResumeLayout(false);
            this.serverPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ncPort)).EndInit();
            this.clientPage.ResumeLayout(false);
            this.clientPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDelay)).EndInit();
            this.contextMenuStrip.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPort)).EndInit();
            this.assemblyPage.ResumeLayout(false);
            this.assemblyPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.iconPreview)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.FKBarTabControl settingTabs;
        private System.Windows.Forms.TabPage serverPage;
        private System.Windows.Forms.TabPage clientPage;
        private System.Windows.Forms.TabPage assemblyPage;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown ncPort;
        private System.Windows.Forms.Button btnListen;
        private System.Windows.Forms.CheckBox chkIPv6Support;
        private System.Windows.Forms.CheckBox chkAutoListen;
        private System.Windows.Forms.CheckBox chkUseUpnp;
        private System.Windows.Forms.TextBox txtTag;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtMutex;
        private System.Windows.Forms.Button btnMutex;
        private System.Windows.Forms.Label lblMutex;
        private System.Windows.Forms.ListBox lstHosts;
        private System.Windows.Forms.NumericUpDown numericUpDownPort;
        private System.Windows.Forms.Button btnAddHost;
        private System.Windows.Forms.Label lblHost;
        private System.Windows.Forms.TextBox txtHost;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericUpDownDelay;
        private System.Windows.Forms.Label lblMS;
        private System.Windows.Forms.Label lblDelay;
        private System.Windows.Forms.CheckBox chkKeylogger;
        private System.Windows.Forms.CheckBox chkStartup;
        private System.Windows.Forms.CheckBox chkInstall;
        private System.Windows.Forms.CheckBox chkHideFileAndRandomName;
        private System.Windows.Forms.PictureBox iconPreview;
        private System.Windows.Forms.Button btnBrowseIcon;
        private System.Windows.Forms.CheckBox chkChangeAsmInfo;
        private System.Windows.Forms.TextBox txtFileVersion;
        private System.Windows.Forms.Label lblProductName;
        private System.Windows.Forms.CheckBox chkChangeIcon;
        private System.Windows.Forms.Label lblFileVersion;
        private System.Windows.Forms.TextBox txtProductName;
        private System.Windows.Forms.TextBox txtProductVersion;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Label lblProductVersion;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.TextBox txtOriginalFilename;
        private System.Windows.Forms.Label lblCompanyName;
        private System.Windows.Forms.Label lblOriginalFilename;
        private System.Windows.Forms.TextBox txtCompanyName;
        private System.Windows.Forms.TextBox txtTrademarks;
        private System.Windows.Forms.Label lblCopyright;
        private System.Windows.Forms.Label lblTrademarks;
        private System.Windows.Forms.TextBox txtCopyright;
        private System.Windows.Forms.TextBox txtMofifyTime;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox chkCopySignature;
        private System.Windows.Forms.Button btnBrowseSignature;
        private System.Windows.Forms.Button btnBrowseAssemblyInfo;
        private System.Windows.Forms.Button btnBrowseAssemblyIcon;
        private System.Windows.Forms.TextBox txtCopySignaturePath;
        private System.Windows.Forms.TextBox txtCopyAssemblyInfoPath;
        private System.Windows.Forms.TextBox txtCopyIconInfoPath;
        private System.Windows.Forms.TextBox txtIconPath;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem removeHostToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
    }
}