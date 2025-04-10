using UnityEngine;
using UnityEngine.Localization.Settings;



namespace Invariable
{
    public static class LanguageManager
    {
        public const string m_table = "Language";

        public static int LanguageIndex
        {
            get
            {
                int index = PlayerPrefs.GetInt(Application.productName + "_LanguageIndex", -1);

                if(index == -1)
                {
                    for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; i++)
                    {
                        if(LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.Locales[i])
                        {
                            return i;
                        }
                    }
                }

                return index;
            }
        }

        public static void SetLanguageIndex(int index, bool isRestart = true)
        {
            if (LocalizationSettings.AvailableLocales.Locales[index] == null)
            {
                return;
            }

            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];

            PlayerPrefs.SetInt(Application.productName + "_LanguageIndex", index);

            if(isRestart)
            {
                CoroutineManager.Instance.InvokeRestartGame();
            }
        }
    }
}