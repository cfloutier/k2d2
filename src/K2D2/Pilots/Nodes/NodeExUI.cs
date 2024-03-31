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

namespace K2D2.Node;

class NodeExUI : K2Page
{
    NodeExPilot pilot;
    public NodeExUI(NodeExPilot pilot)
    {
        this.pilot = pilot;
        code = "node";
    }
    
    public K2UI.Console node_infos_el;

    public FullStatus status_bar;

    public ToggleButton run_button, pause_button;

    public override bool onInit()
    {
        node_infos_el = panel.Q<K2UI.Console>("node_infos");

        run_button = panel.Q<ToggleButton>("run");
        pause_button = panel.Q<ToggleButton>("pause");

        status_bar = new FullStatus(panel);

        pilot.settings.show_node_infos.listen((value) => node_infos_el.Show(value));

        pilot.is_running_event += is_running => run_button.value = is_running;
        run_button.listeners +=  v => 
        {
            pilot.isRunning = v;
            run_button.label = v ? "Stop" : "Start";
        }; 

        pause_button.Bind(pilot.settings.pause_on_end);
        pilot.settings.setupUI(settings_page);
        addSettingsResetButton("node_ex");

        return true;
    }

    void UpdateNodeInfos()
    {
        node_infos_el.Set("<b>Node Infos</b>");

        ManeuverNodeData node = null;
        if (pilot.isRunning)
            node = pilot.execute_node;
        else
            node = pilot.next_maneuver_node;

        if (node == null)
        {
            node_infos_el.Show(false);
            return;
        }
            
        var dt = GeneralTools.remainingStartTime(node);
        node_infos_el.Add($"\n Node in <b>{StrTool.DurationToString(dt)}</b>");
        node_infos_el.Add($"\n dV {node.BurnRequiredDV:n2} m/s");
        node_infos_el.Add($"\n Duration {StrTool.DurationToString(node.BurnDuration)}");
        if (dt < 0)
        {
            node_infos_el.Add($"\n In The Past");
        }  
    }


    public override bool onUpdateUI()
    {
        if (!base.onUpdateUI())
            return false;

        if (pilot.settings.show_node_infos.V)
        {
            UpdateNodeInfos();
        }
       
        return true;
    }



}