using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;

namespace Invariable
{
    public static class DataUtilityManager
    {
#if UNITY_EDITOR
        public static string m_platform = "macOS";
        public static string m_localRootPath = Application.streamingAssetsPath.Replace("Assets/StreamingAssets", "");//本地数据根目录
#else
        public static string m_platform = "iOS";
        public static string m_localRootPath = Application.persistentDataPath + "/";
#endif

        private static string[] m_webData = null;

        public static string WebRootPath//热更新根目录
        {
            get
            {
                return LoadWebData(0);
            }
        }

        public static string WebIpv4Str//服务器的公网地址
        {
            get
            {
                string[] list = LoadWebData(1).Split(":");
                return list[0];
            }
        }

        public static int WebPortInt//服务器用于连接客户端的端口号
        {
            get
            {
                string[] list = LoadWebData(1).Split(":");
                return int.Parse(list[1]);
            }
        }



        public class BypassCertificate : CertificateHandler
        {
            protected override bool ValidateCertificate(byte[] certificateData)
            {
                return true; // 始终返回 true 以忽略证书验证
            }
        }

        public static void InitDirectory(string path)
        {
            path = path.Replace("\\", "/");

            string extension = Path.GetExtension(path);

            string directoryPath;

            if (string.IsNullOrEmpty(extension))
            {
                directoryPath = path;
            }
            else
            {
                directoryPath = Path.GetDirectoryName(path);
            }

            if (!Directory.Exists(directoryPath))
            {
                //确保路径中的所有文件夹都存在
                Directory.CreateDirectory(directoryPath);
            }
        }

        public static void SetWebQuestData(ref UnityWebRequest requestHandler)
        {
            string username = LoadWebData(2);
            string password = LoadWebData(3);
            string encodedAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + password));

            requestHandler.SetRequestHeader("Authorization", "Basic " + encodedAuth);

