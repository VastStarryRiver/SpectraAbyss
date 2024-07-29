using UnityEngine;
using LuaInterface;



public class PrefabInstance : MonoBehaviour
{
    private LuaTable m_luaTable = null;
    private LuaFunction m_awakeFunc = null;
    private LuaFunction m_onEnableFunc = null;
    private LuaFunction m_startFunc = null;
    private LuaFunction m_updateFunc = null;
    private LuaFunction m_onDisableFunc = null;
    private LuaFunction m_onDestroyFunc = null;
    private float m_time = 0;



    private void Awake()
    {
        if (LuaManager.Instance.m_luaClassList.ContainsKey(gameObject.name))
        {
            m_luaTable = LuaManager.Instance.m_luaClassList[gameObject.name];
            m_awakeFunc = (LuaFunction)m_luaTable["Awake"];
            m_onEnableFunc = (LuaFunction)m_luaTable["OnEnable"];
            m_startFunc = (LuaFunction)m_luaTable["Start"];
            m_updateFunc = (LuaFunction)m_luaTable["Update"];
            m_onDisableFunc = (LuaFunction)m_luaTable["OnDisable"];
            m_onDestroyFunc = (LuaFunction)m_luaTable["OnDestroy"];
            m_luaTable["gameObject"] = gameObject;
        }

        if (m_awakeFunc != null)
        {
            m_awakeFunc.Call(this);
        }
    }

    private void OnEnable()
    {
        if (m_onEnableFunc != null)
        {
            m_onEnableFunc.Call();
        }
    }

    private void Start()
    {
        if (m_startFunc != null)
        {
            m_startFunc.Call();
        }
    }

    private void Update()
    {
        m_time += Time.deltaTime;

        if (m_updateFunc != null)
        {
            m_updateFunc.Call(m_time);
        }
    }

    private void OnDisable()
    {
        if (LuaManager.Instance.m_luaClassList == null || !LuaManager.Instance.m_luaClassList.ContainsKey(gameObject.name))
        {
            return;
        }

        if (m_onDisableFunc != null)
        {
            m_onDisableFunc.Call();
        }
    }

    private void OnDestroy()
    {
        if (LuaManager.Instance.m_luaClassList == null || !LuaManager.Instance.m_luaClassList.ContainsKey(gameObject.name))
        {
            return;
        }

        if (m_onDestroyFunc != null)
        {
            m_onDestroyFunc.Call();
        }

        if (LuaManager.Instance.m_luaClassList.ContainsKey(gameObject.name))
        {
            LuaManager.Instance.m_luaClassList.Remove(gameObject.name);
        }
    }



    public LuaTable GetLuaTable()
    {
        return m_luaTable;
    }

    public float GetTime()
    {
        return m_time;
    }
}