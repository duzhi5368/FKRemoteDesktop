namespace FKRemoteDesktop.Forms
{
    partial class RegistryEditorForm
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
            FKRemoteDesktop.Controls.ListViewColumnSorter listViewColumnSorter14 = new FKRemoteDesktop.Controls.ListViewColumnSorter();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RegistryEditorForm));
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.selectedStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.modifyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.modifyBinaryDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.keyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.stringValueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.binaryValueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dword32bitValueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.qword64bitValueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.multiStringValueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.expandableStringValueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.tvRegistryDirectory = new FKRemoteDesktop.Controls.RegistryTreeView();
            this.lstRegistryValues = new FKRemoteDesktop.Controls.FKListView();
            this.colName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tv_ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.newToolStripMenuItem_tv = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteToolStripMenuItem_tv = new System.Windows.Forms.ToolStripMenuItem();
            this.renameToolStripMenuItem_tv = new System.Windows.Forms.ToolStripMenuItem();
            this.selectedItem_ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.modifyToolStripMenuItem_si = new System.Windows.Forms.ToolStripMenuItem();
            this.modifyBinaryDataToolStripMenuItem_si = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteToolStripMenuItem_si = new System.Windows.Forms.ToolStripMenuItem();
            this.renameToolStripMenuItem_si = new System.Windows.Forms.ToolStripMenuItem();
            this.lst_ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.newToolStripMenuItem_c = new System.Windows.Forms.ToolStripMenuItem();
            this.keyToolStripMenuItem_c = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.stringValueToolStripMenuItem_c = new System.Windows.Forms.ToolStripMenuItem();
            this.binaryValueToolStripMenuItem_c = new System.Windows.Forms.ToolStripMenuItem();
            this.dword32bitValueToolStripMenuItem_c = new System.Windows.Forms.ToolStripMenuItem();
            this.qword64bitValueToolStripMenuItem_c = new System.Windows.Forms.ToolStripMenuItem();
            this.multiStringValueToolStripMenuItem_c = new System.Windows.Forms.ToolStripMenuItem();
            this.expandableStringValueToolStripMenuItem_c = new System.Windows.Forms.ToolStripMenuItem();
            this.keyToolStripMenuItem_tv = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.stringValueToolStripMenuItem_tv = new System.Windows.Forms.ToolStripMenuItem();
            this.binaryValueToolStripMenuItem_tv = new System.Windows.Forms.ToolStripMenuItem();
            this.dword32bitValueToolStripMenuItem_tv = new System.Windows.Forms.ToolStripMenuItem();
            this.qword64bitValueToolStripMenuItem_tv = new System.Windows.Forms.ToolStripMenuItem();
            this.multiStringValueToolStripMenuItem_tv = new System.Windows.Forms.ToolStripMenuItem();
            this.expandableStringValueToolStripMenuItem_tv = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip.SuspendLayout();
            this.menuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.tv_ContextMenuStrip.SuspendLayout();
            this.selectedItem_ContextMenuStrip.SuspendLayout();
            this.lst_ContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectedStripStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 428);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(804, 22);
            this.statusStrip.TabIndex = 0;
            // 
            // selectedStripStatusLabel
            // 
            this.selectedStripStatusLabel.Name = "selectedStripStatusLabel";
            this.selectedStripStatusLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(804, 25);
            this.menuStrip.TabIndex = 1;
            this.menuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(58, 21);
            this.fileToolStripMenuItem.Text = "文件(&F)";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.exitToolStripMenuItem.Text = "退出(&X)";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.modifyToolStripMenuItem,
            this.modifyBinaryDataToolStripMenuItem,
            this.toolStripSeparator1,
            this.newToolStripMenuItem,
            this.toolStripSeparator2,
            this.deleteToolStripMenuItem,
            this.renameToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(59, 21);
            this.editToolStripMenuItem.Text = "编辑(&E)";
            this.editToolStripMenuItem.DropDownOpening += new System.EventHandler(this.editToolStripMenuItem_DropDownOpening);
            // 
            // modifyToolStripMenuItem
            // 
            this.modifyToolStripMenuItem.Name = "modifyToolStripMenuItem";
            this.modifyToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.modifyToolStripMenuItem.Text = "修改";
            this.modifyToolStripMenuItem.Click += new System.EventHandler(this.modifyToolStripMenuItem_Click);
            // 
            // modifyBinaryDataToolStripMenuItem
            // 
            this.modifyBinaryDataToolStripMenuItem.Enabled = false;
            this.modifyBinaryDataToolStripMenuItem.Name = "modifyBinaryDataToolStripMenuItem";
            this.modifyBinaryDataToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.modifyBinaryDataToolStripMenuItem.Text = "修改二进制数据";
            this.modifyBinaryDataToolStripMenuItem.Click += new System.EventHandler(this.modifyBinaryDataToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(177, 6);
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.keyToolStripMenuItem,
            this.toolStripSeparator3,
            this.stringValueToolStripMenuItem,
            this.binaryValueToolStripMenuItem,
            this.dword32bitValueToolStripMenuItem,
            this.qword64bitValueToolStripMenuItem,
            this.multiStringValueToolStripMenuItem,
            this.expandableStringValueToolStripMenuItem});
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.newToolStripMenuItem.Text = "新建";
            // 
            // keyToolStripMenuItem
            // 
            this.keyToolStripMenuItem.Name = "keyToolStripMenuItem";
            this.keyToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.keyToolStripMenuItem.Text = "键";
            this.keyToolStripMenuItem.Click += new System.EventHandler(this.keyToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(177, 6);
            // 
            // stringValueToolStripMenuItem
            // 
            this.stringValueToolStripMenuItem.Name = "stringValueToolStripMenuItem";
            this.stringValueToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.stringValueToolStripMenuItem.Text = "字符串值";
            this.stringValueToolStripMenuItem.Click += new System.EventHandler(this.stringValueToolStripMenuItem_Click);
            // 
            // binaryValueToolStripMenuItem
            // 
            this.binaryValueToolStripMenuItem.Name = "binaryValueToolStripMenuItem";
            this.binaryValueToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.binaryValueToolStripMenuItem.Text = "二进制值";
            this.binaryValueToolStripMenuItem.Click += new System.EventHandler(this.binaryValueToolStripMenuItem_Click);
            // 
            // dword32bitValueToolStripMenuItem
            // 
            this.dword32bitValueToolStripMenuItem.Name = "dword32bitValueToolStripMenuItem";
            this.dword32bitValueToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.dword32bitValueToolStripMenuItem.Text = "32位DWORD值";
            this.dword32bitValueToolStripMenuItem.Click += new System.EventHandler(this.dword32bitValueToolStripMenuItem_Click);
            // 
            // qword64bitValueToolStripMenuItem
            // 
            this.qword64bitValueToolStripMenuItem.Name = "qword64bitValueToolStripMenuItem";
            this.qword64bitValueToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.qword64bitValueToolStripMenuItem.Text = "64位QWORD值";
            this.qword64bitValueToolStripMenuItem.Click += new System.EventHandler(this.qword64bitValueToolStripMenuItem_Click);
            // 
            // multiStringValueToolStripMenuItem
            // 
            this.multiStringValueToolStripMenuItem.Name = "multiStringValueToolStripMenuItem";
            this.multiStringValueToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.multiStringValueToolStripMenuItem.Text = "多字符串值";
            this.multiStringValueToolStripMenuItem.Click += new System.EventHandler(this.multiStringValueToolStripMenuItem_Click);
            // 
            // expandableStringValueToolStripMenuItem
            // 
            this.expandableStringValueToolStripMenuItem.Name = "expandableStringValueToolStripMenuItem";
            this.expandableStringValueToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.expandableStringValueToolStripMenuItem.Text = "可扩展字符串值";
            this.expandableStringValueToolStripMenuItem.Click += new System.EventHandler(this.expandableStringValueToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(177, 6);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Enabled = false;
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.deleteToolStripMenuItem.Text = "删除";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // renameToolStripMenuItem
            // 
            this.renameToolStripMenuItem.Enabled = false;
            this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
            this.renameToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.renameToolStripMenuItem.Text = "重命名";
            this.renameToolStripMenuItem.Click += new System.EventHandler(this.renameToolStripMenuItem_Click);
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 25);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.tvRegistryDirectory);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.lstRegistryValues);
            this.splitContainer.Size = new System.Drawing.Size(804, 403);
            this.splitContainer.SplitterDistance = 268;
            this.splitContainer.TabIndex = 2;
            // 
            // tvRegistryDirectory
            // 
            this.tvRegistryDirectory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvRegistryDirectory.HideSelection = false;
            this.tvRegistryDirectory.Location = new System.Drawing.Point(0, 0);
            this.tvRegistryDirectory.Name = "tvRegistryDirectory";
            this.tvRegistryDirectory.Size = new System.Drawing.Size(268, 403);
            this.tvRegistryDirectory.TabIndex = 0;
            this.tvRegistryDirectory.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.tvRegistryDirectory_AfterLabelEdit);
            this.tvRegistryDirectory.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.tvRegistryDirectory_BeforeExpand);
            this.tvRegistryDirectory.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.tvRegistryDirectory_BeforeSelect);
            this.tvRegistryDirectory.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.tvRegistryDirectory_NodeMouseClick);
            this.tvRegistryDirectory.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tvRegistryDirectory_KeyUp);
            // 
            // lstRegistryValues
            // 
            this.lstRegistryValues.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colName,
            this.colType,
            this.colValue});
            this.lstRegistryValues.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstRegistryValues.FullRowSelect = true;
            this.lstRegistryValues.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lstRegistryValues.HideSelection = false;
            listViewColumnSorter14.NeedNumberCompare = false;
            listViewColumnSorter14.Order = System.Windows.Forms.SortOrder.None;
            listViewColumnSorter14.SortColumn = 0;
            this.lstRegistryValues.ListViewColumnSorter = listViewColumnSorter14;
            this.lstRegistryValues.Location = new System.Drawing.Point(0, 0);
            this.lstRegistryValues.Name = "lstRegistryValues";
            this.lstRegistryValues.Size = new System.Drawing.Size(532, 403);
            this.lstRegistryValues.TabIndex = 0;
            this.lstRegistryValues.UseCompatibleStateImageBehavior = false;
            this.lstRegistryValues.View = System.Windows.Forms.View.Details;
            this.lstRegistryValues.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.lstRegistryValues_AfterLabelEdit);
            this.lstRegistryValues.KeyUp += new System.Windows.Forms.KeyEventHandler(this.lstRegistryValues_KeyUp);
            this.lstRegistryValues.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lstRegistryValues_MouseUp);
            // 
            // colName
            // 
            this.colName.Text = "注册表名";
            this.colName.Width = 150;
            // 
            // colType
            // 
            this.colType.Text = "值类型";
            this.colType.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.colType.Width = 100;
            // 
            // colValue
            // 
            this.colValue.Text = "值";
            this.colValue.Width = 260;
            // 
            // tv_ContextMenuStrip
            // 
            this.tv_ContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem_tv,
            this.toolStripSeparator4,
            this.deleteToolStripMenuItem_tv,
            this.renameToolStripMenuItem_tv});
            this.tv_ContextMenuStrip.Name = "tv_ContextMenuStrip";
            this.tv_ContextMenuStrip.Size = new System.Drawing.Size(113, 76);
            // 
            // newToolStripMenuItem_tv
            // 
            this.newToolStripMenuItem_tv.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.keyToolStripMenuItem_tv,
            this.toolStripSeparator7,
            this.stringValueToolStripMenuItem_tv,
            this.binaryValueToolStripMenuItem_tv,
            this.dword32bitValueToolStripMenuItem_tv,
            this.qword64bitValueToolStripMenuItem_tv,
            this.multiStringValueToolStripMenuItem_tv,
            this.expandableStringValueToolStripMenuItem_tv});
            this.newToolStripMenuItem_tv.Name = "newToolStripMenuItem_tv";
            this.newToolStripMenuItem_tv.Size = new System.Drawing.Size(180, 22);
            this.newToolStripMenuItem_tv.Text = "新建";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(109, 6);
            // 
            // deleteToolStripMenuItem_tv
            // 
            this.deleteToolStripMenuItem_tv.Enabled = false;
            this.deleteToolStripMenuItem_tv.Name = "deleteToolStripMenuItem_tv";
            this.deleteToolStripMenuItem_tv.Size = new System.Drawing.Size(112, 22);
            this.deleteToolStripMenuItem_tv.Text = "删除";
            this.deleteToolStripMenuItem_tv.Click += new System.EventHandler(this.deleteToolStripMenuItem_tv_Click);
            // 
            // renameToolStripMenuItem_tv
            // 
            this.renameToolStripMenuItem_tv.Enabled = false;
            this.renameToolStripMenuItem_tv.Name = "renameToolStripMenuItem_tv";
            this.renameToolStripMenuItem_tv.Size = new System.Drawing.Size(112, 22);
            this.renameToolStripMenuItem_tv.Text = "重命名";
            this.renameToolStripMenuItem_tv.Click += new System.EventHandler(this.renameToolStripMenuItem_tv_Click);
            // 
            // selectedItem_ContextMenuStrip
            // 
            this.selectedItem_ContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.modifyToolStripMenuItem_si,
            this.modifyBinaryDataToolStripMenuItem_si,
            this.toolStripSeparator5,
            this.deleteToolStripMenuItem_si,
            this.renameToolStripMenuItem_si});
            this.selectedItem_ContextMenuStrip.Name = "selectedItem_ContextMenuStrip";
            this.selectedItem_ContextMenuStrip.Size = new System.Drawing.Size(149, 98);
            // 
            // modifyToolStripMenuItem_si
            // 
            this.modifyToolStripMenuItem_si.Name = "modifyToolStripMenuItem_si";
            this.modifyToolStripMenuItem_si.Size = new System.Drawing.Size(148, 22);
            this.modifyToolStripMenuItem_si.Text = "修改...";
            this.modifyToolStripMenuItem_si.Click += new System.EventHandler(this.modifyToolStripMenuItem_si_Click);
            // 
            // modifyBinaryDataToolStripMenuItem_si
            // 
            this.modifyBinaryDataToolStripMenuItem_si.Enabled = false;
            this.modifyBinaryDataToolStripMenuItem_si.Name = "modifyBinaryDataToolStripMenuItem_si";
            this.modifyBinaryDataToolStripMenuItem_si.Size = new System.Drawing.Size(148, 22);
            this.modifyBinaryDataToolStripMenuItem_si.Text = "修改二进制值";
            this.modifyBinaryDataToolStripMenuItem_si.Click += new System.EventHandler(this.modifyBinaryDataToolStripMenuItem_si_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(145, 6);
            // 
            // deleteToolStripMenuItem_si
            // 
            this.deleteToolStripMenuItem_si.Name = "deleteToolStripMenuItem_si";
            this.deleteToolStripMenuItem_si.Size = new System.Drawing.Size(148, 22);
            this.deleteToolStripMenuItem_si.Text = "删除";
            this.deleteToolStripMenuItem_si.Click += new System.EventHandler(this.deleteToolStripMenuItem_si_Click);
            // 
            // renameToolStripMenuItem_si
            // 
            this.renameToolStripMenuItem_si.Name = "renameToolStripMenuItem_si";
            this.renameToolStripMenuItem_si.Size = new System.Drawing.Size(148, 22);
            this.renameToolStripMenuItem_si.Text = "重命名";
            this.renameToolStripMenuItem_si.Click += new System.EventHandler(this.renameToolStripMenuItem_si_Click);
            // 
            // lst_ContextMenuStrip
            // 
            this.lst_ContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem_c});
            this.lst_ContextMenuStrip.Name = "lst_ContextMenuStrip";
            this.lst_ContextMenuStrip.Size = new System.Drawing.Size(181, 48);
            // 
            // newToolStripMenuItem_c
            // 
            this.newToolStripMenuItem_c.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.keyToolStripMenuItem_c,
            this.toolStripSeparator6,
            this.stringValueToolStripMenuItem_c,
            this.binaryValueToolStripMenuItem_c,
            this.dword32bitValueToolStripMenuItem_c,
            this.qword64bitValueToolStripMenuItem_c,
            this.multiStringValueToolStripMenuItem_c,
            this.expandableStringValueToolStripMenuItem_c});
            this.newToolStripMenuItem_c.Name = "newToolStripMenuItem_c";
            this.newToolStripMenuItem_c.Size = new System.Drawing.Size(180, 22);
            this.newToolStripMenuItem_c.Text = "新建";
            // 
            // keyToolStripMenuItem_c
            // 
            this.keyToolStripMenuItem_c.Name = "keyToolStripMenuItem_c";
            this.keyToolStripMenuItem_c.Size = new System.Drawing.Size(180, 22);
            this.keyToolStripMenuItem_c.Text = "键";
            this.keyToolStripMenuItem_c.Click += new System.EventHandler(this.keyToolStripMenuItem_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(177, 6);
            // 
            // stringValueToolStripMenuItem_c
            // 
            this.stringValueToolStripMenuItem_c.Name = "stringValueToolStripMenuItem_c";
            this.stringValueToolStripMenuItem_c.Size = new System.Drawing.Size(180, 22);
            this.stringValueToolStripMenuItem_c.Text = "字符串值";
            this.stringValueToolStripMenuItem_c.Click += new System.EventHandler(this.stringValueToolStripMenuItem_Click);
            // 
            // binaryValueToolStripMenuItem_c
            // 
            this.binaryValueToolStripMenuItem_c.Name = "binaryValueToolStripMenuItem_c";
            this.binaryValueToolStripMenuItem_c.Size = new System.Drawing.Size(180, 22);
            this.binaryValueToolStripMenuItem_c.Text = "二进制值";
            this.binaryValueToolStripMenuItem_c.Click += new System.EventHandler(this.binaryValueToolStripMenuItem_Click);
            // 
            // dword32bitValueToolStripMenuItem_c
            // 
            this.dword32bitValueToolStripMenuItem_c.Name = "dword32bitValueToolStripMenuItem_c";
            this.dword32bitValueToolStripMenuItem_c.Size = new System.Drawing.Size(180, 22);
            this.dword32bitValueToolStripMenuItem_c.Text = "32位DWORD值";
            this.dword32bitValueToolStripMenuItem_c.Click += new System.EventHandler(this.dword32bitValueToolStripMenuItem_Click);
            // 
            // qword64bitValueToolStripMenuItem_c
            // 
            this.qword64bitValueToolStripMenuItem_c.Name = "qword64bitValueToolStripMenuItem_c";
            this.qword64bitValueToolStripMenuItem_c.Size = new System.Drawing.Size(180, 22);
            this.qword64bitValueToolStripMenuItem_c.Text = "64位QWORD值";
            this.qword64bitValueToolStripMenuItem_c.Click += new System.EventHandler(this.qword64bitValueToolStripMenuItem_Click);
            // 
            // multiStringValueToolStripMenuItem_c
            // 
            this.multiStringValueToolStripMenuItem_c.Name = "multiStringValueToolStripMenuItem_c";
            this.multiStringValueToolStripMenuItem_c.Size = new System.Drawing.Size(180, 22);
            this.multiStringValueToolStripMenuItem_c.Text = "多重字符串值";
            this.multiStringValueToolStripMenuItem_c.Click += new System.EventHandler(this.multiStringValueToolStripMenuItem_Click);
            // 
            // expandableStringValueToolStripMenuItem_c
            // 
            this.expandableStringValueToolStripMenuItem_c.Name = "expandableStringValueToolStripMenuItem_c";
            this.expandableStringValueToolStripMenuItem_c.Size = new System.Drawing.Size(180, 22);
            this.expandableStringValueToolStripMenuItem_c.Text = "可扩展字符串值";
            this.expandableStringValueToolStripMenuItem_c.Click += new System.EventHandler(this.expandableStringValueToolStripMenuItem_Click);
            // 
            // keyToolStripMenuItem_tv
            // 
            this.keyToolStripMenuItem_tv.Name = "keyToolStripMenuItem_tv";
            this.keyToolStripMenuItem_tv.Size = new System.Drawing.Size(180, 22);
            this.keyToolStripMenuItem_tv.Text = "键";
            this.keyToolStripMenuItem_tv.Click += new System.EventHandler(this.keyToolStripMenuItem_tv_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(177, 6);
            // 
            // stringValueToolStripMenuItem_tv
            // 
            this.stringValueToolStripMenuItem_tv.Name = "stringValueToolStripMenuItem_tv";
            this.stringValueToolStripMenuItem_tv.Size = new System.Drawing.Size(180, 22);
            this.stringValueToolStripMenuItem_tv.Text = "字符串值";
            this.stringValueToolStripMenuItem_tv.Click += new System.EventHandler(this.stringValueToolStripMenuItem_Click);
            // 
            // binaryValueToolStripMenuItem_tv
            // 
            this.binaryValueToolStripMenuItem_tv.Name = "binaryValueToolStripMenuItem_tv";
            this.binaryValueToolStripMenuItem_tv.Size = new System.Drawing.Size(180, 22);
            this.binaryValueToolStripMenuItem_tv.Text = "二进制值";
            this.binaryValueToolStripMenuItem_tv.Click += new System.EventHandler(this.binaryValueToolStripMenuItem_Click);
            // 
            // dword32bitValueToolStripMenuItem_tv
            // 
            this.dword32bitValueToolStripMenuItem_tv.Name = "dword32bitValueToolStripMenuItem_tv";
            this.dword32bitValueToolStripMenuItem_tv.Size = new System.Drawing.Size(180, 22);
            this.dword32bitValueToolStripMenuItem_tv.Text = "32位DWORD值";
            this.dword32bitValueToolStripMenuItem_tv.Click += new System.EventHandler(this.dword32bitValueToolStripMenuItem_Click);
            // 
            // qword64bitValueToolStripMenuItem_tv
            // 
            this.qword64bitValueToolStripMenuItem_tv.Name = "qword64bitValueToolStripMenuItem_tv";
            this.qword64bitValueToolStripMenuItem_tv.Size = new System.Drawing.Size(180, 22);
            this.qword64bitValueToolStripMenuItem_tv.Text = "64位QWORD值";
            this.qword64bitValueToolStripMenuItem_tv.Click += new System.EventHandler(this.qword64bitValueToolStripMenuItem_Click);
            // 
            // multiStringValueToolStripMenuItem_tv
            // 
            this.multiStringValueToolStripMenuItem_tv.Name = "multiStringValueToolStripMenuItem_tv";
            this.multiStringValueToolStripMenuItem_tv.Size = new System.Drawing.Size(180, 22);
            this.multiStringValueToolStripMenuItem_tv.Text = "多行字符串值";
            this.multiStringValueToolStripMenuItem_tv.Click += new System.EventHandler(this.multiStringValueToolStripMenuItem_Click);
            // 
            // expandableStringValueToolStripMenuItem_tv
            // 
            this.expandableStringValueToolStripMenuItem_tv.Name = "expandableStringValueToolStripMenuItem_tv";
            this.expandableStringValueToolStripMenuItem_tv.Size = new System.Drawing.Size(180, 22);
            this.expandableStringValueToolStripMenuItem_tv.Text = "可扩展字符串值";
            this.expandableStringValueToolStripMenuItem_tv.Click += new System.EventHandler(this.expandableStringValueToolStripMenuItem_Click);
            // 
            // RegistryEditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(804, 450);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RegistryEditorForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "FK远控服务器端 - 注册表编辑器";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RegistryEditorForm_FormClosing);
            this.Load += new System.EventHandler(this.RegistryEditorForm_Load);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.tv_ContextMenuStrip.ResumeLayout(false);
            this.selectedItem_ContextMenuStrip.ResumeLayout(false);
            this.lst_ContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel selectedStripStatusLabel;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem modifyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem modifyBinaryDataToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem keyToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem stringValueToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem binaryValueToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dword32bitValueToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem qword64bitValueToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem multiStringValueToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem expandableStringValueToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renameToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer;
        private Controls.RegistryTreeView tvRegistryDirectory;
        private Controls.FKListView lstRegistryValues;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colType;
        private System.Windows.Forms.ColumnHeader colValue;
        private System.Windows.Forms.ContextMenuStrip tv_ContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem_tv;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem_tv;
        private System.Windows.Forms.ToolStripMenuItem renameToolStripMenuItem_tv;
        private System.Windows.Forms.ContextMenuStrip selectedItem_ContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem modifyToolStripMenuItem_si;
        private System.Windows.Forms.ToolStripMenuItem modifyBinaryDataToolStripMenuItem_si;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem_si;
        private System.Windows.Forms.ToolStripMenuItem renameToolStripMenuItem_si;
        private System.Windows.Forms.ContextMenuStrip lst_ContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem_c;
        private System.Windows.Forms.ToolStripMenuItem keyToolStripMenuItem_c;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem stringValueToolStripMenuItem_c;
        private System.Windows.Forms.ToolStripMenuItem binaryValueToolStripMenuItem_c;
        private System.Windows.Forms.ToolStripMenuItem dword32bitValueToolStripMenuItem_c;
        private System.Windows.Forms.ToolStripMenuItem qword64bitValueToolStripMenuItem_c;
        private System.Windows.Forms.ToolStripMenuItem multiStringValueToolStripMenuItem_c;
        private System.Windows.Forms.ToolStripMenuItem expandableStringValueToolStripMenuItem_c;
        private System.Windows.Forms.ToolStripMenuItem keyToolStripMenuItem_tv;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem stringValueToolStripMenuItem_tv;
        private System.Windows.Forms.ToolStripMenuItem binaryValueToolStripMenuItem_tv;
        private System.Windows.Forms.ToolStripMenuItem dword32bitValueToolStripMenuItem_tv;
        private System.Windows.Forms.ToolStripMenuItem qword64bitValueToolStripMenuItem_tv;
        private System.Windows.Forms.ToolStripMenuItem multiStringValueToolStripMenuItem_tv;
        private System.Windows.Forms.ToolStripMenuItem expandableStringValueToolStripMenuItem_tv;
    }
}