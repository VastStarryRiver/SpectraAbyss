using UnityEditor;
using Invariable;
using System.IO;
using HybridCLR.Editor.Commands;



namespace MyTools
{
    public class DllTool
    {
        [MenuItem("VastStarryRiver/DLL/导出热更新DLL", false, 0)]
        public static void BuildHotUpdateDLL()
        {
            PrebuildCommand.GenerateAll();
        }

        [MenuItem("VastStarryRiver/DLL/复制热更新DLL", false, 1)]
        public static void MoveHotUpdateDLL()
        {
            string platform = EditorUserBuildSettings.activeBuildTarget.ToString();
            string path = ConfigUtils.m_localRootPath + "HybridCLRData/HotUpdateDlls/" + platform + "/HotUpdate.dll";
            byte[] bytes = File.ReadAllBytes(path);
            ConfigUtils.SaveSafeFile(bytes, ConfigUtils.m_hotUpdateDllPath + "/" + platform + "/HotUpdate.dll.bin");
            AssetDatabase.Refresh();
        }
    }
}