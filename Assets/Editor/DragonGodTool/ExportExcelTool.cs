using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ExcelDataReader;

public class ExportExcelTool
{
    private static Dictionary<string, Dictionary<string, byte[]>> m_aesKeyAndIvDatas = null;
    private static Dictionary<string, Dictionary<string, int>> m_configKeys = null;



    [MenuItem("GodDragonTool/导出Excel表的配置数据")]
    public static void ExportExcelDataToLuaTableString()
    {
        if (Directory.Exists(DataUtilityManager.m_configPath))
        {
            Directory.Delete(DataUtilityManager.m_configPath, true);
        }

        if (m_aesKeyAndIvDatas == null)
        {
            m_aesKeyAndIvDatas = new Dictionary<string, Dictionary<string, byte[]>>();
        }

        if (m_configKeys == null)
        {
            m_configKeys = new Dictionary<string, Dictionary<string, int>>();
        }

        string configPath = DataUtilityManager.m_localRootPath + "ConfigFile";
        DirectoryInfo directoryInfo = new DirectoryInfo(configPath);

        FileInfo[] fileInfos = directoryInfo.GetFiles();

        foreach (var item in fileInfos)
        {
            string excelPath = item.FullName.Replace("\\", "/");

            using (FileStream fileStream = new FileStream(excelPath, FileMode.Open))
            {
                IExcelDataReader excelReader = null;

                if (excelPath.EndsWith(".xls"))
                {
                    excelReader = ExcelReaderFactory.CreateBinaryReader(fileStream);
                }
                else if (excelPath.EndsWith(".xlsx"))
                {
                    excelReader = ExcelReaderFactory.CreateOpenXmlReader(fileStream);
                }

                if (excelReader != null)
                {
                    //判断Excel文件中是否存在至少一张数据表
                    if (excelReader.ResultsCount > 0)
                    {
                        LoadExcelRowData(excelReader);
                    }

                    excelReader.Dispose();
                    excelReader.Close();
                }
            }
        }

        if (m_aesKeyAndIvDatas.Count > 0)
        {
            LuaCallCS.SaveConfigDecryptData(m_aesKeyAndIvDatas, "AesKeyAndIvData.bin");
            m_aesKeyAndIvDatas.Clear();
        }

        if (m_configKeys.Count > 0)
        {
            foreach (var item in m_configKeys)
            {
                LuaCallCS.SaveConfigDecryptData(item.Value, item.Key + "Keys.bin");
            }

            m_configKeys.Clear();
        }

        EditorUtility.ClearProgressBar();

        AssetDatabase.Refresh();
    }



    private static void LoadExcelRowData(IExcelDataReader excelReader)
    {
        List<int> clientColumnIndex = new List<int>();
        List<int> serverColumnIndex = new List<int>();

        List<string> fieldNameList = new List<string>();
        List<string> dataTypeList = new List<string>();

        Dictionary<string, string> clientConfigData = new Dictionary<string, string>();
        Dictionary<string, string> serverConfigData = new Dictionary<string, string>();

        do
        {
            clientColumnIndex.Clear();
            serverColumnIndex.Clear();

            fieldNameList.Clear();
            dataTypeList.Clear();

            clientConfigData.Clear();
            serverConfigData.Clear();

            int indexColumnIndex = 1;
            int maxCol = 0;

            Dictionary<string, int> configKey = new Dictionary<string, int>();

            while (excelReader.Read()/*下一行*/)
            {
                List<string> columnData = LoadExcelColumnData(excelReader);

                if (excelReader.Depth == 0)
                {
                    continue;
                }
                else if (excelReader.Depth == 1)
                {
                    for (int i = 0; i < columnData.Count; i++)
                    {
                        if (columnData[i] == "1")
                        {
                            clientColumnIndex.Add(i);
                        }
                        else if (columnData[i] == "2")
                        {
                            serverColumnIndex.Add(i);
                        }
                        else if (columnData[i] == "3")
                        {
                            clientColumnIndex.Add(i);
                            serverColumnIndex.Add(i);
                        }
                    }

                    int clientMaxCol = 0;
                    int serverMaxCol = 0;

                    if (clientColumnIndex.Count > 0)
                    {
                        clientMaxCol = clientColumnIndex[clientColumnIndex.Count - 1];
                    }

                    if (serverColumnIndex.Count > 0)
                    {
                        serverMaxCol = serverColumnIndex[serverColumnIndex.Count - 1];
                    }

                    maxCol = Mathf.Max(clientMaxCol, serverMaxCol);

                    continue;
                }
                else if (excelReader.Depth == 2)
                {
                    fieldNameList = columnData;

                    for (int i = 0; i < columnData.Count; i++)
                    {
                        if (columnData[i] == "Index")
                        {
                            indexColumnIndex = i;
                            break;
                        }
                    }

                    continue;
                }
                else if (excelReader.Depth == 3)
                {
                    dataTypeList = columnData;
                    continue;
                }

                if (columnData[0] == "NO")
                {
                    continue;
                }

                string dataIndex = "";

                StringBuilder clientContent = new StringBuilder();
                StringBuilder serverContent = new StringBuilder();

                for (int i = 1; i < columnData.Count; i++)
                {
                    if (i == indexColumnIndex)
                    {
                        dataIndex = columnData[i];
                    }

                    bool isEnd1 = false;
                    bool isEnd2 = i >= maxCol;

                    if (clientColumnIndex.Contains(i))
                    {
                        isEnd1 = LoadExcelData(excelReader, i, dataIndex, columnData, clientColumnIndex, ref clientContent, fieldNameList, dataTypeList, ref clientConfigData, "Client", ref configKey);
                    }

                    if (serverColumnIndex.Contains(i))
                    {
                        isEnd1 = LoadExcelData(excelReader, i, dataIndex, columnData, serverColumnIndex, ref serverContent, fieldNameList, dataTypeList, ref serverConfigData, "Server", ref configKey);
                    }

                    if (isEnd1 && isEnd2)
                    {
                        if (configKey.Count > 0)
                        {
                            m_configKeys[excelReader.Name] = configKey;
                        }

                        goto over;
                    }
                }

                EditorUtility.DisplayProgressBar("配置表" + excelReader.Name + "正在导出数据中", "导出进度" + (excelReader.Depth + 1) + "/" + excelReader.RowCount, (excelReader.Depth + 1) * 1.0f / excelReader.RowCount);
            }

        over:;
        }
        while (excelReader.NextResult()/*下一张表*/);
    }

