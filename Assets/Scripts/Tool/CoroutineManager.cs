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



        public void PlayAnimation(UnityEngine.Object obj, string childPath = "", string animName = "", WrapMode wrapMode = WrapMode.Once, Action callBack = null)
        {
            StartCoroutine(ConvenientUtility.PlayAnimation(obj, childPath, animName, wrapMode, callBack));
        }

        public void LoadAddressables(string addressablesKey, Action<object> callBack)
        {
            StartCoroutine(AddressablesManager.LoadAddressables(addressablesKey, callBack));
        }
    }
}