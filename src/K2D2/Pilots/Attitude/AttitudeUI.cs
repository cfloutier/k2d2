// using System.Reflection.Emit;
using K2UI.Tabs;
using K2UI;
using UnityEngine.UIElements;

namespace K2D2.Controller;

class AttitudeUI : K2Page
{
    AttitudePilot pilot;
    public AttitudeUI(AttitudePilot pilot)
    {
        this.pilot = pilot;
        code = "attitude";
    }
    
    long repeat_delay = 500;
    long repeat_interval = 100;

    FloatField elevation_field;
    ToggleButton run_button;


    public override bool onInit()
    {
        elevation_field = panel.Q<FloatField>("elevation_field");
        elevation_field.Bind(AttitudeSettings.elevation);
        panel.Q<RepeatButton>("elevation_n_rb").SetAction(
            () => elevation_field.value -= 1, repeat_delay, repeat_interval);

        panel.Q<RepeatButton>("elevation_p_rb").SetAction(
            () => elevation_field.value += 1, repeat_delay, repeat_interval);

        panel.Q<K2Slider>("elevation_slider").Bind(AttitudeSettings.elevation);
        var heading_label = panel.Q<Label>("heading_label");
        panel.Q<K2Compass>("heading").Bind(AttitudeSettings.heading);
        AttitudeSettings.heading.listen(v => heading_label.text = $"Heading : {v:n1}");

        run_button = panel.Q<ToggleButton>("run");
    
        pilot.is_running_event += is_running => run_button.Value = is_running;
        run_button.listeners +=  v => 
        {
            pilot.isRunning = v;
            run_button.label = v ? "Stop" : "Start";
        }; 

        return true;
    }

    public override bool onUpdateUI()
    {
        if (!base.onUpdateUI())
            return false;

        return true;
    }
}