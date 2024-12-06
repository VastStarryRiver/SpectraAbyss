using System.Collections.Generic;
using UnityEngine;



namespace Invariable
{
    public class UIManager
    {
        private static Dictionary<string, MonoBehaviour> allPanel;
        public static Dictionary<string, MonoBehaviour> AllPanel
        {
            get
            {
                allPanel ??= new Dictionary<string, MonoBehaviour>();
                return allPanel;
            }

            set
            {
                allPanel = value;
            }
        }
    }
}