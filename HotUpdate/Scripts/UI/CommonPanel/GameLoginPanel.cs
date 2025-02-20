using TapSDK.Login;
using UnityEngine;
using UnityEngine.UI;
using Invariable;



namespace HotUpdate
{
    public class GameLoginPanel
    {
        Button m_btnLogin;
        Text m_textName;
        GameObject m_obj;

        public void Awake(GameObject gameObject, Transform transform)
        {
            m_btnLogin = transform.Find("Btn_Login").GetComponent<Button>();
            m_textName = transform.Find("Text_Name").GetComponent<Text>();
            m_obj = gameObject;
        }

        public void Start()
        {
            m_textName.text = "������ƣ�������";

            CoroutineManager.Instance.PlayAnimation(m_obj, "", "Play", WrapMode.Once, () => {
                m_btnLogin.onClick.AddListener(LoginAccount);
                ConvenientUtility.SetSpriteImage(m_obj, "Img_Icon1", "Atlas02/02_FunOpen10", true);
                ConvenientUtility.SetSpriteImage(m_obj, "Img_Icon2", "Atlas02/02_FunOpen12", true);
                ConvenientUtility.SetGray(m_obj, "Img_Icon1");
            });
        }



        void LoginAccount()
        {
            SdkManager.LoginAccount(LoginAccountRec);
        }

        void LoginAccountRec(TapTapAccount tapTapAccount)
        {
            Debug.Log($"��¼�ɹ�����ǰ�û��ǳƣ�{tapTapAccount.name}");
            m_textName.text = "������ƣ�" + tapTapAccount.name;
        }
    }
}