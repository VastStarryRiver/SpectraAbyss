using UnityEngine;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Intepreter;



namespace Invariable
{
    public class MonoBehaviourAdapter : MonoBehaviour
    {
        public ILTypeInstance ILInstance = null;

        #region 生命周期事件
        private void Awake()
        {
            if(ILInstance == null && HotUpdateManager.m_allInstance.ContainsKey(gameObject.name))
            {
                ILInstance = HotUpdateManager.m_allInstance[gameObject.name];
                HotUpdateManager.m_allInstance.Remove(gameObject.name);
            }

            IMethod iMethod = ILInstance?.Type.GetMethod("Awake", 2);

            if (iMethod != null)
            {
                HotUpdateManager.m_appdomain.Invoke(iMethod, ILInstance, gameObject, transform);
            }
        }

        private void OnEnable()
        {
            IMethod iMethod = ILInstance?.Type.GetMethod("OnEnable", 0);

            if (iMethod != null)
            {
                HotUpdateManager.m_appdomain.Invoke(iMethod, ILInstance);
            }
        }

        private void Start()
        {
            IMethod iMethod = ILInstance?.Type.GetMethod("Start", 0);

            if (iMethod != null)
            {
                HotUpdateManager.m_appdomain.Invoke(iMethod, ILInstance);
            }
        }

        private void FixedUpdate()
        {
            IMethod iMethod = ILInstance?.Type.GetMethod("FixedUpdate", 0);

            if (iMethod != null)
            {
                HotUpdateManager.m_appdomain.Invoke(iMethod, ILInstance);
            }
        }

        private void Update()
        {
            IMethod iMethod = ILInstance?.Type.GetMethod("Update", 0);

            if (iMethod != null)
            {
                HotUpdateManager.m_appdomain.Invoke(iMethod, ILInstance);
            }
        }

        private void LateUpdate()
        {
            IMethod iMethod = ILInstance?.Type.GetMethod("LateUpdate", 0);

            if (iMethod != null)
            {
                HotUpdateManager.m_appdomain.Invoke(iMethod, ILInstance);
            }
        }

        private void OnGUI()
        {
            IMethod iMethod = ILInstance?.Type.GetMethod("OnGUI", 0);

            if (iMethod != null)
            {
                HotUpdateManager.m_appdomain.Invoke(iMethod, ILInstance);
            }
        }

        private void OnApplicationQuit()
        {
            IMethod iMethod = ILInstance?.Type.GetMethod("OnApplicationQuit", 0);

            if (iMethod != null)
            {
                HotUpdateManager.m_appdomain.Invoke(iMethod, ILInstance);
            }
        }

        private void OnDisable()
        {
            IMethod iMethod = ILInstance?.Type.GetMethod("OnDisable", 0);

            if (iMethod != null)
            {
                HotUpdateManager.m_appdomain.Invoke(iMethod, ILInstance);
            }
        }

        private void OnDestroy()
        {
            IMethod iMethod = ILInstance?.Type.GetMethod("OnDestroy", 0);

            if (iMethod != null)
            {
                HotUpdateManager.m_appdomain.Invoke(iMethod, ILInstance);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            IMethod iMethod = ILInstance?.Type.GetMethod("OnTriggerEnter", 1);

            if (iMethod != null)
            {
                HotUpdateManager.m_appdomain.Invoke(iMethod, ILInstance, other);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            IMethod iMethod = ILInstance?.Type.GetMethod("OnTriggerStay", 1);

            if (iMethod != null)
            {
                HotUpdateManager.m_appdomain.Invoke(iMethod, ILInstance, other);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            IMethod iMethod = ILInstance?.Type.GetMethod("OnTriggerExit", 1);

            if (iMethod != null)
            {
                HotUpdateManager.m_appdomain.Invoke(iMethod, ILInstance, other);
            }
        }

        private void OnCollisionEnter(Collision collisionInfo)
        {
            IMethod iMethod = ILInstance?.Type.GetMethod("OnCollisionEnter", 1);

            if (iMethod != null)
            {
                HotUpdateManager.m_appdomain.Invoke(iMethod, ILInstance, collisionInfo);
            }
        }

        private void OnCollisionStay(Collision collisionInfo)
        {
            IMethod iMethod = ILInstance?.Type.GetMethod("OnCollisionStay", 1);

            if (iMethod != null)
            {
                HotUpdateManager.m_appdomain.Invoke(iMethod, ILInstance, collisionInfo);
            }
        }

        private void OnCollisionExit(Collision collisionInfo)
        {
            IMethod iMethod = ILInstance?.Type.GetMethod("OnCollisionExit", 1);

            if (iMethod != null)
            {
                HotUpdateManager.m_appdomain.Invoke(iMethod, ILInstance, collisionInfo);
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            IMethod iMethod = ILInstance?.Type.GetMethod("OnControllerColliderHit", 1);

            if (iMethod != null)
            {
                HotUpdateManager.m_appdomain.Invoke(iMethod, ILInstance, hit);
            }
        }

        private void OnMouseEnter()
        {
            IMethod iMethod = ILInstance?.Type.GetMethod("OnMouseEnter", 0);

            if (iMethod != null)
            {
                HotUpdateManager.m_appdomain.Invoke(iMethod, ILInstance);
            }
        }

        private void OnMouseDown()
        {
            IMethod iMethod = ILInstance?.Type.GetMethod("OnMouseDown", 0);

            if (iMethod != null)
            {
                HotUpdateManager.m_appdomain.Invoke(iMethod, ILInstance);
            }
        }

        //此函数在iPhone上无效
        private void OnMouseUp()
        {
            IMethod iMethod = ILInstance?.Type.GetMethod("OnMouseUp", 0);

            if (iMethod != null)
            {
                HotUpdateManager.m_appdomain.Invoke(iMethod, ILInstance);
            }
        }

        private void OnMouseExit()
        {
            IMethod iMethod = ILInstance?.Type.GetMethod("OnMouseExit", 0);

            if (iMethod != null)
            {
                HotUpdateManager.m_appdomain.Invoke(iMethod, ILInstance);
            }
        }

        private void OnMouseOver()
        {
            IMethod iMethod = ILInstance?.Type.GetMethod("OnMouseOver", 0);

            if (iMethod != null)
            {
                HotUpdateManager.m_appdomain.Invoke(iMethod, ILInstance);
            }
        }
        #endregion
    }
}