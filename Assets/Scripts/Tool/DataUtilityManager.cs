using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;

namespace Invariable
{
    public class DataUtilityManager
    {
#if !UNITY_EDITOR
    public static string m_platform = "Android";
    public static string m_localRootPath = Application.persistentDataPath + "/";
    public static string m_webRootPath = LoadWebDataTxt(0);
#else
        public static string m_platform = "Windows";//当前平台
        public static string m_localRootPath = Application.streamingAssetsPath.Replace("Assets/StreamingAssets", "");//本地数据根目录
        public static string m_webRootPath = "";//服务器数据根目录
#endif

        public static string m_configPath = m_localRootPath + "ConfigData";//存放Excel配置表的路径
        public static string m_webIpv4Str = LoadWebDataTxt(3);//服务器的公网地址
        public static int m_webPortInt = int.Parse(LoadWebDataTxt(4));//服务器用于连接客户端的端口号



        private class BypassCertificate : CertificateHandler
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

            string directoryPath = "";

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
            string username = LoadWebDataTxt(1);
            string password = LoadWebDataTxt(2);
            string encodedAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + password));

            requestHandler.SetRequestHeader("Authorization", "Basic " + encodedAuth);

            requestHandler.certificateHandler = new BypassCertificate();
        }

        private static string LoadWebDataTxt(int index)
        {
            string text = "";

            using (UnityWebRequest requestHandler = UnityWebRequest.Get(Application.streamingAssetsPath + "/WebData.txt"))
            {
                requestHandler.SendWebRequest();

                while (!requestHandler.isDone)
                {
                    // 等待请求完成
                }

                string[] des = requestHandler.downloadHandler.text.Split('\n');

                text = des[index].Replace("\r", "");
            }

            return text;
        }

        public static string GetConfigData(string configName, int id)
        {
            if (id <= 0)
            {
                return null;
            }

            int configFileId;

            if (id % 100 == 0)
            {
                configFileId = id;
            }
            else
            {
                configFileId = 100 - id % 100 + id;
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(DataUtilityManager.m_configPath + "/Client");

            string value = LoadConfigData(directoryInfo, configName, configFileId, id.ToString());

            return value;
        }

        public static string LoadConfigData(DirectoryInfo directoryInfo, string configName, int configFileId, string id)
        {
            string value = "";

            FileInfo[] fileInfos = directoryInfo.GetFiles();

            if (fileInfos.Length > 0)
            {
                foreach (var file in fileInfos)
                {
                    string name = Path.GetFileName(file.FullName);

                    if (name == configName + configFileId + ".bin")
                    {
                        using (FileStream fileStream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                        {
                            value = GetConfigStrByName(fileStream, configName, id);
                        }

                        return value;
                    }
                }
            }

            DirectoryInfo[] directoryInfos = directoryInfo.GetDirectories();

            if (directoryInfos.Length > 0)
            {
                foreach (var directory in directoryInfos)
                {
                    value = LoadConfigData(directory, configName, configFileId, id);

                    if (value != null)
                    {
                        break;
                    }
                }
            }

            return value;
        }

        public static string GetConfigStrByName(FileStream fileStream, string configName, string id)
        {
            string value = "";

            byte[] encryptBytes = ReadFileByteData(fileStream);
            byte[] compressedBytes;

            if (GetAesKeyAndIvByConfigName(configName, out byte[] key, out byte[] iv))
            {
                compressedBytes = DecryptByteData(encryptBytes, key, iv);
            }
            else
            {
                goto A;
            }

            byte[] decompressedBytes = DecompressByteData(compressedBytes);

            Dictionary<string, string> configData = Deserialize<Dictionary<string, string>>(decompressedBytes);

            if (configData.ContainsKey(id))
            {
                value = configData[id];
            }

        A:;

            return value;
        }

        public static byte[] ReadFileByteData(FileStream fileStream)
        {
            byte[] byteData = null;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                fileStream.CopyTo(memoryStream);
                byteData = memoryStream.ToArray();
            }

            return byteData;
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
            DataUtilityManager.InitDirectory(path);

            using (FileStream fileStream = new FileStream(path, FileMode.Create))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
                {
                    binaryWriter.Write(inputBytes);
                }
            }
        }

        public static void CreateTxtFile(string path, string content)
        {
            string suffix = Path.GetExtension(path);

            if (suffix != ".txt")
            {
                return;
            }

            DataUtilityManager.InitDirectory(path);

            using (FileStream fileStream = new FileStream(path, FileMode.Create))
            {
                using (StreamWriter swreamWriter = new StreamWriter(fileStream))
                {
                    swreamWriter.Write(content);
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

        public static byte[] GetRandomByteData(int size)
        {
            byte[] randomByteData = new byte[size];//size大小字节的随机数据

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomByteData);
            }

            return randomByteData;
        }

        public static void SaveConfigDecryptData(object data, string fileName)
        {
            if (data == null)
            {
                return;
            }

            byte[] inputBytes = SerializeData(data);
            byte[] compressBytes = CompressByteData(inputBytes);
            byte[] encryptBytes = EncryptByteData(compressBytes, Encoding.UTF8.GetBytes("95gbt368426hyb13"), Encoding.UTF8.GetBytes("i8g3451h5cxmj6rf"));

            string directoryPath = DataUtilityManager.m_configPath + "/ConfigDecryptData";

            DataUtilityManager.InitDirectory(directoryPath);

            CreateFileByBytes(directoryPath + "/" + fileName, encryptBytes);
        }

        public static bool GetAesKeyAndIvByConfigName(string ConfigName, out byte[] key, out byte[] iv)
        {
            string path = DataUtilityManager.m_configPath + "/ConfigDecryptData/AesKeyAndIvData.bin";

            if (!File.Exists(path))
            {
                goto A;
            }

            Dictionary<string, Dictionary<string, byte[]>> aesKeyAndIvData = GetFixedDecryptionDeviceDataByFileName<Dictionary<string, Dictionary<string, byte[]>>>(path);

            if (aesKeyAndIvData.ContainsKey(ConfigName))
            {
                key = aesKeyAndIvData[ConfigName]["Key"];
                iv = aesKeyAndIvData[ConfigName]["Iv"];
                return true;
            }

        A:;

            key = null;
            iv = null;

            return false;
        }

        public static T GetFixedDecryptionDeviceDataByFileName<T>(string path)
        {
            byte[] inputBytes = ReadFileByteData(path);
            byte[] decryptBytes = DecryptByteData(inputBytes, Encoding.UTF8.GetBytes("95gbt368426hyb13"), Encoding.UTF8.GetBytes("i8g3451h5cxmj6rf"));
            byte[] decompressedBytes = DecompressByteData(decryptBytes);

            T result = Deserialize<T>(decompressedBytes);

            return result;
        }
    }
}