

using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using BepInEx.Logging;
using K2D2.KSPService;
using KSP.Sim;
using KSP.Sim.impl;

using K2D2.Controller;
using SpaceGraphicsToolkit;
using VehiclePhysics;
using System;



namespace K2D2.Controller
{
    public class VSpeedSettings
    {

        public VSpeedController.Direction direction
         {
            get => Settings.s_settings_file.GetEnum<VSpeedController.Direction>("speed.direction", 0);
            set {
                Settings.s_settings_file.SetEnum<VSpeedController.Direction>("speed.direction", value);
                }
        }

        public float wanted_speed
        {
            get => Settings.s_settings_file.GetFloat("speed.wanted_speed", 0);
            set {
                Settings.s_settings_file.SetFloat("speed.wanted_speed", value);
                }
        }

        public int speed_limit
        {
            get => Settings.s_settings_file.GetInt("speed.speed_limit", 10);
            set {
                Settings.s_settings_file.SetInt("speed.speed_limit", value);
                }
        }

        public void onGUI()
        {
            UI_Tools.Title("V-Speed Settings");

            speed_limit = UI_Tools.IntSlider("Min-Max Speed", (int) speed_limit, 5, 100,"m/s", "just affect the Main UI");    
        }
    }

    public class VSpeedController : ComplexControler
    {
        public static VSpeedController Instance { get; set; }

        public enum Direction { SurfaceUp, SurfaceSpeed }
        public static string[] direction_labels = {"V-Speed Control", "Touch-Down" };

        VSpeedSettings settings = new VSpeedSettings();

        public override void onReset()
        {
            isActive = false;
        }

