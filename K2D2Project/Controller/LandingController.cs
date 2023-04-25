
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
    public class LandingSettings
    {
        public bool auto_warp
        {
            get => Settings.s_settings_file.GetBool("land.auto_warp", true);
            set {
                // value = Mathf.Clamp(value, 0 , 1);
                Settings.s_settings_file.SetBool("land.auto_warp", value);
                }
        }

        public float burn_before
        {
            get => Settings.s_settings_file.GetFloat("land.burnBefore", 1f);
            set
            {
                // value = Mathf.Clamp(value, 0 , 1);
                Settings.s_settings_file.SetFloat("land.burnBefore", value);
            }
        }

        // Warp with check of rotation
        public int rotation_warp_duration
        {
            get => Settings.s_settings_file.GetInt("land.rotation_warp_duration", 60);
            set {
                Settings.s_settings_file.SetInt("land.rotation_warp_duration", value);
                }
        }

        public float max_rotation
        {
            get => Settings.s_settings_file.GetFloat("land.max_rotation", 30);
            set {
                Settings.s_settings_file.SetFloat("land.max_rotation", value);
                }
        }


        public float start_touchdown_altitude
        {
            get => Settings.s_settings_file.GetFloat("land.touch_down_altitude", 500);
            set {
                value = Mathf.Clamp(value, 0 , 30000);
                Settings.s_settings_file.SetFloat("land.touch_down_altitude", value);
                }
        }

        public float touch_down_ratio
        {
            get => Settings.s_settings_file.GetFloat("land.touch_down_ratio", 0.5f);
            set {
                // value = Mathf.Clamp(value, 0 , 1);
                Settings.s_settings_file.SetFloat("land.touch_down_ratio", value);
                }
        }

        public float touch_down_speed
        {
            get => Settings.s_settings_file.GetFloat("land.touch_down_speed", 4);
            set {
                value = Mathf.Clamp(value, 0 , 100);
                Settings.s_settings_file.SetFloat("land.touch_down_speed", value);
                }
        }

        public void settings_UI()
        {
        //    UI_Tools.Console("Try to compensate gravity by adding dv to needed burn ");
        //    gravity_compensation = UI_Tools.Toggle(gravity_compensation, "Gravity Compensation");

            UI_Tools.Title("// Landing Settings");
            burn_before = UI_Tools.FloatSlider("Burn Before", burn_before, 0, 10, "s");
            // UI_Tools.Console($"(Safe time before burn)");

          

            auto_warp = UI_Tools.Toggle(auto_warp, "Auto Time-Warp");

            if (auto_warp)
            {
                WarpToSettings.onGUI();


                if (Settings.debug_mode)
                {
                    UI_Tools.Title("// Rotation Warp");
                    rotation_warp_duration = UI_Fields.IntField("rotation_warp_duration", "Rot. Warp Duration", rotation_warp_duration, 0, int.MaxValue,
                    "During Rotation Warp, Attitude is checked");
                    max_rotation = UI_Tools.FloatSlider("Safe Warp Rotation", max_rotation, 0, 90, "°", "Max angle (stop warp when reached)");
                }
                else
                {
                    rotation_warp_duration = 0;
                }

            }

            UI_Tools.Title("// Touch Down");
            start_touchdown_altitude = UI_Tools.FloatSlider("Start TouchDown Altitude", start_touchdown_altitude, 100, 5000, "m", "Altitude for starting Touch-Down Phase");

            touch_down_ratio = UI_Tools.FloatSlider("Altitude/speed ratio", touch_down_ratio, 0.5f, 3, "", "Speed is based on altitude");
            UI_Tools.Right_Left_Text("Safe", "Danger");

            touch_down_speed = UI_Tools.FloatSlider("Touch-Down speed", touch_down_speed, 0.1f, 10, "m/s", "Speed when touching ground");
        }

        public float compute_limit_speed(float altitude)
        {
            // just to have understandable settings (not 0.1)
            float div = 10;
            return altitude * touch_down_ratio / div + touch_down_speed;
        }
    }

    public class LandingController : ComplexControler
    {
        public ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.LandingController");

        LandingSettings land_settings = new LandingSettings();

        public static LandingController Instance { get; set; }

        public KSPVessel current_vessel;

        public BurndV burn_dV = new BurndV();

        public WarpTo warp_to = new WarpTo();

        public BrakeController brake = new BrakeController();

        public SingleExecuteController current_executor = new SingleExecuteController();

        public LandingController()
        {
            Instance = this;
            sub_contollers.Add(burn_dV);
            sub_contollers.Add(current_executor);

            // logger.LogMessage("LandingController !");
            current_vessel = K2D2_Plugin.Instance.current_vessel;
        }


        public enum Mode
        {
            Off,
            QuickWarp,
            RotationWarp,
            Waiting,
            Brake,
            TouchDown
        }

        public Mode mode = Mode.Off;

        public void setMode(Mode mode)
        {
            if (mode == this.mode)
                return;

            logger.LogInfo("setMode " + mode);

            this.mode = mode;

            if (mode == Mode.Off)
            {
                TimeWarpTools.SetRateIndex(0, false);
                current_executor.setController(null);
                return;
            }
            switch (mode)
            {
                case Mode.Off:
                    current_executor.setController(null);
                    break;
                case Mode.QuickWarp:
                    current_vessel.SetThrottle(0);
                    if (!land_settings.auto_warp)
                        setMode(Mode.Waiting);
                    else
                    {
                        current_executor.setController(warp_to);
                        warp_to.Start_UT(startSafeWarp_UT);
                    }
                    break;
                case Mode.RotationWarp:
                    current_vessel.SetThrottle(0);
                    if (!land_settings.auto_warp)
                        setMode(Mode.Waiting);
                    else
                    {
                        current_executor.setController(warp_to);
                        warp_to.Start_UT(startBurn_UT, true);
                    }
                    break;
                case Mode.Waiting:
                    current_vessel.SetThrottle(0);
                    current_executor.setController(null);
                    break;
                case Mode.Brake:
                case Mode.TouchDown:
                    current_executor.setController(brake);
                    break;
            }

            logger.LogInfo("current_pilot " + mode);
        }

        public void nextMode()
        {
            // start
            if (mode == Mode.Off)
            {
                isActive = true;
                return;
            }

            var next = this.mode + 1;
            setMode(next);
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
                    if (current_vessel != null)
                        current_vessel.SetThrottle(0);

                    setMode(Mode.Off);
                    _active = false;
                }
                else
                {
                    // Start total burn counter
                    burn_dV.reset();

                    // reset controller to desactivate other controllers.
                    K2D2_Plugin.ResetControllers();
                    _active = true;
                    setMode(Mode.QuickWarp);
                }
            }
        }

        public override void onReset()
        {
            isActive = false;
        }

        float current_speed = 0;

        bool collision_detected = false;

        double adjusted_collision_UT = 0;
        double startBurn_UT = 0;
        double startSafeWarp_UT = 0;
        double speed_collision;
        double burn_duration;

        public void computeValues()
        {
            collision_detected = false;
            var current_vessel = K2D2_Plugin.Instance.current_vessel;
            if (current_vessel == null)
            {
               // UI_Tools.Console("no vessel");
                return;
            }

            PatchedConicsOrbit orbit = current_vessel.VesselComponent.Orbit;

            collision_detected = compute_real_collision();
            speed_collision = orbit.GetOrbitalVelocityAtUTZup(adjusted_collision_UT).magnitude;
            burn_duration = (speed_collision / burn_dV.full_dv);

            compute_startBurn();
        }

        public void compute_startBurn()
        {
            startBurn_UT = adjusted_collision_UT - burn_duration - land_settings.burn_before;
            startSafeWarp_UT = startBurn_UT - land_settings.rotation_warp_duration;
        }

        public bool compute_real_collision()
        {
             // start in 2 minutes
            double start_time = GeneralTools.Game.UniverseModel.UniversalTime + 2*60;
            bool collide = false;

            PatchedConicsOrbit orbit = current_vessel.VesselComponent.Orbit;
            var body = orbit.referenceBody;
            double current_time_ut = GeneralTools.Game.UniverseModel.UniversalTime;
            double deltaTime = 60; // seconds in the future
            int max_occurrences = 100;
            double time = start_time;
            double terrainAltitude;

            float radius = current_vessel.VesselComponent.SimulationObject.objVesselBehavior.BoundingSphere.radius;

            for (int i = 0; i < max_occurrences; i++)
            {
                Vector3d pos;
                Vector ve;

                orbit.GetStateVectorsFromUT(time, out pos, out ve);

                Position ps = new Position(ve.coordinateSystem, pos);


                double sceneryOffset;

                body.GetAltitudeFromTerrain(ps, out terrainAltitude, out sceneryOffset);
                // terrainAltitude -= radius;

                if (terrainAltitude < 0)
                {
                    collide = true;
                    if (deltaTime > 0)
                    {
                        // dychotomy
                        deltaTime = -deltaTime / 2;
                    }
                    time += deltaTime;
                }
                else
                {
                    if (deltaTime < 0)
                    {
                        // dychotomy
                        deltaTime = -deltaTime / 2;
                    }
                    time += deltaTime;
                }

                if (Math.Abs(terrainAltitude) < 1)
                {
                    break;
                }
            }

            adjusted_collision_UT = time;
            return collide;
        }

        public override void Update()
        {
            if (!ui_visible && !isActive) return;
            if (current_vessel == null || current_vessel.VesselVehicle == null)
                return;

            altitude = (float)current_vessel.GetApproxAltitude();
            current_speed = (float)current_vessel.VesselVehicle.SurfaceSpeed;
            // detect collision
            computeValues();

            if (!collision_detected)
            {
                // after patch ksp detect with altitude and remove the collision point
                if (isActive)
                {
                    setMode(Mode.TouchDown);
                }
                else
                {
                    // no more collision
                    isActive = false;
                }
            }
            if (!isActive)
                return;

            // landing detection....
            if (altitude < 5 && current_speed < 1)
            {
                //current_vessel.SetThrottle(0);
                isActive = false;
                return;
            }
            if (mode == Mode.QuickWarp)
            {
                warp_to.UT = startSafeWarp_UT;
            }
            else if (mode == Mode.RotationWarp)
            {
                warp_to.UT = startBurn_UT;
            }
            else if (mode == Mode.Waiting)
            {
                var dt = startBurn_UT - GeneralTools.Game.UniverseModel.UniversalTime; 
                if (dt <= 0)
                {
                    nextMode();
                    return;
                }
            }
            else if (mode == Mode.Brake )
            {
                brake.max_speed = 0;
                brake.gravity_compensation = true;
                if (current_speed < 30)
                {
                    if (altitude < land_settings.start_touchdown_altitude)
                    {
                        setMode(Mode.TouchDown);
                    }
                    else
                    {
                        // high altitude retry
                        setMode(Mode.QuickWarp);
                    }
                    return;
                }
            }
            else if (mode == Mode.TouchDown )
            {
                TimeWarpTools.SetRateIndex(0, false);
                brake.max_speed = land_settings.compute_limit_speed(altitude);
                brake.gravity_compensation = true;
            }

            // call the sub controllers
            base.Update();

            if (current_executor.finished)
            {
                // auto next
                nextMode();
            }
        }

        public float altitude;

        public void context_infos()
        {
            // UI_Tools.Console($"Altitude : {StrTool.DistanceToString(altitude)}");
            // UI_Tools.Console($"Current Speed : {current_speed:n2} m/s");

            if (collision_detected)
            {
                UI_Tools.Title("Collision detected !");

                //UI_Tools.Console($"collision in  {StrTool.DurationToString(collision_UT - GeneralTools.Game.UniverseModel.UniversalTime)}");
                UI_Tools.Label($"Collision in {StrTool.DurationToString(adjusted_collision_UT - GeneralTools.Game.UniverseModel.UniversalTime)}");
                UI_Tools.Label($"speed collision {speed_collision:n2} m/s");
                UI_Tools.Label($"start_burn in <b>{StrTool.DurationToString(startBurn_UT - GeneralTools.Game.UniverseModel.UniversalTime)}</b>");
                UI_Tools.Label($"burn_duration {burn_duration:n2} s");
            }
            else
            {
                UI_Tools.Title("No Collision detected");
            }
        }

        public override void onGUI()
        {
            if (K2D2_Plugin.Instance.settings_visible)
            {
                Settings.onGUI();
                land_settings.settings_UI();
                return;
            }

            context_infos();

            isActive = UI_Tools.ToggleButton(isActive, "Start", "Stop");
            if (isActive)
            {
                switch (mode)
                {
                    default:
                    case Mode.Off:  break;
                    case Mode.QuickWarp:
                        UI_Tools.OK("Quick Warp"); 
                        break;
                    case Mode.RotationWarp:
                        UI_Tools.Warning("Rotating Warp");
                        break;
                    case Mode.Waiting:
                        UI_Tools.OK($"Waiting : {StrTool.DurationToString(startBurn_UT - GeneralTools.Game.UniverseModel.UniversalTime)}");
                        break;
                    case Mode.Brake:
                        UI_Tools.Warning($"Brake !");
                        break;
                    case Mode.TouchDown:
                        UI_Tools.Warning($"Touch Down...");
                        break;
                }

                if (current_executor != null && !string.IsNullOrEmpty(current_executor.status_line))
                    UI_Tools.Console(current_executor.status_line);
            }
            

            if (burn_dV.burned_dV > 0)
                UI_Tools.Console($"Burned : {burn_dV.burned_dV} m/s");

            // if (!Settings.auto_next)
            // {
            //     UI_Tools.Title($"finished {current_executor.finished}");
            //     if (!current_executor.finished)
            //     {
            //         if (GUILayout.Button("Next /!\\"))
            //             nextMode();
            //     }
            //     else
            //     {
            //         if (GUILayout.Button("Next"))
            //             nextMode();
            //     }
            // }

            
        }
    }
}