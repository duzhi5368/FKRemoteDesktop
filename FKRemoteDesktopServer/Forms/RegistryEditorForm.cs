using FKRemoteDesktop.Controls;
using FKRemoteDesktop.Extensions;
using FKRemoteDesktop.Helpers;
using FKRemoteDesktop.Message;
using FKRemoteDesktop.Message.MessageStructs;
using FKRemoteDesktop.Message.SubMessageHandler;
using FKRemoteDesktop.Network;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Forms
{
    public partial class RegistryEditorForm : Form
    {
        private readonly Client _connectClient;
        private readonly RegistryHandler _registryHandler;
        private static readonly Dictionary<Client, RegistryEditorForm> OpenedForms = new Dictionary<Client, RegistryEditorForm>();

        public RegistryEditorForm(Client client)
        {
            _connectClient = client;
            _registryHandler = new RegistryHandler(client);

            RegisterMessageHandler();
            InitializeComponent();
        }

        public static RegistryEditorForm CreateNewOrGetExisting(Client client)
        {
            if (OpenedForms.ContainsKey(client))
            {
                return OpenedForms[client];
            }
            RegistryEditorForm f = new RegistryEditorForm(client);
            f.Disposed += (sender, args) => OpenedForms.Remove(client);
            OpenedForms.Add(client, f);
            return f;
        }

        private void RegisterMessageHandler()
        {
            _connectClient.ClientState += ClientDisconnected;
            _registryHandler.ProgressChanged += ShowErrorMessage;
            _registryHandler.KeysReceived += AddKeys;
            _registryHandler.KeyCreated += CreateNewKey;
            _registryHandler.KeyDeleted += DeleteKey;
            _registryHandler.KeyRenamed += RenameKey;
            _registryHandler.ValueCreated += CreateValue;
            _registryHandler.ValueDeleted += DeleteValue;
            _registryHandler.ValueRenamed += RenameValue;
            _registryHandler.ValueChanged += ChangeValue;
            MessageHandler.Register(_registryHandler);
        }

        private void UnregisterMessageHandler()
        {
            MessageHandler.Unregister(_registryHandler);
            _registryHandler.ProgressChanged -= ShowErrorMessage;
            _registryHandler.KeysReceived -= AddKeys;
            _registryHandler.KeyCreated -= CreateNewKey;
            _registryHandler.KeyDeleted -= DeleteKey;
            _registryHandler.KeyRenamed -= RenameKey;
            _registryHandler.ValueCreated -= CreateValue;
            _registryHandler.ValueDeleted -= DeleteValue;
            _registryHandler.ValueRenamed -= RenameValue;
            _registryHandler.ValueChanged -= ChangeValue;
            _connectClient.ClientState -= ClientDisconnected;
        }

        private void ClientDisconnected(Client client, bool connected)
        {
            if (!connected)
            {
                this.Invoke((MethodInvoker)this.Close);
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; //WS_EX_COMPOSITED
                return cp;
            }
        }

        private void RegistryEditorForm_Load(object sender, System.EventArgs e)
        {
            if (_connectClient.UserInfo.AccountType != "Admin")
            {
                MessageBox.Show(
                    "远控客户端部分未以管理员身份运行，因此部分功能(如更新，创建，打开和删除)可能无法正常使用.",
                    "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            this.Text = WindowHelper.GetWindowTitle("FK远控服务器端 - 注册表编辑器", _connectClient);
            _registryHandler.LoadRegistryKey(null);
        }

        private void RegistryEditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            UnregisterMessageHandler();
        }

        private void ShowErrorMessage(object sender, string errorMsg)
        {
            MessageBox.Show(errorMsg, "异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        #region TreeView部分函数

        private void AddRootKey(RegSeekerMatch match)
        {
            TreeNode node = CreateNode(match.Key, match.Key, match.Data);
            node.Nodes.Add(new TreeNode());
            tvRegistryDirectory.Nodes.Add(node);
        }

        private TreeNode AddKeyToTree(TreeNode parent, RegSeekerMatch subKey)
        {
            TreeNode node = CreateNode(subKey.Key, subKey.Key, subKey.Data);
            parent.Nodes.Add(node);
            if (subKey.HasSubKeys)
                node.Nodes.Add(new TreeNode());
            return node;
        }

        private TreeNode CreateNode(string key, string text, object tag)
        {
            return new TreeNode()
            {
                Text = text,
                Name = key,
                Tag = tag
            };
        }

        private void AddKeys(object sender, string rootKey, RegSeekerMatch[] matches)
        {
            if (string.IsNullOrEmpty(rootKey))
            {
                tvRegistryDirectory.BeginUpdate();
                foreach (var match in matches)
                    AddRootKey(match);
                tvRegistryDirectory.SelectedNode = tvRegistryDirectory.Nodes[0];
                tvRegistryDirectory.EndUpdate();
            }
            else
            {
                TreeNode parent = GetTreeNode(rootKey);
                if (parent != null)
                {
                    tvRegistryDirectory.BeginUpdate();
                    foreach (var match in matches)
                        AddKeyToTree(parent, match);
                    parent.Expand();
                    tvRegistryDirectory.EndUpdate();
                }
            }
        }

        private void CreateNewKey(object sender, string rootKey, RegSeekerMatch match)
        {
            TreeNode parent = GetTreeNode(rootKey);
            TreeNode node = AddKeyToTree(parent, match);
            node.EnsureVisible();
            tvRegistryDirectory.SelectedNode = node;
            node.Expand();
            tvRegistryDirectory.LabelEdit = true;
            node.BeginEdit();
        }

        private void DeleteKey(object sender, string rootKey, string subKey)
        {
            TreeNode parent = GetTreeNode(rootKey);
            if (parent.Nodes.ContainsKey(subKey))
            {
                parent.Nodes.RemoveByKey(subKey);
            }
        }

        private void RenameKey(object sender, string rootKey, string oldName, string newName)
        {
            TreeNode parent = GetTreeNode(rootKey);
            if (parent.Nodes.ContainsKey(oldName))
            {
                parent.Nodes[oldName].Text = newName;
                parent.Nodes[oldName].Name = newName;
                tvRegistryDirectory.SelectedNode = parent.Nodes[newName];
            }
        }

        private TreeNode GetTreeNode(string path)
        {
            string[] nodePath = path.Split(new char[] { '\\' });
            TreeNode lastNode = tvRegistryDirectory.Nodes[nodePath[0]];
            if (lastNode == null)
                return null;
            for (int i = 1; i < nodePath.Length; i++)
            {
                lastNode = lastNode.Nodes[nodePath[i]];
                if (lastNode == null)
                    return null;
            }
            return lastNode;
        }

        #endregion

        #region ListView部分函数

        private void CreateValue(object sender, string keyPath, RegValueData value)
        {
            TreeNode key = GetTreeNode(keyPath);
            if (key != null)
            {
                List<RegValueData> valuesFromNode = ((RegValueData[])key.Tag).ToList();
                valuesFromNode.Add(value);
                key.Tag = valuesFromNode.ToArray();
                if (tvRegistryDirectory.SelectedNode == key)
                {
                    RegistryValueListItem item = new RegistryValueListItem(value);
                    lstRegistryValues.Items.Add(item);
                    lstRegistryValues.SelectedIndices.Clear();
                    item.Selected = true;
                    lstRegistryValues.LabelEdit = true;
                    item.BeginEdit();
                }
                tvRegistryDirectory.SelectedNode = key;
            }
        }

        private void DeleteValue(object sender, string keyPath, string valueName)
        {
            TreeNode key = GetTreeNode(keyPath);
            if (key != null)
            {
                if (!RegValueHelper.IsDefaultValue(valueName))
                {
                    key.Tag = ((RegValueData[])key.Tag).Where(value => value.Name != valueName).ToArray();

                    if (tvRegistryDirectory.SelectedNode == key)
                        lstRegistryValues.Items.RemoveByKey(valueName);
                }
                else
                {
                    var regValue = ((RegValueData[])key.Tag).First(item => item.Name == valueName);
                    if (tvRegistryDirectory.SelectedNode == key)
                    {
                        var valueItem = lstRegistryValues.Items.Cast<RegistryValueListItem>()
                                                     .SingleOrDefault(item => item.Name == valueName);
                        if (valueItem != null)
                            valueItem.Data = regValue.Kind.RegistryTypeToString(null);
                    }
                }
                tvRegistryDirectory.SelectedNode = key;
            }
        }

        private void RenameValue(object sender, string keyPath, string oldName, string newName)
        {
            TreeNode key = GetTreeNode(keyPath);
            if (key != null)
            {
                var value = ((RegValueData[])key.Tag).First(item => item.Name == oldName);
                value.Name = newName;
                if (tvRegistryDirectory.SelectedNode == key)
                {
                    var valueItem = lstRegistryValues.Items.Cast<RegistryValueListItem>()
                                                     .SingleOrDefault(item => item.Name == oldName);
                    if (valueItem != null)
                        valueItem.RegName = newName;
                }
                tvRegistryDirectory.SelectedNode = key;
            }
        }

        private void ChangeValue(object sender, string keyPath, RegValueData value)
        {
            TreeNode key = GetTreeNode(keyPath);
            if (key != null)
            {
                var regValue = ((RegValueData[])key.Tag).First(item => item.Name == value.Name);
                ChangeRegistryValue(value, regValue);

                if (tvRegistryDirectory.SelectedNode == key)
                {
                    var valueItem = lstRegistryValues.Items.Cast<RegistryValueListItem>()
                                                     .SingleOrDefault(item => item.Name == value.Name);
                    if (valueItem != null)
                        valueItem.Data = RegValueHelper.RegistryValueToString(value);
                }

                tvRegistryDirectory.SelectedNode = key;
            }
        }

        private void ChangeRegistryValue(RegValueData source, RegValueData dest)
        {
            if (source.Kind != dest.Kind) 
                return;
            dest.Data = source.Data;
        }

        private void UpdateLstRegistryValues(TreeNode node)
        {
            selectedStripStatusLabel.Text = node.FullPath;
            RegValueData[] ValuesFromNode = (RegValueData[])node.Tag;
            PopulateLstRegistryValues(ValuesFromNode);
        }

        private void PopulateLstRegistryValues(RegValueData[] values)
        {
            lstRegistryValues.BeginUpdate();
            lstRegistryValues.Items.Clear();
            values = (
                from value in values
                orderby value.Name ascending
                select value
                ).ToArray();
            foreach (var value in values)
            {
                RegistryValueListItem item = new RegistryValueListItem(value);
                lstRegistryValues.Items.Add(item);
            }

            lstRegistryValues.EndUpdate();
        }

        #endregion

        #region tvRegistryDirectory部分函数

        private void tvRegistryDirectory_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Label != null)
            {
                e.CancelEdit = true;
                if (e.Label.Length > 0)
                {
                    if (e.Node.Parent.Nodes.ContainsKey(e.Label))
                    {
                        MessageBox.Show("无效节点. \n一个节点已经存在.", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        e.Node.BeginEdit();
                    }
                    else
                    {
                        _registryHandler.RenameRegistryKey(e.Node.Parent.FullPath, e.Node.Name, e.Label);
                        tvRegistryDirectory.LabelEdit = false;
                    }
                }
                else
                {
                    MessageBox.Show("无效节点. \n节点不能为空.", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    e.Node.BeginEdit();
                }
            }
            else
            {
                tvRegistryDirectory.LabelEdit = false;
            }
        }

        private void tvRegistryDirectory_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            TreeNode parentNode = e.Node;
            if (string.IsNullOrEmpty(parentNode.FirstNode.Name))
            {
                tvRegistryDirectory.SuspendLayout();
                parentNode.Nodes.Clear();
                _registryHandler.LoadRegistryKey(parentNode.FullPath);
                tvRegistryDirectory.ResumeLayout();
                e.Cancel = true;
            }
        }

        private void tvRegistryDirectory_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                tvRegistryDirectory.SelectedNode = e.Node;
                Point pos = new Point(e.X, e.Y);
                CreateTreeViewMenuStrip();
                tv_ContextMenuStrip.Show(tvRegistryDirectory, pos);
            }
        }

        private void tvRegistryDirectory_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            UpdateLstRegistryValues(e.Node);
        }

        private void tvRegistryDirectory_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && GetDeleteState())
                deleteToolStripMenuItem_tv_Click(this, e);
        }

        // 特殊回调
        private void createRegistryKey_AfterExpand(object sender, TreeViewEventArgs e)
        {
            if (e.Node == tvRegistryDirectory.SelectedNode)
            {
                keyToolStripMenuItem_tv_Click(this, e);
                tvRegistryDirectory.AfterExpand -= createRegistryKey_AfterExpand;
            }
        }

        #endregion

        #region UI辅助函数

        private void CreateEditToolStrip()
        {
            this.modifyToolStripMenuItem.Visible =
                this.modifyBinaryDataToolStripMenuItem.Visible = lstRegistryValues.Focused;
            this.modifyToolStripMenuItem.Enabled =
                this.modifyBinaryDataToolStripMenuItem.Enabled = lstRegistryValues.SelectedItems.Count == 1;
            this.renameToolStripMenuItem.Enabled = GetRenameState();
            this.deleteToolStripMenuItem.Enabled = GetDeleteState();
        }

        private void CreateTreeViewMenuStrip()
        {
            this.renameToolStripMenuItem_tv.Enabled = tvRegistryDirectory.SelectedNode.Parent != null;
            this.deleteToolStripMenuItem_tv.Enabled = tvRegistryDirectory.SelectedNode.Parent != null;
        }

        private void CreateListViewMenuStrip()
        {
            this.modifyToolStripMenuItem_si.Enabled =
                this.modifyBinaryDataToolStripMenuItem_si.Enabled = lstRegistryValues.SelectedItems.Count == 1;
            this.renameToolStripMenuItem_si.Enabled = lstRegistryValues.SelectedItems.Count == 1 && !RegValueHelper.IsDefaultValue(lstRegistryValues.SelectedItems[0].Name);
            this.deleteToolStripMenuItem_si.Enabled = tvRegistryDirectory.SelectedNode != null && lstRegistryValues.SelectedItems.Count > 0;
        }

        #endregion

        #region 主菜单栏menuStrip函数

        // 退出 按钮
        private void exitToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        // 编辑 按钮
        private void editToolStripMenuItem_DropDownOpening(object sender, System.EventArgs e)
        {
            CreateEditToolStrip();
        }

        // 编辑->删除 按钮
        private void deleteToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            if (tvRegistryDirectory.Focused)
            {
                deleteToolStripMenuItem_tv_Click(this, e);
            }
            else if (lstRegistryValues.Focused)
            {
                deleteToolStripMenuItem_si_Click(this, e);
            }
        }

        // 编辑->重命名 按钮
        private void renameToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            if (tvRegistryDirectory.Focused)
            {
                renameToolStripMenuItem_tv_Click(this, e);
            }
            else if (lstRegistryValues.Focused)
            {
                deleteToolStripMenuItem_si_Click(this, e);
            }
        }

        // 编辑->新建->键 按钮
        private void keyToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            if (!(tvRegistryDirectory.SelectedNode.IsExpanded) && tvRegistryDirectory.SelectedNode.Nodes.Count > 0)
            {
                tvRegistryDirectory.AfterExpand += this.createRegistryKey_AfterExpand;
                tvRegistryDirectory.SelectedNode.Expand();
            }
            else
            {
                _registryHandler.CreateRegistryKey(tvRegistryDirectory.SelectedNode.FullPath);
            }
        }

        // 编辑->修改 按钮
        private void modifyToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            CreateEditForm(false);
        }

        // 编辑->修改二进制值 按钮
        private void modifyBinaryDataToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            CreateEditForm(true);
        }

        // 编辑->新建->字符串值
        private void stringValueToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            if (tvRegistryDirectory.SelectedNode != null)
            {
                _registryHandler.CreateRegistryValue(tvRegistryDirectory.SelectedNode.FullPath,
                    RegistryValueKind.String);
            }
        }

        // 编辑->新建->二进制值
        private void binaryValueToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            if (tvRegistryDirectory.SelectedNode != null)
            {
                _registryHandler.CreateRegistryValue(tvRegistryDirectory.SelectedNode.FullPath,
                    RegistryValueKind.Binary);
            }
        }

        // 编辑->新建->32位DWORD值
        private void dword32bitValueToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            if (tvRegistryDirectory.SelectedNode != null)
            {
                _registryHandler.CreateRegistryValue(tvRegistryDirectory.SelectedNode.FullPath,
                    RegistryValueKind.DWord);
            }
        }

        // 编辑->新建->64位QWORD值
        private void qword64bitValueToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            if (tvRegistryDirectory.SelectedNode != null)
            {
                _registryHandler.CreateRegistryValue(tvRegistryDirectory.SelectedNode.FullPath,
                    RegistryValueKind.QWord);
            }
        }

        // 编辑->新建->多字符串值
        private void multiStringValueToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            if (tvRegistryDirectory.SelectedNode != null)
            {
                _registryHandler.CreateRegistryValue(tvRegistryDirectory.SelectedNode.FullPath,
                    RegistryValueKind.MultiString);
            }
        }

        // 编辑->新建->可扩展字符串值
        private void expandableStringValueToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            if (tvRegistryDirectory.SelectedNode != null)
            {
                _registryHandler.CreateRegistryValue(tvRegistryDirectory.SelectedNode.FullPath,
                    RegistryValueKind.ExpandString);
            }
        }

        #endregion

        #region 右侧lstRegistryValues函数

        private void lstRegistryValues_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Point pos = new Point(e.X, e.Y);
                if (lstRegistryValues.GetItemAt(pos.X, pos.Y) == null)
                {
                    // 不是元素
                    lst_ContextMenuStrip.Show(lstRegistryValues, pos);
                }
                else
                {
                    // 点中一个元素
                    CreateListViewMenuStrip();
                    selectedItem_ContextMenuStrip.Show(lstRegistryValues, pos);
                }
            }
        }

        private void lstRegistryValues_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && GetDeleteState())
                deleteToolStripMenuItem_si_Click(this, e);
        }

        private void lstRegistryValues_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            if (e.Label != null && tvRegistryDirectory.SelectedNode != null)
            {
                e.CancelEdit = true;
                int index = e.Item;
                if (e.Label.Length > 0)
                {
                    if (lstRegistryValues.Items.ContainsKey(e.Label))
                    {
                        MessageBox.Show("无效节点. \n一个节点已经存在.", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        lstRegistryValues.Items[index].BeginEdit();
                        return;
                    }
                    _registryHandler.RenameRegistryValue(tvRegistryDirectory.SelectedNode.FullPath,
                        lstRegistryValues.Items[index].Name, e.Label);
                    lstRegistryValues.LabelEdit = false;
                }
                else
                {
                    MessageBox.Show("无效节点. \n节点不能为空.", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    lstRegistryValues.Items[index].BeginEdit();
                }
            }
            else
            {
                lstRegistryValues.LabelEdit = false;
            }
        }

        #endregion

        #region tv_ContextMenuStrip函数

        // 新建->键 按钮
        private void keyToolStripMenuItem_tv_Click(object sender, System.EventArgs e)
        {
            this.keyToolStripMenuItem_Click(sender, e);
        }

        // 删除 按钮
        private void deleteToolStripMenuItem_tv_Click(object sender, System.EventArgs e)
        {
            string msg = "你确定要删除该注册表键和其所有子项吗？";
            string caption = "确定删除";
            var answer = MessageBox.Show(msg, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (answer == DialogResult.Yes)
            {
                string parentPath = tvRegistryDirectory.SelectedNode.Parent.FullPath;
                _registryHandler.DeleteRegistryKey(parentPath, tvRegistryDirectory.SelectedNode.Name);
            }
        }

        // 重命名 按钮
        private void renameToolStripMenuItem_tv_Click(object sender, System.EventArgs e)
        {
            tvRegistryDirectory.LabelEdit = true;
            tvRegistryDirectory.SelectedNode.BeginEdit();
        }

        #endregion

        #region selectedItem_ContextMenuStrip函数

        // 删除 按钮
        private void deleteToolStripMenuItem_si_Click(object sender, System.EventArgs e)
        {
            string msg = "删除某些注册表值可能会导致系统不稳定，确定要永久删除吗？";
            string caption = "确定删除";
            var answer = MessageBox.Show(msg, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (answer == DialogResult.Yes)
            {
                foreach (var item in lstRegistryValues.SelectedItems)
                {
                    if (item.GetType() == typeof(RegistryValueListItem))
                    {
                        RegistryValueListItem registryValue = (RegistryValueListItem)item;
                        _registryHandler.DeleteRegistryValue(tvRegistryDirectory.SelectedNode.FullPath, registryValue.RegName);
                    }
                }
            }
        }

        // 重命名 按钮
        private void renameToolStripMenuItem_si_Click(object sender, System.EventArgs e)
        {
            lstRegistryValues.LabelEdit = true;
            lstRegistryValues.SelectedItems[0].BeginEdit();
        }

        // 修改 按钮
        private void modifyToolStripMenuItem_si_Click(object sender, System.EventArgs e)
        {
            CreateEditForm(false);
        }

        // 修改二进制值 按钮
        private void modifyBinaryDataToolStripMenuItem_si_Click(object sender, System.EventArgs e)
        {
            CreateEditForm(true);
        }

        #endregion

        #region 辅助函数

        private bool GetDeleteState()
        {
            if (lstRegistryValues.Focused)
                return lstRegistryValues.SelectedItems.Count > 0;
            else if (tvRegistryDirectory.Focused && tvRegistryDirectory.SelectedNode != null)
                return tvRegistryDirectory.SelectedNode.Parent != null;
            return false;
        }

        private bool GetRenameState()
        {
            if (lstRegistryValues.Focused)
                return lstRegistryValues.SelectedItems.Count == 1 && !RegValueHelper.IsDefaultValue(lstRegistryValues.SelectedItems[0].Name);
            else if (tvRegistryDirectory.Focused && tvRegistryDirectory.SelectedNode != null)
                return tvRegistryDirectory.SelectedNode.Parent != null;
            return false;
        }

        private Form GetEditForm(RegValueData value, RegistryValueKind valueKind)
        {
            switch (valueKind)
            {
                case RegistryValueKind.String:
                case RegistryValueKind.ExpandString:
                    return new RegValueEditStringForm(value);
                case RegistryValueKind.DWord:
                case RegistryValueKind.QWord:
                    return new RegValueEditWordForm(value);
                case RegistryValueKind.MultiString:
                    return new RegValueEditMultiStringForm(value);
                case RegistryValueKind.Binary:
                    return new RegValueEditBinaryForm(value);
                default:
                    return null;
            }
        }

        private void CreateEditForm(bool isBinary)
        {
            string keyPath = tvRegistryDirectory.SelectedNode.FullPath;
            string name = lstRegistryValues.SelectedItems[0].Name;
            RegValueData value = ((RegValueData[])tvRegistryDirectory.SelectedNode.Tag).ToList().Find(item => item.Name == name);
            RegistryValueKind kind = isBinary ? RegistryValueKind.Binary : value.Kind;
            using (var frm = GetEditForm(value, kind))
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    _registryHandler.ChangeRegistryValue(keyPath, (RegValueData)frm.Tag);
                }
            }
        }

        #endregion
    }
}
