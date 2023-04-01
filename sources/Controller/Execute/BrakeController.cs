using K2D2.KSPService;
using KSP.Sim;
using KSP.Sim.impl;
using UnityEngine;


namespace K2D2.Controller
{

    /// apply the wanted speed in the good direction
    public class BrakeController  : ExecuteController
    {

        public bool gravity_compensation;
        public float wanted_speed = 0;

        KSPVessel current_vessel;
        BurndV burn_dV = new BurndV();

        float gravity_inclination = 0;
        float gravity_direction_factor = 0;
        float gravity;

        float wanted_throttle = 0;

        public BrakeController()
        {
            sub_contollers.Add(burn_dV);
            // logger.LogMessage("LandingController !");
            current_vessel = K2D2_Plugin.Instance.current_vessel;
        }

        public void computeGravityRatio()
        {
            // current_vessel.getInclination();
            Vector up_dir = current_vessel.VesselComponent.gravityForPos;
            Rotation vessel_rotation = current_vessel.GetRotation();

            // convert rotation to maneuvre coordinates

            vessel_rotation = Rotation.Reframed(vessel_rotation, up_dir.coordinateSystem);
            Vector3d forward_direction = (vessel_rotation.localRotation * Vector3.down).normalized;

            var gravity_inclination = (float) Vector3d.Angle(up_dir.vector, forward_direction);
            // status_line = $"Waiting for good sas direction\nAngle = {angle:n2}°";

            gravity = (float) current_vessel.VesselComponent.graviticAcceleration.magnitude;
            gravity_direction_factor = Mathf.Cos(gravity_inclination*Mathf.Deg2Rad);
        }

        void compute_Throttle()
        {
            float min_throttle = 0;

            if (gravity_compensation)
            {
                float minimum_dv = gravity_direction_factor * gravity;
                min_throttle = minimum_dv / burn_dV.full_dv;
            }


            delta_speed = current_speed - wanted_speed;

            float remaining_full_burn_time = (float)(delta_speed / burn_dV.full_dv);
            wanted_throttle = Mathf.Clamp(remaining_full_burn_time + min_throttle, 0, 1);
        }

        float delta_speed = 0;

        public bool NeedBurn => delta_speed > 0;
    

        public bool checkSpeed()
        {
            if (delta_speed < 0)
            {
                return true;
            }

            return false;
        }

        float retrograde_angle;

        public bool checkBurnDirection()
        {
            double max_angle = 5;

            var telemetry = SASInfos.getTelemetry();
            Vector retro_dir = telemetry.SurfaceMovementRetrograde;
            Rotation vessel_rotation = current_vessel.GetRotation();

            // convert rotation to maneuvre coordinates
            vessel_rotation = Rotation.Reframed(vessel_rotation, retro_dir.coordinateSystem);
            Vector3d forward_direction = (vessel_rotation.localRotation * Vector3.up).normalized;

            retrograde_angle = (float) Vector3d.Angle(retro_dir.vector, forward_direction);
            // status_line = $"Waiting for good sas direction\nAngle = {angle:n2}°";

            return retrograde_angle < max_angle;
        }

        float current_speed;

        public override void Update()
        {
            if (current_vessel == null || current_vessel.VesselVehicle == null)
                return;

            current_speed = (float)current_vessel.VesselVehicle.SurfaceSpeed;

            delta_speed = current_speed - wanted_speed;

            if (delta_speed > 0) // reset timewarp if it is time to burn
                TimeWarpTools.SetRateIndex(0, false);

            if (gravity_compensation)
                computeGravityRatio();


            current_vessel.SetSpeedMode(KSP.Sim.SpeedDisplayMode.Surface);
            var autopilot = current_vessel.Autopilot;

            // force autopilot
            autopilot.Enabled = true;
            if (autopilot.AutopilotMode != AutopilotMode.Retrograde)
                autopilot.SetMode(AutopilotMode.Retrograde);

            if (!checkBurnDirection())
            {
                current_vessel.SetThrottle(0);
                status_line = $"Turning : {retrograde_angle:n2} °";
                return;
            }

            compute_Throttle();
            status_line = "Burning";

            // no stop for gravity compensation
            current_vessel.SetThrottle(wanted_throttle);
        }


        public override void onGUI()
        {
             // need to burn !
            if (delta_speed > 0)
            {
                GUI.color = Color.red;
                UI_Tools.Console($"Max speed : {wanted_speed:n2} !!");
                UI_Tools.Console($"delta speed  : {delta_speed:n2}  m/s");
                GUI.color = Color.white;
            }
            else
            {
                UI_Tools.Console($"Max speed : {wanted_speed:n2}  m/s");
                UI_Tools.Console($"delta speed  : {-delta_speed:n2}  m/s");
            }

            if (burn_dV.burned_dV > 0)
                UI_Tools.Console($"dV consumed : {burn_dV.burned_dV:n2} m/s");

            if (Settings.debug_mode)
            {
                if (gravity_compensation)
                {
                    GUILayout.Label($"gravity : {gravity:n2}");
                    GUILayout.Label($"gravity inclination : {gravity_inclination:n2}°");
                    //GUILayout.Label($"gravity_direction_factor : {gravity_direction_factor:n2}");
                }

                GUILayout.Label($"wanted_throttle : {wanted_throttle:n2}");



            }
        }


    }

}