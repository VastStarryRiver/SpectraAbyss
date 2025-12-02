using System;
using System.Reflection;



namespace Invariable
{
    public class HotUpdateOver : IStateNode
    {
        private StateMachine m_machine;

        public void OnCreate(StateMachine machine)
        {
            m_machine = machine;
        }

        public void OnEnter()
        {
            StartGame();
        }

        public void OnExit()
        {

        }

        public void OnUpdate()
        {

        }

        /// <summary>
        /// 开始游戏
        /// </summary>
        private void StartGame()
        {
            GameManager.Instance.InvokeEventCallBack("Launcher_ShowTips", "初始化运行系统");

            LanguageManager.Instance.SetLanguageKey(LanguageManager.Instance.LanguageKey, false);
            SdkManager.Instance.InitTapTapSdkOptions();
            MessageNetManager.Instance.Reset();

            YooAssetManager.Instance.PreLoadDll((hotUpdateAss) =>
            {
                GameManager.Instance.InvokeEventCallBack("Launcher_ShowTips", "开始游戏");
                Type type = hotUpdateAss.GetType("HotUpdate.StartGame");
                MethodInfo methodInfo = type.GetMethod("Play", BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                methodInfo.Invoke(null, null);
                GameManager.Instance.InvokeEventCallBack("Launcher_StartGame");
            });
        }
    }
}