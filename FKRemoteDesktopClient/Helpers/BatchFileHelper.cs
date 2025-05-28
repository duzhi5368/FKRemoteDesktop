using System.IO;
using System.Text;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Helpers
{
    public static class BatchFileHelper
    {
        // 创建卸载本软件的批处理文件
        public static string CreateUninstallBatch(string currentFilePath)
        {
            string batchFile = FileHelper.GetTempFilePath(".bat");
            string uninstallBatch =
                "@echo off" + "\r\n" +
                "chcp 65001" + "\r\n" + // 用来支持中文字符
                "echo DONT CLOSE THIS WINDOW!" + "\r\n" +
                "ping -n 10 localhost > nul" + "\r\n" +
                "del /a /q /f " + "\"" + currentFilePath + "\"" + "\r\n" +
                "del /a /q /f " + "\"" + batchFile + "\"";

            File.WriteAllText(batchFile, uninstallBatch, new UTF8Encoding(false));
            return batchFile;
        }

        // 创建更新本软件的批处理文件
        public static string CreateUpdateBatch(string currentFilePath, string newFilePath)
        {
            string batchFile = FileHelper.GetTempFilePath(".bat");
            string updateBatch =
                "@echo off" + "\r\n" +
                "chcp 65001" + "\r\n" + // 用来支持中文字符
                "echo DONT CLOSE THIS WINDOW!" + "\r\n" +
                "ping -n 10 localhost > nul" + "\r\n" +
                "del /a /q /f " + "\"" + currentFilePath + "\"" + "\r\n" +
                "move /y " + "\"" + newFilePath + "\"" + " " + "\"" + currentFilePath + "\"" + "\r\n" +
                "start \"\" " + "\"" + currentFilePath + "\"" + "\r\n" +
                "del /a /q /f " + "\"" + batchFile + "\"";

            File.WriteAllText(batchFile, updateBatch, new UTF8Encoding(false));
            return batchFile;
        }

        // 创建重启本软件的批处理文件
        public static string CreateRestartBatch(string currentFilePath)
        {
            string batchFile = FileHelper.GetTempFilePath(".bat");
            string restartBatch =
                "@echo off" + "\r\n" +
                "chcp 65001" + "\r\n" + // 用来支持中文字符
                "echo DONT CLOSE THIS WINDOW!" + "\r\n" +
                "ping -n 10 localhost > nul" + "\r\n" +
                "start \"\" " + "\"" + currentFilePath + "\"" + "\r\n" +
                "del /a /q /f " + "\"" + batchFile + "\"";

            File.WriteAllText(batchFile, restartBatch, new UTF8Encoding(false));
            return batchFile;
        }
    }
}