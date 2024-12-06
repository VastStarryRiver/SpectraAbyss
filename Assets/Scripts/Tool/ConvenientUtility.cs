using System;
using System.Collections.Generic;
using UnityEngine;
using ILRuntime.CLR.TypeSystem;



namespace Invariable
{
    public class ConvenientUtility
    {
        public static GameObject GetGameObject(UnityEngine.Object obj, string childPath = "")
        {
            GameObject gameObject = null;

            if (obj is GameObject)
            {
                gameObject = obj as GameObject;
            }
            else if (obj is Component)
            {
                var component = obj as Component;
                gameObject = component.gameObject;
            }

            if (!string.IsNullOrEmpty(childPath))
            {
                Transform trans = gameObject.transform.Find(childPath);

                if (trans != null)
                {
                    return trans.gameObject;
                }

                return null;
            }

            return gameObject;
        }

        public static GameObject GetGameObject(string childPath)
        {
            GameObject gameObject = GameObject.Find(childPath);
            return gameObject;
        }

        public static Transform GetTransform(UnityEngine.Object obj, string childPath = "")
        {
            var gameObject = GetGameObject(obj, childPath);

            if (gameObject != null)
            {
                return gameObject.transform;
            }

            return null;
        }

        public static MonoBehaviour OpenUIPrefabPanel(string prefabPath, int layer)
        {
            int startIndex = prefabPath.LastIndexOf("/") + 1;
            string prefabName = prefabPath.Substring(startIndex, prefabPath.Length - startIndex);

            if (!UIManager.AllPanel.ContainsKey(prefabName))
            {
                string[] assetNames = new string[] { prefabName + ".prefab" };

                AssetBundleManager.LoadAssetBundle(DataUtilityManager.m_localRootPath + "AssetBundles/" + DataUtilityManager.m_platform + "/" + prefabPath.ToLower() + ".prefab_ab", assetNames, (name, asset) => {
                    if (name == assetNames[0])
                    {
                        GameObject gameObject = GameObject.Instantiate((GameObject)asset, GameObject.Find("UI_Root/Ts_Panel").transform);

                        gameObject.name = prefabName;

                        Canvas canvas = (Canvas)AddComponent(gameObject, "", "Canvas");
                        canvas.overrideSorting = true;
                        canvas.sortingOrder = layer;
                        canvas.vertexColorAlwaysGammaSpace = true;

                        AddComponent(gameObject, "", "GraphicRaycaster");

                        UIManager.AllPanel[prefabName] = (MonoBehaviour)AddComponent(gameObject, "", prefabName);
                    }
                });
            }

            return UIManager.AllPanel[prefabName];
        }

        public static void CloseUIPrefabPanel(string prefabName)
        {
            if (UIManager.AllPanel.ContainsKey(prefabName))
            {
                GameObject.Destroy(UIManager.AllPanel[prefabName].gameObject);
                UIManager.AllPanel.Remove(prefabName);
            }
        }

        public static Component GetComponent(UnityEngine.Object obj, string childPath, string componentName)
        {
            GameObject gameObject = GetGameObject(obj);
            Transform trans;

            if (gameObject != null)
            {
                trans = gameObject.transform;
            }
            else
            {
                return null;
            }

            if (!string.IsNullOrEmpty(childPath))
            {
                trans = trans.Find(childPath);
            }

            if (trans != null)
            {
                return trans.GetComponent(componentName);
            }

            return null;
        }

        public static Component AddComponent(UnityEngine.Object obj, string childPath, string componentName)
        {
            GameObject gameObject = GetGameObject(obj);

            if (gameObject == null)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(childPath))
            {
                Transform trans = gameObject.transform.Find(childPath);

                if (trans == null)
                {
                    return null;
                }

                gameObject = trans.gameObject;
            }

            Component component = gameObject.GetComponent(componentName);

            if (component == null)
            {
                Dictionary<string, string> typeName = new Dictionary<string, string>
                {
                    { "Invariable.", ", Invariable" },
                    { "UnityEngine.", ", UnityEngine" },
                    { "UnityEngine.UI.", ", UnityEngine.UI" },
                };

                Type type = null;

                foreach (var item in typeName)
                {
                    type = Type.GetType(item.Key + componentName + item.Value);

                    if (type != null)
                    {
                        break;
                    }
                }

                if (type == null)
                {
                    IType itype = HotUpdateManager.m_appdomain.LoadedTypes["HotUpdate." + componentName];

                    if (itype != null)
                    {
                        HotUpdateManager.m_allInstance[componentName] = HotUpdateManager.m_appdomain.Instantiate("HotUpdate." + componentName);
                        MonoBehaviourAdapter adapter = gameObject.AddComponent<MonoBehaviourAdapter>();
                        component = adapter;
                    }
                }
                else
                {
                    component = gameObject.AddComponent(type);
                }
            }

            return component;
        }
    }
}