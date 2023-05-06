using K2D2.KSPService;
using KSP.Sim;
using KTools.UI;
using UnityEngine;

namespace K2D2.Controller;

public class AttitudeController : ComplexControler
{
    public static AttitudeController Instance { get; set; }

    KSPVessel current_vessel;

    float elevation;
    float heading;

    public AttitudeController()
    {
        // logger.LogMessage("LandingController !");
        current_vessel = K2D2_Plugin.Instance.current_vessel;

        Instance = this;
        debug_mode_only = false;
        name = "Attitude";
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

        direction = QuaternionD.Euler(elevation, heading, 0) * Vector3d.forward;

        Vector direction_vector = new Vector(up.coordinateSystem, direction);

        autopilot.SAS.lockedMode = lockedMode;
        autopilot.SAS.SetTargetOrientation(direction_vector, reset);
    }


    bool lockedMode = false;
    bool reset = false;

    public override void onGUI()
    {
        if (K2D2_Plugin.Instance.settings_visible)
        {
            K2D2Settings.onGUI();
            return;
        }

        lockedMode = UI_Tools.Toggle(lockedMode, "Locked Mode");
        reset = UI_Tools.Toggle(reset, "Reset");


        elevation = UI_Tools.FloatSliderTxt("Elevation", elevation, -90, 90, "°");
        heading = UI_Tools.FloatSliderTxt("heading", heading, -180, 180, "°");

        // UI_Tools.ProgressBar(heading, -180, 180);

        // z_direction = UI_Tools.FloatSlider("Z", z_direction, -180, 180, "°");
        if (Mathf.Abs(elevation) < 2) elevation = 0;
        if (Mathf.Abs(heading) < 2) heading = 0;
        // if (Mathf.Abs(z_direction) < 2) z_direction = 0;

        isRunning = UI_Tools.BigToggleButton(isRunning, "Start", "Stop");

        var telemetry = SASTool.getTelemetry();

        var up = telemetry.HorizonUp;
        UI_Tools.Label($"up dir = {StrTool.VectorToString(up.vector)}");
        UI_Tools.Label($"up coor = {up.coordinateSystem}");
        UI_Tools.Label($"wanted dir = {StrTool.VectorToString(direction)}");

    }


}
