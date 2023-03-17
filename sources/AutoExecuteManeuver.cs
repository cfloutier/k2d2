
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using KSP.Sim.Maneuver;
using KSP.Messages;
using BepInEx.Logging;


namespace K2D2
{

    public class AutoExecuteManeuver
    {
        public ManualLogSource logger;

        public static AutoExecuteManeuver Instance { get; set; }

        public ManeuverNodeData current_maneuvre_node = null;

        // Sub Pilots
        TurnToManeuvre turn;
        WarpToManeuvre warp;
        BurnManeuvre burn;

        BasePilot current_pilot = null;

        public AutoExecuteManeuver(ManualLogSource logger)
        {
            this.logger = logger;
            logger.LogMessage("AutoExecuteManeuver !");
            Instance = this;

            Tools.Game().Messages.Subscribe<VesselChangedMessage>(OnActiveVesselChanged);

            turn = new TurnToManeuvre(this);
            warp = new WarpToManeuvre(this);
            burn = new BurnManeuvre(this);
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
                TimeWarpTools.time_warp()?.SetRateIndex(0, false);
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
                    break;
                case Mode.Warp:
                    current_pilot = warp;
                    break;
                case Mode.Burn:
                    current_pilot = burn;
                    break;
            }

            logger.LogInfo("current_pilot " + current_pilot);

            current_pilot.Start();
        }

        public bool canStart()
        {
            if (current_maneuvre_node == null)
                return false;

            var dt = Tools.remainingStartTime(current_maneuvre_node);
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
                Run();
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

        public void onGUI()
        {
            if (current_maneuvre_node == null)
            {
                GUILayout.Label("no Maneuvre node");
                return;
            }

            if (mode == Mode.Off)
            {
                if (!canStart())
                {
                    GUILayout.Button("Run", Styles.button, GUILayout.Height(40));
                    GUILayout.Label("no Maneuver none in the future");
                    return;
                }

                if (GUILayout.Button("Run", Styles.button, GUILayout.Height(40)))
                    Run();
            }
            else
            {
                if (GUILayout.Button("Stop !!!", Styles.button_on, GUILayout.Height(40)))
                    Stop();
            }

            

            node_infos();

            if (current_pilot != null)
            {
                current_pilot.onGui();

                if (Settings.debug_mode)
                {
                    if (GUILayout.Button("Next"))
                        nextMode();
                }
            }
        }

        public void Update()
        {
            current_maneuvre_node = Tools.getNextManeuveurNode();
            if (current_maneuvre_node == null)
            {
                Stop();
            }

            if (current_pilot != null)
            {
                current_pilot.onUpdate();
                if (current_pilot.finished && !Settings.debug_mode)
                {
                    // auto next
                    nextMode();
                }
            }
        }

        void node_infos()
        {
            if (Settings.debug_mode)
            {
                var dt = Tools.remainingStartTime(current_maneuvre_node);
                GUILayout.Label($"tic tac {Tools.printDuration(dt)} s ");
                if (dt < 0)
                {
                    GUILayout.Label("In The Past");
                    return;
                }

                GUILayout.Label($"BurnDuration {current_maneuvre_node.BurnDuration}");
                GUILayout.Label($"BurnRequiredDV {current_maneuvre_node.BurnRequiredDV}");
                GUILayout.Label($"BurnVector {Tools.printVector(current_maneuvre_node.BurnVector)}");

                var telemetry = SASInfos.getTelemetry();

                Vector3 maneuvre_dir = telemetry.ManeuverDirection.vector;

                GUILayout.Label($"maneuvre_dir {Tools.printVector(maneuvre_dir)}");
            }

            //GUILayout.Label($"Time {nextNode.Time}");
        }

        public void Run()
        {
            setMode(Mode.Turn);
        }

        public void Stop()
        {
            setMode(Mode.Off);
        }
    }
}