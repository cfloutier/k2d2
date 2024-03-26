
using UnityEngine.UIElements;
using K2UI;
using K2UI.Graph;
using System.Net.WebSockets;

using K2UI.Tabs;

namespace K2D2.UI.Tests
{
    public class TestAllControls : K2Page
    {
        public TestAllControls()
        {
            code = "controls";
        }

        K2ProgressBar bar;
        K2Slider slider;

        K2Compass compas;

        FloatField float_field;

        float my_value = 5;

        IntegerField int_field;

        TestSettings settings_ui;


        public override bool onInit()
        {
            settings_ui = new TestSettings();
            settings_ui.init(settings_page);
            
            bar = panel.Q<K2ProgressBar>("MyBar");
            slider = panel.Q<K2Slider>("MySlider");
            slider.value = my_value;
            slider.RegisterCallback<ChangeEvent<float>>((evt) =>
            {
                my_value = evt.newValue;
                UpdateBars();
            });

            compas = panel.Q<K2Compass>();
            compas.RegisterCallback<ChangeEvent<float>>((evt) =>
            {
                my_value = evt.newValue;
                UpdateBars();
            });

            float_field = panel.Q<FloatField>();
            float_field.RegisterCallback<ChangeEvent<float>>((evt) =>
            {
                my_value = evt.newValue;
                UpdateBars();
            });

            int_field = panel.Q<IntegerField>();
            int_field.RegisterCallback<ChangeEvent<int>>((evt) =>
            {
                my_value = evt.newValue;
                UpdateBars();
            });


            tuning = panel.Q<K2Slider>("tuning");
            tuning.RegisterCallback<ChangeEvent<float>>((evt) =>
            {
                UpdateLines();
            });

            line_1 = panel.Q<GraphLine>("line_1");
            line_2 = panel.Q<GraphLine>("line_2");

            UpdateLines();

            var big_button = panel.Q<ToggleButton>("StartPilot");
            big_button.RegisterCallback<ChangeEvent<bool>>(evt => isRunning = evt.newValue);

            UpdateBars();
            return true;
        }

        K2Slider tuning;
        GraphLine line_1, line_2;

        void UpdateLines()
        {
            line_1.TestSeed = tuning.value * 5;
            line_2.TestSeed = tuning.value * 10;
        }

        void UpdateBars()
        {
            bar.value = my_value;
            slider.value = my_value;
            compas.value = my_value;
            float_field.value = my_value;
            int_field.value = (int)my_value;
        }


    }
}