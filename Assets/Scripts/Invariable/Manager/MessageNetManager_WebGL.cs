using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Net;
using System.Net.WebSockets;



namespace Invariable
{
    public partial class MessageNetManager
    {
        /// <summary>
        /// 启动客户端并连接服务器
        /// </summary>
        public void PlayWebSocket()
        {

        }

        /// <summary>
        /// 客户端断开连接
        /// </summary>
        public void StopWebSocket()
        {

        }

        /// <summary>
        /// 重置连接
        /// </summary>
        public void ResetWebSocket()
        {
            StopWebSocket();
            PlayWebSocket();
        }
    }
}