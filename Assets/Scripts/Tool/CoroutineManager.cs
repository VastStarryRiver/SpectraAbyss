using UnityEngine;
using System;
using System.Collections;



namespace Invariable
{
    public class CoroutineManager : MonoBehaviour
    {
        public static CoroutineManager Instance = null;



        private void Awake()
        {
            Instance = this;
        }



        public void InvokeLoadAssetAsync(AssetBundle assetBundle, string assetName, Action<string, object> callBack)
        {
            StartCoroutine(LoadAssetAsync(assetBundle, assetName, callBack));
        }

        private IEnumerator LoadAssetAsync(AssetBundle assetBundle, string assetName, Action<string, object> callBack)
        {
            AssetBundleRequest assetRequest = assetBundle.LoadAssetAsync(assetName);

            yield return assetRequest;

            callBack(assetName, assetRequest.asset);
        }

        public void InvokePlayAnimation(UnityEngine.Object obj, string childPath = "", string animName = "", WrapMode wrapMode = WrapMode.Once, Action callBack = null)
        {
            StartCoroutine(PlayAnimation(obj, childPath, animName, wrapMode, callBack));
        }

        private IEnumerator PlayAnimation(UnityEngine.Object obj, string childPath = "", string animName = "", WrapMode wrapMode = WrapMode.Once, Action callBack = null)
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
    }
}