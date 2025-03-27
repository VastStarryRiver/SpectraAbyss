using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using TMPro;



namespace Invariable
{
    [RequireComponent(typeof(LocalizeStringEvent))]
    public class UIText : TextMeshProUGUI
    {
        private LocalizeStringEvent m_localizeStringEvent;



        protected override void Awake()
        {
            base.Awake();

            m_localizeStringEvent = gameObject.GetComponent<LocalizeStringEvent>();

            m_localizeStringEvent.OnUpdateString.AddListener(UpdateText);
        }



        private void UpdateText(string localizedValue)
        {
            this.text = localizedValue;
        }

        public void SetTextByKey(string key, params object[] param)
        {
            m_localizeStringEvent.enabled = true;

            m_localizeStringEvent.StringReference = new LocalizedString(LanguageManager.m_table, key);

            if(param.Length > 0)
            {
                m_localizeStringEvent.StringReference.Arguments = param;
                m_localizeStringEvent.RefreshString();
            }
        }

        public void SetTextByString(string text)
        {
            m_localizeStringEvent.enabled = false;
            this.text = text;
        }
    }
}