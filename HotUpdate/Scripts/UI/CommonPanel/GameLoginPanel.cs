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

        public void Awake(GameObject gameObject, Transform transform)
        {
            m_btnLogin = transform.Find("Btn_Login").GetComponent<Button>();
            m_textName = transform.Find("Text_Name").GetComponent<Text>();
        }

        public void Start()
        {
            m_btnLogin.onClick.AddListener(LoginAccount);
            m_textName.text = "������ƣ�������";
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