
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using BepInEx.Logging;
using K2D2.KSPService;
using KSP.Sim;
using KSP.Sim.impl;
using SpaceGraphicsToolkit;

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

        public void settings_UI()
        {
           UI_Tools.Console("Try to compensate gravity by adding dv to needed burn");
           gravity_compensation = GUILayout.Toggle(gravity_compensation, "Gravity Compensation");
        }

        // always visible
        public void control_UI()
        {
            UI_Tools.Title("// Controls");
            auto_speed = GUILayout.Toggle(auto_speed, "Auto Speed");
            if (auto_speed)
            {
                UI_Tools.Console("speed is based on altitude");
                auto_speed_ratio = UI_Tools.FloatSlider("Altitude/speed ratio", auto_speed_ratio, 0.5f, 3);
                UI_Tools.Right_Left_Text("Safe", "Danger");
                safe_speed = UI_Tools.FloatSlider("Landing speed", safe_speed, 0.1f, 10);
            }
            else
            {
                UI_Tools.Console("speed is directly assigned");
                string[] multiplier_txt = {"x10", "x100", "x1000"};
                GUILayout.BeginHorizontal();
                multiplier_index = GUILayout.SelectionGrid(multiplier_index, multiplier_txt, 3);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                speed_ratio = GUILayout.HorizontalSlider(speed_ratio, 0, 1);
            }
        }
    }

    public class LandingController : ComplexControler
    {
        public ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.LandingController");

        LandingSettings land_settings = new LandingSettings();

        public static LandingController Instance { get; set; }

        public KSPVessel current_vessel;

        public BurndV burn_dV = new BurndV();



        public BrakeController brake = new BrakeController();


        public SingleSubControler sub = new SingleSubControler();


        public LandingController()
        {
            Instance = this;
            sub_contollers.Add(burn_dV);
            sub_contollers.Add(sub);

            // logger.LogMessage("LandingController !");
            current_vessel = K2D2_Plugin.Instance.current_vessel;
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

                    _active = false;
                }
                else
                {
                    // Start total burn counter
                    burn_dV.reset();
                    
                    // reset controller to desactivate other controllers.
                    K2D2_Plugin.ResetControllers();
                    _active = true;
                    //sub.sub_controler = 
                }
            }
        }

        public override void onReset()
        {
            ControlerActive = false;
        }

        string status_line = "";

        float current_speed = 0;

        public override void Update()
        {
            if (current_vessel == null || current_vessel.VesselVehicle == null)
                return;

            altitude = (float)current_vessel.GetDisplayAltitude();

            if (land_settings.auto_speed)
            {
                float div = 10;
                limit_speed = altitude * land_settings.auto_speed_ratio / div + land_settings.safe_speed;
            }
            else
            {
                limit_speed = Mathf.Pow(10, land_settings.multiplier_index+1) * land_settings.speed_ratio;
            }

            current_speed = (float)current_vessel.VesselVehicle.SurfaceSpeed;

            if (!ControlerActive)
                return;

            if (altitude < 5 && current_speed < 1)
            {
                // stop
                status_line = "Landed";
                //current_vessel.SetThrottle(0);
                ControlerActive = false;
                return;
            }

            brake.wanted_speed = limit_speed;
            brake.gravity_compensation = land_settings.gravity_compensation;
            // call the brake
            base.Update();
        }

        public float limit_speed;
        public float altitude;

        public void context_infos()
        {
            var current_vessel = K2D2_Plugin.Instance.current_vessel;
            if (current_vessel == null)
            {
                UI_Tools.Console("no vessel");
                return;
            }

            PatchedConicsOrbit orbit = current_vessel.VesselComponent.Orbit;

            UI_Tools.Console($"Altitude : {GeneralTools.DistanceToString(altitude)}");
            UI_Tools.Console($"Current Speed : {current_speed:n2} m/s");

            if (orbit.PatchEndTransition == PatchTransitionType.Collision)
            {
                var dt = GeneralTools.Game.UniverseModel.UniversalTime - orbit.collisionPointUT;
                UI_Tools.Console($"collision in  {GeneralTools.DurationToString( orbit.collisionPointUT)}");

                var speed_collision = orbit.GetOrbitalVelocityAtUTZup(orbit.collisionPointUT).magnitude;
                UI_Tools.Console($"speed collision  { speed_collision:n2} m/s");

                var burn_duration = (speed_collision / burn_dV.full_dv);
                UI_Tools.Console($"burn_duration { burn_duration:n2} s");
            }
        }

        public override void onGUI()
        {
            if (K2D2_Plugin.Instance.settings_visible)
            {
                SettingsUI.onGUI();
                land_settings.settings_UI();
                return;
            }

            land_settings.control_UI();

            context_infos();

            brake.onGUI();

            if (!ControlerActive && brake.NeedBurn)
            {
                GUI.color = Color.red;
            }

            ControlerActive = UI_Tools.ToggleButton(ControlerActive, "Start", "Stop");
            GUI.color = Color.white;

            if (!string.IsNullOrEmpty(status_line))
                UI_Tools.Console(status_line);
        }
    }
}