using BepInEx.Logging;
using K2D2.KSPService;
using KSP.Sim;
using KSP.UI.Binding;
using KTools;
using KTools.UI;
using UnityEngine;
using static RTG.CameraFocus;
using static System.Net.Mime.MediaTypeNames;

namespace K2D2.Controller;

class AttitudeSettings
{
    public static float elevation
    {
        get => KBaseSettings.sfile.GetFloat("warp.elevation", 0);
        set
        {
            value = Mathf.Clamp(value, -90, 90);
            KBaseSettings.sfile.SetFloat("warp.elevation", value);
        }
    }

    public static float heading
    {
        get => KBaseSettings.sfile.GetFloat("warp.heading", 0);
        set
        {
            value = Mathf.Clamp(value, -180, 180);
            KBaseSettings.sfile.SetFloat("warp.heading", value);
        }

    }
}

public class AttitudeController : ComplexControler
{
    public static AttitudeController Instance { get; set; }

    KSPVessel current_vessel;

    public AttitudeController()
    {
        // logger.LogMessage("LandingController !");
        current_vessel = K2D2_Plugin.Instance.current_vessel;

        Instance = this;
        debug_mode_only = false;
        name = "Att.";
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

                var autopilot = current_vessel.Autopilot;

                // force autopilot
                autopilot.Enabled = true;
                autopilot.SetMode(AutopilotMode.StabilityAssist);
            }
        }
    }



    Vector3d direction = Vector3d.zero;

    public override void Update()
    {
        if (!isRunning) return;

        if (current_vessel == null) return;
        var autopilot = current_vessel.Autopilot;

        // force autopilot
        autopilot.Enabled = true;

        var telemetry = SASTool.getTelemetry();

        var up = telemetry.HorizonUp;

        direction = QuaternionD.Euler(-AttitudeSettings.elevation, AttitudeSettings.heading, 0) * Vector3d.forward;
        Vector direction_vector = new Vector(up.coordinateSystem, direction);

        autopilot.SAS.lockedMode = false;
        autopilot.SAS.SetTargetOrientation(direction_vector, false);
    }

    public override void onGUI()
    {
        if (K2D2_Plugin.Instance.settings_visible)
        {
            K2D2Settings.onGUI();
            return;
        }

        UI_Tools.Title("// Attitude Pilot");

        GUILayout.BeginHorizontal();
        UI_Tools.Label("Elevation (°)");
        GUILayout.FlexibleSpace();
        AttitudeSettings.elevation = RepeatButton.OnGUI("elevation_minus", "--", AttitudeSettings.elevation, -0.2f);
        AttitudeSettings.elevation = UI_Fields.FloatField("elevation_field", AttitudeSettings.elevation);
        UI_Tools.Label("°");
        AttitudeSettings.elevation = RepeatButton.OnGUI("elevation_plus", "++", AttitudeSettings.elevation, 0.2f);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUI.SetNextControlName("attitude.elevationSlider");
        AttitudeSettings.elevation = UI_Tools.FloatSlider(AttitudeSettings.elevation, -90, 90);
        AttitudeSettings.heading = UI_Tools.HeadingSlider("heading", AttitudeSettings.heading);

        // UI_Tools.ProgressBar(heading, -180, 180);

        // z_direction = UI_Tools.FloatSlider("Z", z_direction, -180, 180, "°");
        //    if (Mathf.Abs(AttitudeSettings.elevation) < 2) AttitudeSettings.elevation = 0;
        // if (Mathf.Abs(z_direction) < 2) z_direction = 0;

        isRunning = UI_Tools.BigToggleButton(isRunning, "Start", "Stop");
    }

}
