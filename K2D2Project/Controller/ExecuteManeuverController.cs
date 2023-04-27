
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using KSP.Sim.Maneuver;
using KSP.Messages;
using BepInEx.Logging;


using K2D2.KSPService;
using KSP.Sim;

namespace K2D2.Controller
{
    public class ExecuteSettings
    {

       public bool show_node_infos
        {
            get => Settings.s_settings_file.GetBool("land.show_node_infos", true);
            set {
                // value = Mathf.Clamp(value, 0 , 1);
                Settings.s_settings_file.SetBool("land.show_node_infos", value);
                }
        }

        public void settings_UI()
        {
            UI_Tools.Title("// Execute");
            show_node_infos = UI_Tools.Toggle(show_node_infos, "Show Nodes Infos");
        }
    }


    public class AutoExecuteManeuver : ComplexControler
    {
        public ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.LandingController");

        public static AutoExecuteManeuver Instance { get; set; }

        public ManeuverNodeData current_maneuvre_node = null;


        ExecuteSettings execute_settings = new ExecuteSettings();

        // Sub Pilots
        TurnTo turn;
        WarpTo warp;
        BurnManeuvre burn;

     //  ExecuteController current_pilot = null;
        KSPVessel current_vessel;

        public SingleExecuteController current_executor = new SingleExecuteController();

        public AutoExecuteManeuver()
        {

            Instance = this;
            sub_contollers.Add(current_executor);
            current_vessel = K2D2_Plugin.Instance.current_vessel;

            GeneralTools.Game.Messages.Subscribe<VesselChangedMessage>(OnActiveVesselChanged);

            turn = new TurnTo();
            warp = new WarpTo();
            burn = new BurnManeuvre();
        }

        public void OnActiveVesselChanged(MessageCenterMessage msg)
        {
            Stop();
        }

        public enum Mode
        {
            Off,
            Turn,
            Warp,
            Burn
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
                current_executor.setController( null );
                return;
            }

            switch (mode)
            {
                case Mode.Off:
                    current_executor.setController( null );
                    break;
                case Mode.Turn:
                    current_executor.setController( turn );
                    turn.StartManeuver(current_maneuvre_node);
                    break;
                case Mode.Warp:
                    current_executor.setController( warp );
                    warp.StartManeuver(current_maneuvre_node);
                    break;
                case Mode.Burn:
                    current_executor.setController( burn );
                    burn.StartManeuver(current_maneuvre_node);
                    break;
            }

            logger.LogInfo("setMode " + mode);
        }

        public bool canStart()
        {
            if (current_maneuvre_node == null)
                return false;

            var dt = GeneralTools.remainingStartTime(current_maneuvre_node);
            if (dt < 0)
            {
                return false;
            }

            return true;
        }

        public void nextMode()
        {
            if (mode == Mode.Off)
            {
                Start();
                return;
            }
            if (mode == Mode.Burn)
            {
                Stop();
                return;
            }

            var next = this.mode + 1;
            setMode(next);
        }

        public override void onGUI()
        {
            if (K2D2_Plugin.Instance.settings_visible)
            {
                // default Settings UI
                Settings.onGUI();
                // 
                settingsUI();
                return;
            }

            if (current_maneuvre_node == null)
            {
                UI_Tools.Label("no Maneuvre node");
                return;
            }

            if (!valid_maneuver)
            {
                UI_Tools.Label("Invalid Maneuvre node.");
                UI_Tools.Console("Actually a KSP2 bug when loading scenaries. Please open map to fix it");
                return;
            }


            if (execute_settings.show_node_infos)
            {
                node_infos();
            }


            if (!canStart())
            {
                UI_Tools.Label("No Maneuver node in the future");
                return;
            }

            isActive = UI_Tools.ToggleButton(isActive, "Run", "Stop");

            current_executor.onGUI();
            if (!Settings.auto_next)
            {
                UI_Tools.Label($"finished {current_executor.finished}");
                if (!current_executor.finished)
                {
                    if (UI_Tools.Button("Next /!\\"))
                        nextMode();
                }
                else
                {
                    if (UI_Tools.Button("Next"))
                        nextMode();
                }
            }
        }

        public bool valid_maneuver = false;

        public bool checkManeuver()
        {
            current_maneuvre_node = current_vessel.GetNextManeuveurNode();
            valid_maneuver = false;
            if (current_maneuvre_node == null)
            {
                Stop();
                return false;
            }

            double ut;

            var plan_solver = current_vessel.GetPlanSolver();
            if (plan_solver == null)
            {
                Stop();
                return false;
            }

            // check that the maneuver is well declared.
            Vector velocity_after_maneuver = plan_solver.GetVelocityAfterFirstManeuver(out ut);
            if (ut == 0)
            {
                // error
                Stop();
                return false;
            }

            valid_maneuver = true;
            return true;
        }

        public override void Update()
        {

            checkManeuver();

            base.Update();

            if (current_executor.finished && Settings.auto_next)
            {
                // auto next
                nextMode();
            }
        }

        void settingsUI()
        {
            if (Settings.debug_mode)
            {
                Settings.auto_next = UI_Tools.Toggle(Settings.auto_next, "Auto Next Phase", "Debug Mode : Need to press next");
            }
            else
                Settings.auto_next = true;

            execute_settings.settings_UI();

            WarpToSettings.onGUI();
            BurnManeuvreSettings.ui();
        }

        void node_infos()
        {
            UI_Tools.Title("// Node Infos");
            var dt = GeneralTools.remainingStartTime(current_maneuvre_node);
            UI_Tools.Label($"Node in <b>{StrTool.DurationToString(dt)}</b>");
            UI_Tools.Label($"dV {current_maneuvre_node.BurnRequiredDV:n2} m/s");
            UI_Tools.Label($"Duration {StrTool.DurationToString(current_maneuvre_node.BurnDuration)}");

            if (Settings.debug_mode)
            {
                if (dt < 0)
                {
                    UI_Tools.Label("In The Past");
                    return;
                }
            }
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
                    // reset controller to desactivate other controllers.
                    K2D2_Plugin.ResetControllers();
                    _active = true;
                    setMode(Mode.Turn);
                }
            }
        }

        public void Start()
        {
            isActive = true;
        }

        public void Stop()
        {
            isActive = false;
        }

        public override void onReset()
        {
            Stop();
        }

    }
}