            requestHandler.certificateHandler = new BypassCertificate();
        }

        public static string LoadWebData(int index)
        {
            if(m_webData == null)
            {
                using (UnityWebRequest requestHandler = UnityWebRequest.Get(Application.streamingAssetsPath + "/WebData.bin"))
                {
                    requestHandler.SendWebRequest();

                    while (!requestHandler.isDone)
                    {
                        // 等待请求完成
                    }

                    m_webData = ReadSafeFile<string>(requestHandler.downloadHandler.data).Split('\n');
                }
            }

            string text = m_webData[index].Replace("\r", "");

            return text;
        }

        public static string GetConfigData(string configName, int id)
        {
            return "";
        }

        public static byte[] ReadFileByteData(string path)
        {
            byte[] byteData = null;

            using (FileStream encryptFileStream = new FileStream(path, FileMode.Open))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    encryptFileStream.CopyTo(memoryStream);
                    byteData = memoryStream.ToArray();
                }
            }

            return byteData;
        }

        public static void CreateFileByBytes(string path, byte[] inputBytes)
        {
            InitDirectory(path);

            using (FileStream fileStream = new FileStream(path, FileMode.Create))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
                {
                    binaryWriter.Write(inputBytes);
                }
            }
        }

        public static byte[] SerializeData(object data)
        {
            byte[] serializeBytes = null;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();

                binaryFormatter.Serialize(memoryStream, data);

                serializeBytes = memoryStream.ToArray();
            }

            return serializeBytes;
        }

        public static T Deserialize<T>(byte[] inputBytes)
        {
            T result = default(T);

            using (MemoryStream memoryStream = new MemoryStream(inputBytes))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();

                result = (T)binaryFormatter.Deserialize(memoryStream);
            }

            return result;
        }

        public static byte[] CompressByteData(byte[] inputBytes)
        {
            byte[] compressBytes = null;

            using (MemoryStream compressMemoryStream = new MemoryStream())
            {
                using (GZipStream compressionStream = new GZipStream(compressMemoryStream, CompressionMode.Compress))
                {
                    compressionStream.Write(inputBytes, 0, inputBytes.Length);
                }

                compressBytes = compressMemoryStream.ToArray();
            }

            return compressBytes;
        }

        public static byte[] DecompressByteData(byte[] inputBytes)
        {
            byte[] decompressedBytes = null;

            using (MemoryStream compressedMemoryStream = new MemoryStream(inputBytes))
            {
                using (GZipStream compressionStream = new GZipStream(compressedMemoryStream, CompressionMode.Decompress))
                {
                    using (MemoryStream decompressedMemoryStream = new MemoryStream())
                    {
                        compressionStream.CopyTo(decompressedMemoryStream);
                        decompressedBytes = decompressedMemoryStream.ToArray();
                    }
                }
            }

            return decompressedBytes;
        }

        public static byte[] EncryptByteData(byte[] inputBytes, byte[] key, byte[] iv)
        {
            byte[] encryptBytes = null;

            using (AesManaged aes = new AesManaged())
            {
                aes.Key = key;
                aes.IV = iv;

                using (ICryptoTransform encryptor = aes.CreateEncryptor(key, iv))
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(inputBytes, 0, inputBytes.Length);
                            cryptoStream.FlushFinalBlock();//加密会将最后一个数据块填充为满块(需要)，解密会删除填充的数据块(不需要)
                        }

                        encryptBytes = memoryStream.ToArray();
                    }
                }
            }

            return encryptBytes;
        }

        public static byte[] DecryptByteData(byte[] inputBytes, byte[] key, byte[] iv)
        {
            byte[] decryptBytes = null;

            using (MemoryStream inputMemoryStream = new MemoryStream(inputBytes))
            {
                using (AesManaged aes = new AesManaged())
                {
                    aes.Key = key;
                    aes.IV = iv;

                    using (ICryptoTransform decryptor = aes.CreateDecryptor(key, iv))
                    {
                        using (MemoryStream outputMemoryStream = new MemoryStream())
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(inputMemoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                cryptoStream.CopyTo(outputMemoryStream);
                            }

                            decryptBytes = outputMemoryStream.ToArray();
                        }
                    }
                }
            }

            return decryptBytes;
        }

        public static void SaveSafeFile(object data, string filePath)
        {
            if (data == null)
            {
                return;
            }

            byte[] inputBytes = SerializeData(data);
            byte[] compressBytes = CompressByteData(inputBytes);
            byte[] encryptBytes = EncryptByteData(compressBytes, Encoding.UTF8.GetBytes("86thj449526ujy76"), Encoding.UTF8.GetBytes("i8g7359h5yhtj8hr"));

            CreateFileByBytes(filePath, encryptBytes);
        }

        public static T ReadSafeFile<T>(string path)
        {
            byte[] inputBytes = ReadFileByteData(path);
            byte[] decryptBytes = DecryptByteData(inputBytes, Encoding.UTF8.GetBytes("86thj449526ujy76"), Encoding.UTF8.GetBytes("i8g7359h5yhtj8hr"));
            byte[] decompressedBytes = DecompressByteData(decryptBytes);

            T result = Deserialize<T>(decompressedBytes);

            return result;
        }

        public static T ReadSafeFile<T>(byte[] inputBytes)
        {
            byte[] decryptBytes = DecryptByteData(inputBytes, Encoding.UTF8.GetBytes("86thj449526ujy76"), Encoding.UTF8.GetBytes("i8g7359h5yhtj8hr"));
            byte[] decompressedBytes = DecompressByteData(decryptBytes);

            T result = Deserialize<T>(decompressedBytes);

            return result;
        }

        public static string FormatFileByteSize(long bytes)
        {
            string[] units = { "B", "KB", "MB", "G", "T" };
            int unitIndex = 0;
            double size = bytes;

            while (size >= 1024 && unitIndex < units.Length - 1)
            {
                size /= 1024;
                unitIndex++;
            }

            return $"{size:0.##} {units[unitIndex]}";
        }
    }
}