
using System.Collections.Generic;

using System.Linq;

using UnityEngine;

using KSP.Sim;
using K2D2.KSPService;
using KSP.Sim.Maneuver;


namespace K2D2.Controller
{

    public class TurnTo : ExecuteController
    {
        ManeuverNodeData maneuver = null;
        Vector3 wanted_dir = Vector3.zero;

        KSPVessel current_vessel;

        public void StartManeuver(ManeuverNodeData node)
        {
            maneuver = node;
            Start();
        }


        public void StartSurfaceRetroGrade(ManeuverNodeData node)
        {
            maneuver = null;
            Start();
        }

        public override void Start()
        {
            current_vessel = K2D2_Plugin.Instance.current_vessel;
            // reset time warp
            TimeWarpTools.SetRateIndex(0, false);
            current_vessel = K2D2_Plugin.Instance.current_vessel;
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

            double angle = (float)Vector3d.Angle(retro_dir.vector, forward_direction);
            status_line = $"Waiting for good sas direction\nAngle = {angle:n2}°";

            return angle < max_angle;
        }

        public bool checkManeuvreDirection()
        {
            double max_angle = 1;

            var telemetry = SASInfos.getTelemetry();
            if (!telemetry.HasManeuver)
                return false;

            Vector maneuvre_dir = telemetry.ManeuverDirection;
            Rotation vessel_rotation = current_vessel.GetRotation();

            // convert rotation to maneuvre coordinates
            vessel_rotation = Rotation.Reframed(vessel_rotation, maneuvre_dir.coordinateSystem);
            Vector3d forward_direction = (vessel_rotation.localRotation * Vector3.up).normalized;

            double angle = Vector3d.Angle(maneuvre_dir.vector, forward_direction);
            status_line = $"Waiting for good sas direction\nAngle = {angle:n2}°";

            return angle < max_angle;
        }

        public bool checkAngularRotation()
        {
            double max_angular_speed = 0.5;
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
            GUILayout.Label("Check Attitude", Styles.phase_ok);
            GUILayout.Label(status_line, Styles.console_text);

            // GUILayout.Label($"sas.sas_response v {Tools.print_vector(sas_response)}");

            if (Settings.debug_mode)
            {
                var telemetry = SASInfos.getTelemetry();
                if (!telemetry.HasManeuver)
                    return;

                var autopilot = current_vessel.Autopilot;

                // var angulor_vel_coord = VesselInfos.GetAngularSpeed().coordinateSystem;
                var angularVelocity = current_vessel.GetAngularSpeed().vector;

                // GUILayout.Label($"angulor_vel_coord {angulor_vel_coord}");
                Vector maneuvre_dir = telemetry.ManeuverDirection;
                GUILayout.Label($"maneuvre_dir ref {maneuvre_dir.coordinateSystem}");
                GUILayout.Label($"maneuvre_dir {GeneralTools.VectorToString(maneuvre_dir.vector)}");
                GUILayout.Label($"angularVelocity {GeneralTools.VectorToString(angularVelocity)}");
                GUILayout.Label($"angularVelocity {GeneralTools.VectorToString(angularVelocity)}");
                GUILayout.Label($"autopilot {autopilot.AutopilotMode}");
            }
        }
    }
}