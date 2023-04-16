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
    public class AutoLiftController : ComplexControler
    {
        public static AutoLiftController Instance { get; set; }

        KSPVessel current_vessel;

        float elevation;
        float heading = 90;

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

                    OnStartController();
                }
            }
        }

        void OnStartController()
        {
            var autopilot = current_vessel.Autopilot;

            // force autopilot
            autopilot.Enabled = true;
            autopilot.SetMode(AutopilotMode.StabilityAssist);

            elevation = -90;
            
        }

        public AutoLiftController()
        {
            // logger.LogMessage("LandingController !");
            current_vessel = K2D2_Plugin.Instance.current_vessel;

            Instance = this;
        }

         Vector3d direction = Vector3d.zero;

        public void applyDirection()
        {
            var autopilot = current_vessel.Autopilot;

            // force autopilot
            autopilot.Enabled = true;

            var telemetry = SASInfos.getTelemetry();

            var up = telemetry.HorizonUp;

            direction = QuaternionD.Euler(-elevation, heading, 0) * Vector3d.forward;

            Vector direction_vector = new Vector(up.coordinateSystem,  direction);

            autopilot.SAS.lockedMode = false;
            autopilot.SAS.SetTargetOrientation(direction_vector, false);
        }

        double altitude = 0;

        int startAltitude = 2000;
        int rot_1_altitude = 10000;
        int rot_1_direction = 80;

        int rot_2_altitude = 15000;
        int rot_2_direction = 45;

        int rot_3_altitude = 55000;
        int rot_3_direction = 10;


        public override void Update()
        {
            if (!isActive) return;
            if (current_vessel == null) return;

            altitude = current_vessel.GetSeaAltitude();

            if (altitude < startAltitude)
            {
                elevation = 90;
            }
            else if (altitude < rot_1_altitude)
            {
                var ratio = Mathf.InverseLerp((float)startAltitude, (float)rot_1_altitude, (float)altitude);
                elevation = Mathf.Lerp(90, rot_1_direction, ratio);
            }
            else if (altitude < rot_2_altitude)
            {
                var ratio = Mathf.InverseLerp((float)rot_1_altitude, (float)rot_2_altitude, (float)altitude);
                elevation = Mathf.Lerp(rot_1_direction, rot_2_direction, ratio);
            }
            else if (altitude < rot_3_altitude)
            {
                var ratio = Mathf.InverseLerp( (float)rot_2_altitude, (float)rot_3_altitude, (float)altitude);
                elevation = Mathf.Lerp(rot_2_direction, rot_3_direction, ratio);
            }
            else
                elevation = 0;

            applyDirection();
        }


        public override void onGUI()
        {
            if (K2D2_Plugin.Instance.settings_visible)
            {
                Settings.onGUI();

                startAltitude = UI_Fields.IntField("lift.startAltitude", "start Alt.", startAltitude, 0, Int32.MaxValue);
                rot_1_altitude = UI_Fields.IntField("lift.rot_1_altitude", "Alt. 1", rot_1_altitude, 0, Int32.MaxValue);
                rot_1_direction = UI_Fields.IntField("lift.rot_1_direction", "Dir. 1", rot_1_direction, 0, 90);
                rot_2_altitude = UI_Fields.IntField("lift.rot_2_altitude", "Alt. 2", rot_2_altitude, 0, Int32.MaxValue);
                rot_2_direction = UI_Fields.IntField("lift.rot_2_direction", "Dir. 2", rot_2_direction, 0, 90);
                rot_3_altitude = UI_Fields.IntField("lift.rot_3_altitude", "Alt. 3", rot_3_altitude, 0, Int32.MaxValue);
                rot_3_direction = UI_Fields.IntField("lift.rot_3_direction", "Dir. 3", rot_3_direction, 0, 90);
                return;
            }

            heading = UI_Tools.FloatSlider("heading", heading, -180, 180, "°");

            isActive = UI_Tools.ToggleButton(isActive, "Start", "Stop");

            UI_Tools.Console($"Altitude = {altitude:n2} m");
            UI_Tools.Console($"elevation = {elevation:n2} °");
        }
    }
}