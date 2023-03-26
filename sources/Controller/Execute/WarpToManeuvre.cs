
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using KSP.Sim.impl;
using KSP.Sim.Maneuver;



namespace K2D2.Controller
{
    public class WarpToManeuvre : ManeuvreController
    {
        int wanted_warp_index = 0;

        TimeWarp time_warp = null;

        public override void Update()
        {
            finished = false;
            time_warp = TimeWarpTools.time_warp();
            if (time_warp == null) return;
            if (maneuver == null) return;

            var dt = GeneralTools.remainingStartTime(maneuver);
            dt = dt  - Settings.warp_safe_duration;

            wanted_warp_index = compute_wanted_warp_index(dt);
            float wanted_rate = TimeWarpTools.indexToRatio(wanted_warp_index);

            if (dt < 0)
            {
                wanted_warp_index = 0;
                finished = true;
            }

            status_line = $"{GeneralTools.DurationToString(dt)} | x{wanted_rate}";
            if (time_warp.CurrentRateIndex != wanted_warp_index)
                time_warp.SetRateIndex(wanted_warp_index, false);
        }

        int compute_wanted_warp_index(double dt)
        {
            double factor = Settings.warp_speed;
            double ratio = dt / factor;

            return TimeWarpTools.ratioToIndex((float)ratio);
        }

        public override void onGUI()
        {
            GUILayout.Label("Time Warp", Styles.phase_ok);
            GUILayout.Label(status_line, Styles.small_dark_text);

            if (time_warp == null) return;

            if (Settings.debug_mode)
            {
                GUILayout.Label($"CurrentRateIndex {time_warp.CurrentRateIndex}");
                GUILayout.Label($"CurrentRate x{time_warp.CurrentRate}");
                GUILayout.Label($"index_rate x{TimeWarpTools.indexToRatio(time_warp.CurrentRateIndex)}");
            }
        }
    }
}