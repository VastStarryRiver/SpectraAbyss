using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class DataUtilityManager
{
#if !UNITY_EDITOR
    public static string m_platform = "Android";
    public static string m_localRootPath = Application.persistentDataPath + "/";
    public static string m_webRootPath = LoadWebDataTxt(0);
#else
    public static string m_platform = "Windows";//当前平台
    public static string m_localRootPath = Application.streamingAssetsPath.Replace("Assets/StreamingAssets", "");//本地数据根目录
    public static string m_webRootPath = "";//服务器数据根目录
#endif

    public static string m_configPath = m_localRootPath + "ConfigData";//存放Excel配置表的路径
    public static string m_webIpv4Str = LoadWebDataTxt(3);//服务器的公网地址
    public static int m_webPortInt = int.Parse(LoadWebDataTxt(4));//服务器用于连接客户端的端口号



    private class BypassCertificate : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true; // 始终返回 true 以忽略证书验证
        }
    }

    public static void InitDirectory(string path)
    {
        path = path.Replace("\\", "/");

        string extension = Path.GetExtension(path);

        string directoryPath = "";

        if(string.IsNullOrEmpty(extension))
        {
            directoryPath = path;
        }
        else
        {
            directoryPath = Path.GetDirectoryName(path);
        }

        if (!Directory.Exists(directoryPath))
        {
            //确保路径中的所有文件夹都存在
            Directory.CreateDirectory(directoryPath);
        }
    }

    public static void SetWebQuestData(ref UnityWebRequest requestHandler)
    {
        string username = LoadWebDataTxt(1);
        string password = LoadWebDataTxt(2);
        string encodedAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + password));

        requestHandler.SetRequestHeader("Authorization", "Basic " + encodedAuth);

        requestHandler.certificateHandler = new BypassCertificate();
    }

    private static string LoadWebDataTxt(int index)
    {
        string text = "";

        using (UnityWebRequest requestHandler = UnityWebRequest.Get(Application.streamingAssetsPath + "/WebData.txt"))
        {
            requestHandler.SendWebRequest();

            while (!requestHandler.isDone)
            {
                // 等待请求完成
            }

            string[] des = requestHandler.downloadHandler.text.Split('\n');

            text = des[index].Replace("\r", "");
        }

        return text;
    }
}