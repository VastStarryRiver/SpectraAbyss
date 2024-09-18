using UnityEditor;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using LitJson;



public static class ExportCatalogueFile
{
    private static string m_rootPath = Application.streamingAssetsPath.Replace("Assets/StreamingAssets", "");
    private static string m_clientConfigPath = m_rootPath + "ConfigData/Client";
    private static string m_serverConfigPath = m_rootPath + "ConfigData/Server";
    private static string m_aesKeyAndIvDataPath = m_rootPath + "ConfigData/ConfigDecryptData";
    private static string m_catalogueFilePath_Windows = m_rootPath + "CatalogueFiles/Windows";
    private static string m_catalogueFilePath_Android = m_rootPath + "CatalogueFiles/Android";
    private static string m_luaPath = m_rootPath + "Assets/Lua";

    private static Dictionary<string,string> m_filesContent = null;



    [MenuItem("GodDragonTool/导出热更新目录文件/BuildCatalogueFile_Windows")]
    public static void BuildCatalogueFile_Windows()
    {
        CreeateFiles(m_catalogueFilePath_Windows);
    }

    [MenuItem("GodDragonTool/导出热更新目录文件/BuildCatalogueFile_Android")]
    public static void BuildCatalogueFile_Android()
    {
        CreeateFiles(m_catalogueFilePath_Android);
    }

    [MenuItem("GodDragonTool/打包流程/一键导出所有热更新资源")]
    public static void OneKeyExportAllAssets()
    {
        ExportExcelTool.ExportExcelDataToLuaTableString();
        AtlasBuilder.PackSpriteAtlas();
        ExportAssetBundle.BuildAssetBundles_Windows();
        ExportAssetBundle.BuildAssetBundles_Android();
        BuildCatalogueFile_Windows();
        BuildCatalogueFile_Android();
    }



    private static void CreeateFiles(string catalogueDirectoryPath)
    {
        DataUtilityManager.InitDirectory(catalogueDirectoryPath);

        using (FileStream fs = new FileStream(catalogueDirectoryPath + "/CatalogueFile.txt", FileMode.Create))
        {
            using (StreamWriter sw = new StreamWriter(fs))
            {
                SetMd5Files(m_clientConfigPath);
                SetMd5Files(m_serverConfigPath);
                SetMd5Files(m_aesKeyAndIvDataPath);
                SetMd5Files(m_luaPath);
                SetMd5Files(catalogueDirectoryPath.Replace("CatalogueFiles", "AssetBundles"));

                if (m_filesContent != null && m_filesContent.Count > 0)
                {
                    sw.Write(JsonMapper.ToJson(m_filesContent));
                    m_filesContent.Clear();
                }

                m_filesContent = null;
            }
        }
    }

    private static void SetMd5Files(string directoryPath)
    {
        DirectoryInfo folder = new DirectoryInfo(directoryPath);

        //遍历文件
        foreach (FileInfo nextFile in folder.GetFiles())
        {
            string suffix = Path.GetExtension(nextFile.Name);

            if (suffix == ".meta" || nextFile.Name == ".emmyrc.json")
            {
                goto A;
            }

            string fullPath = directoryPath + "/" + nextFile.Name;
            string savePath = fullPath.Replace(m_rootPath, "");

            if (m_filesContent == null)
            {
                m_filesContent = new Dictionary<string, string>();
            }

            m_filesContent.Add(savePath, Get32MD5(nextFile.OpenText().ReadToEnd()));

        A:;
        }

        //遍历文件夹
        foreach (DirectoryInfo nextFolder in folder.GetDirectories())
        {
            if (nextFolder.Name == ".idea")
            {
                goto B;
            }

            SetMd5Files(directoryPath + "/" + nextFolder.Name);

        B:;
        }
    }

    private static string Get32MD5(string content)
    {
        MD5 md5 = MD5.Create();

        StringBuilder stringBuilder = new StringBuilder();

        byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(content)); //该方法的参数也可以传入Stream

        for (int i = 0; i < bytes.Length; i++)
        {
            stringBuilder.Append(bytes[i].ToString("X2"));
        }

        string md5Str = stringBuilder.ToString();

        return md5Str;
    }
}