
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

    /// <summary>
    /// TODO : - better UI
    /// Detect landing
    /// Crash Time out
    /// print distance and speed
    /// print time depending on level
    /// Stop when speed is up !!
    /// </summary>
    public class LandingSettings
    {

        // public int multiplier_index
        // {
        //     get => Settings.s_settings_file.GetInt("land.multiplier_index", 0);
        //     set {             // value = Mathf.Clamp(0.1,)
        //         Settings.s_settings_file.SetInt("land.multiplier_index", value); }
        // }


        // public float speed_ratio
        // {
        //     get => Settings.s_settings_file.GetFloat("land.speed_ratio", 0);
        //     set {             // value = Mathf.Clamp(0.1,)
        //         Settings.s_settings_file.SetFloat("land.speed_ratio", value); }
        // }


        // public bool gravity_compensation
        //  {
        //     get => Settings.s_settings_file.GetBool("land.gravity_compensation", false);
        //     set {             // value = Mathf.Clamp(0.1,)
        //         Settings.s_settings_file.SetBool("land.gravity_compensation", value); }
        // }

        // public bool auto_speed
        // {
        //     get => Settings.s_settings_file.GetBool("land.auto_speed", false);
        //     set {
        //         Settings.s_settings_file.SetBool("land.auto_speed", value); }
        // }

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



        public bool suicide_burn
        {
            get => Settings.s_settings_file.GetBool("land.suicide_burn", false);
            set {
                Settings.s_settings_file.SetBool("land.suicide_burn", value);
            }
        }




        public void settings_UI()
        {
        //    UI_Tools.Console("Try to compensate gravity by adding dv to needed burn ");
        //    gravity_compensation = GUILayout.Toggle(gravity_compensation, "Gravity Compensation");

            UI_Tools.Title("// Landing Settings");
            //auto_speed = GUILayout.Toggle(auto_speed, "Auto Speed");
            auto_warp = UI_Tools.Toggle("Auto Time-Warp", auto_warp);
            suicide_burn = UI_Tools.Toggle("Suicide burn mode", suicide_burn);

            UI_Tools.Console("speed is based on altitude");
            touch_down_ratio = UI_Tools.FloatSlider("Altitude/speed ratio", touch_down_ratio, 0.5f, 3);
            UI_Tools.Right_Left_Text("Safe", "Danger");
            // float limit_speed = compute_limit_speed(100);
            // UI_Tools.Console($"ex : speed is limit at {limit_speed:n2} m/s for 100 meters altitude");

            touch_down_speed = UI_Tools.FloatSlider("Landing speed", touch_down_speed, 0.1f, 10);

            burn_before = UI_Tools.FloatSlider("Burn Before", burn_before, 0, 10);
            UI_Tools.Console($"(Safe time before burn)");



        /*    else
            {
                UI_Tools.Console("speed is directly assigned");
                string[] multiplier_txt = {"x10", "x100", "x1000"};
                GUILayout.BeginHorizontal();
                multiplier_index = GUILayout.SelectionGrid(multiplier_index, multiplier_txt, 3);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                speed_ratio = GUILayout.HorizontalSlider(speed_ratio, 0, 1);
            }*/
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
        public TurnTo turn = new TurnTo();
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
            Turn,
            TimeWarp,
            Waiting,
            Burn,
            Land
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
                case Mode.Turn:
                    current_executor.setController(turn);
                    turn.StartSurfaceRetroGrade();
                    break;
                case Mode.TimeWarp:
                    if (!land_settings.auto_warp)
                        setMode(Mode.Waiting);
                    else
                    {
                        current_executor.setController(warp_to);
                        warp_to.Start_UT(startBurn_UT);
                    }
                    break;
                case Mode.Waiting:
                    compute_startBurn();
                    current_executor.setController(null);
                    break;
                case Mode.Burn:
                case Mode.Land:
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
                ControlerActive = true;
                return;
            }

            var next = this.mode + 1;
            setMode(next);
        }


        bool _active = false;
        bool ControlerActive
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
                    setMode(Mode.Turn);
                }
            }
        }

        public override void onReset()
        {
            ControlerActive = false;
        }

        float current_speed = 0;

        bool is_falling = false;

        double collision_UT = 0;
        double adjusted_collision_UT = 0;
        double startBurn_UT = 0;
        double speed_collision;
        double burn_duration;

        public void computeValues()
        {
            is_falling = false;
            var current_vessel = K2D2_Plugin.Instance.current_vessel;
            if (current_vessel == null)
            {
               // UI_Tools.Console("no vessel");
                return;
            }

            PatchedConicsOrbit orbit = current_vessel.VesselComponent.Orbit;
            if (orbit.PatchEndTransition == PatchTransitionType.Collision)
            {
                is_falling = true;
                collision_UT = orbit.collisionPointUT;
                adjusted_collision_UT = compute_real_collision();

                //var dt = GeneralTools.Game.UniverseModel.UniversalTime - orbit.collisionPointUT;
                speed_collision = orbit.GetOrbitalVelocityAtUTZup(adjusted_collision_UT).magnitude;
                burn_duration = (speed_collision / burn_dV.full_dv);
                if (!ControlerActive)
                {
                    compute_startBurn();
                }
            }
        }

        public void compute_startBurn()
        {
            if (land_settings.suicide_burn)
            {
                // compute distance corrected by accelation
                var delta_move = speed_collision * burn_duration - 0.5f * burn_dV.full_dv * burn_duration * burn_duration;
                // correct time
                var delta_time = delta_move / speed_collision;

                startBurn_UT = adjusted_collision_UT - burn_duration - land_settings.burn_before + delta_time;
            }
            else
            {
                 startBurn_UT = adjusted_collision_UT - burn_duration - land_settings.burn_before;
            }
        }


        public double compute_real_collision()
        {
            PatchedConicsOrbit orbit = current_vessel.VesselComponent.Orbit;
            var body = orbit.referenceBody;
            double current_time_ut = GeneralTools.Game.UniverseModel.UniversalTime;
            double deltaTime = -10; // seconds in the past
            int max_occurrences = 100;
            double time = orbit.collisionPointUT;
            double terrainAltitude;

            for (int i = 0; i < max_occurrences; i++)
            {
                Vector3d pos;
                Vector ve;

                orbit.GetStateVectorsFromUT(time, out pos, out ve);

                Position ps = new Position(ve.coordinateSystem, pos);


               double sceneryOffset;

                body.GetAltitudeFromTerrain(ps, out terrainAltitude, out sceneryOffset);

                if (terrainAltitude < 0)
                {
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

            return time;
        }

        public override void Update()
        {
            if (current_vessel == null || current_vessel.VesselVehicle == null)
                return;

            altitude = (float)current_vessel.GetDisplayAltitude();
            current_speed = (float)current_vessel.VesselVehicle.SurfaceSpeed;

            computeValues();

            if (!is_falling)
            {
                ControlerActive = false;
                return;
            }
            if (!ControlerActive)
                return;

            // landing detection....
            if (altitude < 5 && current_speed < 1)
            {
                //current_vessel.SetThrottle(0);
                ControlerActive = false;
                return;
            }

            if (mode == Mode.TimeWarp)
            {
                warp_to.UT = startBurn_UT;

            }
            else if (mode == Mode.Waiting)
            {
                compute_startBurn();
                var dt = startBurn_UT - GeneralTools.Game.UniverseModel.UniversalTime; 
                if (dt <= 0 && Settings.auto_next)
                {
                    nextMode();
                    return;
                }
            }
            else if (mode == Mode.Burn )
            {
                brake.wanted_speed = 0;
                brake.gravity_compensation = true;
                if (current_speed < 30 && Settings.auto_next)
                {
                    nextMode();
                    return;
                }
            }
            else if (mode == Mode.Land )
            {
                brake.wanted_speed = land_settings.compute_limit_speed(altitude);;
                brake.gravity_compensation = true;
            }

            // call the sub controllers
            base.Update();

            if (current_executor.finished && Settings.auto_next)
            {
                // auto next
                nextMode();
            }
        }

        public float altitude;

        public void context_infos()
        {
            UI_Tools.Console($"Altitude : {StrTool.DistanceToString(altitude)}");
            UI_Tools.Console($"Current Speed : {current_speed:n2} m/s");

            if (is_falling)
            {
                UI_Tools.Title("Collision detected by KSP 2 Orbit");

                UI_Tools.Console($"collision in  {StrTool.DurationToString(collision_UT - GeneralTools.Game.UniverseModel.UniversalTime)}");
                UI_Tools.Console($"adjusted_collision in {StrTool.DurationToString(adjusted_collision_UT - GeneralTools.Game.UniverseModel.UniversalTime)}");
                
                UI_Tools.Console($"start_burn in  {StrTool.DurationToString(startBurn_UT - GeneralTools.Game.UniverseModel.UniversalTime)}");
                UI_Tools.Console($"speed collision  {speed_collision:n2} m/s");
                UI_Tools.Console($"burn_duration {burn_duration:n2} s");
            }
            else
            {
                UI_Tools.Title("No Collision detected by KSP 2 Orbit");
            }
        }

        public override void onGUI()
        {
            if (K2D2_Plugin.Instance.settings_visible)
            {
                SettingsUI.onGUI();
                land_settings.settings_UI();
                warp_to.setting_UI();
                return;
            }

            UI_Tools.Title(mode.ToString());

            context_infos();

            if (!ControlerActive && brake.NeedBurn)
            {
                GUI.color = Color.red;
            }

            ControlerActive = UI_Tools.ToggleButton(ControlerActive, "Start", "Stop");
            GUI.color = Color.white;
          
            if (!Settings.auto_next)
            {
                UI_Tools.Title($"finished {current_executor.finished}");
                if (!current_executor.finished)
                {
                    if (GUILayout.Button("Next /!\\"))
                        nextMode();
                }
                else
                {
                    if (GUILayout.Button("Next"))
                        nextMode();
                }
            }

            if (! string.IsNullOrEmpty(current_executor.status_line))
                UI_Tools.Console(current_executor.status_line);

            UI_Tools.Console($"burned : {burn_dV.burned_dV} m/s");
        }
    }
}