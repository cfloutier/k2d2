
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace K2D2
{
    public class BurnManeuvre  : BasePilot
    {
        public AutoExecuteManeuver parent;

        BurndV burn_dV = new BurndV();

        public BurnManeuvre(AutoExecuteManeuver parent)
        {
            this.parent = parent;
        }

        double start_dt;

        public enum Mode
        {
            Waiting,
            Burning
        }

        Mode mode = Mode.Waiting;

        public override void Start()
        {
            finished = false;
            mode = Mode.Waiting;
            burn_dV.reset();
        }

        public override void onUpdate()
        {
            var current_maneuvre_node = parent.current_maneuvre_node;
            if (current_maneuvre_node == null) return;

            var time_warp = TimeWarpTools.time_warp();
            if (time_warp.CurrentRateIndex != 0)
                time_warp.SetRateIndex(0, false);

            if (mode == Mode.Waiting)
            {
                start_dt = Tools.remainingStartTime(current_maneuvre_node);
                if (start_dt > 0)
                {
                    status_line = $"start in {Tools.printDuration(start_dt)}";
                    VesselInfos.SetThrottle(0);
                    return;
                }
                else
                    mode = Mode.Burning;
            }
        }

        public override void FixedUpdate()
        {
            if (mode == Mode.Burning)
            {
                burn_dV.FixedUpdate();
                var current_maneuvre_node = parent.current_maneuvre_node;
                if (current_maneuvre_node == null)
                {
                    finished = true;
                    return;
                }

                var required_dv = current_maneuvre_node.BurnRequiredDV;
                if (burn_dV.burned_dV >= required_dv)
                {
                    VesselInfos.SetThrottle(0);
                    status_line = $"ended, error is {required_dv-burn_dV.burned_dV}";
                    finished = true;
                }
                else
                {
                    VesselInfos.SetThrottle(1);
                    status_line = $"actual dV : {burn_dV.burned_dV}";
                }
            }
        }

        public override void onGui()
        {
            switch(mode)
            {
                case Mode.Waiting:
                    GUILayout.Label("Waiting !", Styles.phase_ok);
                    break;
                case Mode.Burning:
                    GUILayout.Label("Burning !", Styles.warning);
                    break;
            }

            GUILayout.Label(status_line, Styles.small_dark_text);

            if (Settings.debug_mode)
            {
                var current_maneuvre_node = parent.current_maneuvre_node;
                if (current_maneuvre_node == null) return;

                GUILayout.Label($"start_dt {Tools.remainingStartTime(current_maneuvre_node)}");
                GUILayout.Label($"end_dt {Tools.remainingEndTime(current_maneuvre_node)}");
                GUILayout.Label($"BurnRequiredDV {current_maneuvre_node.BurnRequiredDV}");

                burn_dV.onGUI();
            }
        }
    }
}