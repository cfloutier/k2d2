
using UnityEngine.UIElements;
using K2UI;
using System.Net.WebSockets;

using K2UI.Tabs;

public class TestAllControls : K2Panel
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


    public override bool onInit()
    {
        bar = panel.Q<K2ProgressBar>("MyBar");
        slider = panel.Q<K2Slider>("MySlider");
        slider.Value = my_value;
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
        
        var big_button = panel.Q<BigToggleButton>("StartPilot");
        big_button.RegisterCallback<ChangeEvent<bool>>(evt => isRunning = evt.newValue);

        UpdateBars();
        return true;
    }

    void UpdateBars()
    {
        bar.Value = my_value;
        slider.Value = my_value;
        compas.Value = my_value;
        float_field.value = my_value;
        int_field.value = (int) my_value;
    }
}
