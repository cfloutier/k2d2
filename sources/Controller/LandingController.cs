
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using BepInEx.Logging;
using K2D2.KSPService;
using KSP.Sim;

namespace K2D2.Controller
{
    public class LandingController : ComplexControler
    {
        public ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.LandingController");

        public static LandingController Instance { get; set; }

        public KSPVessel current_vessel;

        public BurndV burn_dV = new BurndV();

        public LandingController()
        {
            Instance = this;
            sub_contollers.Add(burn_dV);
            logger.LogMessage("LandingController !");
            current_vessel = K2D2_Plugin.Instance.current_vessel;
        }

        bool _land_controler_active = false;
        bool land_controler_active
        {
            get { return _land_controler_active; }
            set {
                if (value == _land_controler_active)
                    return;
                _land_controler_active = value;

                if (!value)
                {
                    // stop
                    if (current_vessel != null)
                        current_vessel.SetThrottle(0);
                }
                else
                {
                    // Start
                    burn_dV.reset();

                }
            }
        }

        string status_line = "";

        public override void Update()
        {
            if (current_vessel == null || current_vessel.VesselVehicle == null)
                return;

            altitude = (float)current_vessel.GetDisplayAltitude();
            current_speed = (float)current_vessel.VesselVehicle.SurfaceSpeed;
            delta_speed = current_speed - limit_speed;

            if (delta_speed > 0) // reset timewarp if it is time to burn
                TimeWarpTools.SetRateIndex(0, false);

            if (gravity_compensation)
                computeInclination();

            if (!land_controler_active)
                return;

            current_vessel.SetSpeedMode(KSP.Sim.SpeedDisplayMode.Surface);
            var autopilot = current_vessel.Autopilot;

            // force autopilot
            autopilot.Enabled = true;
            if (autopilot.AutopilotMode != AutopilotMode.Retrograde)
                autopilot.SetMode(AutopilotMode.Retrograde);

            if (!checkManeuvreDirection())
            {
                current_vessel.SetThrottle(0);
                status_line = $"Turning : {retrograde_angle} °";
                return;
            }

            compute_Throttle();
            status_line = "Burning";
            status_line = $"current speed : {current_speed} m/s\ndelta speed : {delta_speed} m/s";

            if (gravity_compensation)
            {
                // no stop for gravity compensation
                current_vessel.SetThrottle(wanted_throttle);
            }
            else if (!checkSpeed())
            {
                current_vessel.SetThrottle(wanted_throttle);
            }
            else
            {
                // stop
                land_controler_active =  false;
            }

            // status_line = "ok";
            // current_vessel.SetThrottle(0);
            // land_controler_active = false;
        }

        public float limit_speed;
        public float altitude;

        public float retrograde_angle;
        public float inclination;
        public float gravity;
        public float gravity_direction_factor;

        public void computeInclination()
        {
            // current_vessel.getInclination();
            Vector up_dir = current_vessel.VesselComponent.gravityForPos;
            Rotation vessel_rotation = current_vessel.GetRotation();

            // convert rotation to maneuvre coordinates

            vessel_rotation = Rotation.Reframed(vessel_rotation, up_dir.coordinateSystem);
            Vector3d forward_direction = (vessel_rotation.localRotation * Vector3.down).normalized;

            inclination = (float) Vector3d.Angle(up_dir.vector, forward_direction);
            // status_line = $"Waiting for good sas direction\nAngle = {angle:n2}°";

            gravity = (float) current_vessel.VesselComponent.graviticAcceleration.magnitude;
            gravity_direction_factor = Mathf.Cos(inclination*Mathf.Deg2Rad);
        }

        public bool checkManeuvreDirection()
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

        float wanted_throttle = 0;

        void compute_Throttle()
        {
            float min_throttle = 0;

            if (gravity_compensation)
            {
                float minimum_dv = gravity_direction_factor * gravity;
                min_throttle = minimum_dv / burn_dV.full_dv;
            }

            float remaining_full_burn_time = (float)(delta_speed / burn_dV.full_dv);
            wanted_throttle = Mathf.Clamp(remaining_full_burn_time + min_throttle, 0, 1);
        }

