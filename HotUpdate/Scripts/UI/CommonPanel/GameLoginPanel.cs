using UnityEngine;
using Invariable;
using TapSDK.Login;



namespace HotUpdate
{
    public class GameLoginPanel
    {
        UIButton m_btnLogin;
        UIButton m_btnIcon1;
        UIButton m_btnIcon2;
        UIText m_textName;
        GameObject m_obj;

        public void Awake(GameObject gameObject, Transform transform)
        {
            m_btnLogin = transform.Find("parent/Btn_Login").GetComponent<UIButton>();
            m_btnIcon1 = transform.Find("parent/Img_Icon1").GetComponent<UIButton>();
            m_btnIcon2 = transform.Find("parent/Img_Icon2").GetComponent<UIButton>();
            m_textName = transform.Find("parent/Text_Name").GetComponent<UIText>();
            m_obj = gameObject;
        }

        public void Start()
        {
            m_textName.SetTextByKey("scene_key8", "#FF0000", "？？？");

            CoroutineManager.Instance.PlayAnimation(m_obj, "", "Play", WrapMode.Once, () => {
                ConvenientUtility.SetSpriteImage(m_obj, "parent/Img_Icon1", "Atlas02/02_FunOpen10", true);
                ConvenientUtility.SetSpriteImage(m_obj, "parent/Img_Icon2", "Atlas02/02_FunOpen12", true);

                ConvenientUtility.SetGray(m_obj, "parent/Img_Icon1");

                m_btnLogin.AddClickListener(LoginAccount);
                m_btnIcon1.AddClickListener(() => { LanguageManager.SetLanguageIndex(0); });
                m_btnIcon2.AddClickListener(() => { LanguageManager.SetLanguageIndex(1); });
            });
        }



        void LoginAccount()
        {
            SdkManager.LoginAccount(LoginAccountRec);
        }

        void LoginAccountRec(TapTapAccount tapTapAccount)
        {
            Debug.Log($"登录成功，当前用户昵称：{tapTapAccount.name}");
            m_textName.SetTextByString("玩家名称：" + tapTapAccount.name);
        }
    }
}