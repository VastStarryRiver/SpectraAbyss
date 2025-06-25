using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ILRuntime.CLR.TypeSystem;
using UnityEngine.U2D;
using UnityEngine.SceneManagement;



namespace Invariable
{
    public static class ConvenientUtility
    {
        public static Camera MainUICamera
        {
            get
            {
                return GameObject.Find("UI_Root/UI_Camera").GetComponent<Camera>();
            }
        }

        public static RectTransform MainUIRoot
        {
            get
            {
                return GameObject.Find("UI_Root").GetComponent<RectTransform>();
            }
        }

        public static Camera MainSceneCamera
        {
            get
            {
                return GameObject.Find("SceneGameObject/Main Camera").GetComponent<Camera>();
            }
        }

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

        public static MonoBehaviour OpenUIPrefabPanel(string prefabPath, int layer, Action callBack = null)
        {
            int startIndex = prefabPath.LastIndexOf("/") + 1;
            string prefabName = prefabPath.Substring(startIndex, prefabPath.Length - startIndex);

            if (!UIManager.AllPanel.ContainsKey(prefabName))
            {
                CoroutineManager.Instance.LoadAddressables(prefabPath + ".prefab", (asset) => {
                    GameObject gameObject = GameObject.Instantiate((GameObject)asset, GameObject.Find("UI_Root/Ts_Panel").transform);

                    gameObject.name = prefabName;

                    Canvas canvas = (Canvas)AddComponent(gameObject, "", "Canvas");
                    canvas.overrideSorting = true;
                    canvas.sortingOrder = layer;
                    canvas.vertexColorAlwaysGammaSpace = true;

                    AddComponent(gameObject, "", "GraphicRaycaster");

                    UIManager.AllPanel[prefabName] = (MonoBehaviour)AddComponent(gameObject, "", prefabName);

                    callBack?.Invoke();
                });
            }
            else
            {
                return UIManager.AllPanel[prefabName];
            }

            return null;
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

        public static void SetGray(UnityEngine.Object obj, string childPath = "", bool isGray = true, bool isMask = false)
        {
            Transform trans = GetTransform(obj);

            if (!string.IsNullOrEmpty(childPath))
            {
                trans = trans.Find(childPath);
            }

            if (trans == null)
            {
                return;
            }

            Image image = trans.GetComponent<Image>();
            RawImage rawImage = trans.GetComponent<RawImage>();

            if (image == null && rawImage == null)
            {
                return;
            }

            if (isGray)
            {
                string path;

                if(isMask)
                {
                    path = "Materials/UIMaskGrayscale/UIMaskGrayscaleMaterial.mat";
                }
                else
                {
                    path = "Materials/Grayscale/GrayscaleMaterial.mat";
                }

                CoroutineManager.Instance.LoadAddressables(path, (asset) =>
                {
                    Material material = asset as Material;

                    if (image != null)
                    {
                        image.material = material;
                    }
                    else if (rawImage != null)
                    {
                        rawImage.material = material;
                    }
                });
            }
            else if (image != null)
            {
                image.material = null;
            }
            else if (rawImage != null)
            {
                rawImage.material = null;
            }
        }

        public static void SetSpriteImage(UnityEngine.Object obj, string childPath = "", string spritePath = "", bool isSetNativeSize = false)
        {
            if (string.IsNullOrEmpty(spritePath))
            {
                return;
            }

            Transform trans = GetTransform(obj);

            if (!string.IsNullOrEmpty(childPath))
            {
                trans = trans.Find(childPath);
            }

            if (trans != null)
            {
                Image image = trans.GetComponent<Image>();

                string[] atlasInfo = spritePath.Split('/');
                string atlasName = atlasInfo[0];
                string imageName = atlasInfo[1];

                if (atlasName == imageName)
                {
                    CoroutineManager.Instance.LoadAddressables("Png/" + atlasName + ".png", (asset) => {
                        Texture2D texture = asset as Texture2D;

                        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                        image.sprite = sprite;

                        image.sprite.name = imageName;

                        if (isSetNativeSize)
                        {
                            image.SetNativeSize();
                        }
                    });
                }
                else
                {
                    CoroutineManager.Instance.LoadAddressables("Atlas/" + atlasName + "/" + atlasName + ".spriteatlasv2", (asset) => {
                        SpriteAtlas atlas = asset as SpriteAtlas;

                        Sprite sprite = atlas.GetSprite(imageName);

                        image.sprite = sprite;

                        image.sprite.name = imageName;

                        if (isSetNativeSize)
                        {
                            image.SetNativeSize();
                        }
                    });
                }
            }
        }

        public static void SetTextureRawImage(UnityEngine.Object obj, string childPath = "", string texturePath = "", bool isSetNativeSize = false)
        {
            if (string.IsNullOrEmpty(texturePath))
            {
                return;
            }

            Transform trans = GetTransform(obj);

            if (!string.IsNullOrEmpty(childPath))
            {
                trans = trans.Find(childPath);
            }

            if (trans != null)
            {
                RawImage rawImage = trans.GetComponent<RawImage>();

                string[] atlasInfo = texturePath.Split('/');
                string atlasName = atlasInfo[0];
                string imageName = atlasInfo[1];

                if (atlasName == imageName)
                {
                    CoroutineManager.Instance.LoadAddressables("Png/" + atlasName + ".png", (asset) => {
                        Texture2D texture = asset as Texture2D;

                        rawImage.texture = texture;

                        rawImage.texture.name = imageName;

                        if (isSetNativeSize)
                        {
                            rawImage.SetNativeSize();
                        }
                    });
                }
                else
                {
                    CoroutineManager.Instance.LoadAddressables("Atlas/" + atlasName + "/" + atlasName + ".spriteatlasv2", (asset) => {
                        SpriteAtlas atlas = asset as SpriteAtlas;

                        Sprite sprite = atlas.GetSprite(imageName);

                        rawImage.texture = sprite.texture;

                        rawImage.texture.name = imageName;

                        if (isSetNativeSize)
                        {
                            rawImage.SetNativeSize();
                        }
                    });
                }
            }
        }

        public static IEnumerator PlayAnimation(UnityEngine.Object obj, string childPath = "", string animName = "", WrapMode wrapMode = WrapMode.Once, Action callBack = null)
        {
            if (string.IsNullOrEmpty(animName))
            {
                yield break;
            }

            Transform trans = ConvenientUtility.GetTransform(obj);

            if (trans == null)
            {
                yield break;
            }

            if (!string.IsNullOrEmpty(childPath))
            {
                trans = trans.Find(childPath);
            }

            if (trans == null)
            {
                yield break;
            }

            Animation animation = trans.GetComponent<Animation>();

            if (animation == null)
            {
                yield break;
            }

            animation.wrapMode = wrapMode;

            animation.Play(animName);

            if (wrapMode == WrapMode.Once)
            {
                yield return new WaitWhile(() => animation.isPlaying);

                callBack?.Invoke();
            }
        }

        public static IEnumerator RestartGame()
        {
            MainSceneCamera.clearFlags = CameraClearFlags.SolidColor;
            MainSceneCamera.backgroundColor = Color.black;

            List<string> list = new List<string>();

            foreach (var item in UIManager.AllPanel)
            {
                list.Add(item.Key);
            }

            if (list.Count > 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    CloseUIPrefabPanel(list[i]);
                }
            }

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);

            while (!asyncLoad.isDone)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                Debug.Log("╪сть╫Ь╤х: " + (progress * 100) + "%");
                yield return null;
            }
        }
    }
}