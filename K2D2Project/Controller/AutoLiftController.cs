using K2D2.KSPService;
using KSP.Sim;
using KSP.Sim.impl;
using KTools;
using KTools.UI;
using UnityEngine;

namespace K2D2.Controller;

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
        }
    }

    public float mid_rotate_ratio
    {
        get => KBaseSettings.sfile.GetFloat("lift.mid_rotate_ratio", 0.2f);
        set
        {
            value = Mathf.Clamp(value, 0, end_rotate_ratio);
            KBaseSettings.sfile.SetFloat("lift.mid_rotate_ratio", value);
        }
    }

    public float end_rotate_ratio
    {
        get => KBaseSettings.sfile.GetFloat("lift.end_rotate_ratio", 0.5f);
        set
        {
            value = Mathf.Clamp(value, mid_rotate_ratio, 1);
            KBaseSettings.sfile.SetFloat("lift.end_rotate_ratio", value);
        }
    }

    public int destination_Ap_km
    {
        get => KBaseSettings.sfile.GetInt("lift.destination_Ap_km", 100);
        set
        {
            KBaseSettings.sfile.SetInt("lift.destination_Ap_km", value);
        }
    }
}

public class AutoLiftController : ComplexControler
{
    public static AutoLiftController Instance { get; set; }

    AutoLiftSettings lift_settings = new AutoLiftSettings();

    KSPVessel current_vessel;

    float elevation;

    public AutoLiftController()
    {
        // logger.LogMessage("LandingController !");
        current_vessel = K2D2_Plugin.Instance.current_vessel;

        Instance = this;
        debug_mode_only = false;
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

        elevation = -90;
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

        direction = QuaternionD.Euler(-elevation, lift_settings.heading, 0) * Vector3d.forward;

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
        if (current_vessel.VesselComponent == null)
            return;

        PatchedConicsOrbit orbit = current_vessel.VesselComponent.Orbit;
        ap_km = (float)(orbit.Apoapsis - orbit.referenceBody.radius) / 1000;
        altitude_km = (float)(current_vessel.GetSeaAltitude() / 1000);
        mid_rotate_altitude_km = Mathf.Lerp(lift_settings.start_altitude_km, lift_settings.destination_Ap_km, lift_settings.mid_rotate_ratio);
        end_rotate_altitude_km = Mathf.Lerp(lift_settings.start_altitude_km, lift_settings.destination_Ap_km, lift_settings.end_rotate_ratio);

        if (altitude_km < lift_settings.start_altitude_km)
        {
            elevation = 90;
        }
        else if (altitude_km < mid_rotate_altitude_km)
        {
            var ratio = Mathf.InverseLerp(lift_settings.start_altitude_km, mid_rotate_altitude_km, altitude_km);
            elevation = Mathf.Lerp(90, 45, ratio);
        }
        else
        {
            var ratio = Mathf.InverseLerp(mid_rotate_altitude_km, end_rotate_altitude_km, (float)altitude_km);
            elevation = Mathf.Lerp(45, 5, ratio);
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
            K2D2Settings.onGUI();

            GUILayout.Label($"45° Alt. : {mid_rotate_altitude_km:n0} km", KBaseStyle.slider_text);
            lift_settings.mid_rotate_ratio = UI_Tools.FloatSlider(lift_settings.mid_rotate_ratio, 0, 1);

            GUILayout.Label($"5° Alt. : {end_rotate_altitude_km:n0} km", KBaseStyle.slider_text);
            lift_settings.end_rotate_ratio = UI_Tools.FloatSlider(lift_settings.end_rotate_ratio, 0, 1);

             return;
        }

        lift_settings.heading = HeadingSlider.onGUI("lift.heading", "Heading", lift_settings.heading, true);
        lift_settings.start_altitude_km = UI_Fields.IntFieldLine("lift.start_altitude_km", "90° Alt", lift_settings.start_altitude_km, 0, Int32.MaxValue, "km");
        lift_settings.destination_Ap_km = UI_Fields.IntFieldLine("lift.destination_Ap_km", "Ap Altitude", lift_settings.destination_Ap_km, 0, Int32.MaxValue, "km");

        isRunning = UI_Tools.BigToggleButton(isRunning, "Start", "Stop");

        if (isRunning)
        {
            UI_Tools.Console($"Altitude = {altitude_km:n2} km");
            UI_Tools.Console($"Apoapsis Alt. = {ap_km:n2} km");
            UI_Tools.Console($"Inclination = {elevation:n2} °");
        }
    }
}
