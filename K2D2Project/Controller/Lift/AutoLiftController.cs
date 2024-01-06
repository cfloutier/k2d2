using K2D2.KSPService;
using KSP.Sim;
using KSP.Sim.impl;
using KTools;
using KTools.UI;
using UnityEngine;

namespace K2D2.Controller;



public class AutoLiftController : ComplexController
{
    public static AutoLiftController Instance { get; set; }

    AutoLiftSettings lift_settings = null;
    LiftAscentPath ascentPath = null;

    KSPVessel current_vessel;

    float wanted_elevation;

    public AutoLiftController()
    {
        // logger.LogMessage("LandingController !");
        current_vessel = K2D2_Plugin.Instance.current_vessel;

        lift_settings = new AutoLiftSettings();
        ascentPath = new LiftAscentPath(lift_settings);

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

        wanted_elevation = -90;
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

        direction = QuaternionD.Euler(-wanted_elevation, lift_settings.heading, 0) * Vector3d.forward;

        Vector direction_vector = new Vector(up.coordinateSystem, direction);

        autopilot.SAS.lockedMode = false;
        autopilot.SAS.SetTargetOrientation(direction_vector, false);
    }

    float current_altitude_km = 0;
    float ap_km = 0;
    bool show_profile = false;

    void computeValues()
    {
        if (current_vessel.VesselComponent == null)
            return;

        PatchedConicsOrbit orbit = current_vessel.VesselComponent.Orbit;
        ap_km = (float)(orbit.Apoapsis - orbit.referenceBody.radius) / 1000;
        current_altitude_km = (float)(current_vessel.GetSeaAltitude() / 1000);

        wanted_elevation = ascentPath.compute_elevation(current_altitude_km);
    }

    public override void Update()
    {
        // if (!isRunning && !ui_visible) return;
        if (current_vessel == null) return;

        if (isRunning || ui_visible)
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
        UI_Tools.Title("Lift Pilot");

        if (K2D2_Plugin.Instance.settings_visible)
        {
            K2D2Settings.onGUI();
            return;
        }

        if (show_profile)
        {
            lift_settings.destination_Ap_km = UI_Fields.IntFieldLine("lift.destination_Ap_km", "Ap Altitude", lift_settings.destination_Ap_km, 0, Int32.MaxValue, "km");

            GUILayout.BeginHorizontal();
            // GUILayout.Label("5° Alt");
            GUILayout.Label($"5° Alt. : {lift_settings.end_rotate_altitude_km:n0} km", KBaseStyle.slider_text);
            lift_settings.end_rotate_ratio = UI_Tools.FloatSlider(lift_settings.end_rotate_ratio, 0, 1);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"45° Alt. : {lift_settings.mid_rotate_altitude_km:n0} km", KBaseStyle.slider_text);
            lift_settings.mid_rotate_ratio = UI_Tools.FloatSlider(lift_settings.mid_rotate_ratio, 0, 1);
            GUILayout.EndHorizontal();

            lift_settings.start_altitude_km = UI_Fields.IntFieldLine("lift.start_altitude_km", "90° Altitude", lift_settings.start_altitude_km, 0, Int32.MaxValue, "km");


            ascentPath.drawProfile(current_altitude_km);

            lift_settings.heading = UI_Tools.HeadingControl("lift.heading", lift_settings.heading);

            GUILayout.BeginHorizontal();
            if (UI_Tools.miniButton("Hide"))
            {
                show_profile = false;
            }

            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.BeginHorizontal();
            if (UI_Tools.miniButton("Show Profile"))
            {
                show_profile = true;
            }

            GUILayout.EndHorizontal();
        }

        isRunning = UI_Tools.BigToggleButton(isRunning, "Start", "Stop");

        if (isRunning)
        {
            UI_Tools.Console($"Altitude = {current_altitude_km:n2} km");
            UI_Tools.Console($"Inclination = {wanted_elevation:n2} °");
            UI_Tools.Console($"Apoapsis Alt. = {ap_km:n2} km");
        }
    }
}
