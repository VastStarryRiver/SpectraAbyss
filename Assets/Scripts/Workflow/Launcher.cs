using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine.UI;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;



namespace Invariable
{
    public class Launcher : MonoBehaviour
    {
        private float m_needDownloadNum;
        private float m_sizeGetNum;
        private long m_needDownloadSize;
        private Dictionary<string, long> m_currSizeList;
        private List<string> m_deletePath;
        private List<string> m_updatePath;
        private Slider m_sliProgress;
        private UIText m_textDes;
        private UIText m_textProgress;
        private bool m_isDownloading;
        private string m_downloadCatalogueText;



        private void Awake()
        {
#if !UNITY_EDITOR
            DebugLogTool.InitDebugErrorLog();
            Application.logMessageReceived += DebugLogTool.ShowDebugErrorLog;
#endif

            CreateInitScene();

            m_needDownloadNum = -1;
            m_sizeGetNum = 0;
            m_needDownloadSize = 0;
            m_currSizeList = new Dictionary<string, long>();
            m_deletePath = new List<string>();
            m_updatePath = new List<string>();
            m_isDownloading = false;
            m_downloadCatalogueText = "";

#if UNITY_EDITOR
            m_needDownloadNum = 200;
            m_sizeGetNum = 200;
            m_needDownloadSize = 200;
#else
            AddressablesManager.SetWebQuestData();
            StartCoroutine(DownloadCatalogueFile());
#endif
        }

