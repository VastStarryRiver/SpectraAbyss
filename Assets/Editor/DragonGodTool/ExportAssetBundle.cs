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

        SetAnimImportSettings();

        BuildPipeline.BuildAssetBundles(dir, BuildAssetBundleOptions.None, buildTarget); //把所有设置了AssetBundle信息的资源都打包
    }

    private static void RenameMainAssetBundleFile(string mainBundlePath)
    {
        if (File.Exists(mainBundlePath))
        {
            string newBundlePath = mainBundlePath + ".mainbundle";
            File.Move(mainBundlePath, newBundlePath);
        }
        else
        {
            Debug.LogWarning("主AssetBundle文件不存在");
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
        string dir = "Assets/UpdateAssets/UI";

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
}