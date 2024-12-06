using UnityEngine;
using UnityEngine.UI;

namespace HotUpdate
{
    public class GameLoadingPanel
    {
        private Slider m_sliProgress;
        private Text m_textDes;
        private Text m_textProgress;



        public void Awake(GameObject gameObject, Transform transform)
        {
            m_sliProgress = gameObject.transform.Find("Sli_Progress").GetComponent<Slider>();
            m_textDes = gameObject.transform.Find("Text_Des").GetComponent<Text>();
            m_textProgress = gameObject.transform.Find("Text_Progress").GetComponent<Text>();
        }



        public void SetProgress(float nowDownloadNum, float needDownloadNum)
        {
            m_sliProgress.value = nowDownloadNum / needDownloadNum;
            m_textProgress.text = nowDownloadNum + "/" + needDownloadNum;
        }

        public void SetDes(string text)
        {
            m_textDes.text = text;
        }
    }
}