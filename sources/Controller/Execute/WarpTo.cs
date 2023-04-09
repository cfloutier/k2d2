
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using KSP.Sim.impl;
using KSP.Sim.Maneuver;

using BepInEx.Logging;
using System;

namespace K2D2.Controller
{

    class WarpToSettings
    {

        public static bool ksp_warp
        {
            get => Settings.s_settings_file.GetBool("warp.ksp_warp", true);
            set { Settings.s_settings_file.SetBool("warp.ksp_warp", value); }
        }


        public static float warp_speed
        {
            get => Settings.s_settings_file.GetFloat("warp.speed", 2);
            set {
                    value = Mathf.Clamp(value, 0, 7);
                    Settings.s_settings_file.SetFloat("warp.speed", value); 
                }
        }

        public static int warp_safe_duration
        {
            get => Settings.s_settings_file.GetInt("warp.safe_duration", 10);
            set
            {
                if (value < 5) value = 5;
                Settings.s_settings_file.SetInt("warp.safe_duration", value);
            }
        }

        public static void ui()
        {
            UI_Tools.Title("// Warp");

            ksp_warp = UI_Tools.Toggle(ksp_warp, "Use Ksp Warp");

            if (!ksp_warp)
            {
                warp_speed = UI_Tools.FloatSlider("Warp Speed", warp_speed, 0, 7);
                UI_Tools.Right_Left_Text("safe", "quick");
            }

            UI_Tools.Console("Safe time (s)");
            warp_safe_duration = UI_Fields.IntField("warp_safe_duration", warp_safe_duration, 5, int.MaxValue,
                "Nb seconds in x1 before next phase (min:5)");
        }

    }

    public class WarpTo : ExecuteController
    {

        public ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.Controller.WarpTo");

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

        public override void Start()
        {
            finished = false;

        }

        double dt;

        public override void Update()
        {
            finished = false;
            if (maneuver != null)
            {
                UT = maneuver.Time;
            }

            var ut_modified = UT - WarpToSettings.warp_safe_duration;

            dt = ut_modified - GeneralTools.Game.UniverseModel.UniversalTime;

            if (dt < 0)
            {
                TimeWarpTools.SetRateIndex(0, false);
                finished = true;
                return;
            }

            if (WarpToSettings.ksp_warp)
            {
                if (TimeWarpTools.CurrentRateIndex == 0)
                {
                    logger.LogInfo("set KspWarpTo");
                    TimeWarpTools.KspWarpTo(ut_modified);
                }

                status_line = $"End warp : {StrTool.DurationToString(dt)} | use ksp warp";
            }
            else
            {
                wanted_warp_index = compute_wanted_warp_index(dt);
                float wanted_rate = TimeWarpTools.indexToRatio(wanted_warp_index);
                TimeWarpTools.SetRateIndex(wanted_warp_index, false);
                status_line = $"End warp : {StrTool.DurationToString(dt)} | x{wanted_rate}";
            }
        }

        int compute_wanted_warp_index(double dt)
        {
            if (dt < 0)
                return 0;

            double time_ratio = 1 + dt / ( 10 + WarpToSettings.warp_speed );

            // adding 1 because x1 during the warp mode is a lame
            return TimeWarpTools.ratioToIndex((float)time_ratio);
        }

        public override void onGUI()
        {
            UI_Tools.OK("Time Warp");
            UI_Tools.Console(status_line);

            if (Settings.debug_mode)
            {
                GUILayout.Label($"CurrentRateIndex {TimeWarpTools.CurrentRateIndex}");
                GUILayout.Label($"CurrentRate x{TimeWarpTools.CurrentRate}");
                GUILayout.Label($"index_rate x{TimeWarpTools.indexToRatio(TimeWarpTools.CurrentRateIndex)}");
            }
        }
    }
}