using HybridCLR.Editor.Commands;
using Invariable;
using System.IO;
using UnityEditor;



namespace MyTools
{
    public class DllTool
    {
        /// <summary>
        /// 生成全部 HybridCLR DLL
        /// </summary>
        [MenuItem("VastStarryRiver/DLL/导出所有DLL", false, 10)]
        public static void BuildHotUpdateDLL()
        {
            PrebuildCommand.GenerateAll();
        }

        /// <summary>
        /// 复制热更新 DLL 到资源目录
        /// </summary>
        [MenuItem("VastStarryRiver/DLL/复制热更新DLL", false, 11)]
        public static void MoveHotUpdateDLL()
        {
            string hybridClrPlatform = EditorUserBuildSettings.activeBuildTarget.ToString();
            string appPlatform = ConfigUtils.GetAppPlatformFolder(hybridClrPlatform);
            string path = $"{ConfigUtils.LocalRootPath}HybridCLRData/HotUpdateDlls/{hybridClrPlatform}/HotUpdate.dll";
            byte[] bytes = File.ReadAllBytes(path);
            ConfigUtils.SaveSafeFile(bytes, $"{ConfigUtils.HotUpdateDllPath}/{appPlatform}/HotUpdate.dll.bin");
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 复制 AOT 元数据 DLL 到资源目录
        /// </summary>
        [MenuItem("VastStarryRiver/DLL/复制元数据DLL", false, 12)]
        public static void MoveMetadataForAOTDLL()
        {
            string hybridClrPlatform = EditorUserBuildSettings.activeBuildTarget.ToString();
            string appPlatform = ConfigUtils.GetAppPlatformFolder(hybridClrPlatform);

            foreach (string aotDllName in InvariableConst.AotDllNames)
            {
                string path = $"{ConfigUtils.LocalRootPath}HybridCLRData/AssembliesPostIl2CppStrip/{hybridClrPlatform}/{aotDllName}.dll";
                byte[] bytes = File.ReadAllBytes(path);
                ConfigUtils.SaveSafeFile(bytes, $"{ConfigUtils.HotUpdateDllPath}/{appPlatform}/{aotDllName}.dll.bin");
            }

            AssetDatabase.Refresh();
        }
    }
}