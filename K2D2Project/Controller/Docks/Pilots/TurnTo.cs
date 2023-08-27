using K2D2.KSPService;
using KSP.Sim;
using KSP.Sim.Maneuver;
using KTools;
using KTools.UI;
using UnityEngine;
using static KSP.Api.UIDataPropertyStrings.View;

namespace K2D2.Controller.Docks.Pilots;

/// <summary>
/// rotation used for docking
/// </summary>
public class TurnTo : ExecuteController
{
    Vector3 wanted_dir = Vector3.zero;

    KSPVessel current_vessel;

    public double angle;

    public enum Mode
    {
        Off,
        RetroSpeed,
    }

    public Mode mode = Mode.Off;

    public void StartRetroSpeed()
    {
        mode = Mode.RetroSpeed;
        Start();
    }

  
    public override void Start()
    {
        current_vessel = K2D2_Plugin.Instance.current_vessel;
         
        // reset time warp
        TimeWarpTools.SetRateIndex(0, false);
        var autopilot = current_vessel.Autopilot;
        autopilot.Enabled = true;
        autopilot.SetMode(AutopilotMode.StabilityAssist);
    }

    void CheckRetroSpeed()
    {
        var autopilot = current_vessel.Autopilot;

        // force autopilot
        autopilot.Enabled = true;
        autopilot.SAS.lockedMode = false;

        Vector direction = current_vessel.VesselComponent.TargetVelocity;
        direction.vector = -direction.vector;

        autopilot.SAS.SetTargetOrientation(direction, false);

        if (!checkRetroSpeed())
            return;

        if (!checkAngularRotation())
            return;
        
    }

    public override void Update()
    {
        switch(mode) 
        {
            case Mode.RetroSpeed: CheckRetroSpeed(); break;
        }   
    }

    public bool checkRetroSpeed()
    {
        double max_angle = 5;

        Vector retro_dir = current_vessel.VesselComponent.TargetVelocity;
        Rotation vessel_rotation = current_vessel.GetRotation();

        // convert rotation to speed coordinates system
        vessel_rotation = Rotation.Reframed(vessel_rotation, retro_dir.coordinateSystem);

        Vector3d forward_direction = (vessel_rotation.localRotation * Vector3.up).normalized;

        angle = (float)Vector3d.Angle(retro_dir.vector, forward_direction);
        status_line = $"Waiting for good sas direction\nAngle = {angle:n2}°";

        return angle < max_angle;
    }

    public bool checkAngularRotation()
    {
        double max_angular_speed = TurnToSettings.max_angular_speed;
        var angular_rotation_pc = current_vessel.GetAngularSpeed().vector;

        status_line = "Waiting for stabilisation";
        if (System.Math.Abs(angular_rotation_pc.x) > max_angular_speed)
            return false;

        if (System.Math.Abs(angular_rotation_pc.y) > max_angular_speed)
            return false;

        if (System.Math.Abs(angular_rotation_pc.z) > max_angular_speed)
            return false;

        return true;
    }

    public override void onGUI()
    {
        UI_Tools.Warning("Check Attitude (dockin)");
        UI_Tools.Console(status_line);

        // UI_Tools.Console($"sas.sas_response v {Tools.print_vector(sas_response)}");

        if (K2D2Settings.debug_mode)
        {
            var telemetry = SASTool.getTelemetry();
            if (!telemetry.HasManeuver)
                return;

            var autopilot = current_vessel.Autopilot;

            // var angulor_vel_coord = VesselInfos.GetAngularSpeed().coordinateSystem;
            var angularVelocity = current_vessel.GetAngularSpeed().vector;

            
            UI_Tools.Console($"angle {angle:n2} °");
            UI_Tools.Console($"angularVelocity {StrTool.Vector3ToString(angularVelocity)}");
            
            UI_Tools.Console($"autopilot {autopilot.AutopilotMode}");
        }
    }
}
