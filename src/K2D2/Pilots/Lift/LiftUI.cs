
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

namespace K2D2.Lift;

class LiftUI : K2Page
{
    LiftPilot pilot;
    public LiftUI(LiftPilot pilot)
    {
        this.pilot = pilot;
        code = "lift";
    }

    public FullStatus status_bar;

    K2Slider end_rotate_ratio, mid_rotate_ratio;

    Label heading_label;

    VisualElement final_grp;

    public override bool onInit()
    {
        LiftSettings settings = pilot.settings;
       
        status_bar = new FullStatus(panel);
        panel.Q<IntegerField>("start_altitude_km").Bind(settings.start_altitude_km);
        mid_rotate_ratio = panel.Q<K2Slider>("mid_rotate_ratio").Bind(settings.mid_rotate_ratio);
        end_rotate_ratio = panel.Q<K2Slider>("end_rotate_ratio").Bind(settings.end_rotate_ratio);  
        panel.Q<IntegerField>("destination_Ap_km").Bind(settings.destination_Ap_km);


        final_grp = panel.Q<VisualElement>("final_grp");

        settings.mid_rotate_ratio.listeners += v => 
        {
            if (end_rotate_ratio.value < v)
                end_rotate_ratio.value = v;  
        };
  
        settings.end_rotate_ratio.listeners += v => 
        {
            if (mid_rotate_ratio.value > v)
                mid_rotate_ratio.value = v;       
        };       
        
        heading_label = panel.Q<Label>("heading_label");
        var heading = panel.Q<K2Compass>("heading").Bind(settings.heading);

        var graph = panel.Q<VisualElement>("graph");
        pilot.ascent_path.InitUI(graph);

        var max_throttle = panel.Q<K2Slider>("max_throttle").Bind(settings.max_throttle);

        var run_button = panel.Q<ToggleButton>("run");
        pilot.is_running_event += is_running => run_button.Value = is_running;
        run_button.listeners +=  v => 
        {
            pilot.isRunning = v;
            run_button.label = v ? "Stop" : "Start";
        };

        settings.setupUI(settings_page);
        addSettingsResetButton("lift");

        return true;
    }

    public override bool onUpdateUI()
    {
        if (!base.onUpdateUI())
            return false;

        LiftSettings settings = pilot.settings;

        pilot.ascent_path.updateProfile(pilot.ascent.current_altitude_km);

        end_rotate_ratio.Label = $"5° Alt. : {settings.end_rotate_altitude_km:n0} km";
        mid_rotate_ratio.Label = $"45° Alt. : {settings.mid_rotate_altitude_km:n0} km";
        heading_label.text = $"Heading : {settings.heading.V:n1} °";

        final_grp.Show(false);

        status_bar.Reset();
        if (pilot.isRunning)
        {
           status_bar.Warning($"Status : {pilot.status}");

           if (pilot.current_subpilot != null)
                pilot.current_subpilot.updateUI(panel, status_bar);
        }
        else
        {
            if (!string.IsNullOrEmpty(pilot.end_status))
                status_bar.Status("Final status : " + pilot.end_status, 
                    pilot.result_ok ? StatusLine.Level.Normal : StatusLine.Level.Warning);
        }

        return true;
    }
          


}