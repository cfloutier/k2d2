
using UnityEngine.UIElements;
using K2UI;
using K2UI.Graph;
using System.Net.WebSockets;

using K2UI.Tabs;
using KTools;

public class MySettingsClass
{
    public SettingsBoolValue bool_item  = new SettingsBoolValue("my_settings.bool_item", false);
}

public class TestSettings : K2Panel
{

    MySettingsClass settings;


    public TestSettings()
    {
        code = "settings";
        settings = new MySettingsClass();

       
    }

    public override bool onInit()
    {
        SlideToggle toggle = panel.Q<SlideToggle>("bool_settings");

        toggle.value = settings.bool_item.Value;
        settings.bool_item.ListenTo(toggle);

        return true;
    }

}