using UnityEngine;
using Invariable;
using TapSDK.Login;



namespace HotUpdate
{
    public class GameLoginPanel
    {
        UIButton m_btnLogin;
        UIText m_textName;
        GameObject m_obj;

        public void Awake(GameObject gameObject, Transform transform)
        {
            m_btnLogin = transform.Find("Btn_Login").GetComponent<UIButton>();
            m_textName = transform.Find("Text_Name").GetComponent<UIText>();
            m_obj = gameObject;
        }

        public void Start()
        {
            LanguageManager.SetLanguageIndex(0);

            m_textName.SetTextByKey("scene_key5");

            CoroutineManager.Instance.PlayAnimation(m_obj, "", "Play", WrapMode.Once, () => {
                m_btnLogin.AddClickListener(LoginAccount);
                ConvenientUtility.SetSpriteImage(m_obj, "Img_Icon1", "Atlas02/02_FunOpen10", true);
                ConvenientUtility.SetSpriteImage(m_obj, "Img_Icon2", "Atlas02/02_FunOpen12", true);
                ConvenientUtility.SetGray(m_obj, "Img_Icon1");
                LanguageManager.SetLanguageIndex(1);
            });
        }



        void LoginAccount()
        {
            SdkManager.LoginAccount(LoginAccountRec);
        }

        void LoginAccountRec(TapTapAccount tapTapAccount)
        {
            Debug.Log($"��¼�ɹ�����ǰ�û��ǳƣ�{tapTapAccount.name}");
            m_textName.SetTextByString("������ƣ�" + tapTapAccount.name);
        }
    }
}