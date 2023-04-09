
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
    public class AutoExecuteManeuver : ComplexControler
    {
        public ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.LandingController");

        public static AutoExecuteManeuver Instance { get; set; }

        public ManeuverNodeData current_maneuvre_node = null;

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
                SettingsUI.onGUI();
                // 
                settingsUI();
                return;
            }
            
            if (current_maneuvre_node == null)
            {
                UI_Tools.Label("no Maneuvre node");
                return;
            }

            if (! valid_maneuver)
            {
                UI_Tools.Label("invalid Maneuvre node.");
                UI_Tools.Console("Actually a KSP2 big when loading scenaries. Please open map and adjust node");
                return;
            }

            if (mode == Mode.Off)
            {
                if (!canStart())
                {
                    UI_Tools.Label("no Maneuver node in the future");
                    return;
                }

                if (UI_Tools.BigButton("Run"))
                    Start();
            }
            else
            {
                if (UI_Tools.BigButton("Stop !!", true))
                    Stop();
            }

            node_infos();

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

        public override void onReset()
        {
            Stop();
        }

        bool valid_maneuver = false;

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

            WarpToSettings.ui();
            BurnManeuvreSettings.ui();
        }

        void node_infos()
        {
            if (Settings.debug_mode)
            {
                var dt = GeneralTools.remainingStartTime(current_maneuvre_node);
                UI_Tools.Label($"Node in {StrTool.DurationToString(dt)}");
                UI_Tools.Label($"BurnDuration {current_maneuvre_node.BurnDuration}");
                // UI_Tools.Label($"BurnRequiredDV {current_maneuvre_node.BurnRequiredDV}");
                // UI_Tools.Label($"BurnVector {StrTool.VectorToString(current_maneuvre_node.BurnVector)}");

                var telemetry = SASInfos.getTelemetry();

                Vector3 maneuvre_dir = telemetry.ManeuverDirection.vector;

                UI_Tools.Label($"maneuvre_dir {StrTool.VectorToString(maneuvre_dir)}");

                if (dt < 0)
                {
                    UI_Tools.Label("In The Past");
                    return;
                }
            }
        }

        public void Start()
        {
            K2D2_Plugin.ResetControllers();
            setMode(Mode.Turn);
        }

        public void Stop()
        {
            setMode(Mode.Off);
        }
    }
}