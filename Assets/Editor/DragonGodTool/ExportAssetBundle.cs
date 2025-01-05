using System.IO;
using UnityEditor;
using UnityEngine;



public class ExportAssetBundle
{
    private static string m_rootPath = Application.streamingAssetsPath.Replace("Assets/StreamingAssets", "");



    [MenuItem("GodDragonTool/导出AssetBundles文件/BuildAssetBundles_Windows")]
    public static void BuildAssetBundles_Windows()
    {
        BuildAssetBundles(m_rootPath + "AssetBundles/Windows", BuildTarget.StandaloneWindows64);
        RenameMainAssetBundleFile(m_rootPath + "AssetBundles/Windows/Windows");
    }

    [MenuItem("GodDragonTool/导出AssetBundles文件/BuildAssetBundles_Android")]
    public static void BuildAssetBundles_Android()
    {
        BuildAssetBundles(m_rootPath + "AssetBundles/Android", BuildTarget.Android);
        RenameMainAssetBundleFile(m_rootPath + "AssetBundles/Android/Android");
    }



    private static void BuildAssetBundles(string dir, BuildTarget buildTarget)
    {
        if (Directory.Exists(dir))
        {
            Directory.Delete(dir, true);
        }

        Directory.CreateDirectory(dir);

        SetPrefabImportSettings();

        SetAtlasImportSettings();

        SetMaterialImportSettings();

        SetAnimImportSettings();

        SetAudioImportSettings();

        BuildPipeline.BuildAssetBundles(dir, BuildAssetBundleOptions.None, buildTarget); //把所有设置了AssetBundle信息的资源都打包
    }

    private static void RenameMainAssetBundleFile(string mainBundlePath)
    {
        if (File.Exists(mainBundlePath))
        {
            string newBundlePath = mainBundlePath + ".mainbundle";
            File.Move(mainBundlePath, newBundlePath);
        }

        string platform = Path.GetFileName(mainBundlePath);
        string tempFile = mainBundlePath.Substring(0, mainBundlePath.LastIndexOf(platform)) + "tempAssetBundle";

        if (File.Exists(tempFile))
        {
            File.Delete(tempFile);
        }

        if (File.Exists(tempFile + ".manifest"))
        {
            File.Delete(tempFile + ".manifest");
        }
    }

    private static void SetAssetImportSettings(string assetPath, string assetBundleName, string assetBundleVariant)
    {
        AssetImporter materialImporter = AssetImporter.GetAtPath(assetPath);

        materialImporter.assetBundleName = assetBundleName;
        materialImporter.assetBundleVariant = assetBundleVariant;

        EditorUtility.SetDirty(materialImporter);

        materialImporter.SaveAndReimport();

        AssetDatabase.Refresh();
    }

    private static void SetPrefabImportSettings()
    {
        string dir = "Assets/UpdateAssets/Prefabs";

        string[] assetGUIDs = AssetDatabase.FindAssets("t:Prefab", new string[] { dir });//会包括子文件夹内符合要求的文件

        if (assetGUIDs.Length <= 0)
        {
            return;
        }

        for (int i = 0; i < assetGUIDs.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);
            string assetBundleName = assetPath.Replace("Assets/UpdateAssets/", "");
            assetBundleName = assetBundleName.Replace(".prefab", "");
            SetAssetImportSettings(assetPath, assetBundleName, "prefab_ab");
        }
    }

    private static void SetAtlasImportSettings()
    {
        string dir = "Assets/UpdateAssets/Atlas";

        string[] assetGUIDs = AssetDatabase.FindAssets("t:Sprite", new string[] { dir });//会包括子文件夹内符合要求的文件

        if (assetGUIDs.Length <= 0)
        {
            return;
        }

        for (int i = 0; i < assetGUIDs.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);
            string assetBundleName = assetPath.Replace("Assets/UpdateAssets/", "");
            assetBundleName = assetBundleName.Replace(".png", "");
            SetAssetImportSettings(assetPath, assetBundleName, "atlas_ab");
        }
    }

    private static void SetMaterialImportSettings()
    {
        string dir = m_rootPath + "Assets/UpdateAssets/Materials";

        DirectoryInfo directoryInfo = new DirectoryInfo(dir);
        DirectoryInfo[] directoryInfos = directoryInfo.GetDirectories();

        foreach (var directory in directoryInfos)
        {
            string assetDirectoryPath = directory.FullName.Replace("\\", "/").Replace(m_rootPath, "");

            //会包括子文件夹内符合要求的文件
            string[] assetGUIDs1 = AssetDatabase.FindAssets("t:Material", new string[] { assetDirectoryPath });

            if (assetGUIDs1.Length != 1)
            {
                continue;
            }

            string assetPath1 = AssetDatabase.GUIDToAssetPath(assetGUIDs1[0]);
            string assetBundleName = assetPath1.Replace("Assets/UpdateAssets/", "");
            assetBundleName = assetBundleName.Replace(".mat", "");
            SetAssetImportSettings(assetPath1, assetBundleName, "mat_ab");

            string[] assetGUIDs2 = AssetDatabase.FindAssets("t:Shader", new string[] { assetDirectoryPath });

            if (assetGUIDs2.Length != 1)
            {
                continue;
            }

            string assetPath2 = AssetDatabase.GUIDToAssetPath(assetGUIDs2[0]);
            SetAssetImportSettings(assetPath2, assetBundleName, "mat_ab");
        }
    }

    private static void SetAnimImportSettings()
    {
        string dir = "Assets/UpdateAssets/Animtion";

        string[] assetGUIDs = AssetDatabase.FindAssets("t:AnimationClip", new string[] { dir });//会包括子文件夹内符合要求的文件

        if (assetGUIDs.Length <= 0)
        {
            return;
        }

        for (int i = 0; i < assetGUIDs.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);
            string assetBundleName = assetPath.Replace("Assets/UpdateAssets/", "");
            assetBundleName = assetBundleName.Replace(".anim", "");
            SetAssetImportSettings(assetPath, assetBundleName, "anim_ab");
        }
    }

    private static void SetAudioImportSettings()
    {
        string dir = "Assets/UpdateAssets/Audios";

        string[] assetGUIDs = AssetDatabase.FindAssets("t:AudioClip", new string[] { dir });//会包括子文件夹内符合要求的文件

        if (assetGUIDs.Length <= 0)
        {
            return;
        }

        for (int i = 0; i < assetGUIDs.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);
            string assetBundleName = assetPath.Replace("Assets/UpdateAssets/", "");
            assetBundleName = assetBundleName.Replace(".mp3", "");
            SetAssetImportSettings(assetPath, assetBundleName, "audio_ab");
        }
    }
}