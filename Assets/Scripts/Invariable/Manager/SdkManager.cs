using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;



namespace Invariable
{
    public class SdkManager : Singleton<SdkManager>
    {
        private List<ScreenAdapter> m_screenAdapters = null;



        #region 登录
        /// <summary>
        /// 平台登录，编辑器返回空，真机返回设备访客标识
        /// </summary>
        public Task<string> PlatformLogin()
        {
#if UNITY_EDITOR
            return Task.FromResult<string>(null);
#else
            return Task.FromResult(GetOrCreateDeviceId());
#endif
        }

        /// <summary>
        /// 读取设备标识，空值时生成并写入本地存储
        /// </summary>
        private string GetOrCreateDeviceId()
        {
            string deviceId = SystemInfo.deviceUniqueIdentifier;

            if (!string.IsNullOrEmpty(deviceId) && deviceId != "n/a")
            {
                return deviceId;
            }

            deviceId = GetLocalData(InvariableConst.LocalKey_AppDeviceId, "");

            if (string.IsNullOrEmpty(deviceId))
            {
                deviceId = Guid.NewGuid().ToString("N");
                SetLocalData(InvariableConst.LocalKey_AppDeviceId, deviceId);
            }

            return deviceId;
        }
        #endregion

        #region 数据存储
        /// <summary>
        /// 写入本地存储
        /// </summary>
        public void SetLocalData(string key, string data)
        {
            PlayerPrefs.SetString(key, data);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 读取本地存储
        /// </summary>
        public string GetLocalData(string key, string defaultValue = "")
        {
            string data = PlayerPrefs.GetString(key, defaultValue);

            if (string.IsNullOrEmpty(data))
            {
                data = defaultValue;
            }

            return data;
        }

        /// <summary>
        /// 写入云端缓存
        /// </summary>
        public void SetCloudData(string key, string data)
        {
#if UNITY_EDITOR
            SetLocalData(key, data);
#else
            CloudManager.Instance.SetCloudCache(key, data);
#endif
        }

        /// <summary>
        /// 读取云端缓存
        /// </summary>
        public string GetCloudData(string key, string defaultValue = "")
        {
#if UNITY_EDITOR
            return GetLocalData(key, defaultValue);
#else
            return CloudManager.Instance.GetCloudCache(key, defaultValue);
#endif
        }
        #endregion

        #region 屏幕适配
        /// <summary>
        /// 注册屏幕适配器并按安全区刷新锚点
        /// </summary>
        public void AddScreenAdapter(ScreenAdapter screenAdapter)
        {
            m_screenAdapters ??= new List<ScreenAdapter>();

            if (!m_screenAdapters.Contains(screenAdapter))
            {
                m_screenAdapters.Add(screenAdapter);
            }

            ApplySafeArea(screenAdapter);
        }

        /// <summary>
        /// 移除屏幕适配器
        /// </summary>
        public void RemoveScreenAdapter(ScreenAdapter screenAdapter)
        {
            if (m_screenAdapters != null && m_screenAdapters.Contains(screenAdapter))
            {
                m_screenAdapters.Remove(screenAdapter);
            }
        }

        /// <summary>
        /// 按设备安全区刷新屏幕适配器锚点
        /// </summary>
        public void ApplySafeArea(ScreenAdapter screenAdapter = null)
        {
            if (screenAdapter != null)
            {
                ApplySafeAreaToAdapter(screenAdapter);
            }
            else if (m_screenAdapters != null)
            {
                for (int i = 0; i < m_screenAdapters.Count; i++)
                {
                    ApplySafeAreaToAdapter(m_screenAdapters[i]);
                }
            }
        }

        /// <summary>
        /// 把设备 Screen.safeArea 换成画布偏移后写入适配节点
        /// </summary>
        private void ApplySafeAreaToAdapter(ScreenAdapter screenAdapter)
        {
            if (screenAdapter == null)
            {
                return;
            }

            RectTransform panel = screenAdapter.transform.GetComponent<RectTransform>();

            if (panel == null)
            {
                return;
            }

            if (!TryGetSafeAnchor(panel, out Vector2 offsetMin, out Vector2 offsetMax))
            {
                return;
            }

            if (panel.offsetMin == offsetMin && panel.offsetMax == offsetMax)
            {
                return;
            }

            panel.offsetMin = offsetMin;
            panel.offsetMax = offsetMax;
        }

        /// <summary>
        /// 按宿主 Canvas 把 Screen.safeArea 换成画布 offsetMin / offsetMax
        /// </summary>
        public bool TryGetSafeAnchor(RectTransform panel, out Vector2 offsetMin, out Vector2 offsetMax)
        {
            offsetMin = Vector2.zero;
            offsetMax = Vector2.zero;

            if (panel == null || Screen.width == 0 || Screen.height == 0)
            {
                return false;
            }

            // 适配节点挂在运行时 UI 根下，父级 Canvas 不固定，不能预绑定
            Canvas canvas = panel.GetComponentInParent<Canvas>();

            if (canvas == null)
            {
                return false;
            }

            RectTransform canvasRect = canvas.transform as RectTransform;

            if (canvasRect == null)
            {
                return false;
            }

            Rect canvasArea = canvasRect.rect;
            float scaleX = canvasArea.width / Screen.width;
            float scaleY = canvasArea.height / Screen.height;
            Rect safeArea = Screen.safeArea;
            offsetMin = new Vector2(safeArea.xMin * scaleX, safeArea.yMin * scaleY);
            offsetMax = new Vector2((safeArea.xMax - Screen.width) * scaleX, (safeArea.yMax - Screen.height) * scaleY);

            return true;
        }
        #endregion

        #region 环境
        /// <summary>
        /// 登录令牌与玩家存档显示名使用的平台键
        /// </summary>
        public string GetPlatformId()
        {
#if UNITY_EDITOR
            return "editor";
#elif UNITY_ANDROID
            return "android";
#elif UNITY_IOS
            return "ios";
#elif UNITY_OPENHARMONY
            return "ohos";
#else
            return "unknown";
#endif
        }

        /// <summary>
        /// HybridCLR DLL 与本机 CDN 目录前缀，苹果为 iOS
        /// </summary>
        public string GetDllPlatform()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return "Android";
#elif UNITY_IOS && !UNITY_EDITOR
            return "iOS";
#elif UNITY_OPENHARMONY && !UNITY_EDITOR
            return "OpenHarmony";
#else
            return "Editor";
#endif
        }

        /// <summary>
        /// 获取当前平台 CDN 根地址，编辑器返回空
        /// </summary>
        public string GetCDNPath()
        {
            string platform = GetDllPlatform();

            if (platform == "Android")
            {
                return InvariableConst.CDNPathAndroid;
            }

            if (platform == "iOS")
            {
                return InvariableConst.CDNPathiOS;
            }

            if (platform == "OpenHarmony")
            {
                return InvariableConst.CDNPathOpenHarmony;
            }

            return "";
        }
        #endregion
    }
}