using System;
using System.Collections.Generic;
using System.IO;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Helpers
{
    public static class PEFileInfoHelper
    {
        public static Dictionary<string, dynamic> GatherFileInfoWin(string binaryPath)
        {
            var flItms = new Dictionary<string, dynamic>();
            using (var binary = new FileStream(binaryPath, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(binary))
            {
                binary.Seek(0x3C, SeekOrigin.Begin);
                flItms["buffer"] = 0;
                flItms["JMPtoCodeAddress"] = 0;
                flItms["dis_frm_pehdrs_sectble"] = 248;
                flItms["pe_header_location"] = reader.ReadInt32();

                // COFF起始信息
                flItms["COFF_Start"] = flItms["pe_header_location"] + 4;
                binary.Seek(flItms["COFF_Start"], SeekOrigin.Begin);
                flItms["MachineType"] = reader.ReadUInt16();
                binary.Seek(flItms["COFF_Start"] + 2, SeekOrigin.Begin);
                flItms["NumberOfSections"] = reader.ReadUInt16();
                flItms["TimeDateStamp"] = reader.ReadUInt32();
                binary.Seek(flItms["COFF_Start"] + 16, SeekOrigin.Begin);
                flItms["SizeOfOptionalHeader"] = reader.ReadUInt16();
                flItms["Characteristics"] = reader.ReadUInt16();

                // 可选头信息
                flItms["OptionalHeader_start"] = flItms["COFF_Start"] + 20;
                binary.Seek(flItms["OptionalHeader_start"], SeekOrigin.Begin);
                flItms["Magic"] = reader.ReadUInt16();
                flItms["MajorLinkerVersion"] = reader.ReadByte();
                flItms["MinorLinkerVersion"] = reader.ReadByte();
                flItms["SizeOfCode"] = reader.ReadUInt32();
                flItms["SizeOfInitializedData"] = reader.ReadUInt32();
                flItms["SizeOfUninitializedData"] = reader.ReadUInt32();
                flItms["AddressOfEntryPoint"] = reader.ReadUInt32();
                flItms["PatchLocation"] = flItms["AddressOfEntryPoint"];
                flItms["BaseOfCode"] = reader.ReadUInt32();

                if (flItms["Magic"] != 0x20B)
                {
                    flItms["BaseOfData"] = reader.ReadUInt32();
                }

                // 可选标头的 Windows 特定字段
                if (flItms["Magic"] == 0x20B)
                {
                    flItms["ImageBase"] = reader.ReadUInt64();
                }
                else
                {
                    flItms["ImageBase"] = reader.ReadUInt32();
                }

                flItms["SectionAlignment"] = reader.ReadUInt32();
                flItms["FileAlignment"] = reader.ReadUInt32();
                flItms["MajorOperatingSystemVersion"] = reader.ReadUInt16();
                flItms["MinorOperatingSystemVersion"] = reader.ReadUInt16();
                flItms["MajorImageVersion"] = reader.ReadUInt16();
                flItms["MinorImageVersion"] = reader.ReadUInt16();
                flItms["MajorSubsystemVersion"] = reader.ReadUInt16();
                flItms["MinorSubsystemVersion"] = reader.ReadUInt16();
                flItms["Win32VersionValue"] = reader.ReadUInt32();
                flItms["SizeOfImageLoc"] = binary.Position;
                flItms["SizeOfImage"] = reader.ReadUInt32();
                flItms["SizeOfHeaders"] = reader.ReadUInt32();
                flItms["CheckSum"] = reader.ReadUInt32();
                flItms["Subsystem"] = reader.ReadUInt16();
                flItms["DllCharacteristics"] = reader.ReadUInt16();

                if (flItms["Magic"] == 0x20B)
                {
                    flItms["SizeOfStackReserve"] = reader.ReadUInt64();
                    flItms["SizeOfStackCommit"] = reader.ReadUInt64();
                    flItms["SizeOfHeapReserve"] = reader.ReadUInt64();
                    flItms["SizeOfHeapCommit"] = reader.ReadUInt64();
                }
                else
                {
                    flItms["SizeOfStackReserve"] = reader.ReadUInt32();
                    flItms["SizeOfStackCommit"] = reader.ReadUInt32();
                    flItms["SizeOfHeapReserve"] = reader.ReadUInt32();
                    flItms["SizeOfHeapCommit"] = reader.ReadUInt32();
                }
                flItms["LoaderFlags"] = reader.ReadUInt32();
                flItms["NumberofRvaAndSizes"] = reader.ReadUInt32();

                // 数据词典
                flItms["ExportTableRVA"] = reader.ReadUInt32();
                flItms["ExportTableSize"] = reader.ReadUInt32();
                flItms["ImportTableLOCInPEOptHdrs"] = binary.Position;
                flItms["ImportTableRVA"] = reader.ReadUInt32();
                flItms["ImportTableSize"] = reader.ReadUInt32();
                flItms["ResourceTable"] = reader.ReadUInt64();
                flItms["ExceptionTable"] = reader.ReadUInt64();
                flItms["CertTableLOC"] = binary.Position;
                flItms["CertLOC"] = reader.ReadUInt32();
                flItms["CertSize"] = reader.ReadUInt32();
            }
            return flItms;
        }

        public static byte[] CopyCert(string exeFilePath)
        {
            var flItms = GatherFileInfoWin(exeFilePath);
            if (flItms["CertLOC"] == 0 || flItms["CertSize"] == 0)
            {
                return new byte[0];
            }
            using (var f = new FileStream(exeFilePath, FileMode.Open, FileAccess.Read))
            {
                f.Seek((long)flItms["CertLOC"], SeekOrigin.Begin);
                var cert = new byte[(int)flItms["CertSize"]];
                f.Read(cert, 0, (int)flItms["CertSize"]);
                return cert;
            }
        }

        public static void WriteCert(byte[] cert, string inputExe, string outputExe)
        {
            var flItms = GatherFileInfoWin(inputExe);
            if (string.IsNullOrEmpty(outputExe))
            {
                outputExe = $"{inputExe}_signed";
            }
            File.Copy(inputExe, outputExe, true);

            using (var g = new FileStream(inputExe, FileMode.Open, FileAccess.Read))
            using (var f = new FileStream(outputExe, FileMode.Open, FileAccess.Write))
            {
                var exeData = new byte[g.Length];
                g.Read(exeData, 0, exeData.Length);
                f.Write(exeData, 0, exeData.Length);
                f.Seek((long)flItms["CertTableLOC"], SeekOrigin.Begin);
                var exeLength = exeData.Length;
                f.Write(BitConverter.GetBytes(exeLength), 0, 4);
                f.Write(BitConverter.GetBytes(cert.Length), 0, 4);
                f.Seek(0, SeekOrigin.End);
                f.Write(cert, 0, cert.Length);
            }
        }

        public static void OutputCert(string exe, string output)
        {
            var cert = CopyCert(exe);
            if (string.IsNullOrEmpty(output))
            {
                // 默认输出路径为 exe 文件名加 _sig.p7b
                output = Path.ChangeExtension(exe, "_sig.p7b");
            }
            else if (!Path.HasExtension(output))
            {
                // 如果用户指定了 output 但没有扩展名，添加 .p7b
                output = output + ".p7b";
            }
            File.WriteAllBytes(output, cert);
        }

        public static bool CheckSig(string exe)
        {
            var flItms = GatherFileInfoWin(exe);
            if (flItms["CertLOC"] == 0 || flItms["CertSize"] == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool Truncate(string exe, string output)
        {
            var flItms = GatherFileInfoWin(exe);
            if (flItms["CertLOC"] == 0 || flItms["CertSize"] == 0)
            {
                return false;
            }
            if (string.IsNullOrEmpty(output))
            {
                output = $"{exe}_nosig";
            }
            File.Copy(exe, output, true);
            using (var binary = new FileStream(output, FileMode.Open, FileAccess.ReadWrite))
            {
                binary.Seek(-flItms["CertSize"], SeekOrigin.End);
                binary.SetLength(binary.Position);
                binary.Seek((long)flItms["CertTableLOC"], SeekOrigin.Begin);
                binary.Write(new byte[8], 0, 8); // 写入8字节的空数据，用以擦除证书表指针
            }
            return true;
        }

        public static void SignFile(string exe, string sigfile, string output)
        {
            var flItms = GatherFileInfoWin(exe);
            var cert = File.ReadAllBytes(sigfile);
            if (string.IsNullOrEmpty(output))
            {
                output = $"{exe}_signed";
            }

            File.Copy(exe, output, true);
            using (var g = new FileStream(exe, FileMode.Open, FileAccess.Read))
            using (var f = new FileStream(output, FileMode.Open, FileAccess.Write))
            {
                var exeData = new byte[g.Length];
                g.Read(exeData, 0, exeData.Length);
                f.Write(exeData, 0, exeData.Length);

                f.Seek((long)flItms["CertTableLOC"], SeekOrigin.Begin);
                f.Write(BitConverter.GetBytes(exeData.Length), 0, 4);
                f.Write(BitConverter.GetBytes(cert.Length), 0, 4);
                f.Seek(0, SeekOrigin.End);
                f.Write(cert, 0, cert.Length);
            }
        }
    }
}
