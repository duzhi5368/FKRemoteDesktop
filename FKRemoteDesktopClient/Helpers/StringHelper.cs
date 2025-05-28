using FKRemoteDesktop.Utilities;
using System;
using System.Text;
using System.Text.RegularExpressions;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Helpers
{
    public static class StringHelper
    {
        // 生成随机字符串的字母表
        private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        // 可读的文件大小类型
        private static readonly string[] Sizes = { "B", "KB", "MB", "GB", "TB", "PB" };

        // 随机数字生成器
        private static readonly SafeRandom Random = new SafeRandom();

        // 生成指定长度的随机字符串
        public static string GetRandomString(int length)
        {
            StringBuilder randomName = new StringBuilder(length);
            for (int i = 0; i < length; i++)
                randomName.Append(Alphabet[Random.Next(Alphabet.Length)]);

            return randomName.ToString();
        }

        // 转换文件大小为可读风格
        public static string GetHumanReadableFileSize(long size)
        {
            double len = size;
            int order = 0;
            while (len >= 1024 && order + 1 < Sizes.Length)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {Sizes[order]}";
        }

        // 获取格式化的MAC地址
        public static string GetFormattedMacAddress(string macAddress)
        {
            return (macAddress.Length != 12)
                ? "00:00:00:00:00:00"
                : Regex.Replace(macAddress, "(.{2})(.{2})(.{2})(.{2})(.{2})(.{2})", "$1:$2:$3:$4:$5:$6");
        }

        // 删除字符串中最后N个字符
        public static string RemoveLastChars(string input, int amount = 2)
        {
            if (input.Length > amount)
                input = input.Remove(input.Length - amount);
            return input;
        }

        // 将过长的字符串显示为 XX...XX 的方式
        public static string ToShortString(string input)
        {
            if (string.IsNullOrEmpty(input) || input.Length <= 8)
                return input;

            return input.Substring(0, 4) + "..." + input.Substring(input.Length - 4, 4);
        }


        public static byte[] SubArray(byte[] data, int index, int length)
        {
            byte[] result = new byte[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
    }
}