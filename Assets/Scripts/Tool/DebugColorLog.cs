using System.IO;
using UnityEngine;

public static class DebugLogDes
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

    public static void ShowDebugErrorLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception)
        {
            string error = "";
            string luaDebug = "";

            if(LuaManager.Instance.m_luaState != null)
            {
                luaDebug = LuaManager.Instance.m_luaState.GetFunction("debug.traceback").Invoke<object, object, object, string>(null, null, null);
            }

            if (!string.IsNullOrEmpty(luaDebug) && luaDebug.Contains("[C]: in function"))
            {
                error = "<color=#04FF10FF>Lua调用C#导致的报错：Lua堆栈</color>\n" + luaDebug;
                DebugGreenLog(error);
            }
            else
            {
                error = logString + "\n" + stackTrace;
            }

#if !UNITY_EDITOR
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
#endif
        }
    }

    public static void InitDebugErrorLog()
    {
#if !UNITY_EDITOR
        DataUtilityManager.InitDirectory(DataUtilityManager.m_localRootPath);

        using (FileStream fileStream = new FileStream(DataUtilityManager.m_localRootPath + "Error.txt", FileMode.Create))
        {
            using (StreamWriter streamWriter = new StreamWriter(fileStream))
            {
                streamWriter.Write("");
            }
        }
#endif
    }
}