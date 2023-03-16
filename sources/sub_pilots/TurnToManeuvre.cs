
using System.Collections.Generic;

using System.Linq;

using UnityEngine;

using KSP.Sim;
using KSP.Sim.impl;
using KSP.Sim.Maneuver;
using KSP.Game;
using KSP.Sim.ResourceSystem;



using BepInEx.Logging;
using KSP.ScriptInterop.impl.moonsharp;

namespace K2D2
{
    public class TurnToManeuvre : BasePilot
    {
        public AutoExecuteManeuver parent;

        public TurnToManeuvre(AutoExecuteManeuver parent)
        {
            this.parent = parent;
        }

        Vector3 maneuvre_dir = Vector3.zero;

        public override void Start()
        {
            // reset time warp
            var time_warp = TimeWarpTools.time_warp();
            time_warp.SetRateIndex(0, false);
        }

        public override void onUpdate()
        {
            finished = false;

            if (!checkManeuvreDirection())
                return;

            if (!checkAngularRotation())
                return;

            status_line = "Ready !";
            finished = true;
        }

        public bool checkManeuvreDirection()
        {
            double max_angle = 1;

            var telemetry = SASInfos.getTelemetry();
            if (!telemetry.HasManeuver)
                return false;

            // 
            Vector maneuvre_dir = telemetry.ManeuverDirection;
            Rotation vessel_rotation = VesselInfos.GetRotation();

            // convert rotation to maneuvre coordinates
            vessel_rotation = Rotation.Reframed(vessel_rotation, maneuvre_dir.coordinateSystem);
            Vector3d forward_direction = (vessel_rotation.localRotation * Vector3.up).normalized;

            double angle = Vector3d.Angle(maneuvre_dir.vector, forward_direction);
            status_line = $"Waiting for good sas direction\nAngle = {angle:n2}°";

            return angle < max_angle;
        }

        public bool checkAngularRotation()
        {
            double max_angular_speed = 2;

            var angular_rotation_pc = VesselInfos.GetAngularSpeed().vector;

            status_line = "Waiting for stabilisation";
            if (System.Math.Abs(angular_rotation_pc.x) > max_angular_speed)
                return false;

            if (System.Math.Abs(angular_rotation_pc.y) > max_angular_speed)
                return false;

            if (System.Math.Abs(angular_rotation_pc.z) > max_angular_speed)
                return false;

            return true;
        }


        public override void onGui()
        {
            GUILayout.Label("Check Attitude", Styles.phase_ok);
            GUILayout.Label(status_line, Styles.console);

            // GUILayout.Label($"sas.sas_response v {Tools.print_vector(sas_response)}");

            if (parent.debug_infos)
            {
                var telemetry = SASInfos.getTelemetry();
                if (!telemetry.HasManeuver)
                    return;

                var autopilot = VesselInfos.currentVessel().Autopilot;
                // var sas = autopilot.SAS;

                // var angulor_vel_coord = VesselInfos.GetAngularSpeed().coordinateSystem;
                var angularVelocity = VesselInfos.GetAngularSpeed().vector;

                // GUILayout.Label($"angulor_vel_coord {angulor_vel_coord}");
                Vector maneuvre_dir = telemetry.ManeuverDirection;
                GUILayout.Label($"maneuvre_dir ref {maneuvre_dir.coordinateSystem}");
                GUILayout.Label($"maneuvre_dir {Tools.printVector(maneuvre_dir.vector)}");


                GUILayout.Label($"angularVelocity {Tools.printVector(angularVelocity)}");

                GUILayout.Label($"angularVelocity {Tools.printVector(angularVelocity)}");
                GUILayout.Label($"autopilot {autopilot.AutopilotMode}");

                if (GUILayout.Button("Force SaS"))
                    autopilot.SetMode(AutopilotMode.Maneuver);

                GUILayout.Label($"finished {finished}");
            }

            
        }
    }

}