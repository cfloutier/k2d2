

using KTools;
namespace K2D2.Controller;

using KTools.UI;
using UnityEngine;

public class AutoLiftSettings
{
    public float heading
    {
        get => KBaseSettings.sfile.GetFloat("lift.heading", 90);
        set
        {
            value = Mathf.Clamp(value, 0 , 360);
            KBaseSettings.sfile.SetFloat("lift.heading", value);
        }
    }

    public int start_altitude_km
    {
        get => KBaseSettings.sfile.GetInt("lift.start_altitude_km", 2);
        set
        {
            // value = Mathf.Clamp(value, 0 , 1);
            KBaseSettings.sfile.SetInt("lift.start_altitude_km", value);
            _mid_rotate_altitude_km = -1;
            _end_rotate_altitude_km = -1;
        }
    }

    public float mid_rotate_ratio
    {
        get => KBaseSettings.sfile.GetFloat("lift.mid_rotate_ratio", 0.2f);
        set
        {
            value = Mathf.Clamp(value, 0, end_rotate_ratio);
            KBaseSettings.sfile.SetFloat("lift.mid_rotate_ratio", value);
            _mid_rotate_altitude_km = -1;
        }
    }

    float _mid_rotate_altitude_km = -1;
    public float mid_rotate_altitude_km
    {
        get {
            if (_mid_rotate_altitude_km == -1)
                _mid_rotate_altitude_km = Mathf.Lerp(start_altitude_km, destination_Ap_km, mid_rotate_ratio);

            return _mid_rotate_altitude_km;
        }
    }

    public float end_rotate_ratio
    {
        get => KBaseSettings.sfile.GetFloat("lift.end_rotate_ratio", 0.5f);
        set
        {
            value = Mathf.Clamp(value, mid_rotate_ratio, 1);
            KBaseSettings.sfile.SetFloat("lift.end_rotate_ratio", value);
            _mid_rotate_altitude_km = -1;
        }
    }

    float _end_rotate_altitude_km = -1;
    public float end_rotate_altitude_km
    {
         get {
            if (_end_rotate_altitude_km == -1)
                _end_rotate_altitude_km = Mathf.Lerp(start_altitude_km, destination_Ap_km, end_rotate_ratio);

            return _end_rotate_altitude_km;
        }
    }

    public int destination_Ap_km
    {
        get => KBaseSettings.sfile.GetInt("lift.destination_Ap_km", 100);
        set
        {
            KBaseSettings.sfile.SetInt("lift.destination_Ap_km", value);
            _end_rotate_altitude_km = -1;
            _mid_rotate_altitude_km = -1;

        }
    }

    public float max_throttle
    {
        get => KBaseSettings.sfile.GetFloat("lift.max_throttle", 1);
        set
        {
            KBaseSettings.sfile.SetFloat("lift.max_throttle", value);
        }
    }

    public float end_ascent_pc
    {
        get => KBaseSettings.sfile.GetFloat("lift.end_ascent_pc", 0.1f);
        set
        {
            KBaseSettings.sfile.SetFloat("lift.end_ascent_pc", value);
        }
    }

    public float end_ascent_error
    {
         get => destination_Ap_km * end_ascent_pc/100;
    }

    public bool coasting_warp
    {
        get => KBaseSettings.sfile.GetBool("lift.coasting_warp", true);
        set
        {
            KBaseSettings.sfile.SetBool("lift.coasting_warp", value);
        }
    }

    public bool adjust
    {
        get => KBaseSettings.sfile.GetBool("lift.adjust", true);
        set
        {
            KBaseSettings.sfile.SetBool("lift.adjust", value);
        }
    }

    public float end_adjust_pc
    {
        get => KBaseSettings.sfile.GetFloat("lift.end_adjust_pc", 0.01f);
        set
        {
            if (value > end_ascent_pc)
                value = end_ascent_pc;

            KBaseSettings.sfile.SetFloat("lift.end_adjust_pc", value);
        }
    }
    public float end_adjust_error
    {
         get => destination_Ap_km * end_adjust_pc/100;
    }

    public bool pause_on_final
    {
        get => KBaseSettings.sfile.GetBool("lift.pause_on_final", true);
        set
        {
            KBaseSettings.sfile.SetBool("lift.pause_on_final", value);
        }
    }



    public void onGUI()
    {
        UI_Tools.Title("Lift Settings");

        UI_Tools.Label($"End Ascent Alt. : {destination_Ap_km - end_ascent_error:n2} km");
        end_ascent_pc = UI_Tools.FloatSliderTxt("Ap Alt Error ", end_ascent_pc, 0.001f, 0.5f, "%");

        coasting_warp = UI_Tools.Toggle(coasting_warp, "Coasting Warp", "Auto Warp while waiting for leaving Atm.");

        adjust = UI_Tools.Toggle(adjust, "Fine Adjust", "Precisely adjust AP");

        if (adjust)
        {
            UI_Tools.Label($"End Adjust Alt : {destination_Ap_km - end_adjust_error:n2} km");
            end_adjust_pc = UI_Tools.FloatSliderTxt("Ap Alt Error ", end_adjust_pc, 0.001f, 0.5f, "%");
        }

        pause_on_final = UI_Tools.Toggle(pause_on_final, "Pause on Final", "Pause when creating final node");
    }
}