        bool _active = false;
        public override bool isActive
        {
            get { return _active; }
            set {
                if (value == _active)
                    return;

                if (!value)
                {
                    // stop
                    var current_vessel = K2D2_Plugin.Instance.current_vessel;
                    if (current_vessel != null)
                        current_vessel.SetThrottle(0);

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



        float max_DiffDirection = 90;

        public bool gravity_compensation;
        public float V_Speed = 0;

        KSPVessel current_vessel;
        BurndV burn_dV = new BurndV();

        float gravity_inclination = 0;
        float gravity_direction_factor = 0;
        float gravity;

        float wanted_throttle = 0;

        public VSpeedController()
        {
            sub_contollers.Add(burn_dV);
            // logger.LogMessage("LandingController !");
            current_vessel = K2D2_Plugin.Instance.current_vessel;

            Instance = this;
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
            float gravity_throttle = 0;

            if (gravity_compensation)
            {
                float minimum_dv = gravity_direction_factor * gravity;
                gravity_throttle = minimum_dv / burn_dV.full_dv;
            }

            delta_speed = settings.wanted_speed - current_speed;

            double full_dv_in_direction = burn_dV.full_dv*delta_angle_ratio;

            float remaining_full_burn_time = (float)(delta_speed / full_dv_in_direction);
            wanted_throttle = Mathf.Clamp(remaining_full_burn_time + gravity_throttle, 0, 1);
        }

        float delta_speed = 0;

        float delta_angle;
        float delta_angle_ratio;

        string status_line;

        public bool checkBurnDirection()
        {
            var telemetry = SASInfos.getTelemetry();
            Vector retro_dir = default(Vector);

            switch(settings.direction)
            {
                case Direction.SurfaceSpeed:
                    retro_dir = telemetry.SurfaceMovementRetrograde;
                    break;
                case Direction.SurfaceUp:
                    retro_dir = telemetry.HorizonUp;
                    break;
            }

            Rotation vessel_rotation = current_vessel.GetRotation();

            // convert rotation to maneuvre coordinates
            vessel_rotation = Rotation.Reframed(vessel_rotation, retro_dir.coordinateSystem);
            Vector3d forward_direction = (vessel_rotation.localRotation * Vector3.up).normalized;

            delta_angle = (float) Vector3d.Angle(retro_dir.vector, forward_direction);
            delta_angle_ratio = Mathf.Cos(delta_angle* Mathf.Deg2Rad);
            if (delta_angle < max_DiffDirection)
                return true;
            else
            {
                status_line = $"Waiting for good sas direction\nAngle = {delta_angle:n2}° > {max_DiffDirection:n2}°";
                return false;
            }
        }

        float current_speed;

        public override void Update()
        {
            if (!ui_visible && !isActive) return;
            if (current_vessel == null || current_vessel.VesselVehicle == null)
                return;

            V_Speed = (float)current_vessel.VesselVehicle.VerticalSpeed;
            if (!isActive) return;

            var autopilot = current_vessel.Autopilot;

            // force autopilot
            autopilot.Enabled = true;
            switch(settings.direction)
            {
                case Direction.SurfaceSpeed:
                    gravity_compensation = true;
                    current_vessel.SetSpeedMode(KSP.Sim.SpeedDisplayMode.Surface);
                    current_speed = -(float)current_vessel.VesselVehicle.SurfaceSpeed;
                    if (autopilot.AutopilotMode != AutopilotMode.Retrograde)
                        autopilot.SetMode(AutopilotMode.Retrograde);

                    max_DiffDirection = 20;

                    break;
                case Direction.SurfaceUp:
                    gravity_compensation = true;
                    current_vessel.SetSpeedMode(KSP.Sim.SpeedDisplayMode.Surface);
                    current_speed = (float)current_vessel.VesselVehicle.VerticalSpeed;
                    if (autopilot.AutopilotMode != AutopilotMode.RadialOut)
                        autopilot.SetMode(AutopilotMode.RadialOut);

                    max_DiffDirection = 85;
                    break;
            }

            if (gravity_compensation)
                computeGravityRatio();

            if (!checkBurnDirection())
            {
                current_vessel.SetThrottle(0);
                status_line = $"Turning : {delta_angle:n2} °";
                return;
            }

            compute_Throttle();
            status_line = $"Max Speed : {settings.wanted_speed:n2} m/s";

            // no stop for gravity compensation
            current_vessel.SetThrottle(wanted_throttle);

            base.Update();
        }


        public override void onGUI()
        {
            if (K2D2_Plugin.Instance.settings_visible)
            {
                Settings.onGUI();
                settings.onGUI();
                return;
            }

            UI_Tools.Title("Vertical Speed controller");
            settings.direction = UI_Tools.EnumGrid<Direction>("Direction",
                                                    settings.direction, direction_labels);

            if (settings.direction == Direction.SurfaceSpeed)
            {
                settings.wanted_speed = Mathf.Min(settings.wanted_speed, -0.5f);
                settings.wanted_speed = UI_Tools.FloatSlider("Speed ", settings.wanted_speed, -settings.speed_limit, -0.5f, "m/s");
                UI_Tools.Right_Left_Text($"{-settings.speed_limit:n2}", $"{-0.5f:n2}");
            }
            else
            {
                settings.wanted_speed = UI_Tools.FloatSlider("Speed ", settings.wanted_speed, -settings.speed_limit, settings.speed_limit, "m/s");
                UI_Tools.Right_Left_Text($"{-settings.speed_limit:n2}", $"{settings.speed_limit:n2}");
            }

            if (Mathf.Abs(settings.wanted_speed) < 0.3f)
                settings.wanted_speed = 0;

            isActive = UI_Tools.ToggleButton(isActive, "Run", "Stop");

            if (!isActive)
                return;

             // need to burn !

            UI_Tools.Console($"V Speed  : {V_Speed:n2} m/s");

            // UI_Tools.Console($"Wanted speed : {wanted_speed:n2} !!");
            UI_Tools.Console($"delta speed  : {delta_speed:n2}  m/s");
            GUI.color = Color.white;


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

            if (!string.IsNullOrEmpty(status_line))
            {
                GUILayout.Label(status_line);
            }
        }

    

       

    }
}