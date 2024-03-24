
using UnityEngine.UIElements;
using K2UI;

using K2UI.Tabs;
using KTools;



namespace K2D2.UI.Tests
{
    public enum MyEnum
    {
        Down,
        Center,
        Up,
        Left,
        Right
    }

    public class MySettingsClass
    {
        public Setting<bool> bool_item = new Setting<bool>("my_settings.bool_item", true);
        public ClampSetting<float> float_item = new ClampSetting<float>("my_settings.float_item", 5, 0, 100);
        public ClampSetting<int> int_item = new ClampSetting<int>("my_settings.int_item", -5, -10, 10);

        public EnumSetting<MyEnum> enum_item = new EnumSetting<MyEnum>("my_settings.enum_item", MyEnum.Center);
    }

    public class TestSettings
    {
        MySettingsClass settings;

        public TestSettings()
        {
            settings = new MySettingsClass();
        }

        public void init(VisualElement panel)
        {
            panel.Q<K2Toggle>("bool_settings").Bind(settings.bool_item);
            panel.Q<K2Toggle>("bool_linked").Bind(settings.bool_item);
       
            panel.Q<K2Slider>("float_settings").Bind(settings.float_item);
            panel.Q<K2SliderInt>("int_settings").Bind(settings.int_item);


            var inline_enum = panel.Q<InlineEnum>("enum");    
            settings.enum_item.Bind(inline_enum);
            inline_enum.value = settings.enum_item.int_value;

            var bt = panel.Q<Button>("reset");
            bt.RegisterCallback<ClickEvent>(evt => SettingsFile.Instance.Reset());


        }
    }
}