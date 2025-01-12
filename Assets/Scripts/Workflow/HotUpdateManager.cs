using System;
using System.IO;
using System.Collections.Generic;
using ILRuntime.Runtime.Intepreter;
using TapSDK.Login;



namespace Invariable
{
    public class HotUpdateManager
    {
        public static ILRuntime.Runtime.Enviorment.AppDomain m_appdomain;
        private static FileStream m_dllFileStream;
        public static Dictionary<string, ILTypeInstance> m_allInstance;



        public static void Init()
        {
            UnloadDll();

            LoadDll();

            m_appdomain.Invoke("HotUpdate.GameEnterMagaer", "Play", null, null);
        }

        public static void UnloadDll()
        {
            if (m_appdomain != null && m_dllFileStream != null)
            {
                m_appdomain.Invoke("HotUpdate.AssetBundleManager", "Clear", null, null);
                m_dllFileStream.Dispose();
                m_dllFileStream.Close();
                m_dllFileStream = null;
                m_appdomain = null;
                m_allInstance = null;
            }
        }

        public static void LoadDll()
        {
            if (m_appdomain == null && m_dllFileStream == null)
            {
                m_appdomain = new ILRuntime.Runtime.Enviorment.AppDomain();

                RegisterDelegateConvertor();

                LoadAssembly();
            }
        }

        private static void RegisterDelegateConvertor()
        {
            m_appdomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction>((act) =>
            {
                return new UnityEngine.Events.UnityAction(() =>
                {
                    ((Action)act)();
                });
            });

            m_appdomain.DelegateManager.RegisterMethodDelegate<TapTapAccount>();

            m_appdomain.DelegateManager.RegisterDelegateConvertor<Action<TapTapAccount>>((act) =>
            {
                return new Action<TapTapAccount>((param) =>
                {
                    ((Action<TapTapAccount>)act)(param);
                });
            });

            m_appdomain.DelegateManager.RegisterMethodDelegate<string, object>();
        }

        private static void LoadAssembly()
        {
            m_allInstance = new Dictionary<string, ILTypeInstance>();

            string dllPath = Path.Combine(DataUtilityManager.m_localRootPath, "Dll", "HotUpdate.dll");
            m_dllFileStream = new FileStream(dllPath, FileMode.Open);

            m_appdomain.LoadAssembly(m_dllFileStream);
        }
    }
}