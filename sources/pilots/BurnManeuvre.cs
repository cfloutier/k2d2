
using BepInEx.Logging;
using KSP.Messages.PropertyWatchers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using KSP.Sim;


namespace K2D2
{
    public class BurnManeuvre  : ManeuvrePilot
    {
        public ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.BurnManeuvre");

        BurndV burn_dV = new BurndV();

        public BurnManeuvre()
        {

        }

        double start_dt;

        public enum Mode
        {
            Waiting,
            Burning
        }

        Mode mode = Mode.Waiting;

        public double remaining_dv;
        public float needed_throttle = 0;
        public float remaining_full_burn_time = 0;

        public override void Start()
        {
            finished = false;
            mode = Mode.Waiting;
            burn_dV.reset();
            remaining_dv = 0;

            var vessel = VesselInfos.currentVessel();
            if (vessel == null) return;
            var autopilot = vessel.Autopilot;

            // force autopilot
            autopilot.Enabled = true;
            autopilot.SetMode(AutopilotMode.StabilityAssist);
        }

        public override void onUpdate()
        {
            if (maneuver == null) return;

            var time_warp = TimeWarpTools.time_warp();
            if (time_warp.CurrentRateIndex != 0)
                time_warp.SetRateIndex(0, false);

            if (mode == Mode.Waiting)
            {
                start_dt = Tools.remainingStartTime(maneuver);
                if (start_dt > 0)
                {
                    status_line = $"start in {Tools.printDuration(start_dt)}";
                    set_throttle(0);
                    return;
                }
                else
                    mode = Mode.Burning;
            }

            if (mode == Mode.Burning)
            {
                burn_dV.Update();
                burn_dV.LateUpdate();
                if (maneuver == null) return;

                var required_dv = maneuver.BurnRequiredDV;
                remaining_dv = required_dv - burn_dV.burned_dV;
                if (remaining_dv <= Settings.max_dv_error )
                {
                    set_throttle(0);
                    status_line = $"ended, error is {remaining_dv} m/S";
                    finished = true;
                    return;
                }
                else
                {
                    compute_throttle();
                    set_throttle(needed_throttle);
                    status_line = $"remaining dV : {remaining_dv} m/S";
                }
            }
        }

        public override void FixedUpdate()
        {
            if (mode == Mode.Burning)
            {
                burn_dV.FixedUpdate();
            }
        }

        public override void LateUpdate()
        {

        }

        float last_throttle = -1;

        public void set_throttle(float throttle)
        {
            throttle = Mathf.Clamp01(throttle);
            // throttle = (float) Math.Round(throttle, 2);

           // if (last_throttle == throttle) return;

            // logger.LogInfo($"set_throttle {throttle}");

            VesselInfos.SetThrottle(throttle);
            last_throttle = throttle;
        }

        public void compute_throttle()
        {
            if (remaining_dv <= 0)
            {
                needed_throttle = 0;
                return;
            }

            remaining_full_burn_time = (float)(remaining_dv / burn_dV.full_dv);
            if (remaining_full_burn_time >= 1)
            {
                needed_throttle = 1;
                return;
            }

            needed_throttle = remaining_full_burn_time * Settings.burn_adjust;
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
                if (maneuver == null) return;

                //GUILayout.Label($"start_dt {Tools.remainingStartTime(maneuver)}");
                //GUILayout.Label($"end_dt {Tools.remainingEndTime(maneuver)}");

                GUILayout.Label($"BurnRequiredDV {maneuver.BurnRequiredDV}");

                GUILayout.Label($"remaining_dv {remaining_dv}");
                GUILayout.Label($"remaining_full_burn_time {remaining_full_burn_time}");

                GUILayout.Label($"needed_throttle {needed_throttle}");

                burn_dV.onGUI();
            }
        }
    }
}