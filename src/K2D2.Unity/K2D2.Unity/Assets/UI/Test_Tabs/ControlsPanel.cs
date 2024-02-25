
using UnityEngine.UIElements;
using K2UI;
using System.Net.WebSockets;

using K2UI.Tabs;

public class ControlsPanel : K2Panel
{
    public ControlsPanel()
    {
        code = "controls";
    }

    K2ProgressBar bar;
    K2Slider slider;

    float my_value = 5;

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

        var big_button = panel.Q<BigToggleButton>("StartPilot");
        big_button.RegisterCallback<ChangeEvent<bool>>(evt => isRunning = evt.newValue);

        UpdateBars();
        return true;
    }

    void UpdateBars()
    {
        bar.Value = my_value;
    }
}
