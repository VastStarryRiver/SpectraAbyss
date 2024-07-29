using UnityEngine;

public static class LuaConst
{
#if !UNITY_EDITOR
    public static string luaDir = DataUtilityManager.m_localRootPath + "Lua";                //lua逻辑代码目录
    public static string toluaDir = DataUtilityManager.m_localRootPath + "Lua/ToLua";        //tolua lua文件目录
    public static string luaResDir = DataUtilityManager.m_localRootPath + "Lua";      //手机运行时lua文件下载目录
#else
    public static string luaDir = DataUtilityManager.m_localRootPath + "Assets/Lua";                //lua逻辑代码目录
    public static string toluaDir = DataUtilityManager.m_localRootPath + "Assets/Lua/ToLua";        //tolua lua文件目录
    public static string luaResDir = DataUtilityManager.m_localRootPath + "Assets/Lua";      //手机运行时lua文件下载目录
#endif

#if UNITY_STANDALONE
    public static string osDir = "Win";
#elif UNITY_ANDROID
    public static string osDir = "Android";
#elif UNITY_IPHONE
    public static string osDir = "iOS";
#else
    public static string osDir = "";
#endif

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN    
    public static string zbsDir = "D:/ZeroBraneStudio/lualibs/mobdebug";        //ZeroBraneStudio目录
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
	public static string zbsDir = "/Applications/ZeroBraneStudio.app/Contents/ZeroBraneStudio/lualibs/mobdebug";
#else
    public static string zbsDir = luaResDir + "/mobdebug/";
#endif

    public static bool openLuaSocket = true;            //是否打开Lua Socket库
    public static bool openLuaDebugger = false;         //是否连接lua调试器
}