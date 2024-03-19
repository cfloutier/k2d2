using K2D2.KSPService;
using KSP.Sim;
using KSP.Sim.Maneuver;
using KTools;
// using KTools.UI;
using UnityEngine;

namespace K2D2.Controller;

class TurnToSettings
{
    // Need a clamp !!!! 
    public static ClampSetting<float> max_angle_maneuver = new ("turn.max_angle_maneuver", 0.3f, 0, 7);

    public static ClampSetting<float> max_angular_speed = new ("turn.max_angular_speed", 1f, 0, 7);
   
    // public static void onGUI()
    // {
    //     max_angle_maneuver = UI_Tools.FloatSliderTxt("Max Angle", max_angle_maneuver, 0.01f, 1, "°", "Accepted Angular error.");
    //     max_angular_speed = UI_Tools.FloatSliderTxt("Max Angular Speed", max_angular_speed, 0.01f, 1, "°/s", "Accepted Angular speed.");
    // }
}

public class TurnTo : ExecuteController
{
    public enum Mode
    {
        Maneuver,
        Retrograde,
        Prograde
    }

    public SpeedDisplayMode speedMode = SpeedDisplayMode.Surface;

    ManeuverNodeData maneuver = null;
    Vector3 wanted_dir = Vector3.zero;

    KSPVessel current_vessel;

    public double angle;

    public Mode mode = Mode.Maneuver;

    public void StartManeuver(ManeuverNodeData node)
    {
        
        mode = Mode.Maneuver;
        maneuver = node;
        Start();
    }


    public void StartRetroGrade(SpeedDisplayMode speedMode = SpeedDisplayMode.Surface)
    {
        this.speedMode = speedMode;
        maneuver = null;

        mode = Mode.Retrograde;
        Start();
    }

    public void StartProGrade(SpeedDisplayMode speedMode = SpeedDisplayMode.Surface)
    {
        this.speedMode = speedMode;
        maneuver = null;

        mode = Mode.Prograde;
        Start();
    }

    public override void Start()
    {
        current_vessel = K2D2Plugin.Instance.current_vessel;
        // reset time warp
        TimeWarpTools.SetRateIndex(0, false);
    }

    public override void Update()
    {
        switch (mode)
        {
            case Mode.Maneuver:
                {
                    finished = false;

                    SASTool.setAutoPilot(AutopilotMode.Maneuver);

                    if (!checkManeuverDirection())
                        return;

                    if (!checkAngularRotation())
                        return;

                    status_line = "Ready !";
                    finished = true;
                }
                break;
            case Mode.Retrograde:
                {
                    // SURFACE RETROGRADE MODE
                    current_vessel.SetSpeedMode(speedMode);
                    SASTool.setAutoPilot(AutopilotMode.Retrograde);

                    if (!checkRetroGradeDirection())
                        return;

                    if (!checkAngularRotation())
                        return;

                    status_line = "Ready !";
                    finished = true;
                }
                break;
            case Mode.Prograde:
                {
                    // SURFACE RETROGRADE MODE
                    current_vessel.SetSpeedMode(speedMode);
                    SASTool.setAutoPilot(AutopilotMode.Prograde);

                    if (!checkProGradeDirection())
                        return;

                    if (!checkAngularRotation())
                        return;

                    status_line = "Ready !";
                    finished = true;
                }
                break;
        }
    }




    bool checkProGradeDirection()
    {
        double max_angle = 5;

        var telemetry = SASTool.getTelemetry();
        if (telemetry == null)
            return false;
            
        Vector prograde_dir;
        if (speedMode == SpeedDisplayMode.Orbit)
            prograde_dir = telemetry.OrbitMovementPrograde;
        else
            prograde_dir = telemetry.SurfaceMovementPrograde;

        Rotation vessel_rotation = current_vessel.GetRotation();

        // convert rotation to maneuver coordinates
        vessel_rotation = Rotation.Reframed(vessel_rotation, prograde_dir.coordinateSystem);
        Vector3d forward_direction = (vessel_rotation.localRotation * Vector3.up).normalized;

        angle = (float)Vector3d.Angle(prograde_dir.vector, forward_direction);
        status_line = $"Waiting for good sas direction\nAngle = {angle:n2}°";

        return angle < max_angle;
    }

    bool checkRetroGradeDirection()
    {
        double max_angle = 5;

        var telemetry = SASTool.getTelemetry();
        Vector retro_dir;
        if (speedMode == SpeedDisplayMode.Orbit)
            retro_dir = telemetry.OrbitMovementRetrograde;
        else
            retro_dir = telemetry.SurfaceMovementRetrograde;
        Rotation vessel_rotation = current_vessel.GetRotation();

        // convert rotation to maneuver coordinates
        vessel_rotation = Rotation.Reframed(vessel_rotation, retro_dir.coordinateSystem);
        Vector3d forward_direction = (vessel_rotation.localRotation * Vector3.up).normalized;

        angle = (float)Vector3d.Angle(retro_dir.vector, forward_direction);
        status_line = $"Waiting for good sas direction\nAngle = {angle:n2}°";

        return angle < max_angle;
    }

    bool checkManeuverDirection()
    {
        double max_angle = TurnToSettings.max_angle_maneuver.V;

        var telemetry = SASTool.getTelemetry();
        if (!telemetry.HasManeuver)
            return false;

        Vector maneuver_dir = telemetry.ManeuverDirection;
        Rotation vessel_rotation = current_vessel.GetRotation();

        // convert rotation to maneuver coordinates
        vessel_rotation = Rotation.Reframed(vessel_rotation, maneuver_dir.coordinateSystem);
        Vector3d forward_direction = (vessel_rotation.localRotation * Vector3.up).normalized;

        angle = Vector3d.Angle(maneuver_dir.vector, forward_direction);
        status_line = $"Waiting for good sas direction\nAngle = {angle:n2}°";

        return angle < max_angle;
    }

    bool checkAngularRotation()
    {
        double max_angular_speed = TurnToSettings.max_angular_speed.V;
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

    public override void updateUI(FullStatus st)
    {
        st.Status("Check Attitude", K2UI.StatusLine.Level.Warning);
        st.Console(status_line);

        // UI_Tools.Console($"sas.sas_response v {Tools.print_vector(sas_response)}");

        if (K2D2Settings.debug_mode.V)
        {
            var telemetry = SASTool.getTelemetry();
            if (!telemetry.HasManeuver)
                return;

            var autopilot = current_vessel.Autopilot;

            // var angulor_vel_coord = VesselInfos.GetAngularSpeed().coordinateSystem;
            var angularVelocity = current_vessel.GetAngularSpeed().vector;

            // UI_Tools.Console($"angulor_vel_coord {angulor_vel_coord}");
            Vector maneuver_dir = telemetry.ManeuverDirection;
            // UI_Tools.Console($"maneuver_dir ref {maneuver_dir.coordinateSystem}");
            // UI_Tools.Console($"maneuver_dir {StrTool.VectorToString(maneuver_dir.vector)}");
            st.Console($"angularVelocity {StrTool.Vector3ToString(angularVelocity)}");
            // UI_Tools.Console($"angularVelocity {StrTool.VectorToString(angularVelocity)}");
            st.Console($"autopilot {autopilot.AutopilotMode}");
        }
    }
}