        private void Update()
        {
            if (m_needDownloadNum >= 0 && m_sizeGetNum >= m_needDownloadNum)
            {
#if UNITY_EDITOR
                m_currSizeList[Time.time.ToString()] = 1;
#else
                if(!m_isDownloading)
                {
                    m_isDownloading = true;

                    foreach (var filePath in m_deletePath)
                    {
                        FileInfo fileInfo = new FileInfo(DataUtilityManager.m_localRootPath + "/" + filePath);

                        if (fileInfo.Exists)
                        {
                            File.Delete(DataUtilityManager.m_localRootPath + "/" + filePath);
                            long fileSizeInBytes = fileInfo.Length;
                            m_currSizeList[filePath] = fileSizeInBytes;
                        }
                    }

                    foreach (var filePath in m_updatePath)
                    {
                        StartCoroutine(DownloadWebFile(filePath));
                    }
                }
#endif

                SetProgress(m_currSizeList, m_needDownloadSize);

                if (GetCurrDownloadSize(m_currSizeList) >= m_needDownloadSize)
                {
                    m_needDownloadNum = -1;

                    m_currSizeList.Clear();

                    if (!string.IsNullOrEmpty(m_downloadCatalogueText))
                    {
                        DataUtilityManager.SaveSafeFile(m_downloadCatalogueText, DataUtilityManager.m_localRootPath + "CatalogueFile.bin");
                    }

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
            m_textDes = loadingPanel.transform.Find("Text_Des").GetComponent<UIText>();
            m_textProgress = loadingPanel.transform.Find("Text_Progress").GetComponent<UIText>();

            SetDes("更新中");
        }

        private IEnumerator DownloadCatalogueFile()
        {
            string webPath = DataUtilityManager.WebRootPath + "CatalogueFiles/CatalogueFile.txt";
            UnityWebRequest requestHandler = UnityWebRequest.Get(webPath);//下载路径需要加上文件的后缀，没有后缀则不加

            DataUtilityManager.SetWebQuestData(ref requestHandler);

            yield return requestHandler.SendWebRequest();

            if (requestHandler.result == UnityWebRequest.Result.Success)
            {
                string downloadCatalogueText = requestHandler.downloadHandler.text;

                m_downloadCatalogueText = downloadCatalogueText;

                if (File.Exists(DataUtilityManager.m_localRootPath + "CatalogueFile.bin"))
                {
                    string localCatalogueText = DataUtilityManager.ReadSafeFile<string>(DataUtilityManager.m_localRootPath + "CatalogueFile.bin");

                    GetCatalogueDifferentData(downloadCatalogueText, localCatalogueText, out List<string> updatePath, out List<string> deletePath);

                    m_needDownloadNum = updatePath.Count + deletePath.Count;

                    m_deletePath = deletePath;
                    m_updatePath = updatePath;

                    foreach (var filePath in updatePath)
                    {
                        StartCoroutine(GetDownloadWebFileSize(filePath));
                    }

                    foreach (var filePath in deletePath)
                    {
                        FileInfo fileInfo = new FileInfo(DataUtilityManager.m_localRootPath + "/" + filePath);

                        if (fileInfo.Exists)
                        {
                            m_needDownloadSize += fileInfo.Length;
                        }

                        m_sizeGetNum++;
                    }
                }
                else
                {
                    Dictionary<string, string> webFileData = JsonConvert.DeserializeObject<Dictionary<string, string>>(downloadCatalogueText);

                    m_needDownloadNum = webFileData.Count;

                    foreach (var filePath in webFileData.Keys)
                    {
                        m_updatePath.Add(filePath);
                    }

                    foreach (var filePath in webFileData.Keys)
                    {
                        StartCoroutine(GetDownloadWebFileSize(filePath));
                    }
                }
            }
            else
            {
                SetDes(requestHandler.error + "\n" + webPath);
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

        private IEnumerator GetDownloadWebFileSize(string path)
        {
            string webPath = DataUtilityManager.WebRootPath + path;

            if (path.Contains("Assets/UpdateAssets/"))
            {
                AsyncOperationHandle<long> sizeHandle = Addressables.GetDownloadSizeAsync(path.Replace("Assets/UpdateAssets/", ""));

                yield return sizeHandle;

                if (sizeHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    m_needDownloadSize += sizeHandle.Result;
                }

                Addressables.Release(sizeHandle);

                m_sizeGetNum++;
            }
            else
            {
                UnityWebRequest headRequest = UnityWebRequest.Head(webPath);//发送HEAD请求获取文件大小

                DataUtilityManager.SetWebQuestData(ref headRequest);

                yield return headRequest.SendWebRequest();

                if (headRequest.result == UnityWebRequest.Result.Success)
                {
                    string contentLength = headRequest.GetResponseHeader("Content-Length");

                    if (!string.IsNullOrEmpty(contentLength) && long.TryParse(contentLength, out long totalSize))
                    {
                        m_needDownloadSize += totalSize;
                    }
                }
                else
                {
                    SetDes(headRequest.error + "\n" + webPath);
                }

                m_sizeGetNum++;

                headRequest.Dispose();
            }
        }

        private IEnumerator DownloadWebFile(string path)
        {
            string webPath = DataUtilityManager.WebRootPath + path;

            if (path.Contains("Assets/UpdateAssets/"))
            {
                AsyncOperationHandle downloadHandle = Addressables.DownloadDependenciesAsync(path.Replace("Assets/UpdateAssets/", ""));

                downloadHandle.Completed += (handle) =>
                {
                    if (handle.Status != AsyncOperationStatus.Succeeded)
                    {
                        SetDes(handle.OperationException + "\n" + webPath);
                    }

                    long downloadedBytes = downloadHandle.GetDownloadStatus().DownloadedBytes;
                    m_currSizeList[path] = downloadedBytes;

                    Addressables.Release(downloadHandle);
                };

                while (!downloadHandle.IsDone)
                {
                    long downloadedBytes = downloadHandle.GetDownloadStatus().DownloadedBytes;
                    m_currSizeList[path] = downloadedBytes;
                    yield return null;//确保主线程不被阻塞
                }
            }
            else
            {
                UnityWebRequest requestHandler = UnityWebRequest.Get(webPath);//下载路径需要加上文件的后缀，没有后缀则不加

                DataUtilityManager.SetWebQuestData(ref requestHandler);

                requestHandler.SendWebRequest();

                while (!requestHandler.isDone)
                {
                    long downloadedBytes = (long)requestHandler.downloadedBytes;
                    m_currSizeList[path] = downloadedBytes;
                    yield return null;//确保主线程不被阻塞
                }

                if (requestHandler.result == UnityWebRequest.Result.Success)
                {
                    string savePath = DataUtilityManager.m_localRootPath + path;

                    DataUtilityManager.InitDirectory(savePath);

                    using (FileStream fileStream = new FileStream(savePath, FileMode.Create))
                    {
                        using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
                        {
                            binaryWriter.Write(requestHandler.downloadHandler.data);
                        }
                    }

                    long downloadedBytes = (long)requestHandler.downloadedBytes;
                    m_currSizeList[path] = downloadedBytes;
                }
                else
                {
                    SetDes(requestHandler.error + "\n" + webPath);
                }

                requestHandler.Dispose();
            }
        }

        private void SetProgress(Dictionary<string, long> currSizeList, long needDownloadSize)
        {
            long currSize = GetCurrDownloadSize(currSizeList);
            m_sliProgress.value = (currSize * 1.00f) / (needDownloadSize * 1.00f);
            m_textProgress.SetTextByString(DataUtilityManager.FormatFileByteSize(currSize) + "/" + DataUtilityManager.FormatFileByteSize(needDownloadSize));
        }

        private long GetCurrDownloadSize(Dictionary<string, long> currSizeList)
        {
            long currSize = 0;

            foreach (var item in currSizeList)
            {
                currSize += item.Value;
            }

            return currSize;
        }

        private void SetDes(string text)
        {
            m_textDes.SetTextByString(text);
        }
    }
}