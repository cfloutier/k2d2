
// This script defines the tab selection logic.
using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using K2D2;
using KTools;

namespace K2UI.Tabs
{
    /// <summary>
    /// K2Pages are classes added by the TabbedPage user. 
    /// each one is mapped to the proper root VisualElement 
    /// in the #content, found by it's #name == code
    /// a TabButton must be added to the #buttons 
    /// 
    /// create an overriden class and add it to the TabbedPage in the Init fct
    /// </summary>
    [System.Serializable]
    public class K2Page
    {
        public string code;

        // the main panel (including page and settings)
        public TabPage panel;

        // the settings page if found
        public VisualElement settings_page;
        // the main page if found
        public VisualElement main_page;

        public TabButton tab_button;

        TabbedPage tabbed_page;

        public K2Page()
        {

        }

        public bool Init(VisualElement buttons, VisualElement panels)
        {
            tabbed_page = panels.GetFirstAncestorOfType<TabbedPage>();

            tab_button = buttons.Q<TabButton>(code);
            if (tab_button == null)
            {
                Debug.LogError("missing TabButton with name " + code);
                return false;
            }

            panel = panels.Q<TabPage>(code);
            if (panel == null)
            {
                Debug.LogError("missing content with name " + code);
                return false;
            }

            settings_page = panel.Q<VisualElement>("settings");
            main_page = panel.Q<VisualElement>("page");

            tab_button.Show(enabled);

            if (settings_page != null && main_page != null)
            {
                GlobalSetting.settings_visible.listeners += onSettingsChanged;
                onSettingsChanged(GlobalSetting.settings_visible.V);
            }
            else
            {
                Debug.LogWarning("no settings for page name " + code);
            }

            return onInit();
        }

        private void onSettingsChanged(bool value)
        {
            settings_page.Show(value);
            main_page.Show(!value);
        }

        public virtual bool onInit()
        {
            return false;
        }

        public virtual bool onUpdateUI()
        {
            if (!isVisible)
                return false;

            return true;
        }

        bool _is_running = false;

        // true if the controller is running
        public virtual bool isRunning
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

        public bool _is_visible = false;
        public bool isVisible
        {
            get
            {
                return _is_visible;
            }
            set
            {
                _is_visible = value;
            }
        }

        public bool _enabled = true;
        public virtual bool enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                if (value == _enabled) return;
                _enabled = value;

                tab_button.Show(_enabled);

                // check if this is the current tab
                if (tabbed_page.CurrentTabCode == code)
                {
                    tabbed_page.SelectFirst();
                }
            }
        }

        public void addSettingsResetButton(string chapter)
        {
            if (settings_page == null)
                return;

            VisualElement group = new VisualElement();
            group.style.flexDirection = FlexDirection.Row;
            group.style.justifyContent = Justify.SpaceBetween;

            Button reset_bt = new Button() { text = "Reset" };
            reset_bt.style.height = 25;
            Button close = new Button() { text = "Close" };
            close.style.height = 25;

            settings_page.Add(group);

            group.Add(reset_bt);
            group.Add(close);

            reset_bt.RegisterCallback<ClickEvent>(evt => SettingsFile.Instance.Reset(chapter));
            close.RegisterCallback<ClickEvent>(evt => GlobalSetting.settings_visible.V = false);
        }
    }
}