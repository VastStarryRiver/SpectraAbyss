using Invariable;
using UnityEngine;



namespace HotUpdate
{
    public class GameEnterMagaer
    {
        public static void Play()
        {
            ConvenientUtility.OpenUIPrefabPanel("Prefabs/UI/CommonPanel/GameLoginPanel", 100);
            GameObject.Destroy(GameObject.Find("UI_Root/Ts_Panel/HotUpdateLoadingPanel"));
        }
    }
}