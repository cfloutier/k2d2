
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using KSP.Sim.impl;
using KSP.Sim.Maneuver;



namespace K2D2.Controller
{
    public class WarpTo : ExecuteController
    {
        int wanted_warp_index = 0;
        ManeuverNodeData maneuver = null;
        public double UT;

        public void StartManeuver(ManeuverNodeData node)
        {
            maneuver = node;
            Start();
        }

        public void Start_UT(double UT)
        {
            maneuver = null;
            this.UT = UT;
            Start();
        }

        /*public double remainingStartTime(ManeuverNodeData node)
        {
            var dt = node.Time - GeneralTools.Game.UniverseModel.UniversalTime;
            return dt;
        }

        public double remainingEndTime(ManeuverNodeData node)
        {
            var dt = node.Time + node.BurnDuration - GeneralTools.Game.UniverseModel.UniversalTime;
            return dt;
        }*/


        double dt;


        public override void Update()
        {
            finished = false;
            if (maneuver != null)
            {
                UT = maneuver.Time;
            }

            dt = UT - GeneralTools.Game.UniverseModel.UniversalTime;
            dt = dt - Settings.warp_safe_duration;

            wanted_warp_index = compute_wanted_warp_index(dt);
            float wanted_rate = TimeWarpTools.indexToRatio(wanted_warp_index);

            if (dt < 0)
            {
                wanted_warp_index = 0;
                finished = true;
            }

            status_line = $"End warp : {StrTool.DurationToString(dt)} | x{wanted_rate}";
            TimeWarpTools.SetRateIndex(wanted_warp_index, false);
        }

        int compute_wanted_warp_index(double dt)
        {
            double factor = Settings.warp_speed;
            double ratio = dt / factor;

            // adding 1 because x1 during the warp mode is a lame
            return TimeWarpTools.ratioToIndex((float)ratio) + 1;
        }

        public override void onGUI()
        {
            GUILayout.Label("Time Warp", Styles.phase_ok);
            GUILayout.Label(status_line, Styles.console_text);

            if (Settings.debug_mode)
            {
                GUILayout.Label($"CurrentRateIndex {TimeWarpTools.CurrentRateIndex}");
                GUILayout.Label($"CurrentRate x{TimeWarpTools.CurrentRate}");
                GUILayout.Label($"index_rate x{TimeWarpTools.indexToRatio(TimeWarpTools.CurrentRateIndex)}");
            }
        }

        public void setting_UI()
        {
            UI_Tools.Title("// Warp");

            Settings.warp_speed = UI_Tools.IntSlider("Warp Speed", Settings.warp_speed, 4, 10);
            UI_Tools.Right_Left_Text("quick", "slow");

            GUILayout.Label("", Styles.console_text);

            Settings.warp_safe_duration = UI_Tools.IntField("Safe time (s)", Settings.warp_safe_duration, 5, int.MaxValue,
                "Nb seconds in x1 before next phase (min:5)");
        }
    }
}