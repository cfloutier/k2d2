
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

        public LandingController()
        {
            Instance = this;
            logger.LogMessage("LandingController !");
            current_vessel = K2D2_Plugin.Instance.current_vessel;
        }

        bool land_controler_active = false;

        public override void Update()
        {
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
                return;
            }
        }

        public float wanted_speed;
        public float wanted_speed_10;
        public float wanted_speed_100;

        public float retrograde_angle;

        public bool checkManeuvreDirection()
        {
            double max_angle = 1;

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

        public override void onGUI()
        {
            GUILayout.Label("// Landing ", Styles.title);

            land_controler_active = UI_Tools.ToggleButton(land_controler_active, "Active");

            wanted_speed_10 = UI_Tools.FloatSlider(" 10 : ", wanted_speed_10, 0, 10);
            wanted_speed_100 = UI_Tools.FloatSlider(" 100 : ", wanted_speed_100, 0, 100);

            wanted_speed = wanted_speed_10 + wanted_speed_100;

            GUILayout.Label($"limit speed : {wanted_speed:n2}", Styles.title);
            GUILayout.Label($"angle : {retrograde_angle} °", Styles.title);
        }
    }
}