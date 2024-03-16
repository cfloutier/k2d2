
// This script defines the tab selection logic.
using UnityEditor.Animations;
using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using K2D2;

namespace K2UI.Tabs
{
    /// <summary>
    /// K2Panels are classes added by the TabbedPage user. 
    /// each one is mapped to the proper root VisualElement 
    /// in the #content, found by it's #name == code
    /// a TabButton must be added to the #buttons 
    /// 
    /// create an overriden class and add it to the TabbedPage in the Init fct
    /// </summary>
    [System.Serializable]
    public class K2Panel
    {
        public string code;

        // the main panel (including page and settings)
        public VisualElement panel;



        // the settings page if found
        public VisualElement settings_page;
        // the main page if found
        public VisualElement main_page;

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

            settings_page = panel.Q<VisualElement>("settings");
            main_page = panel.Q<VisualElement>("page");

            if (settings_page != null && main_page != null)
            {     
                GlobalSetting.settings_visible.listeners += onSettingsChanged;
                onSettingsChanged(GlobalSetting.settings_visible.Value);
            }
            else
            {
                Debug.LogWarning("no settings for page name " + code);
            }

            return onInit();
        }

        private void onSettingsChanged(bool value)
        {
            settings_page.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
            main_page.style.display = !value ? DisplayStyle.Flex : DisplayStyle.None;         
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