using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;



public class MessageReceiver : MonoBehaviour
{
    private List<AndroidMessage> m_allMessages = new List<AndroidMessage>();
    private struct AndroidMessage
    {
        public int iMsgId;
        public int iParam1;
        public int iParam2;
        public int iParam3;
        public string strParam1;
        public string strParam2;
        public string strParam3;
    }



    private void Update()
    {
        if(m_allMessages.Count > 0)
        {
            for(int i = 0; i < m_allMessages.Count; i++)
            {
                AndroidMessage androidMessage = m_allMessages[i];
            }

            m_allMessages.Clear();
        }
    }



    protected void UnityReceiveAndroidMessage(string message)
    {
        JsonData jsonData = JsonMapper.ToObject(message);

        AndroidMessage androidMessage = new AndroidMessage();

        androidMessage.iMsgId = (int)jsonData["iMsgId"];
        androidMessage.iParam1 = (int)jsonData["iParam1"];
        androidMessage.iParam2 = (int)jsonData["iParam2"];
        androidMessage.iParam3 = (int)jsonData["iParam3"];
        androidMessage.strParam1 = (string)jsonData["strParam1"];
        androidMessage.strParam2 = (string)jsonData["strParam2"];
        androidMessage.strParam3 = (string)jsonData["strParam3"];

        m_allMessages.Add(androidMessage);
    }
}
