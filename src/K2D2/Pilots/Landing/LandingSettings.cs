using KTools;
using UnityEngine.UIElements;
using K2UI;
using K2D2.Controller;
using K2D2.Node;
namespace K2D2.Landing;

public class LandingSettings
{
    public Setting<bool> verbose_infos = new("land.verbose_infos", true);
    public Setting<bool> auto_warp = new("land.auto_warp", true);
    public ClampSetting<float> burn_before = new("land.burnBefore", 0, 0, 10);
    public Setting<int> rotation_warp_duration = new("land.rotation_warp_duration", 60);
    // Warp with check of rotation
    public Setting<float> max_rotation = new("land.max_rotation", 30);

    public float brake_speed
    {
        get => 50;
        // get => Settings.s_settings_file.GetFloat("land.brake_speed", 20);
        // set { Settings.s_settings_file.SetFloat("land.brake_speed", value); }
    }

    public float pause_time
    {
        get => 1;
        // get => Settings.s_settings_file.GetFloat("land.brake_speed", 20);
        // set { Settings.s_settings_file.SetFloat("land.brake_speed", value); }
    }

    public Setting<float> start_touchdown_altitude = new("land.touch_down_altitude", 1500);

    public Setting<float> touch_down_ratio = new("land.touch_down_ratio", 0.5f);

    public ClampSetting<float> touch_down_speed = new("land.touch_down_speed", 2.5f,  0, 100);


    public void setupUI(VisualElement root)
    {
        root.Q<K2Toggle>("verbose_infos").Bind(verbose_infos);
        root.Q<K2Slider>("burn_before").Bind(burn_before);
        root.Q<K2Toggle>("auto_warp").Bind(auto_warp);

        // WARP
        var warp_settings = root.Q<VisualElement>("warp_settings");    
        auto_warp.listeners += v => warp_settings.Show(v); 

        warp_settings.Q<K2Slider>("warp_speed").Bind(WarpToSettings.warp_speed);
        warp_settings.Q<IntegerField>("warp_safe_duration").Bind(WarpToSettings.warp_safe_duration);

        var debug_mode = root.Q<VisualElement>("debug_mode");    
        K2D2Settings.debug_mode.listeners += v => 
        {
            debug_mode.Show(v);
            // reset warp duration if not in debug mode
            if (!v)
                rotation_warp_duration.V = 0;
        };

        warp_settings.Q<IntegerField>("rotation_warp_duration").Bind(rotation_warp_duration);
        warp_settings.Q<K2Slider>("max_rotation").Bind(max_rotation);

        // TOUCH DOWN

        var el_touchdown_altitude = root.Q<K2Slider>("start_touchdown_altitude").Bind(start_touchdown_altitude);
        start_touchdown_altitude.listen(v =>    
            el_touchdown_altitude.Label = "Start TouchDown Altitude : " + StrTool.DistanceToString(v)
        );

        root.Q<K2Slider>("touch_down_ratio").Bind(touch_down_ratio);
        root.Q<K2Slider>("touch_down_speed").Bind(touch_down_speed);      
    }

    public float compute_limit_speed(float altitude)
    {
        // just to have understandable settings (not 0.1)
        float div = 10;
        return altitude * touch_down_ratio.V / div + touch_down_speed.V;
    }
}
