using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;



namespace Invariable
{
    public class AddressablesManager
    {
        private static Dictionary<string, AsyncOperationHandle<object>> m_allAddressables = null;//�Ѿ����ص�Addressables��



        public static void SetWebQuestData()
        {
            // ����ȫ����֤ͷ
            Addressables.WebRequestOverride = (UnityWebRequest requestHandler) =>
            {
                string username = DataUtilityManager.LoadWebData(1);
                string password = DataUtilityManager.LoadWebData(2);
                string encodedAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + password));

                requestHandler.SetRequestHeader("Authorization", "Basic " + encodedAuth);

                requestHandler.certificateHandler = new DataUtilityManager.BypassCertificate();
            };
        }

        public static void Clear()
        {
            if (m_allAddressables != null && m_allAddressables.Count > 0)
            {
                foreach (var item in m_allAddressables)
                {
                    Addressables.Release(item);
                }

                m_allAddressables.Clear();
            }

            m_allAddressables = null;

            Caching.ClearCache();
        }

        public static IEnumerator LoadAddressables(string addressablesKey, Action<object> callBack)
        {
            if (m_allAddressables == null)
            {
                m_allAddressables = new Dictionary<string, AsyncOperationHandle<object>>();
            }

            if (!m_allAddressables.ContainsKey(addressablesKey))
            {
                AsyncOperationHandle<object> handle = Addressables.LoadAssetAsync<object>(addressablesKey);

                yield return handle;

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    object asset = handle.Result;
                    callBack(asset);
                }
            }
            else if (m_allAddressables[addressablesKey].Status == AsyncOperationStatus.Succeeded)
            {
                object asset = m_allAddressables[addressablesKey].Result;
                callBack(asset);
            }
        }
    }
}