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
using UnityEngine.UI;


namespace K2D2.Controller;

class LiftUI : K2Page
{
    LiftPilot pilot;
    public LiftUI(LiftPilot pilot)
    {
        this.pilot = pilot;
        code = "node";
    }
    
    public K2UI.Console node_infos_el;

    public FullStatus status_bar;

    K2Slider end_rotate_ratio, mid_rotate_ratio;

    Label heading_label;
    public override bool onInit()
    {
        node_infos_el = panel.Q<K2UI.Console>("node_infos");

        LiftSettings settings = pilot.settings;
       
        status_bar = new FullStatus(panel);

        panel.Q<IntegerField>("destination_Ap_km").Bind(settings.destination_Ap_km);

        end_rotate_ratio = panel.Q<K2Slider>("end_rotate_ratio").Bind(settings.end_rotate_ratio);  
        end_rotate_ratio.listeners += v => end_rotate_ratio.Label = $"5° Alt. : {settings.end_rotate_altitude_km:n0} km";
            
        mid_rotate_ratio = panel.Q<K2Slider>("mid_rotate_ratio").Bind(settings.mid_rotate_ratio);
        mid_rotate_ratio.listeners += v => mid_rotate_ratio.Label = $"45° Alt. : {settings.mid_rotate_altitude_km:n0} km";
        
        panel.Q<IntegerField>("start_altitude_km").Bind(settings.start_altitude_km);

        heading_label = panel.Q<Label>("heading_label");
        var heading = panel.Q<K2Compass>("heading").Bind(settings.heading);
        heading.listeners += v => heading_label.text = $"Heading : {v:n1} °";

        var graph = panel.Q<VisualElement>("graph");
        pilot.ascent_path.InitUI(graph);

        var max_throttle = panel.Q<K2Compass>("max_throttle").Bind(settings.max_throttle);

        var run_button = panel.Q<ToggleButton>("run");
        pilot.is_running_event += is_running => run_button.value = is_running;
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

        pilot.ascent_path.updateProfile(pilot.ascent.current_altitude_km);

        if (pilot.isRunning)
        {
           status_bar.Reset();
           status_bar.Status($"Status : {pilot.status}", StatusLine.Level.Warning);

           if (pilot.current_subpilot != null)
                pilot.current_subpilot.updateUI(panel, status_bar);
        }
        else
        {
            status_bar.Status("Final status : " + pilot.end_status, 
                pilot.result_ok ? StatusLine.Level.Normal : StatusLine.Level.Warning);
        }

        return true;
    }
          


}