

using System.Net.WebSockets;
using K2UI;
using KTools;
using UnityEngine;
using UnityEngine.UIElements;

namespace K2D2.Controller;

public class LiftSettings
{
    public ClampSetting<float> heading = new ("lift.heading", 90,  0 , 360);
 
    public Setting<int> start_altitude_km = new ("lift.start_altitude_km", 2);
   
    public ClampSetting<float> mid_rotate_ratio = new ("lift.mid_rotate_ratio", 0.2f, 0, 1);

    
    public ClampSetting<float> end_rotate_ratio = new ("lift.end_rotate_ratio", 0.5f, 0, 1);

    public LiftSettings()
    {
        start_altitude_km.listeners += v => reset_altitudes();
      
        mid_rotate_ratio.listeners += v => 
        {
            end_rotate_ratio.min = v;   
            reset_altitudes();
        };

        end_rotate_ratio.listeners += v => 
        {
            mid_rotate_ratio.max = v;
            reset_altitudes();
        };

        destination_Ap_km.listeners += v => reset_altitudes();
    }

    void reset_altitudes()
    {
        _mid_rotate_altitude_km = -1; 
    }

    float _mid_rotate_altitude_km = -1;
    public float mid_rotate_altitude_km
    {
        get {
            if (_mid_rotate_altitude_km == -1)
                _mid_rotate_altitude_km = Mathf.Lerp(start_altitude_km.V, destination_Ap_km.V, mid_rotate_ratio.V);

            return _mid_rotate_altitude_km;
        }
    }

    float _end_rotate_altitude_km = -1;
    public float end_rotate_altitude_km
    {
         get {
            if (_end_rotate_altitude_km == -1)
                _end_rotate_altitude_km = Mathf.Lerp(start_altitude_km.V, destination_Ap_km.V, end_rotate_ratio.V);

            return _end_rotate_altitude_km;
        }
    }

    public Setting<int> destination_Ap_km = new("lift.destination_Ap_km", 100);

    public ClampSetting<float> max_throttle = new ("lift.max_throttle", 1, 0, 1);
    public ClampSetting<float> end_ascent_pc = new ("lift.end_ascent_pc", 0.1f, 0, 1);

    public float end_ascent_error
    {
         get => destination_Ap_km.V * end_ascent_pc.V/100;
    }

    public Setting<bool> coasting_warp = new ("lift.coasting_warp", true);
    public Setting<bool> adjust = new ("lift.adjust", true);
    public ClampSetting<float> end_adjust_pc = new ("lift.end_adjust_pc", 0.01f, 0.001f, 0.5f);

    public float end_adjust_error
    {
         get => destination_Ap_km.V * end_adjust_pc.V/100;
    }

    public Setting<bool> pause_on_final = new ("lift.pause_on_final", true);

    public Setting<bool> heading_correction = new ("lift.heading_correction", true);


    Label end_ascent_alt;
    Label end_adjust_alt;

    Group adjust_group;


    void setLabels()
    {
        end_ascent_alt.text = $"End Ascent Alt. : {destination_Ap_km.V - end_ascent_error:n2} km";
        end_adjust_alt.text = $"Final Adjust Alt : {destination_Ap_km.V - end_adjust_error:n2} km";
    }

    public void setupUI(VisualElement root)
    {
        end_ascent_alt =  root.Q<Label>("end_ascent_alt");
        end_adjust_alt = root.Q<Label>("end_adjust_alt");
        adjust_group = root.Q<Group>("adjust_group");

        root.Q<K2Toggle>("heading_correction").Bind(heading_correction);  

        // bind to settings and add listener on the same line !
        root.Q<K2Slider>("end_ascent_pc").Bind(end_ascent_pc).listeners += v => setLabels();
        root.Q<K2Toggle>("coasting_warp").Bind(coasting_warp);

        // adjust is binded to setting and to show adjust group 
        root.Q<K2Toggle>("adjust").Bind(adjust).listeners += v => adjust_group.Show(v);

        root.Q<K2Slider>("end_adjust_pc").Bind(end_adjust_pc).listeners += v => setLabels();

        root.Q<K2Toggle>("pause_on_final").Bind(pause_on_final);
    }
}
