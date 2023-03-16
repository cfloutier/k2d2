
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using KSP.Sim.impl;
using KSP.Sim.Maneuver;

namespace COSMAT
{
    public class WarpToManeuvre : BasePilot
    {
        public AutoExecuteManeuver parent;

        public WarpToManeuvre(AutoExecuteManeuver parent)
        {
            this.parent = parent;
        }

        int wanted_warp_index = 0;

        TimeWarp time_warp = null;

        public override void onUpdate()
        {
            finished = false;
            time_warp = TimeWarpTools.time_warp();
            if (time_warp == null) return;

            ManeuverNodeData next_node = parent.current_maneuvre_node;
            if (next_node == null) return;

            var dt = Tools.remainingStartTime(next_node);
            if (dt < 0)
            {
                status_line = $"dt ({dt:n2}) < 0";
                // parent.Stop();
            }

            wanted_warp_index = compute_wanted_warp_index(dt);
            float wanted_rate = TimeWarpTools.indexToRatio(wanted_warp_index);

            status_line = $"{Tools.printDuration(dt)} | x{wanted_rate}";

            if (time_warp.CurrentRateIndex != wanted_warp_index)
                time_warp.SetRateIndex(wanted_warp_index, false);

            if (dt < 10)
            {
                finished = true;
            }
        }

        int compute_wanted_warp_index(double dt)
        {
            double factor = 5;
            double ratio = dt / factor;

            return TimeWarpTools.ratioToIndex((float)ratio);
        }

        public override void onGui()
        {
            GUILayout.Label("Time Warp", Styles.phase_ok);
            GUILayout.Label(status_line, Styles.console);

            if (time_warp == null) return;

            if (parent.debug_infos)
            {
                GUILayout.Label($"CurrentRateIndex {time_warp.CurrentRateIndex}");
                GUILayout.Label($"CurrentRate x{time_warp.CurrentRate}");
                GUILayout.Label($"index_rate x{TimeWarpTools.indexToRatio(time_warp.CurrentRateIndex)}");
                GUILayout.Label($"finished {finished}");
            }
        }
    }
}