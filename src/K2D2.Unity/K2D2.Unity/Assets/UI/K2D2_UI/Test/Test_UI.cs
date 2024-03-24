// This script attaches the tabbed menu logic to the game.
using UnityEngine;
using UnityEngine.UIElements;

using System.Collections.Generic;

using K2UI;
using K2UI.Tabs;
using KTools;

namespace K2D2.UI.Tests
{
    public class TimerPanel: K2Panel
    {
        public TimerPanel()
        {
            code = "timer";
        }


        Label my_label;

        public override bool onInit()
        {
            my_label = panel.Q<Label>("timer_label");     
            return true;
        }

        public override bool onUpdateUI()
        {
            if (!base.onUpdateUI())
                return false;

            Debug.Log("calling update");
            my_label.text = $"time is {Time.time:n1} s";

            return true;
        }
    }

    public class DisablePage: K2Panel
    {
        public DisablePage()
        {
            code = "disable";
        }

        public override bool onInit()
        {
            var parent_tabs = panel.GetFirstAncestorOfType<TabbedPage>();

            var toogle = panel.Q<K2Toggle>("controls");     
            toogle.RegisterCallback<ChangeEvent<bool>>( evt => parent_tabs.Enable("controls", !evt.newValue));
            toogle = panel.Q<K2Toggle>("timer");     
            toogle.RegisterCallback<ChangeEvent<bool>>( evt => parent_tabs.Enable("timer", !evt.newValue));
            return true;
        }
    }

    public class AboutPage: K2Panel
    {
        public AboutPage()
        {
            code = "about";
        }

        public override bool onInit()
        {
            // var parent_tabs = panel.GetFirstAncestorOfType<TabbedPage>();
            Button reset_bt = panel.Q<Button>("reset");  
            reset_bt.RegisterCallback<ClickEvent>( evt => SettingsFile.Instance.Reset());

            return true;
        }
    }

  
    //Inherits from class `MonoBehaviour`. This makes it attachable to a game object as a component.
    public class Test_UI : MonoBehaviour
    {
        private TabbedPage pages_controler;

        public string start_selected = "controls";

        // public List<Panel> panels;
        private void OnEnable()
        {


        }

        private void Start()
        {
            UIDocument menu = GetComponent<UIDocument>();
            VisualElement root = menu.rootVisualElement;
            root.MakeDraggable();

            SettingsFile.Init("settings.json");

            List<K2Panel> panels = new()
            {
                new TestAllControls(),
                // new TestSettings(),
                new TimerPanel(),
                new DisablePage(),
            };

            var settings_toggle = root.Q<ToggleButton>("settings-toggle");
            settings_toggle.Bind(GlobalSetting.settings_visible);

            pages_controler = root.Q<TabbedPage>();
            pages_controler.Init(panels);
            pages_controler.Select(start_selected);
        }

        public void Update()
        {
            if (pages_controler != null)
                pages_controler.Update();
        }
    }



}