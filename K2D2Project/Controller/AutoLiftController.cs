using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using BepInEx.Logging;
using K2D2.KSPService;
using KSP.Sim;
using KSP.Sim.impl;



using K2D2.UI;

namespace K2D2.Controller;

public class AutoLiftSettings
{
    public float heading
    {
        get => Settings.s_settings_file.GetFloat("lift.heading", 90);
        set
        {
            // value = Mathf.Clamp(value, 0 , 1);
            Settings.s_settings_file.SetFloat("lift.heading", value);
        }
    }

    public int start_altitude_km
    {
        get => Settings.s_settings_file.GetInt("lift.start_altitude_km", 2);
        set
        {
            // value = Mathf.Clamp(value, 0 , 1);
            Settings.s_settings_file.SetInt("lift.start_altitude_km", value);
        }
    }

    public float mid_rotate_ratio
    {
        get => Settings.s_settings_file.GetFloat("lift.mid_rotate_ratio", 0.2f);
        set
        {
            value = Mathf.Clamp(value, 0, end_rotate_ratio);
            Settings.s_settings_file.SetFloat("lift.mid_rotate_ratio", value);
        }
    }

    public float end_rotate_ratio
    {
        get => Settings.s_settings_file.GetFloat("lift.end_rotate_ratio", 0.5f);
        set
        {
            value = Mathf.Clamp(value, mid_rotate_ratio, 1);
            Settings.s_settings_file.SetFloat("lift.end_rotate_ratio", value);
        }
    }

    public int destination_Ap_km
    {
        get => Settings.s_settings_file.GetInt("lift.destination_Ap_km", 100);
        set
        {
            Settings.s_settings_file.SetInt("lift.destination_Ap_km", value);
        }
    }
}

public class AutoLiftController : ComplexControler
{
    public static AutoLiftController Instance { get; set; }

    AutoLiftSettings lift_settings = new AutoLiftSettings();

    KSPVessel current_vessel;

    float inclination;

    public AutoLiftController()
    {
        // logger.LogMessage("LandingController !");
        current_vessel = K2D2_Plugin.Instance.current_vessel;

        Instance = this;
        debug_mode = false;
        name = "Lift";
    }

    public override void onReset()
    {
        isRunning = false;
    }

    bool _active = false;
    public override bool isRunning
    {
        get { return _active; }
        set
        {
            if (value == _active)
                return;

            if (!value)
            {
                // stop
                // var current_vessel = K2D2_Plugin.Instance.current_vessel;
                // if (current_vessel != null)
                //     current_vessel.SetThrottle(0);

                _active = false;
            }
            else
            {
                // reset controller to desactivate other controllers.
                K2D2_Plugin.ResetControllers();
                _active = true;

                OnStartController();
            }
        }
    }

    void OnStartController()
    {
        var autopilot = current_vessel.Autopilot;

        SASTool.setAutoPilot(AutopilotMode.StabilityAssist);

        inclination = -90;
        current_vessel.SetThrottle(1);
    }

    Vector3d direction = Vector3d.zero;

    public void applyDirection()
    {
        var autopilot = current_vessel.Autopilot;

        // force autopilot
        autopilot.Enabled = true;

        var telemetry = SASTool.getTelemetry();

        var up = telemetry.HorizonUp;

        direction = QuaternionD.Euler(-inclination, lift_settings.heading, 0) * Vector3d.forward;

        Vector direction_vector = new Vector(up.coordinateSystem, direction);

        autopilot.SAS.lockedMode = false;
        autopilot.SAS.SetTargetOrientation(direction_vector, false);
    }

    float altitude_km = 0;
    float ap_km = 0;

    float mid_rotate_altitude_km;
    float end_rotate_altitude_km;


    void computeValues()
    {
        PatchedConicsOrbit orbit = current_vessel.VesselComponent.Orbit;
        ap_km = (float)(orbit.Apoapsis - orbit.referenceBody.radius) / 1000;
        altitude_km = (float)(current_vessel.GetSeaAltitude() / 1000);
        mid_rotate_altitude_km = Mathf.Lerp(lift_settings.start_altitude_km, lift_settings.destination_Ap_km, lift_settings.mid_rotate_ratio);
        end_rotate_altitude_km = Mathf.Lerp(lift_settings.start_altitude_km, lift_settings.destination_Ap_km, lift_settings.end_rotate_ratio);

        if (altitude_km < lift_settings.start_altitude_km)
        {
            inclination = 90;
        }
        else if (altitude_km < mid_rotate_altitude_km)
        {
            var ratio = Mathf.InverseLerp(lift_settings.start_altitude_km, mid_rotate_altitude_km, altitude_km);
            inclination = Mathf.Lerp(90, 45, ratio);
        }
        else
        {
            var ratio = Mathf.InverseLerp(mid_rotate_altitude_km, end_rotate_altitude_km, (float)altitude_km);
            inclination = Mathf.Lerp(45, 5, ratio);
        }
    }

    public override void Update()
    {
        if (!isRunning && !ui_visible) return;
        if (current_vessel == null) return;

        computeValues();

        if (!isRunning)
            return;

        applyDirection();


        if (ap_km > lift_settings.destination_Ap_km)
        {
            current_vessel.SetThrottle(0);
            isRunning = false;
        }
    }

    public override void onGUI()
    {
        if (K2D2_Plugin.Instance.settings_visible)
        {
            Settings.onGUI();
            lift_settings.heading = UI_Tools.HeadingSlider("heading", lift_settings.heading);

            lift_settings.start_altitude_km = UI_Fields.IntField("lift.start_altitude_km", "90° Alt (km)", lift_settings.start_altitude_km, 0, Int32.MaxValue);

            GUILayout.Label($"45° Alt. : {mid_rotate_altitude_km:n0} km", GenericStyle.slider_text);
            lift_settings.mid_rotate_ratio = UI_Tools.FloatSlider(lift_settings.mid_rotate_ratio, 0, 1);

            GUILayout.Label($"5° Alt. : {end_rotate_altitude_km:n0} km", GenericStyle.slider_text);
            lift_settings.end_rotate_ratio = UI_Tools.FloatSlider(lift_settings.end_rotate_ratio, 0, 1);

            lift_settings.destination_Ap_km = UI_Fields.IntField("lift.destination_Ap_km", "Ap Altitude (km)", lift_settings.destination_Ap_km, 0, Int32.MaxValue);
            return;
        }

        isRunning = UI_Tools.ToggleButton(isRunning, "Start", "Stop");

        if (isRunning)
        {
            UI_Tools.Console($"Altitude = {altitude_km:n2} km");
            UI_Tools.Console($"Apoapsis Alt. = {ap_km:n2} km");
            UI_Tools.Console($"Inclination = {inclination:n2} °");
        }
    }
}
