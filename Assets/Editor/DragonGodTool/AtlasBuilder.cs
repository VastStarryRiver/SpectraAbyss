using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;



public class AtlasBuilder
{
    private static string m_texturesRootPath = Application.dataPath + "/UpdateAssets/Atlas/Atlas03";//小图文件夹的根路径



    [MenuItem("GodDragonTool/Atlas/导出需要的列表png资源")]
    public static void PackSpriteAtlas()
    {
        SetImageAtlasData(m_texturesRootPath.Replace(Application.dataPath, "Assets"), out Dictionary<string, List<Texture2D>> textureAtlas);

        CreateAtlas(textureAtlas);

        EditorUtility.ClearProgressBar();

        AssetDatabase.Refresh();
    }



    private static void CreateAtlas(Dictionary<string, List<Texture2D>> atlasTexture)
    {
        int progressIndex = 0;

        foreach (var item in atlasTexture)
        {
            //创建图集
            string atlasName = item.Key;
            Texture2D[] textures = item.Value.ToArray();
            Texture2D atlas = null;
            Rect[] rects = null;

            atlas = new Texture2D(2048, 2048);

            rects = atlas.PackTextures(textures, 2, 2048, false);

            Color[] atlasPixels = atlas.GetPixels();

            for (int i = 0; i < textures.Length; i++)
            {
                Texture2D texture = textures[i];
                Rect rect = rects[i];
                Color[] texturePixels = texture.GetPixels();

                int x = (int)(rect.x * atlas.width);
                int y = (int)(rect.y * atlas.height);

                for (int h = 0; h < texture.height; h++)
                {
                    for (int w = 0; w < texture.width; w++)
                    {
                        int atlasX = x + w;
                        int atlasY = y + h;

                        int index = atlasX + atlasY * atlas.width;

                        atlasPixels[index] = texturePixels[w + h * texture.width];
                    }
                }

                EditorUtility.DisplayProgressBar("设置" + atlasName + "图集的像素数据中......", "进度：" + i + "/" + textures.Length, i * 1.0f / textures.Length);
            }

            atlas.SetPixels(atlasPixels);

            atlas.Apply();

            byte[] bytes = atlas.EncodeToPNG();

            using (FileStream fileStream = new FileStream(m_texturesRootPath + "/" + atlasName + ".png", FileMode.Create))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
                {
                    binaryWriter.Write(bytes);
                }
            }

            AssetDatabase.Refresh();

            string assetsAtlasPath = m_texturesRootPath.Replace(Application.dataPath, "Assets") + "/" + atlasName + ".png";

            //设置图集的ImportSettings
            SetAtlasImportSettings(assetsAtlasPath, atlasName, atlas, textures, rects);

            progressIndex++;

            EditorUtility.DisplayProgressBar("生成" + atlasName + "图集及其材质" + "中......", "进度：" + progressIndex + "/" + atlasTexture.Count, progressIndex * 1.0f / atlasTexture.Count);
        }
    }

    private static void SetAtlasImportSettings(string assetsAtlasPath, string atlasName, Texture2D atlas, Texture2D[] textures, Rect[] rects = null)
    {
        TextureImporter atlasImporter = AssetImporter.GetAtPath(assetsAtlasPath) as TextureImporter;

        atlasImporter.textureType = TextureImporterType.Sprite;

        if (textures.Length > 1 && rects != null && rects.Length > 1)
        {
            atlasImporter.spriteImportMode = SpriteImportMode.Multiple;

            List<SpriteMetaData> spriteMetaDatas = new List<SpriteMetaData>();

            for (int i = 0; i < textures.Length; i++)
            {
                Rect rect = rects[i];

                SpriteMetaData spriteMetaData = GetSpriteMetaData(new Rect(rect.x * atlas.width, rect.y * atlas.height, rect.width * atlas.width, rect.height * atlas.height), textures[i].name);
                spriteMetaDatas.Add(spriteMetaData);

                EditorUtility.DisplayProgressBar("设置" + atlasName + "图集ImporterSetting中......", "进度：" + i + "/" + textures.Length, i * 1.0f / textures.Length);
            }

            atlasImporter.spritesheet = spriteMetaDatas.ToArray();
        }
        else
        {
            atlasImporter.spriteImportMode = SpriteImportMode.Single;

            EditorUtility.DisplayProgressBar("设置" + atlasName + "图集ImporterSetting中......", "进度：1/1", 1);
        }

        EditorUtility.SetDirty(atlasImporter);

        atlasImporter.SaveAndReimport();
    }

    private static SpriteMetaData GetSpriteMetaData(Rect rect, string name)
    {
        SpriteMetaData spriteMetaData = new SpriteMetaData();

        spriteMetaData.alignment = (int)SpriteAlignment.Center;
        spriteMetaData.name = name;
        spriteMetaData.rect = new Rect(rect.x, rect.y, rect.width, rect.height);

        return spriteMetaData;
    }

    private static void SetImageAtlasData(string imageRootPath, out Dictionary<string, List<Texture2D>> atlasTextures)
    {
        SetImageImportSettings(imageRootPath);

        atlasTextures = new Dictionary<string, List<Texture2D>>();

        string[] assetGUIDs = AssetDatabase.FindAssets("t:Sprite", new string[] { imageRootPath });//会包括子文件夹内符合要求的文件

        if (assetGUIDs.Length <= 0)
        {
            return;
        }

        for (int i = 0; i < assetGUIDs.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);
            string name = Path.GetFileName(assetPath);
            string atlasName = assetPath.Replace(imageRootPath + "/", "");

            atlasName = atlasName.Replace("/" + name, "");

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);

            if (sprite != null)
            {
                if (!atlasTextures.ContainsKey(atlasName))
                {
                    atlasTextures[atlasName] = new List<Texture2D>();
                }

                atlasTextures[atlasName].Add(sprite.texture);
            }
        }
    }

    private static void SetImageImportSettings(string imageRootPath)
    {
        string[] assetGUIDs = AssetDatabase.FindAssets("t:Texture2D", new string[] { imageRootPath });//会包括子文件夹内符合要求的文件

        if (assetGUIDs.Length <= 0)
        {
            return;
        }

        for (int i = 0; i < assetGUIDs.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);
            TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.isReadable = true;
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }

        AssetDatabase.Refresh();
    }
}