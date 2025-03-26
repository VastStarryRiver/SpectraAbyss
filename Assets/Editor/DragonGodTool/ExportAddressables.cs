using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;



public class ExportAddressables
{
    private static string m_rootPath = Application.streamingAssetsPath.Replace("Assets/StreamingAssets", "");
    private static AddressableAssetSettings m_settings = AddressableAssetSettingsDefaultObject.Settings;



    [MenuItem("GodDragonTool/Addressables/BuildAddressables_Android", false, 1)]
    public static void BuildAddressables_Android()
    {
        SetProfileValue("Android");
        BuildAddressables("Android");
    }

    [MenuItem("GodDragonTool/Addressables/BuildAddressables_Windows", false, 2)]
    public static void BuildAddressables_Windows()
    {
        SetProfileValue("Windows");
        BuildAddressables("Windows");
    }



    public static void SetProfileValue(string platform)
    {
        AddressableAssetProfileSettings profileSettings = m_settings.profileSettings;

        string profileId = profileSettings.GetProfileId(platform);

        m_settings.activeProfileId = profileId;

        EditorUtility.SetDirty(m_settings);

        AssetDatabase.SaveAssets();

        AssetDatabase.Refresh();
    }

    private static void BuildAddressables(string platform)
    {
        string dir = m_rootPath + "Addressables/" + platform;

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

        AddressableAssetSettings.BuildPlayerContent();
    }

    private static void SetAssetImportSettings(string guid, string groupName, string labels)
    {
        AddressableAssetGroup group = m_settings.FindGroup(groupName);

        if (group == null)
        {
            group = m_settings.CreateGroup(groupName, false, false, true, null);
        }

        BundledAssetGroupSchema schema = group.GetSchema<BundledAssetGroupSchema>();

        if (schema == null)
        {
            schema = group.AddSchema<BundledAssetGroupSchema>();
        }

        schema.BuildPath.SetVariableByName(m_settings, AddressableAssetSettings.kRemoteBuildPath);
        schema.LoadPath.SetVariableByName(m_settings, AddressableAssetSettings.kRemoteLoadPath);

        schema.UseUnityWebRequestForLocalBundles = true;

        AddressableAssetEntry assetEntry = m_settings.CreateOrMoveEntry(guid, group);

        string address = assetEntry.address.Replace("Assets/UpdateAssets/", "");
        address = address.Replace("Addressables/", "");
        assetEntry.SetAddress(address);

        assetEntry.labels.Clear();

        if (!string.IsNullOrEmpty(labels))
        {
            assetEntry.SetLabel(labels, true, true);
        }

        AssetDatabase.SaveAssets();

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
            string labels = "";

            if (assetPath.Contains("/UI/"))
            {
                labels = "UI";
            }
            else if (assetPath.Contains("/Model/"))
            {
                labels = "Model";
            }
            else if (assetPath.Contains("/VFX/"))
            {
                labels = "VFX";
            }

            SetAssetImportSettings(assetGUIDs[i], "Prefabs", labels);
        }
    }

    private static void SetAtlasImportSettings()
    {
        string dir1 = "Assets/UpdateAssets/Atlas";
        string dir2 = "Assets/UpdateAssets/Png";

        string[] assetGUIDs = AssetDatabase.FindAssets("t:Texture2D", new string[] { dir1, dir2 });//会包括子文件夹内符合要求的文件

        if (assetGUIDs.Length <= 0)
        {
            return;
        }

        for (int i = 0; i < assetGUIDs.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);

            if (assetPath.Contains("/Atlas/"))
            {
                SetAssetImportSettings(assetGUIDs[i], "Atlas", "Sprite");
            }
            else
            {
                SetAssetImportSettings(assetGUIDs[i], "Png", "Texture");
            }
        }

        string[] assetGUIDs2 = AssetDatabase.FindAssets("t:SpriteAtlas", new string[] { dir1 });//会包括子文件夹内符合要求的文件

        if (assetGUIDs2.Length <= 0)
        {
            return;
        }

        for (int i = 0; i < assetGUIDs2.Length; i++)
        {
            SetAssetImportSettings(assetGUIDs2[i], "Atlas", "SpriteAtlas");
        }
    }

    private static void SetMaterialImportSettings()
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(m_rootPath + "Assets/UpdateAssets/Materials");
        DirectoryInfo[] directoryInfos = directoryInfo.GetDirectories();

        foreach (var directory in directoryInfos)
        {
            string assetDirectoryPath = directory.FullName.Replace("\\", "/").Replace(m_rootPath, "");

            //会包括子文件夹内符合要求的文件
            string[] assetGUIDs1 = AssetDatabase.FindAssets("t:Material", new string[] { assetDirectoryPath });

            if (assetGUIDs1.Length <= 0)
            {
                continue;
            }

            foreach (var guid in assetGUIDs1)
            {
                SetAssetImportSettings(guid, "Materials", "Mat");
            }

            string[] assetGUIDs2 = AssetDatabase.FindAssets("t:Shader", new string[] { assetDirectoryPath });

            if (assetGUIDs2.Length <= 0)
            {
                continue;
            }

            SetAssetImportSettings(assetGUIDs2[0], "Materials", "Shader");
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
            SetAssetImportSettings(assetGUIDs[i], "Animtion", "Anim");
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
            SetAssetImportSettings(assetGUIDs[i], "Audios", "Audio");
        }
    }
}