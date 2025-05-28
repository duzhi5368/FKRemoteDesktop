using FKRemoteDesktop.Certificate;
using FKRemoteDesktop.Configs;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows.Forms;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Forms
{
    public partial class CertificateForm : Form
    {
        private X509Certificate2 _certificate;
        private const string CA_NAME = "Google LLC";

        public CertificateForm()
        {
            InitializeComponent();
        }

        // 导入第三方签名文件
        private void btnImport_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.CheckFileExists = true;
                ofd.Filter = "*.p12|*.p12";
                ofd.Multiselect = false;
                ofd.InitialDirectory = Application.StartupPath;
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        SetCertificate(new X509Certificate2(ofd.FileName, "", X509KeyStorageFlags.Exportable));
                        SaveCertificate();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, $"Error importing the certificate:\n{ex.Message}", "Save error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // 创建签名文件
        private async void btnCreate_Click(object sender, EventArgs e)
        {
            txtDetails.Text = "创建签名证书，这个过程约需 10 秒，请稍候...";
            txtDetails.Text += "\r\n=======================================\r\n";
            bool isProcessing = true;

            try
            {
                // 后台任务仅负责创建证书
                var certificate = await Task.Run(() =>
                    CertificateHelper.CreateCertificateAuthority(CA_NAME, 4096));

                // 主线程执行 SetCertificate
                SetCertificate(certificate);
                txtDetails.Text += "\r\n=======================================\r\n";
                txtDetails.Text += "证书已创建完成，设置过程约需 5 秒，请稍候...";

                // 非阻塞等待 5 秒
                await Task.Delay(5000);

                // 主线程执行 SaveCertificate
                SaveCertificate();
                txtDetails.Text += "证书创建并保存完成！";

                // 非阻塞等待 0.5 秒
                await Task.Delay(500);

                // 标记处理完成
                isProcessing = false;
            }
            catch (Exception ex)
            {
                txtDetails.Text = $"错误：{ex.Message}";
                isProcessing = false;
            }

            // 主线程持续更新 UI，直到处理完成
            while (isProcessing && !Task.CurrentId.HasValue)
            {
                txtDetails.Text += $"处理中... ({DateTime.Now.Second})";
                await Task.Delay(1000);
            }
        }

        private void SetCertificate(X509Certificate2 certificate)
        {
            _certificate = certificate;
            txtDetails.Text += _certificate.ToString(false);
        }

        private void SaveCertificate()
        {
            try
            {
                if (_certificate == null)
                    throw new ArgumentNullException();
                if (!_certificate.HasPrivateKey)
                    throw new ArgumentException();

                File.WriteAllBytes(ServerConfig.CertificatePath, _certificate.Export(X509ContentType.Pkcs12));
                MessageBox.Show(this,
                    "请注意备份签名证书。一旦证书丢失，将会丢失全部客户端!!!",
                    "保存签名证书提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

                string argument = "/select, \"" + ServerConfig.CertificatePath + "\"";
                Process.Start("explorer.exe", argument);

                this.DialogResult = DialogResult.OK;
            }
            catch (ArgumentNullException)
            {
                MessageBox.Show(this, "请先创建本地证书或导入第三方证书.", "保存签名证书失败",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (ArgumentException)
            {
                MessageBox.Show(this,
                    "导入的证书没有关联的私钥，请导入其他有效证书.",
                    "保存签名证书失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception)
            {
                MessageBox.Show(this,
                    "保存证书失败，请检查你的 FKRemoteDesktop 目录有写入权限.",
                    "保存签名证书失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
