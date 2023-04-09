
using BepInEx.Logging;
using KSP.Messages.PropertyWatchers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using KSP.Sim;
using K2D2.KSPService;
using KSP.Sim.Maneuver;

namespace K2D2.Controller
{
    class BurnManeuvreSettings
    {

        public static float burn_adjust
        {
            get => Settings.s_settings_file.GetFloat("warp.burn_adjust", 1.5f);
            set
            {             // value = Mathf.Clamp(0.1,)
                Settings.s_settings_file.SetFloat("warp.burn_adjust", value);
            }
        }

        public static float max_dv_error
        {
            get => Settings.s_settings_file.GetFloat("warp.max_dv_error", 0.1f);
            set
            {             // value = Mathf.Clamp(0.1,)
                Settings.s_settings_file.SetFloat("warp.max_dv_error", value);
            }
        }

        static public void ui()
        {
            UI_Tools.Title("// Burn");

            burn_adjust = UI_Tools.FloatSlider("Adjusting rate", burn_adjust, 0, 2);
            UI_Tools.Right_Left_Text("Precise", "Quick");

            max_dv_error = UI_Tools.FloatSlider("Precision",
                        max_dv_error, 0.001f, 0.5f, "m/s");
        }
    }

    public class BurnManeuvre  : ExecuteController
    {
        public ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.BurnManeuvre");

        BurndV burn_dV = new BurndV();
        KSPVessel current_vessel;

        public BurnManeuvre()
        {
            current_vessel = K2D2_Plugin.Instance.current_vessel;
        }

        double start_dt;

        public enum Mode
        {
            Waiting,
            Burning
        }

        Mode mode = Mode.Waiting;

        public double remaining_dv;
        public double last_remaining_dv = -1;


        public float needed_throttle = 0;
        public float remaining_full_burn_time = 0;

        ManeuverNodeData maneuver;

        public void StartManeuver(ManeuverNodeData node)
        {
            maneuver = node;
            Start();
        }

        public override void Start()
        {
            finished = false;
            mode = Mode.Waiting;
            remaining_dv = 0;
            last_remaining_dv = -1;

            if (current_vessel == null) return;
            var autopilot = current_vessel.Autopilot;

            // force autopilot
            autopilot.Enabled = true;
            autopilot.SetMode(AutopilotMode.Maneuver);


            // compute initial direction

            double ut;

            Vector velocity_after_maneuver = current_vessel.VesselComponent.Orbiter.ManeuverPlanSolver.GetVelocityAfterFirstManeuver(out ut);
            var current_vel = current_vessel.VesselComponent.Orbit.GetOrbitalVelocityAtUTZup(ut);
            initial_dir = velocity_after_maneuver.vector - current_vel;
        }

        Vector3d initial_dir = Vector3d.zero;

        public override void Update()
        {
            if (maneuver == null) return;

            TimeWarpTools.SetRateIndex(0, false);

            if (finished)
                return;

            if (mode == Mode.Waiting)
            {
                start_dt = GeneralTools.remainingStartTime(maneuver);
                if (start_dt > 0)
                {
                    status_line = $"start in {StrTool.DurationToString(start_dt)}";
                    set_throttle(0);
                    return;
                }
                else
                {
                    mode = Mode.Burning;
                    var autopilot = current_vessel.Autopilot;

                    // force autopilot
                    autopilot.Enabled = true;
                    autopilot.SetMode(AutopilotMode.StabilityAssist);
                }
            }

            if (mode == Mode.Burning)
            {
                burn_dV.LateUpdate();

                if (maneuver == null) return;

                // ActiveVessel = GameManager.Instance?.Game?.ViewController?.GetActiveVehicle(true)?.GetSimVessel(true);
                double ut = 0;

                Vector velocity_after_maneuver = current_vessel.VesselComponent.Orbiter.ManeuverPlanSolver.GetVelocityAfterFirstManeuver(out ut);
                var current_vel = current_vessel.VesselComponent.Orbit.GetOrbitalVelocityAtUTZup(ut);
                var delta_speed_vector = velocity_after_maneuver.vector - current_vel;

                var sign = Math.Sign(Vector3d.Dot(initial_dir, delta_speed_vector));

                remaining_dv = delta_speed_vector.magnitude;

                if (last_remaining_dv > 0 && last_remaining_dv  < 1 && remaining_dv > last_remaining_dv)
                {
                    Finished();
                    return;
                }

                last_remaining_dv = remaining_dv;

                // var required_dv = maneuver.BurnRequiredDV;
                // remaining_dv = required_dv - burn_dV.burned_dV;
                if (remaining_dv <= BurnManeuvreSettings.max_dv_error )
                {
                    Finished();
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

        void Finished()
        {
            status_line = $"ended, error is {remaining_dv} m/S";
            set_throttle(0);
            finished = true;
        }

        float last_throttle = -1;

        public void set_throttle(float throttle)
        {
            throttle = Mathf.Clamp01(throttle);
            current_vessel.SetThrottle(throttle);
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

            needed_throttle = remaining_full_burn_time * BurnManeuvreSettings.burn_adjust;
        }

        public override void onGUI()
        {
            switch(mode)
            {
                case Mode.Waiting:
                    UI_Tools.OK("Waiting !");
                    break;
                case Mode.Burning:
                    UI_Tools.Warning("Burning !");
                    break;
            }

            UI_Tools.Console(status_line);

            if (Settings.debug_mode)
            {
                if (maneuver == null) return;

                //UI_Tools.Console($"start_dt {Tools.remainingStartTime(maneuver)}");
                //UI_Tools.Console($"end_dt {Tools.remainingEndTime(maneuver)}");

                UI_Tools.Console($"BurnRequiredDV {maneuver.BurnRequiredDV}");

                UI_Tools.Console($"remaining_dv {remaining_dv}");
                UI_Tools.Console($"remaining_full_burn_time {remaining_full_burn_time}");


                UI_Tools.Console($"needed_throttle {needed_throttle}");

                //burn_dV.onGUI();
            }
        }
    }
}