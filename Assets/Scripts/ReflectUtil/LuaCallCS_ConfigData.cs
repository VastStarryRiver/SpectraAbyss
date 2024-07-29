using UnityEngine;
using LuaInterface;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;



public static partial class LuaCallCS
{
    public static LuaTable LoadConfigData(string configName, int id)
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

        LuaTable luaTable = LoadConfigData(directoryInfo, configName, configFileId, id.ToString());

        return luaTable;
    }

    public static LuaTable LoadConfigData(string configName, string index)
    {
        if (string.IsNullOrEmpty(index))
        {
            return null;
        }

        if (int.TryParse(index, out int result))
        {
            return LoadConfigData(configName, result);
        }

        if (GetKeysDataByConfigName(configName, out List<string> keys, out int configFileId, index))
        {
            if (configFileId != 0)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(DataUtilityManager.m_configPath + "/Client");

                LuaTable luaTable = LoadConfigData(directoryInfo, configName, configFileId, index);

                return luaTable;
            }
        }

        return null;
    }

    public static LuaTable LoadConfigData(DirectoryInfo directoryInfo, string configName, int configFileId, string id)
    {
        LuaTable luaTable = null;

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
                        string luaTableStr = GetLuaTableStr(fileStream, configName, id);
                        luaTable = LuaManager.Instance.m_luaState.DoString<LuaTable>(luaTableStr);
                    }

                    return luaTable;
                }
            }
        }

        DirectoryInfo[] directoryInfos = directoryInfo.GetDirectories();

        if (directoryInfos.Length > 0)
        {
            foreach (var directory in directoryInfos)
            {
                luaTable = LoadConfigData(directory, configName, configFileId, id);

                if (luaTable != null)
                {
                    break;
                }
            }
        }

        return luaTable;
    }

    public static string GetLuaTableStr(FileStream fileStream, string configName, string id)
    {
        string luaTableStr = "";

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
            luaTableStr = configData[id];
        }

    A:;

        return luaTableStr;
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

    public static bool GetKeysDataByConfigName(string configName, out List<string> keys, out int configFileId, string index = "")
    {
        keys = null;
        configFileId = 0;

        string path = DataUtilityManager.m_configPath + "/ConfigDecryptData/" + configName + "Keys.bin";

        if (!File.Exists(path))
        {
            goto A;
        }

        Dictionary<string, int> keysData = GetFixedDecryptionDeviceDataByFileName<Dictionary<string, int>>(path);

        if (keysData.Count > 0)
        {
            if (string.IsNullOrEmpty(index))
            {
                keys = new List<string>();

                foreach (var item in keysData)
                {
                    keys.Add(item.Key);
                }
            }
            else
            {
                configFileId = keysData[index];

                if (configFileId % 100 != 0)
                {
                    configFileId = 100 - configFileId % 100 + configFileId;
                }
            }

            return true;
        }

    A:;

        return false;
    }

    public static List<string> GetConfigListKeysByName(string configName)
    {
        List<string> result = null;

        if (GetKeysDataByConfigName(configName, out List<string> keys, out int configFileId))
        {
            result = keys;
        }

        return result;
    }

    public static LuaTable GetConfigLuaTableKeysByName(string configName)
    {
        LuaTable result = null;

        if (GetKeysDataByConfigName(configName, out List<string> keys, out int configFileId))
        {
            if (keys != null && keys.Count > 0)
            {
                StringBuilder content = new StringBuilder();

                content.Append("local data = {");

                for (int i = 0; i < keys.Count; i++)
                {
                    content.Append("\r\n\t[");
                    content.Append(i + 1);
                    content.Append("] = \"");
                    content.Append(keys[i]);
                    content.Append("\",");
                }

                content.Append("}\r\n\nreturn data");

                result = LuaManager.Instance.m_luaState.DoString<LuaTable>(content.ToString());
            }
        }

        return result;
    }

    public static bool GetTextureRectByAtlasName(string atlasName, string textureName, out float[] rect)
    {
        rect = new float[4];

        string path = DataUtilityManager.m_configPath + "/ConfigDecryptData/" + atlasName + ".bin";

        if (!File.Exists(path))
        {
            goto A;
        }

        Dictionary<string, float[]> textureRect = GetFixedDecryptionDeviceDataByFileName<Dictionary<string, float[]>>(path);

        if (textureRect.ContainsKey(textureName))
        {
            rect = textureRect[textureName];
            return true;
        }

    A:;

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