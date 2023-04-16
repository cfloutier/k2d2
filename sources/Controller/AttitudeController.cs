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
    public class AttitudeController : ComplexControler
    {
        public static AttitudeController Instance { get; set; }

        KSPVessel current_vessel;

        float x_direction;
        float y_direction;
        float z_direction;

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
                }
            }
        }

        public AttitudeController()
        {
            // logger.LogMessage("LandingController !");
            current_vessel = K2D2_Plugin.Instance.current_vessel;

            Instance = this;
        }

        Vector3d direction = Vector3d.zero;

        public override void Update()
        {
            if (!isActive) return;

            if (current_vessel == null) return;
            var autopilot = current_vessel.Autopilot;

            // force autopilot
            autopilot.Enabled = true;

            var telemetry = SASInfos.getTelemetry();

            var up = telemetry.HorizonUp;

            direction = QuaternionD.Euler(x_direction, y_direction, z_direction) * Vector3d.up;



            Vector direction_vector = new Vector(up.coordinateSystem,  direction); 

            
            autopilot.SAS.lockedMode = lockedMode;
            autopilot.SAS.SetTargetOrientation(direction_vector, reset);

            // autopilot.SetMode(AutopilotMode.Maneuver);
        }


        bool lockedMode = false;
        bool reset = false;

        public override void onGUI()
        {
            if (K2D2_Plugin.Instance.settings_visible)
            {
                Settings.onGUI();
                return;
            }

            lockedMode = UI_Tools.Toggle(lockedMode, "Locked Mode");
            reset = UI_Tools.Toggle(reset, "Reset");


            x_direction = UI_Tools.FloatSlider("X", x_direction, -180, 180, "°");
            y_direction = UI_Tools.FloatSlider("Y", y_direction, -180, 180, "°");
            z_direction = UI_Tools.FloatSlider("Z", z_direction, -180, 180, "°");

            isActive = UI_Tools.ToggleButton(isActive, "Start", "Stop");

            var telemetry = SASInfos.getTelemetry();

            var up = telemetry.HorizonUp;
            UI_Tools.Label($"up dir = {StrTool.VectorToString(up.vector)}");
            UI_Tools.Label($"up coor = {up.coordinateSystem}");
            UI_Tools.Label($"wanted dir = {StrTool.VectorToString(direction)}");

        }


    }
}