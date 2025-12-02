using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditor.Build;
using Invariable;



public class CustomBuildScript
{
    [MenuItem("Tools/打包成APK文件", false, -3)]
    public static void PackageProject_Android()
    {
        SetAndroidKeystore();
        PackageProject(BuildTarget.Android, Environment.GetFolderPath(Environment.SpecialFolder.Desktop).Replace("\\", "/") + "/SpectraAbyss.apk");
    }



    private static void SetAndroidKeystore()
    {
        string keystorePath = ConfigUtils.m_keystorePath; // Keystore 文件路径

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

        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }
}