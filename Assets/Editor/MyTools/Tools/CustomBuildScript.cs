using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Invariable;

#if UNITY_WEBGL
using WeChatWASM;
#endif



namespace MyTools
{
    public class CustomBuildScript
    {
        [MenuItem("VastStarryRiver/打包/打包成APK文件", false, 30)]
        public static void PackageProject_Android()
        {
            SetAndroidKeystore();
            PackageProject(BuildTarget.Android, Environment.GetFolderPath(Environment.SpecialFolder.Desktop).Replace("\\", "/") + "/SpectraAbyss.apk");
        }

        [MenuItem("VastStarryRiver/打包/打包成APK文件", true, 30)]
        private static bool PackageProject_Android_Validate()
        {
#if UNITY_ANDROID
            return true;
#else
            return false;
#endif
        }

        [MenuItem("VastStarryRiver/打包/打包微信小游戏", false, 31)]
        public static void PackageProject_WeiXin()
        {
#if UNITY_WEBGL
            if (Directory.Exists(ConfigUtils.m_miniBuildPath))
            {
                Directory.Delete(ConfigUtils.m_miniBuildPath, true);
            }

            ConfigUtils.InitDirectory(ConfigUtils.m_miniBuildPath);

            if (WXConvertCore.DoExport() == WXConvertCore.WXExportError.SUCCEED)
            {
                if (WXConvertCore.IsInstantGameAutoStreaming())
                {
                    if (!string.IsNullOrEmpty(WXConvertCore.FirstBundlePath) && File.Exists(WXConvertCore.FirstBundlePath))
                    {
                        Debug.Log("转换成功");
                    }
                    else
                    {
                        Debug.LogError("转换失败");
                    }
                }
            }
#endif
        }

        [MenuItem("VastStarryRiver/打包/打包微信小游戏", true, 31)]
        private static bool PackageProject_WeiXin_Validate()
        {
#if UNITY_ANDROID
            return false;
#else
            return true;
#endif
        }

        [MenuItem("VastStarryRiver/打包/复制文件到CDN目录", false, 32)]
        public static void MoveFileToCND()
        {
            if (Directory.Exists(ConfigUtils.m_cdnPath))
            {
                Directory.Delete(ConfigUtils.m_cdnPath, true);
            }

            ConfigUtils.InitDirectory(ConfigUtils.m_cdnPath);

            MoveBundleToCND();

#if UNITY_WEBGL
            MoveMiniGameToCND();
#endif
        }



        private static void MoveBundleToCND()
        {
            string path = AssetBundleTool.GetOutPath();

            if (!Directory.Exists(path))
            {
                return;
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(path);

            FileInfo[] fileInfos = directoryInfo.GetFiles();

            foreach (var item in fileInfos)
            {
                string sourceFilePath = item.FullName.Replace("\\", "/");
                string targetFilePath = ConfigUtils.m_cdnPath + "/" + Path.GetFileName(sourceFilePath);
                File.Copy(sourceFilePath, targetFilePath);
            }
        }

        private static void MoveMiniGameToCND()
        {
            string path = ConfigUtils.m_miniWebglPath;

            if (!Directory.Exists(path))
            {
                return;
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(path);

            FileInfo[] fileInfos = directoryInfo.GetFiles();

            foreach (var item in fileInfos)
            {
                if (!item.FullName.Contains(".webgl.data.unityweb.bin.br"))
                {
                    continue;
                }

                string sourceFilePath = item.FullName.Replace("\\", "/");
                string targetFilePath = ConfigUtils.m_cdnPath + "/" + Path.GetFileName(sourceFilePath);
                File.Copy(sourceFilePath, targetFilePath);

                break;
            }
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
}