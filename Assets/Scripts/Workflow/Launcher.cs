using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using LitJson;



public class Launcher : MonoBehaviour
{
    private GameObject m_loadingPanel;
    private GameLoadingPanel m_loadingInfo;
    private float m_nowDownloadNum;
    private float m_needDownloadNum;



    private void Awake()
    {
        DebugLogDes.InitDebugErrorLog();

        Application.logMessageReceived += DebugLogDes.ShowDebugErrorLog;

        CreateInitScene();

        m_nowDownloadNum = 0;
        m_needDownloadNum = -1;

        if (DataUtilityManager.m_platform == "Windows")
        {
            m_needDownloadNum = 3;
        }
        else
        {
            StartCoroutine(DownloadCatalogueFile());
        }
    }

    private void Update()
    {
        if (m_needDownloadNum >= 0)
        {
#if UNITY_EDITOR
            m_nowDownloadNum += Time.deltaTime;
#endif

            m_loadingInfo.SetProgress(m_nowDownloadNum, m_needDownloadNum);

            if (m_nowDownloadNum >= m_needDownloadNum)
            {
                m_needDownloadNum = -1;
                SdkMsgManager.Instance.Init();
                MessageNetManager.Instance.Play();
                LuaManager.Instance.Reset();
                Destroy(m_loadingPanel);
            }
        }
    }

    private void OnDestroy()
    {
        LuaManager.Instance.Stop();
        MessageNetManager.Instance.Stop();
        AssetBundleManager.Clear();
        Application.logMessageReceived -= DebugLogDes.ShowDebugErrorLog;
    }



    private void CreateInitScene()
    {
        GameObject SceneGameObject = Instantiate(Resources.Load<GameObject>("SceneGameObject"), Vector3.zero, Quaternion.identity);
        GameObject UI_Root = Instantiate(Resources.Load<GameObject>("UI_Root"), Vector3.zero, Quaternion.identity);

        SceneGameObject.name = "SceneGameObject";
        UI_Root.name = "UI_Root";

        DontDestroyOnLoad(SceneGameObject);
        DontDestroyOnLoad(UI_Root);

        m_loadingPanel = Instantiate(Resources.Load<GameObject>("GameLoadingPanel"), UI_Root.transform.Find("Canvas_0/Ts_Panel"));
        m_loadingPanel.name = "GameLoadingPanel";
        m_loadingInfo = m_loadingPanel.GetComponent<GameLoadingPanel>();
    }

    private IEnumerator DownloadCatalogueFile()
    {
        string webPath = DataUtilityManager.m_webRootPath + "CatalogueFiles/" + DataUtilityManager.m_platform + "/CatalogueFile.txt";
        UnityWebRequest requestHandler = UnityWebRequest.Get(webPath);//下载路径需要加上文件的后缀，没有后缀则不加

        DataUtilityManager.SetWebQuestData(ref requestHandler);

        yield return requestHandler.SendWebRequest();

        if (requestHandler.isHttpError || requestHandler.isNetworkError)
        {
            m_loadingInfo.SetDes(requestHandler.error + "\n" + webPath);
        }
        else
        {
            string downloadCatalogueText = requestHandler.downloadHandler.text;

            DataUtilityManager.InitDirectory(DataUtilityManager.m_localRootPath);

            using (FileStream fileStream = new FileStream(DataUtilityManager.m_localRootPath + "CatalogueFile.txt", FileMode.OpenOrCreate))
            {
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    string localCatalogueText = streamReader.ReadToEnd();

                    if (string.IsNullOrEmpty(localCatalogueText))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(fileStream))
                        {
                            streamWriter.Write(downloadCatalogueText);

                            Dictionary<string, string> webFileData = JsonMapper.ToObject<Dictionary<string, string>>(downloadCatalogueText);

                            m_needDownloadNum = webFileData.Count;

                            foreach (var filePath in webFileData.Keys)
                            {
                                StartCoroutine(DownloadWebFile(filePath));
                            }
                        }
                    }
                    else
                    {
                        using (StreamWriter streamWriter = new StreamWriter(fileStream))
                        {
                            GetCatalogueDifferentData(downloadCatalogueText, localCatalogueText, out List<string> updatePath, out List<string> deletePath);

                            m_needDownloadNum = updatePath.Count + deletePath.Count;

                            if(m_needDownloadNum > 0)
                            {
                                foreach (var filePath in deletePath)
                                {
                                    if (File.Exists(DataUtilityManager.m_localRootPath + "/" + filePath))
                                    {
                                        File.Delete(DataUtilityManager.m_localRootPath + "/" + filePath);
                                    }

                                    m_nowDownloadNum++;
                                }

                                foreach (var filePath in updatePath)
                                {
                                    StartCoroutine(DownloadWebFile(filePath));
                                }

                                streamWriter.Write(downloadCatalogueText);
                            }
                        }
                    }
                }
            }
        }

        requestHandler.Dispose();
    }

    private void GetCatalogueDifferentData(string downloadCatalogueText, string localCatalogueText, out List<string> updatePath, out List<string> deletePath)
    {
        updatePath = new List<string>();
        deletePath = new List<string>();

        Dictionary<string, string> webFileData = JsonMapper.ToObject<Dictionary<string, string>>(downloadCatalogueText);
        Dictionary<string, string> localFileData = JsonMapper.ToObject<Dictionary<string, string>>(localCatalogueText);

        foreach (var filePath in webFileData.Keys)
        {
            if (!localFileData.ContainsKey(filePath) || (localFileData.ContainsKey(filePath) && localFileData[filePath] != webFileData[filePath]))
            {
                updatePath.Add(filePath);
            }
        }

        foreach (var filePath in localFileData.Keys)
        {
            if (!webFileData.ContainsKey(filePath))
            {
                deletePath.Add(filePath);
            }
        }
    }

    private IEnumerator DownloadWebFile(string path)
    {
        string webPath = DataUtilityManager.m_webRootPath + path;
        UnityWebRequest requestHandler = UnityWebRequest.Get(webPath);//下载路径需要加上文件的后缀，没有后缀则不加

        DataUtilityManager.SetWebQuestData(ref requestHandler);

        yield return requestHandler.SendWebRequest();

        if (requestHandler.isHttpError || requestHandler.isNetworkError)
        {
            m_loadingInfo.SetDes(requestHandler.error + "\n" + webPath);
        }
        else
        {
            string savePath = "";

            if (path.IndexOf("Assets/") == 0)
            {
                savePath = DataUtilityManager.m_localRootPath + path.Replace("Assets/", "");
            }
            else
            {
                savePath = DataUtilityManager.m_localRootPath + path;
            }

            DataUtilityManager.InitDirectory(savePath);

            using (FileStream fileStream = new FileStream(savePath, FileMode.Create))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
                {
                    binaryWriter.Write(requestHandler.downloadHandler.data);
                }
            }

            m_nowDownloadNum++;
        }

        requestHandler.Dispose();
    }
}