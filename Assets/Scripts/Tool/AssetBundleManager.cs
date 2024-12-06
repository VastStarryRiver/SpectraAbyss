using UnityEngine;
using System.Collections.Generic;
using System;



namespace Invariable
{
    public class AssetBundleManager
    {
        private static Dictionary<string, AssetBundle> m_assetBundles = null;//已经加载的AssetBundle包
        private static AssetBundleManifest m_mainManifest = null;//主AssetBundle的目录文件



        public static void Clear()
        {
            if (m_assetBundles != null && m_assetBundles.Count > 0)
            {
                foreach (var item in m_assetBundles)
                {
                    item.Value.Unload(true);
                }

                m_assetBundles.Clear();
            }

            m_assetBundles = null;

            Caching.ClearCache();
        }

        public static void LoadAssetBundle(string assetBundlePath, string[] assetNames, Action<string, object> callBack)
        {
            if (m_assetBundles == null)
            {
                m_assetBundles = new Dictionary<string, AssetBundle>();
            }

            if (!m_assetBundles.ContainsKey(assetBundlePath))
            {
                string rootPath = DataUtilityManager.m_localRootPath + "AssetBundles/" + DataUtilityManager.m_platform + "/";

                if (m_mainManifest == null)
                {
                    LoadAssetBundle(rootPath + DataUtilityManager.m_platform + ".mainbundle");
                    AssetBundle mainAb = m_assetBundles[rootPath + DataUtilityManager.m_platform + ".mainbundle"];
                    m_mainManifest = mainAb.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                }

                string[] dependencies = m_mainManifest.GetAllDependencies(assetBundlePath.Replace(rootPath, ""));
                foreach (string dependency in dependencies)
                {
                    LoadAssetBundle(rootPath + dependency);
                }

                LoadAssetBundle(assetBundlePath);
            }

            AssetBundle assetBundle = m_assetBundles[assetBundlePath];

            if (assetNames.Length > 0)
            {
                foreach (string assetName in assetNames)
                {
                    object asset = assetBundle.LoadAsset(assetName);
                    callBack(assetName, asset);
                }
            }
        }

        private static void LoadAssetBundle(string assetBundlePath)
        {
            if (!m_assetBundles.ContainsKey(assetBundlePath))
            {
                m_assetBundles[assetBundlePath] = AssetBundle.LoadFromFile(assetBundlePath);
            }
        }
    }
}