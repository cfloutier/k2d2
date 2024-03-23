using K2UI;
using KTools;
// using KTools.UI;
using UnityEngine;
using UnityEngine.UIElements;

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

    public void setupUI(VisualElement root)
    {
        root.Q<K2Toggle>("show_node_infos").Bind(show_node_infos);
        
        // Turn To
        root.Q<K2Slider>("max_angle_maneuver").Bind(TurnToSettings.max_angle_maneuver);
        root.Q<K2Slider>("max_angular_speed").Bind(TurnToSettings.max_angular_speed);

        // Warp
        root.Q<K2Toggle>("auto_warp").Bind(auto_warp);
        var warp_settings = root.Q<VisualElement>("warp_settings");    
        auto_warp.listeners += (value) => warp_settings.Show(value); 

        warp_settings.Q<K2Slider>().Bind(WarpToSettings.warp_speed);
        warp_settings.Q<K2Slider>().Bind(WarpToSettings.warp_safe_duration);

        // Burn
        root.Q<K2Slider>("burn_adjust").Bind(BurnManeuverSettings.burn_adjust);
        root.Q<K2Slider>("max_dv_error").Bind(BurnManeuverSettings.max_dv_error);

        // Experimental
        var inline_enum = root.Q<InlineEnum>("start_mode");    
        start_mode.Bind(inline_enum);
        inline_enum.value = start_mode.int_value;
        
        var start_before_el = root.Q<K2Slider>("start_before");    
        // hide if start mode is not constant
        start_before_el.Show(start_mode.V == StartMode.constant); 
        start_mode.listeners += (value) => start_before_el.Show(value == StartMode.constant); 
        start_before_el.Bind(start_before);

        root.Q<K2Toggle>("rotate_during_burn").Bind(BurnManeuverSettings.rotate_during_burn);
    }



    

    
                           
    


}

