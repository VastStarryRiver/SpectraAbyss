using System.IO;
using UnityEngine;



namespace Invariable
{
    public static class DebugLogTool
    {
        public static void DebugRedLog(string log)
        {
            Debug.Log("<color=#FF2D00>" + log + "</color>");
        }

        public static void DebugYellowLog(string log)
        {
            Debug.Log("<color=#FFFF00>" + log + "</color>");
        }

        public static void DebugBlueLog(string log)
        {
            Debug.Log("<color=#002EFF>" + log + "</color>");
        }

        public static void DebugGreenLog(string log)
        {
            Debug.Log("<color=#00FF11>" + log + "</color>");
        }

        public static void DebugBlackLog(string log)
        {
            Debug.Log("<color=#000000>" + log + "</color>");
        }

#if !UNITY_EDITOR
        public static void InitDebugErrorLog()
        {
            DataUtilityManager.InitDirectory(DataUtilityManager.m_localRootPath);

            using (FileStream fileStream = new FileStream(DataUtilityManager.m_localRootPath + "Error.txt", FileMode.Create))
            {
                using (StreamWriter streamWriter = new StreamWriter(fileStream))
                {
                    streamWriter.Write("");
                }
            }
        }

        public static void ShowDebugErrorLog(string logString, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception)
            {
                string error = logString + "\n" + stackTrace;

                using (FileStream fileStream = new FileStream(DataUtilityManager.m_localRootPath + "Error.txt", FileMode.Open))
                {
                    using (StreamReader streamReader = new StreamReader(fileStream))
                    {
                        string lastLog = streamReader.ReadToEnd();

                        using (StreamWriter streamWriter = new StreamWriter(fileStream))
                        {
                            if (!string.IsNullOrEmpty(lastLog))
                            {
                                streamWriter.Write(lastLog + "\n\n\n" + error);
                            }
                            else
                            {
                                streamWriter.Write(error);
                            }
                        }
                    }
                }
            }
        }
#endif
    }
}