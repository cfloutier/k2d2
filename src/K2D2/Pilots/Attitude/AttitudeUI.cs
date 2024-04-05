// using System.Reflection.Emit;
using BepInEx.Logging;
using K2D2.KSPService;
using K2UI.Tabs;
using K2UI;
using KSP.Messages;
using KSP.Sim;
using KSP.Sim.Maneuver;
using KTools;
using UnityEngine.UIElements;
using K2D2.UI;

namespace K2D2.Controller;

class AttitudeUI : K2Page
{
    AttitudePilot pilot;
    public AttitudeUI(AttitudePilot pilot)
    {
        this.pilot = pilot;
        code = "attitude";
    }
    
    public FullStatus st;

    // public RepeatButton run_button, pause_button;

    // public FlightPlanCall call_fp;

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

        run_button = panel.Q<ToggleButton>("run");
        st = new FullStatus(panel);

    
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

        st.Reset();
        return true;
    }
}