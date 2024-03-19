
using UnityEngine.UIElements;
using K2UI;

using K2UI.Tabs;
using KTools;



namespace K2D2.UI.Tests
{
    public enum MyEnum
    {
        Down,
        Middle,
        Up,
        Left,
        Right,
        Front,
        Etc,
 
    }

    public class MySettingsClass
    {
        public Setting<bool> bool_item = new Setting<bool>("my_settings.bool_item", false);
        public Setting<float> float_item = new Setting<float>("my_settings.float_item", 5);

        public Setting<int> int_item = new Setting<int>("my_settings.int_item", -5);

        public EnumSetting<MyEnum> enum_item = new EnumSetting<MyEnum>("my_settings.enum_item", MyEnum.Middle);
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
            toggle.value = settings.bool_item.V;
            settings.bool_item.Bind(toggle);

            var float_slider = panel.Q<K2Slider>("float_settings");
            float_slider.value = settings.float_item.V;
            settings.float_item.Bind(float_slider);

            var int_slider = panel.Q<K2SliderInt>("int_settings");
            int_slider.value = settings.int_item.V;
            settings.int_item.Bind(int_slider);

            var inline_enum = panel.Q<InlineEnum>("enum");    
            settings.enum_item.Bind(inline_enum);
            inline_enum.value = settings.enum_item.int_value;
            
            return true;
        }
    }
}