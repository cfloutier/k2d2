using KTools;
using KTools.UI;
using UnityEngine;

namespace K2D2.Controller;

public class NodeExecuteSettings
{
    public bool show_node_infos
    {
        get => KBaseSettings.sfile.GetBool("execute.show_node_infos", true);
        set
        {
            // value = Mathf.Clamp(value, 0 , 1);
            KBaseSettings.sfile.SetBool("execute.show_node_infos", value);
        }
    }

    public bool auto_warp
    {
        get => KBaseSettings.sfile.GetBool("execute.auto_warp", true);
        set
        {
            // value = Mathf.Clamp(value, 0 , 1);
            KBaseSettings.sfile.SetBool("execute.auto_warp", value);
        }
    }


    public bool pause_on_end
    {
        get => KBaseSettings.sfile.GetBool("execute.pause_on_end", false);
        set
        {
            // value = Mathf.Clamp(value, 0 , 1);
            KBaseSettings.sfile.SetBool("execute.pause_on_end", value);
        }
    }
    

    public enum StartMode { precise, constant, half_duration }
    private static string[] StartMode_Labels = { "T0", "before", "mid-duration" };
    public StartMode start_mode
    {
        get => KBaseSettings.sfile.GetEnum<StartMode>("execute.start_mode", StartMode.precise);
        set
        {
            // value = Mathf.Clamp(value, 0 , 1);
            KBaseSettings.sfile.SetEnum<StartMode>("execute.start_mode", value);
        }
    }

    public float start_before
    {
        get => KBaseSettings.sfile.GetFloat("execute.start_before", 1);
        set
        {
            // value = Mathf.Clamp(value, 0 , 1);
            KBaseSettings.sfile.SetFloat("execute.start_before", value);
        }
    }

    public void settings_UI()
    {
        show_node_infos = UI_Tools.Toggle(show_node_infos, "Show Nodes Infos");

        start_mode = UI_Tools.EnumGrid<StartMode>("Start Burn at :", start_mode, StartMode_Labels);

        if (start_mode == StartMode.constant)
        {
            start_before = UI_Tools.FloatSliderTxt("Start before T0", start_before, 0, 10, "s");
        }

        pause_on_end = UI_Tools.Toggle(pause_on_end, 
                                "Pause When Done", 
                                "Pause when the Node is executed");   
    }



    public void warp_ui()
    {
        auto_warp = UI_Tools.Toggle(auto_warp, "Auto Warp", "warp time before burn");
        if (auto_warp)
            WarpToSettings.onGUI();
    }
}

