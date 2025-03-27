using UnityEngine;
using UnityEngine.UI;
using Invariable;



namespace HotUpdate
{
    public class GameLoadingPanel
    {
        private Slider m_sliProgress;
        private UIText m_textDes;
        private UIText m_textProgress;



        public void Awake(GameObject gameObject, Transform transform)
        {
            m_sliProgress = gameObject.transform.Find("Sli_Progress").GetComponent<Slider>();
            m_textDes = gameObject.transform.Find("Text_Des").GetComponent<UIText>();
            m_textProgress = gameObject.transform.Find("Text_Progress").GetComponent<UIText>();
        }



        public void SetProgress(float nowDownloadNum, float needDownloadNum)
        {
            m_sliProgress.value = nowDownloadNum / needDownloadNum;
            m_textProgress.SetTextByString(nowDownloadNum + "/" + needDownloadNum);
        }

        public void SetDes(string text)
        {
            m_textDes.SetTextByString(text);
        }
    }
}