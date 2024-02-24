
using UnityEngine.UIElements;
using K2UI;
using System.Net.WebSockets;

public class ControlsPanel : Panel
{
    public ControlsPanel()
    {
        code = "controls";
        button_label = "All Controls";
        title = "All controls test";
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

        UpdateBars();
        return true;
    }

    void UpdateBars()
    {
        bar.Value = my_value;
    }
}
