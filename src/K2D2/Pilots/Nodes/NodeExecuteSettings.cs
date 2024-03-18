using KTools;
// using KTools.UI;
using UnityEngine;

namespace K2D2.Controller;

public class NodeExecuteSettings
{
    public Setting<bool> show_node_infos = new Setting<bool>("execute.show_node_infos", true);
    public Setting<bool> auto_warp = new Setting<bool>("execute.auto_warp", true);

    public Setting<bool> pause_on_end = new Setting<bool>("execute.pause_on_end", true);

    public enum StartMode { precise, constant, half_duration }
    private static string[] StartMode_Labels = { "T0", "before", "mid-duration" };

    public EnumSetting<StartMode> start_mode = new EnumSetting<StartMode>("execute.start_mode", StartMode.precise);

    public Setting<float> start_before = new Setting<float>("execute.start_before", 1);

    //  public void settings_UI()
    // {
    //     show_node_infos = UI_Tools.Toggle(show_node_infos, "Show Nodes Infos");

    //     start_mode = UI_Tools.EnumGrid<StartMode>("Start Burn at :", start_mode, StartMode_Labels);

    //     if (start_mode == StartMode.constant)
    //     {
    //         start_before = UI_Tools.FloatSliderTxt("Start before T0", start_before, 0, 10, "s");
    //     }

    //     pause_on_end = UI_Tools.Toggle(pause_on_end, 
    //                             "Pause When Done", 
    //                             "Pause when the Node is executed");   
    // }

    // public void warp_ui()
    // {
    //     auto_warp = UI_Tools.Toggle(auto_warp, "Auto Warp", "warp time before burn");
    //     if (auto_warp)
    //         WarpToSettings.onGUI();
    // }
}