        float current_speed;
        float delta_speed;

        public bool checkSpeed()
        {
            if (delta_speed < 0)
            {
                return true;
            }

            return false;
        }

        //////////// Settings ////////////////
        public int multiplier_index
        {
            get => Settings.s_settings_file.GetInt("land.multiplier_index", 0);
            set {             // value = Mathf.Clamp(0.1,)
                Settings.s_settings_file.SetInt("land.multiplier_index", value); }
        }


        public float speed_ratio
        {
            get => Settings.s_settings_file.GetFloat("land.speed_ratio", 0);
            set {             // value = Mathf.Clamp(0.1,)
                Settings.s_settings_file.SetFloat("land.speed_ratio", value); }
        }


        public bool gravity_compensation
         {
            get => Settings.s_settings_file.GetBool("land.gravity_compensation", false);
            set {             // value = Mathf.Clamp(0.1,)
                Settings.s_settings_file.SetBool("land.gravity_compensation", value); }
        }

        public bool auto_speed
        {
            get => Settings.s_settings_file.GetBool("land.auto_speed", false);
            set {
                Settings.s_settings_file.SetBool("land.auto_speed", value); }
        }


        public float auto_speed_ratio
        {
            get => Settings.s_settings_file.GetFloat("land.auto_speed_ratio", 0.5f);
            set {
                // value = Mathf.Clamp(value, 0 , 1);
                Settings.s_settings_file.SetFloat("land.auto_speed_ratio", value);
                }
        }

        public float safe_speed
        {
            get => Settings.s_settings_file.GetFloat("land.safe_speed", 4);
            set {
                value = Mathf.Clamp(value, 0 , 100);
                Settings.s_settings_file.SetFloat("land.safe_speed", value);
                }
        }

        public override void onGUI()
        {
            // GUILayout.Label("// Landing ", Styles.title);

            auto_speed = GUILayout.Toggle(auto_speed, "Auto Speed");
            if (auto_speed)
            {
                float div = 10; //Mathf.Pow(10, multiplier_index);

                auto_speed_ratio = UI_Tools.FloatSlider("Altitude/speed ratio", auto_speed_ratio, 0.5f, 3);
                UI_Tools.RightLeftText("Slow", "Quick");

                safe_speed = UI_Tools.FloatSlider("Landing speed", safe_speed, 0.1f, 10);
                GUILayout.Label($"altitude : {altitude:n2} m");

                limit_speed = altitude * auto_speed_ratio / div + safe_speed;
            }
            else
            {
                string[] multiplier_txt = {"x10", "x100", "x1000"};
                GUILayout.BeginHorizontal();
                multiplier_index = GUILayout.SelectionGrid(multiplier_index, multiplier_txt, 3);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                speed_ratio = GUILayout.HorizontalSlider(speed_ratio, 0, 1);
                limit_speed = Mathf.Pow(10, multiplier_index+1) * speed_ratio;
            }

            if (delta_speed > 0)
                GUI.color = Color.red;

            GUILayout.Label($"limit speed : {limit_speed:n2}");
            GUI.color = Color.white;
            gravity_compensation = GUILayout.Toggle(gravity_compensation, "Gravity Compensation");

            if (gravity_compensation && Settings.debug_mode)
            {
                GUILayout.Label($"inclination : {inclination:n2}°");
                GUILayout.Label($"gravity : {gravity:n2}");
                GUILayout.Label($"gravity_direction_factor : {gravity_direction_factor:n2}");
            }

            if (Settings.debug_mode)
            {
                GUILayout.Label($"wanted_throttle : {wanted_throttle:n2}");
                GUILayout.Label($"current speed : {current_speed:n2} m/s");

                if (burn_dV.burned_dV > 0)
                    GUILayout.Label($"dV consumed : {burn_dV.burned_dV:n2} m/s");
            }

            land_controler_active = UI_Tools.ToggleButton(land_controler_active, "Start", "Stop");
            GUILayout.Label(status_line, Styles.small_dark_text);
        }



    }
}