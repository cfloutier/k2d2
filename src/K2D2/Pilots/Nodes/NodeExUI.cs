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

    public void Progess(double ratio)
    {
        progressBar.value = (float)(ratio * 100);
        progressBar.Show(true);
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
    
    public K2UI.Console node_infos;

    public FullStatus status_bar = new FullStatus();

    public ToggleButton run_button, pause_button;

    public override bool onInit()
    {
        node_infos = panel.Q<K2UI.Console>("node_infos");

        run_button = panel.Q<ToggleButton>("run");
        pause_button = panel.Q<ToggleButton>("pause");

        status_bar = new FullStatus()
        {
            status = panel.Q<StatusLine>("status_pilot"),
            console = panel.Q<K2UI.Console>("pilot_console"),
            progressBar = panel.Q<K2UI.K2ProgressBar>("progress"),
        };

        pilot.settings.show_node_infos.listen((value) => node_infos.Show(value));

        run_button.RegisterCallback<ChangeEvent<bool>>(evt => pilot.isRunning = evt.newValue);
        pause_button.Bind(pilot.settings.pause_on_end);

        return true;
    }
}