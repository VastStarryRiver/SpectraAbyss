using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using Invariable;



public class CustomBuildScript
{
    private static List<string> m_excludedFolders = new List<string> { "Assets/Png", "Assets/Textures" };// 设置需要排除的文件夹
    private static string m_scriptsSourcePath = Application.dataPath + "/Scripts/HotUpdate";
    private static string m_scriptsTargetPath = Application.dataPath.Replace("/Assets", "/HotUpdateScripts/HotUpdate");
    private static HashSet<string> m_excludedAssets = new HashSet<string>();// 收集所有需要排除的资源路径
    private static string keystorePath = DataUtilityManager.m_localRootPath + "SpectraAbyss.keystore"; // Keystore 文件路径
    private const string keystorePassword = "149630764"; // Keystore 密码
    private const string keyAlias = "spectraabyss"; // Alias 名称
    private const string keyAliasPassword = "149630764"; // Alias 密码
    private static string m_locationPathName = Environment.GetFolderPath(Environment.SpecialFolder.Desktop).Replace("\\","/") + "/SpectraAbyss.apk";//打包的输出路径



    [MenuItem("GodDragonTool/打包流程/打包成APK文件")]
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
        m_excludedAssets.Clear();

        foreach (string folder in m_excludedFolders)
        {
            string[] assetPaths = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);

            foreach (string assetPath in assetPaths)
            {
                if (Path.GetExtension(assetPath) == ".meta")// 忽略.meta文件
                {
                    continue;
                }

                string relativePath = assetPath.Replace(Application.dataPath, "Assets");
                m_excludedAssets.Add(relativePath);
            }
        }

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
                foreach (var assetPath in m_excludedAssets)
                {
                    AssetImporter importer = AssetImporter.GetAtPath(assetPath);

                    if (importer != null)
                    {
                        importer.SetAssetBundleNameAndVariant("tempAssetBundle", "");
                    }
                }

                // 开始构建
                BuildPipeline.BuildPlayer(buildPlayerOptions);

                // 构建完成后清理标记
                foreach (var assetPath in m_excludedAssets)
                {
                    AssetImporter importer = AssetImporter.GetAtPath(assetPath);

                    if (importer != null)
                    {
                        importer.SetAssetBundleNameAndVariant("", "");
                    }
                }
            }
            finally
            {
                EditorApplication.delayCall = originalCallback;
            }
        };

        EditorApplication.delayCall();
    }
}