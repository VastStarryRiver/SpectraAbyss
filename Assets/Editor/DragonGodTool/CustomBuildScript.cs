using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Invariable;



public class CustomBuildScript
{
    private static string m_rootPath = Application.streamingAssetsPath.Replace("Assets/StreamingAssets", "");
    private static Dictionary<string, string> m_filesContent = null;



    [MenuItem("GodDragonTool/打包流程/Android/一键导出所有热更新资源", false, -3)]
    public static void OneKeyExportAllAssets_Android()
    {
        BuildWebBinFile();

        ExportExcelTool.ExportExcelToDictionary();

        ExportDll.CopyInvariableDll();
        ExportDll.ExportUpdateDll();

        ExportAddressables.BuildAddressables_Android();

        CreateCatalogueFile();
    }

    [MenuItem("GodDragonTool/打包流程/Android/打包成APK文件", false, -2)]
    public static void PackageProject_Android()
    {
        string keystorePath = DataUtilityManager.m_localRootPath + "SpectraAbyss.keystore"; // Keystore 文件路径

        // 确保 keystore 文件存在
        if (!File.Exists(keystorePath))
        {
            Debug.LogError("Keystore文件不存在: " + keystorePath);
            return;
        }

        // 设置 keystore 信息
        PlayerSettings.Android.keystoreName = keystorePath;
        PlayerSettings.Android.keystorePass = "149630764"; // Keystore 密码
        PlayerSettings.Android.keyaliasName = "spectraabyss"; // Alias 名称
        PlayerSettings.Android.keyaliasPass = "149630764"; // Alias 密码

        PackageProject(BuildTarget.Android, Environment.GetFolderPath(Environment.SpecialFolder.Desktop).Replace("\\", "/") + "/SpectraAbyss.apk");
    }

    [MenuItem("GodDragonTool/打包流程/Windows/一键导出所有热更新资源", false, -1)]
    public static void OneKeyExportAllAssets_Windows()
    {
        ExportExcelTool.ExportExcelToDictionary();

        ExportDll.CopyInvariableDll();
        ExportDll.ExportUpdateDll();

        ExportAddressables.BuildAddressables_Windows();
    }



    private static void BuildWebBinFile()
    {
        using (FileStream fileStream = new FileStream(DataUtilityManager.m_localRootPath + "WebData.txt", FileMode.Open))
        {
            using (StreamReader streamReader = new StreamReader(fileStream))
            {
                DataUtilityManager.SaveSafeFile(streamReader.ReadToEnd(), Application.streamingAssetsPath + "/WebData.bin");
            }
        }

        AssetDatabase.Refresh();
    }

    private static void PackageProject(BuildTarget target, string locationPathName)
    {
        // 获取所有场景
        string[] scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);

        // 设置构建选项
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = locationPathName,//打包的输出路径
            target = target,
            options = BuildOptions.None
        };

        // 创建资源排除规则
        var originalCallback = EditorApplication.delayCall;

        EditorApplication.delayCall += () =>
        {
            try
            {
                ExportAddressables.SetProfileValue("Android");

                // 开始构建
                BuildPipeline.BuildPlayer(buildPlayerOptions);

                ExportAddressables.SetProfileValue("Windows");
            }
            finally
            {
                EditorApplication.delayCall = originalCallback;
            }
        };

        EditorApplication.delayCall();
    }

    private static void CreateCatalogueFile()
    {
        string dir = m_rootPath + "CatalogueFiles";

        if (Directory.Exists(dir))
        {
            Directory.Delete(dir, true);
        }

        DataUtilityManager.InitDirectory(dir);

        using (FileStream fs = new FileStream(dir + "/CatalogueFile.txt", FileMode.Create))
        {
            using (StreamWriter sw = new StreamWriter(fs))
            {
                SetMd5Files(m_rootPath + "Config/Client");
                SetMd5Files(m_rootPath + "Dll");
                SetMd5Files(m_rootPath + "Assets/UpdateAssets");

                if (m_filesContent != null && m_filesContent.Count > 0)
                {
                    sw.Write(JsonConvert.SerializeObject(m_filesContent));
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

            if (suffix == ".meta" || suffix == ".json")
            {
                continue;
            }

            string fullPath = directoryPath + "/" + nextFile.Name;
            string savePath = fullPath.Replace(m_rootPath, "");

            m_filesContent ??= new Dictionary<string, string>();

            m_filesContent.Add(savePath, Get32MD5(nextFile.OpenText().ReadToEnd()));
        }

        //遍历文件夹
        foreach (DirectoryInfo nextFolder in folder.GetDirectories())
        {
            if (nextFolder.Name == ".idea")
            {
                continue;
            }

            SetMd5Files(directoryPath + "/" + nextFolder.Name);
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