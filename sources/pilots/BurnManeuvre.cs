
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace K2D2
{
    public class BurnManeuvre  : BasePilot
    {
        public AutoExecuteManeuver parent;

        public BurnManeuvre(AutoExecuteManeuver parent)
        {
            this.parent = parent;
        }

        public override void onUpdate()
        {
            finished = false;

            var time_warp = TimeWarpTools.time_warp();
            var current_maneuvre_node = parent.current_maneuvre_node;
            if (current_maneuvre_node == null) return;

            if (time_warp.CurrentRateIndex != 0)
                time_warp.SetRateIndex(0, false);

            var dt = Tools.remainingStartTime(current_maneuvre_node);
            var end_dt = Tools.remainingEndTime(current_maneuvre_node);

            if (end_dt < 0)
            {
                status_line = $"ended";
                VesselInfos.SetThrottle(0);
                finished=true;
                // mode = Mode.Off;
            }
            else if (dt < 0)
            {
                status_line = $"burning, end in {Tools.printDuration(end_dt)}";
                VesselInfos.SetThrottle(1);
            }
            else
            {
                status_line = $"start in {Tools.printDuration(dt)}";
                VesselInfos.SetThrottle(0);
            }
        }

        public override void onGui()
        {
            GUILayout.Label("Burn !", Styles.phase_ok);
            GUILayout.Label(status_line, Styles.small_dark_text);

            if (Settings.debug_mode)
            {
                var current_maneuvre_node = parent.current_maneuvre_node;
                if (current_maneuvre_node == null) return;

                var dt = Tools.remainingStartTime(current_maneuvre_node);
                var end_dt = Tools.remainingEndTime(current_maneuvre_node);

                GUILayout.Label($"start_dt {dt}");
                GUILayout.Label($"end_dt {end_dt}");
            }
        }
    }
}