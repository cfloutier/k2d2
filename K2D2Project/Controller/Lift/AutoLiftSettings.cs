

using KTools;
namespace K2D2.Controller;
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
}
