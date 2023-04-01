
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using KSP.Sim.Maneuver;
using KSP.Messages;
using BepInEx.Logging;


using K2D2.KSPService;

namespace K2D2.Controller
{
    public class AutoExecuteManeuver : BaseController
    {
        public ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.LandingController");

        public static AutoExecuteManeuver Instance { get; set; }

        public ManeuverNodeData current_maneuvre_node = null;

        // Sub Pilots
        TurnTo turn;
        WarpToManeuvre warp;
        BurnManeuvre burn;

        BaseController current_pilot = null;
        KSPVessel current_vessel;

        public AutoExecuteManeuver()
        {
            logger.LogMessage("AutoExecuteManeuver !");
            Instance = this;
            current_vessel = K2D2_Plugin.Instance.current_vessel;

            GeneralTools.Game.Messages.Subscribe<VesselChangedMessage>(OnActiveVesselChanged);

            turn = new TurnTo();
            warp = new WarpToManeuvre();
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
                current_pilot = null;
                return;
            }

            switch (mode)
            {
                case Mode.Off:
                    current_pilot = null;
                    break;
                case Mode.Turn:
                    current_pilot = turn;
                    turn.StartManeuver(current_maneuvre_node);
                    break;
                case Mode.Warp:
                    current_pilot = warp;
                    warp.StartManeuver(current_maneuvre_node);
                    break;
                case Mode.Burn:
                    current_pilot = burn;
                    burn.StartManeuver(current_maneuvre_node);
                    break;
            }

            logger.LogInfo("current_pilot " + current_pilot);
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
                SettingsUI.onGUI();
                settingsUI();
                return;
            }
            
            if (current_maneuvre_node == null)
            {
                GUILayout.Label("no Maneuvre node");
                return;
            }

            if (mode == Mode.Off)
            {
                if (!canStart())
                {
                    // UI_Tools.BigButton("Run");
                    GUILayout.Label("no Maneuver none in the future");
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

            if (current_pilot != null)
            {
                current_pilot.onGUI();

                if (!Settings.auto_next)
                {
                    GUILayout.Label($"finished {current_pilot.finished}");
                    if (!current_pilot.finished)
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
            }
        }

        public override void onReset()
        {
            Stop();
        }

        public override void Update()
        {
            base.Update();

            current_maneuvre_node = current_vessel.GetNextManeuveurNode();
            if (current_maneuvre_node == null)
            {
                Stop();
            }

            if (current_pilot != null)
            {
                current_pilot.setManeuver(current_maneuvre_node);
                current_pilot.Update();
                if (current_pilot.finished && Settings.auto_next)
                {
                    // auto next
                    nextMode();
                }
            }
        }

        public override void FixedUpdate()
        {
            if (current_pilot != null)
            {
                current_pilot.FixedUpdate();
            }
        }

        public override void LateUpdate()
        {
            if (current_pilot != null)
            {
                current_pilot.LateUpdate();
            }
        }

        void settingsUI()
        {
            if (Settings.debug_mode)
            {
                GUILayout.Label("Auto_Execute Next phase.", Styles.console_text);
                Settings.auto_next = GUILayout.Toggle(Settings.auto_next, "Auto Next Phase");
            }
            else
                Settings.auto_next = true;

            GUILayout.Label("Warp", Styles.title);

            Settings.warp_speed = UI_Tools.IntSlider("Warp Speed", Settings.warp_speed, 1, 10);
            GUILayout.Label("(1 : quick, 10 slow) ", Styles.console_text);

            GUILayout.Label("Safe time", Styles.console_text);

            Settings.warp_safe_duration = UI_Tools.IntField("warp_safe_duration", Settings.warp_safe_duration, 5, int.MaxValue);
            GUILayout.Label("nb seconds in x1 before launch (min:5)", Styles.console_text);

            GUILayout.Label("Burn", Styles.title);

            Settings.burn_adjust = UI_Tools.FloatSlider("burn_adjust", Settings.burn_adjust, 0, 2);
            GUILayout.Label("adjusting rate", Styles.console_text);

            Settings.max_dv_error = UI_Tools.FloatSlider("max_dv_error", Settings.max_dv_error, 0.001f, 2, " m/s");
            GUILayout.Label("accepted dV difference (m/s)", Styles.console_text);
        }

        void node_infos()
        {
            if (Settings.debug_mode)
            {
                var dt = GeneralTools.remainingStartTime(current_maneuvre_node);
                GUILayout.Label($"Node in {GeneralTools.DurationToString(dt)}");
                GUILayout.Label($"BurnDuration {current_maneuvre_node.BurnDuration}");
                GUILayout.Label($"BurnRequiredDV {current_maneuvre_node.BurnRequiredDV}");
                GUILayout.Label($"BurnVector {GeneralTools.VectorToString(current_maneuvre_node.BurnVector)}");

                var telemetry = SASInfos.getTelemetry();

                Vector3 maneuvre_dir = telemetry.ManeuverDirection.vector;

                GUILayout.Label($"maneuvre_dir {GeneralTools.VectorToString(maneuvre_dir)}");

                if (dt < 0)
                {
                    GUILayout.Label("In The Past");
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