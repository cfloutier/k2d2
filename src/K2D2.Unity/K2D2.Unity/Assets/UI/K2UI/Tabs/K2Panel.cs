
// This script defines the tab selection logic.
using UnityEditor.Animations;
using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace K2UI.Tabs
{
    /// <summary>
    /// Panels are classes added by the TabbedPage user. 
    /// each one is mapped to the proper root VisualElement (the in #content, found by it's name == code )
    /// 
    /// 
    /// please create an overriden class and add it to the TabbedPage 
    /// 
    /// </summary>
    [System.Serializable]
    public class K2Panel
    {
        public string code;

        public VisualElement panel;
        public TabButton tab_button;

        public K2Panel()
        {

        }

        public bool Init(VisualElement buttons, VisualElement panels)
        {
            tab_button = buttons.Q<TabButton>(code);
            if (tab_button == null)
            {
                Debug.LogError("missing TabButton with name " + code);
                return false;
            }

            panel = panels.Q<VisualElement>(code);
            if (panel == null)
            {
                Debug.LogError("missing content with name " + code);
                return false;
            }

            return onInit();
        }

        public virtual bool onInit()
        {
            return false;
        }

        bool _is_running = false;

        public bool isRunning
        {
            get
            {
                return _is_running;
            }
            set
            {
                _is_running = value;
                tab_button.Lighted = value;
            }
        }
    }
}