    private static List<string> LoadExcelColumnData(IExcelDataReader excelReader)
    {
        List<string> columnData = new List<string>();

        for (int i = 0; i < excelReader.FieldCount; i++)
        {
            string value;

            try
            {
                value = excelReader.GetString(i);
            }
            catch
            {
                value = excelReader.GetDouble(i).ToString();
            }

            columnData.Add(value);
        }

        return columnData;
    }

    private static bool LoadExcelData(IExcelDataReader excelReader, int columnIndex, string dataIndex, List<string> columnData, List<int> platformColumnIndex, ref StringBuilder content, List<string> fieldNameList, List<string> dataTypeList, ref Dictionary<string, string> platformConfigData, string platformName, ref Dictionary<string, int> configKey)
    {
        if (columnIndex == platformColumnIndex[0])
        {
            content.Append("local data = {");
        }

        content.Append("\r\n\t[\"");
        content.Append(fieldNameList[columnIndex]);

        if (dataTypeList[columnIndex] == "Number")
        {
            content.Append("\"] = ");
            content.Append(columnData[columnIndex]);
            content.Append(",");
        }
        else if (dataTypeList[columnIndex] == "String")
        {
            content.Append("\"] = \"");
            content.Append(columnData[columnIndex]);
            content.Append("\",");
        }

        if (columnIndex == platformColumnIndex[platformColumnIndex.Count - 1])
        {
            content.Append("\r\n}\r\n\nreturn data");

            if (!string.IsNullOrEmpty(dataIndex))
            {
                platformConfigData.Add(dataIndex, content.ToString());
                configKey[dataIndex] = platformConfigData.Count;
            }

            if (platformConfigData.Count >= 100 || excelReader.Depth + 1 == excelReader.RowCount)
            {
                SaveConfigData(excelReader, ref platformConfigData, platformName);
            }

            if (columnData[0] == "END")
            {
                return true;
            }
        }

        return false;
    }

    private static void SaveConfigData(IExcelDataReader excelReader, ref Dictionary<string, string> configData, string platform)
    {
        if (configData.Count <= 0)
        {
            return;
        }

        string configName = excelReader.Name;

        if (!m_aesKeyAndIvDatas.ContainsKey(configName))
        {
            Dictionary<string, byte[]> aesKeyAndIvData = new Dictionary<string, byte[]>();

            byte[] key = LuaCallCS.GetRandomByteData(16);
            byte[] iv = LuaCallCS.GetRandomByteData(16);

            aesKeyAndIvData.Add("Key", key);
            aesKeyAndIvData.Add("Iv", iv);

            m_aesKeyAndIvDatas.Add(configName, aesKeyAndIvData);
        }

        byte[] serializeBytes = LuaCallCS.SerializeData(configData);
        byte[] compressBytes = LuaCallCS.CompressByteData(serializeBytes);
        byte[] encryptBytes = LuaCallCS.EncryptByteData(compressBytes, m_aesKeyAndIvDatas[configName]["Key"], m_aesKeyAndIvDatas[configName]["Iv"]);

        string directoryPath = DataUtilityManager.m_configPath + "/" + platform + "/" + configName;

        DataUtilityManager.InitDirectory(directoryPath);

        int fileIndex = excelReader.Depth - 3;

        if (fileIndex % 100 != 0)
        {
            fileIndex = 100 - fileIndex % 100 + fileIndex;
        }

        CreateBinFile(directoryPath + "/" + configName + fileIndex + ".bin", encryptBytes);

        configData.Clear();
    }

    private static void CreateBinFile(string path, byte[] inputBytes)
    {
        LuaCallCS.CreateFileByBytes(path, inputBytes);
        AssetDatabase.Refresh();
    }

    private static void CreateTxtFile(string path, string content)
    {
        LuaCallCS.CreateTxtFile(path, content);
        AssetDatabase.Refresh();
    }
}