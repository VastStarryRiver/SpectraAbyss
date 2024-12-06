using System.Collections.Generic;
using UnityEngine;
using TapSDK.Core;
using TapSDK.Login;
using System.Threading.Tasks;
using System;

namespace Invariable
{
    public class SdkManager
    {
        private const string tapTapId = "lipmkcrzigd2ymxbsr";
        private const string tapTapToken = "X9bqJ44ejsHetkqrrtRFgGpX3wU8stawQB3Aaaei";
        private static TapTapSdkOptions tapTapSdkOptions = null;
        private static TapTapAccount tapTapAccount = null;

        public static void InitTapTapSdkOptions()
        {
            if (tapTapSdkOptions != null)
            {
                return;
            }

            tapTapSdkOptions = new TapTapSdkOptions
            {
                clientId = tapTapId,//ID�������ߺ�̨��ȡ

                clientToken = tapTapToken,//���ƣ������ߺ�̨��ȡ

                region = TapTapRegionType.CN,// ������CN Ϊ���ڣ�Overseas Ϊ����

                preferredLanguage = TapTapLanguageType.zh_Hans,// ���ԣ�Ĭ��Ϊ Auto��Ĭ������£�����Ϊ zh_Hans������Ϊ en

                enableLog = true,// �Ƿ�����־��Release �汾������Ϊ false
            };

            TapTapSDK.Init(tapTapSdkOptions);

            // ����Ҫ�������ģ��ĳ�ʼ�����������Ϲ���֤���ɾ͵ȣ� ��ʹ������ API
            TapTapSdkBaseOptions[] otherOptions = new TapTapSdkBaseOptions[]
            {

            };

            TapTapSDK.Init(tapTapSdkOptions, otherOptions);
        }

        public static void LoginAccount(Action<TapTapAccount> callBack = null)
        {
            try
            {
                AsyncLoginAccount(callBack);
            }
            catch (TaskCanceledException)
            {
                Debug.Log("�û�ȡ����¼");
            }
            catch (Exception exception)
            {
                Debug.Log($"��¼ʧ�ܣ������쳣��{exception}");
            }
        }

        private static async void AsyncLoginAccount(Action<TapTapAccount> callBack = null)
        {
            // ������Ȩ��Χ
            List<string> scopes = new List<string>() { TapTapLogin.TAP_LOGIN_SCOPE_PUBLIC_PROFILE };
            tapTapAccount = await TapTapLogin.Instance.LoginWithScopes(scopes.ToArray());
            callBack?.Invoke(tapTapAccount);
        }

        public static void LogoutAccount()
        {
            if (tapTapAccount == null)
            {
                return;
            }

            TapTapLogin.Instance.Logout();
        }
    }
}