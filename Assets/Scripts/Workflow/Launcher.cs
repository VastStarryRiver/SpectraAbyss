using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine.UI;



namespace Invariable
{
    public class Launcher : MonoBehaviour
    {
        private float m_nowDownloadNum;
        private float m_needDownloadNum;
        private Slider m_sliProgress;
        private Text m_textDes;
        private Text m_textProgress;



        private void Awake()
        {
#if !UNITY_EDITOR
            DebugLogTool.InitDebugErrorLog();
            Application.logMessageReceived += DebugLogTool.ShowDebugErrorLog;
#endif

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

                SetProgress(m_nowDownloadNum, m_needDownloadNum);

                if (m_nowDownloadNum >= m_needDownloadNum)
                {
                    m_needDownloadNum = -1;
                    SdkManager.InitTapTapSdkOptions();
                    MessageNetManager.Play();
                    HotUpdateManager.Init();
                }
            }
        }

        private void OnDestroy()
        {
            SdkManager.LogoutAccount();
            MessageNetManager.Stop();
            HotUpdateManager.UnloadDll();

#if !UNITY_EDITOR
            Application.logMessageReceived -= DebugLogTool.ShowDebugErrorLog;
#endif
        }



        private void CreateInitScene()
        {
            GameObject SceneGameObject = Instantiate(Resources.Load<GameObject>("SceneGameObject"), Vector3.zero, Quaternion.identity);
            GameObject UI_Root = Instantiate(Resources.Load<GameObject>("UI_Root"), Vector3.zero, Quaternion.identity);

            SceneGameObject.name = "SceneGameObject";
            UI_Root.name = "UI_Root";

            DontDestroyOnLoad(SceneGameObject);
            DontDestroyOnLoad(UI_Root);

            GameObject loadingPanel = Instantiate(Resources.Load<GameObject>("HotUpdateLoadingPanel"), Vector3.zero, Quaternion.identity, UI_Root.transform.Find("Ts_Panel"));
            loadingPanel.name = "HotUpdateLoadingPanel";
            m_sliProgress = loadingPanel.transform.Find("Sli_Progress").GetComponent<Slider>();
            m_textDes = loadingPanel.transform.Find("Text_Des").GetComponent<Text>();
            m_textProgress = loadingPanel.transform.Find("Text_Progress").GetComponent<Text>();

            SetDes("加载中");
        }

        private IEnumerator DownloadCatalogueFile()
        {
            string webPath = DataUtilityManager.m_webRootPath + "CatalogueFiles/" + DataUtilityManager.m_platform + "/CatalogueFile.txt";
            UnityWebRequest requestHandler = UnityWebRequest.Get(webPath);//下载路径需要加上文件的后缀，没有后缀则不加

            DataUtilityManager.SetWebQuestData(ref requestHandler);

            yield return requestHandler.SendWebRequest();

            if (requestHandler.isHttpError || requestHandler.isNetworkError)
            {
                SetDes(requestHandler.error + "\n" + webPath);
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

                                Dictionary<string, string> webFileData = JsonConvert.DeserializeObject<Dictionary<string, string>>(downloadCatalogueText);

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

                                if (m_needDownloadNum > 0)
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

            Dictionary<string, string> webFileData = JsonConvert.DeserializeObject<Dictionary<string, string>>(downloadCatalogueText);
            Dictionary<string, string> localFileData = JsonConvert.DeserializeObject<Dictionary<string, string>>(localCatalogueText);

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
                SetDes(requestHandler.error + "\n" + webPath);
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

        private void SetProgress(float nowDownloadNum, float needDownloadNum)
        {
            m_sliProgress.value = nowDownloadNum / needDownloadNum;
            m_textProgress.text = nowDownloadNum + "/" + needDownloadNum;
        }

        private void SetDes(string text)
        {
            m_textDes.text = text;
        }
    }
}