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

namespace K2D2.Controller;

public class FullStatus
{
    public K2UI.Console console;
    public K2UI.StatusLine status;
    public K2UI.K2ProgressBar progressBar;  

    public void Reset()
    {
        console.Show(false);
        console.text = "";

        status.Show(false);
        status.text = "";
        
        progressBar.Show(false);          
    }

    public void Console(string txt)
    {
        console.Add(txt);
    }

    public void Status(string text, StatusLine.Level level = StatusLine.Level.Normal)
    {
        status.Set(text, level);
        status.Show(true);
    }

    public void Progess(double ratio, string label = null)
    {
        progressBar.value = (float)(ratio * 100);
        progressBar.Show(true);
        if (!string.IsNullOrEmpty(label))
        {
            progressBar.Label = label;
        }
    }
}

class NodeExUI : K2Panel
{
    NodeExPilot pilot;
    public NodeExUI(NodeExPilot pilot)
    {
        this.pilot = pilot;
        code = "node";
    }
    
    public K2UI.Console node_infos_el;

    public FullStatus status_bar = new FullStatus();

    public ToggleButton run_button, pause_button;

    public override bool onInit()
    {
        node_infos_el = panel.Q<K2UI.Console>("node_infos");

        run_button = panel.Q<ToggleButton>("run");
        pause_button = panel.Q<ToggleButton>("pause");

        status_bar = new FullStatus()
        {
            status = panel.Q<StatusLine>("status_pilot"),
            console = panel.Q<K2UI.Console>("pilot_console"),
            progressBar = panel.Q<K2UI.K2ProgressBar>("progress"),
        };

        pilot.settings.show_node_infos.listen((value) => node_infos_el.Show(value));

        pilot.is_running_event += is_running => run_button.value = is_running;
        run_button.RegisterCallback<ChangeEvent<bool>>(evt => pilot.isRunning = evt.newValue);     
        pause_button.Bind(pilot.settings.pause_on_end);
        pilot.settings.setupUI(settings_page);

        return true;
    }

    void UpdateNodeInfos()
    {
        string txt = "<b>Node Infos</b>";

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
        txt += $"\n Node in <b>{StrTool.DurationToString(dt)}</b>";
        txt += $"\n dV {node.BurnRequiredDV:n2} m/s";
        txt += $"\n Duration {StrTool.DurationToString(node.BurnDuration)}";
        if (dt < 0)
        {
            txt += $"\n In The Past";
        }
        
        node_infos_el.Set(txt);
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