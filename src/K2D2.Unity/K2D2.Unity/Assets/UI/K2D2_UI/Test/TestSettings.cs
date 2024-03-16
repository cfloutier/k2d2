
using UnityEngine.UIElements;
using K2UI;

using K2UI.Tabs;
using KTools;
namespace K2D2.UI.Tests
{
    public class MySettingsClass
    {
        public Settings<bool> bool_item = new Settings<bool>("my_settings.bool_item", false);
        public Settings<float> float_item = new Settings<float>("my_settings.float_item", 5);

        public Settings<int> int_item = new Settings<int>("my_settings.int_item", -5);
    }

    public class TestSettings : K2Panel
    {
        MySettingsClass settings;

        public TestSettings()
        {
            code = "test_settings";
            settings = new MySettingsClass();
        }

        public override bool onInit()
        {
            var toggle = panel.Q<SlideToggle>("bool_settings");
            toggle.value = settings.bool_item.Value;
            settings.bool_item.Bind(toggle);

            var float_slider = panel.Q<K2Slider>("float_settings");
            float_slider.value = settings.float_item.Value;
            settings.float_item.Bind(float_slider);

            var int_slider = panel.Q<K2SliderInt>("int_settings");
            int_slider.value = settings.int_item.Value;
            settings.int_item.Bind(int_slider);
            
            return true;
        }
    }
}