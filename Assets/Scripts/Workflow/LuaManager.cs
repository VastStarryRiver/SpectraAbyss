using UnityEngine;
using LuaInterface;
using System.Collections.Generic;



public class LuaManager : Singleton<LuaManager>
{
    public LuaState m_luaState = null;//Lua的虚拟机
    public Dictionary<string, LuaTable> m_luaClassList;//所有预制体对应的lua脚本



    public void Play()
    {
        if (m_luaState != null)
        {
            return;
        }

        m_luaClassList = new Dictionary<string, LuaTable>();

        new LuaResLoader();//加载Lua文件

        m_luaState = new LuaState();//虚拟机初始化

        m_luaState.Start();//开始虚拟机

        LuaBinder.Bind(m_luaState);//绑定虚拟机

        m_luaState.DoFile("Workflow/Main.lua");//任意已经添加的搜索路径下的完整lua路径
    }

    public void Stop()
    {
        if (m_luaState != null)
        {
            m_luaState.Dispose();
            m_luaState = null;
        }

        if(m_luaClassList != null)
        {
            m_luaClassList.Clear();
            m_luaClassList = null;
        }
    }

    public void Reset()
    {
        Stop();
        Play();
    }
}