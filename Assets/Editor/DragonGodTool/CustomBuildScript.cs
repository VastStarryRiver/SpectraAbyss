using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using Invariable;



public class CustomBuildScript
{
    private static string keystorePath = DataUtilityManager.m_localRootPath + "SpectraAbyss.keystore"; // Keystore 文件路径
    private const string keystorePassword = "149630764"; // Keystore 密码
    private const string keyAlias = "spectraabyss"; // Alias 名称
    private const string keyAliasPassword = "149630764"; // Alias 密码
    private static string m_locationPathName = Environment.GetFolderPath(Environment.SpecialFolder.Desktop).Replace("\\","/") + "/SpectraAbyss.apk";//打包的输出路径



    [MenuItem("GodDragonTool/打包流程/一键导出所有Android热更新资源", false, -3)]
    public static void OneKeyExportAllAssets_Android()
    {
        ExportExcelTool.ExportExcelToDictionary();

        ExportDll.CopyInvariableDll();
        ExportDll.ExportUpdateDll();

        ExportAddressables.BuildAddressables_Android();
        ExportCatalogueFile.BuildCatalogueFile_Android();
    }

    [MenuItem("GodDragonTool/打包流程/一键导出所有Windows热更新资源", false, -2)]
    public static void OneKeyExportAllAssets_Windows()
    {
        ExportExcelTool.ExportExcelToDictionary();

        ExportDll.CopyInvariableDll();
        ExportDll.ExportUpdateDll();

        ExportAddressables.BuildAddressables_Windows();
        ExportCatalogueFile.BuildCatalogueFile_Windows();
    }

    [MenuItem("GodDragonTool/打包流程/打包成APK文件", false, -1)]
    public static void PackageProject_Android()
    {
        // 确保 keystore 文件存在
        if (!File.Exists(keystorePath))
        {
            Debug.LogError("Keystore文件不存在: " + keystorePath);
            return;
        }

        // 设置 keystore 信息
        PlayerSettings.Android.keystoreName = keystorePath;
        PlayerSettings.Android.keystorePass = keystorePassword;
        PlayerSettings.Android.keyaliasName = keyAlias;
        PlayerSettings.Android.keyaliasPass = keyAliasPassword;

        PackageProject(BuildTarget.Android);
    }



    private static void PackageProject(BuildTarget target)
    {
        // 获取所有场景
        string[] scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);

        // 设置构建选项
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = m_locationPathName,
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
}