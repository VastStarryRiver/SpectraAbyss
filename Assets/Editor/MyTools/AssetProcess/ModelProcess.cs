using Invariable;
using System.IO;
using UnityEditor;



namespace MyTools
{
    public static class ModelProcess
    {
        private const string FbxFolder = "Assets/GameAssets/Models/Fbx";
        private const string TextureFolder = "Assets/GameAssets/Models/Textures";



        /// <summary>
        /// 批量设置 3D 模型与 3D 贴图导入格式
        /// </summary>
        [MenuItem("VastStarryRiver/资源处理/设置3D模型", false, 42)]
        public static void ApplyModelImportSettings()
        {
            int modelCount = ApplyModels();
            int textureCount = ApplyTextures();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            GameLog.Info($"3D 模型导入格式已设置: 模型 {modelCount} 个，贴图 {textureCount} 个");
        }



        /// <summary>
        /// 对 Fbx 目录下模型写入网格与动画导入设置
        /// </summary>
        private static int ApplyModels()
        {
            if (!AssetDatabase.IsValidFolder(FbxFolder))
            {
                GameLog.Error($"模型目录不存在: {FbxFolder}");

                return 0;
            }

            string[] guids = AssetDatabase.FindAssets("t:Model", new[] { FbxFolder });
            int count = 0;

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;

                if (importer == null)
                {
                    continue;
                }

                ApplyModelImporter(importer);
                importer.SaveAndReimport();
                count++;
            }

            return count;
        }

        /// <summary>
        /// 对 3D 贴图写入 sRGB/mipmap 与三端 ASTC
        /// </summary>
        private static int ApplyTextures()
        {
            if (!AssetDatabase.IsValidFolder(TextureFolder))
            {
                GameLog.Error($"3D 贴图目录不存在: {TextureFolder}");

                return 0;
            }

            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { TextureFolder });
            int count = 0;

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

                if (importer == null)
                {
                    continue;
                }

                ApplyModelTextureSettings(importer, path);
                importer.SaveAndReimport();
                count++;
            }

            return count;
        }

        /// <summary>
        /// 写入模型网格、法线与动画导入设置
        /// </summary>
        private static void ApplyModelImporter(ModelImporter importer)
        {
            importer.meshCompression = ModelImporterMeshCompression.Medium;
            importer.isReadable = false;
            importer.optimizeMeshPolygons = true;
            importer.optimizeMeshVertices = true;
            importer.addCollider = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.importNormals = ModelImporterNormals.Import;
            importer.importTangents = ModelImporterTangents.None;
            importer.importAnimation = true;
            importer.animationCompression = ModelImporterAnimationCompression.Optimal;
        }

        /// <summary>
        /// 反照率开 sRGB，法线与 Mask 关 sRGB，一律开 mipmap 并写三端 ASTC
        /// </summary>
        private static void ApplyModelTextureSettings(TextureImporter importer, string path)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);
            bool isLinearMap = fileName.IndexOf("Normal", System.StringComparison.OrdinalIgnoreCase) >= 0
                || fileName.IndexOf("Mask", System.StringComparison.OrdinalIgnoreCase) >= 0
                || importer.textureType == TextureImporterType.NormalMap;

            if (isLinearMap && importer.textureType != TextureImporterType.NormalMap)
            {
                importer.sRGBTexture = false;
            }
            else if (!isLinearMap)
            {
                importer.sRGBTexture = true;
            }

            importer.mipmapEnabled = true;
            importer.isReadable = false;

            ApplyAstcPlatform(importer, "Android");
            ApplyAstcPlatform(importer, "iPhone");
            ApplyAstcPlatform(importer, "OpenHarmony");
        }

        /// <summary>
        /// 为指定平台写入 ASTC 覆盖
        /// </summary>
        private static void ApplyAstcPlatform(TextureImporter importer, string platform)
        {
            TextureImporterPlatformSettings platformSettings = importer.GetPlatformTextureSettings(platform);
            platformSettings.name = platform;
            platformSettings.overridden = true;
            platformSettings.format = TextureImporterFormat.ASTC_6x6;
            platformSettings.textureCompression = TextureImporterCompression.Compressed;
            importer.SetPlatformTextureSettings(platformSettings);
        }
    }
}
