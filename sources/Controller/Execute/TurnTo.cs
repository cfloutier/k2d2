
using System.Collections.Generic;

using System.Linq;

using UnityEngine;

using KSP.Sim;
using K2D2.KSPService;
using KSP.Sim.Maneuver;


namespace K2D2.Controller
{
    class TurnToSettings
    {

        public static float max_angle_maneuver
        {
            get => Settings.s_settings_file.GetFloat("turn.max_angle_maneuver", 0.3f);
            set {
                    value = Mathf.Clamp(value, 0, 7);
                    Settings.s_settings_file.SetFloat("turn.max_angle_maneuver", value); 
                }
        }

        public static float max_angular_speed
        {
            get => Settings.s_settings_file.GetFloat("turn.max_angular_speed", 1f);
            set {
                    value = Mathf.Clamp(value, 0, 7);
                    Settings.s_settings_file.SetFloat("turn.max_angular_speed", value); 
                }
        }

        public static void onGUI()
        {
            max_angle_maneuver = UI_Tools.FloatSlider("Max Angle", max_angle_maneuver, 0.01f, 1, "°", "Accepted Angular error.");
            max_angular_speed = UI_Tools.FloatSlider("Max Angular Speed", max_angular_speed, 0.01f, 1, "°/s", "Accepted Angular speed.");
        }

    }

    public class TurnTo : ExecuteController
    {
        ManeuverNodeData maneuver = null;
        Vector3 wanted_dir = Vector3.zero;

        KSPVessel current_vessel;

        public double angle;

        public void StartManeuver(ManeuverNodeData node)
        {
            maneuver = node;
            Start();
        }


        public void StartSurfaceRetroGrade()
        {
            maneuver = null;
            Start();
        }

        public override void Start()
        {
            current_vessel = K2D2_Plugin.Instance.current_vessel;
            // reset time warp
            TimeWarpTools.SetRateIndex(0, false);
        }

        public override void Update()
        {
            if (maneuver != null)
            {
                finished = false;
                var autopilot = current_vessel.Autopilot;

                // force autopilot
                autopilot.Enabled = true;
                if (autopilot.AutopilotMode != AutopilotMode.Maneuver)
                    autopilot.SetMode(AutopilotMode.Maneuver);

                if (!checkManeuvreDirection())
                    return;

                if (!checkAngularRotation())
                    return;

                status_line = "Ready !";
                finished = true;
            }
            else
            {
                // SURFACE RETROGRADE MODE
                current_vessel.SetSpeedMode(KSP.Sim.SpeedDisplayMode.Surface);
                var autopilot = current_vessel.Autopilot;

                // force autopilot
                autopilot.Enabled = true;
                if (autopilot.AutopilotMode != AutopilotMode.Retrograde)
                    autopilot.SetMode(AutopilotMode.Retrograde);

                if (!checkRetroGradeDirection())
                    return;

                if (!checkAngularRotation())
                    return;

                status_line = "Ready !";
                finished = true;
            }
        }

        public bool checkRetroGradeDirection()
        {
            double max_angle = 5;

            var telemetry = SASInfos.getTelemetry();
            Vector retro_dir = telemetry.SurfaceMovementRetrograde;
            Rotation vessel_rotation = current_vessel.GetRotation();

            // convert rotation to maneuvre coordinates
            vessel_rotation = Rotation.Reframed(vessel_rotation, retro_dir.coordinateSystem);
            Vector3d forward_direction = (vessel_rotation.localRotation * Vector3.up).normalized;

            angle = (float)Vector3d.Angle(retro_dir.vector, forward_direction);
            status_line = $"Waiting for good sas direction\nAngle = {angle:n2}°";

            return angle < max_angle;
        }

        public bool checkManeuvreDirection()
        {
            double max_angle = TurnToSettings.max_angle_maneuver;

            var telemetry = SASInfos.getTelemetry();
            if (!telemetry.HasManeuver)
                return false;

            Vector maneuvre_dir = telemetry.ManeuverDirection;
            Rotation vessel_rotation = current_vessel.GetRotation();

            // convert rotation to maneuvre coordinates
            vessel_rotation = Rotation.Reframed(vessel_rotation, maneuvre_dir.coordinateSystem);
            Vector3d forward_direction = (vessel_rotation.localRotation * Vector3.up).normalized;

            angle = Vector3d.Angle(maneuvre_dir.vector, forward_direction);
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
            UI_Tools.Warning("Check Attitude");
            UI_Tools.Console(status_line);

            // UI_Tools.Console($"sas.sas_response v {Tools.print_vector(sas_response)}");

            if (Settings.debug_mode)
            {
                var telemetry = SASInfos.getTelemetry();
                if (!telemetry.HasManeuver)
                    return;

                var autopilot = current_vessel.Autopilot;

                // var angulor_vel_coord = VesselInfos.GetAngularSpeed().coordinateSystem;
                var angularVelocity = current_vessel.GetAngularSpeed().vector;

                // UI_Tools.Console($"angulor_vel_coord {angulor_vel_coord}");
                Vector maneuvre_dir = telemetry.ManeuverDirection;
                // UI_Tools.Console($"maneuvre_dir ref {maneuvre_dir.coordinateSystem}");
                // UI_Tools.Console($"maneuvre_dir {StrTool.VectorToString(maneuvre_dir.vector)}");
                UI_Tools.Console($"angularVelocity {StrTool.VectorToString(angularVelocity)}");
                // UI_Tools.Console($"angularVelocity {StrTool.VectorToString(angularVelocity)}");
                UI_Tools.Console($"autopilot {autopilot.AutopilotMode}");
            }
        }
    }
}