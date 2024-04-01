
using BepInEx.Logging;
using K2D2.KSPService;
using K2UI;
using KSP.Sim;
using KSP.Sim.Maneuver;
using KTools;
using UitkForKsp2.API;
using K2D2.UI;

// using KTools.UI;
using UnityEngine;
using UnityEngine.UIElements;
using K2D2.Controller;

namespace K2D2.Node;

class BurnManeuverSettings
{
    public static Setting<float> burn_adjust = new ("node_ex.burn.burn_adjust", 1.5f);
    public static Setting<float> max_dv_error = new ("node_ex.burn.max_dv_error", 0.1f);
    public static Setting<bool> rotate_during_burn = new ("node_ex.burn.rotate_during_burn", false);
}

public class BurnManeuver : ExecuteController
{
    public ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.BurnManeuver");

    BurndV burn_dV = new BurndV();
    KSPVessel current_vessel;

    public BurnManeuver()
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
        UT = node.Time;
        Start();
    }

    public override void Start()
    {
        finished = false;
        mode = Mode.Waiting;
        remaining_dv = 0;
        last_remaining_dv = -1;

        if (current_vessel == null) return;

        SASTool.setAutoPilot(AutopilotMode.Maneuver);

        // compute initial direction

        double ut;

        Vector velocity_after_maneuver = current_vessel.VesselComponent.Orbiter.ManeuverPlanSolver.GetVelocityAfterFirstManeuver(out ut);
        var current_vel = current_vessel.VesselComponent.Orbit.GetOrbitalVelocityAtUTZup(ut);
        initial_dir = velocity_after_maneuver.vector - current_vel;
    }

    Vector3d initial_dir = Vector3d.zero;
    int sign = 0;
    float angle;

    public double UT = 0;

    public override void Update()
    {
        if (maneuver == null) return;

        if (finished)
            return;

        if (mode == Mode.Waiting)
        {
            start_dt = UT - GeneralTools.Game.UniverseModel.UniverseTime;
            if (start_dt > 0)
            {
                status_line = $"start in {StrTool.DurationToString(start_dt)}";
                set_throttle(0);
                return;
            }
            else
            {
                mode = Mode.Burning;

                if (BurnManeuverSettings.rotate_during_burn.V)
                    SASTool.setAutoPilot(AutopilotMode.Maneuver);
                else
                    SASTool.setAutoPilot(AutopilotMode.StabilityAssist);

            }
        }

        if (mode == Mode.Burning)
        {
            burn_dV.LateUpdate();

            if (maneuver == null) return;

            // ActiveVessel = GameManager.Instance?.Game?.ViewController?.GetActiveVehicle(true)?.GetSimVessel(true);
            double ut = 0;

            Vector velocity_after_maneuver = current_vessel.VesselComponent.Orbiter.ManeuverPlanSolver.GetVelocityAfterFirstManeuver(out ut);

            double dt = ut - GeneralTools.Game.UniverseModel.UniverseTime;

            // allow time warp up to what the auto warp can do.
            int max_warp_index = WarpToSettings.compute_wanted_warp_index(dt);
            if (max_warp_index == 1)
                max_warp_index = 0; // no x2 speed

            if (max_warp_index < TimeWarpTools.CurrentRateIndex)
                TimeWarpTools.SetRateIndex(max_warp_index, false);

            var current_vel = current_vessel.VesselComponent.Orbit.GetOrbitalVelocityAtUTZup(ut);
            var delta_speed_vector = velocity_after_maneuver.vector - current_vel;
            sign = Math.Sign(Vector3d.Dot(initial_dir, delta_speed_vector));

            angle = (float)Vector3d.Angle(initial_dir, delta_speed_vector);
            remaining_dv = sign * delta_speed_vector.magnitude;

            if (last_remaining_dv > 0 && last_remaining_dv < 1 && remaining_dv > last_remaining_dv)
            {
                Finished();
                return;
            }

            last_remaining_dv = remaining_dv;

            // var required_dv = maneuver.BurnRequiredDV;
            // remaining_dv = required_dv - burn_dV.burned_dV;
            if (remaining_dv <= BurnManeuverSettings.max_dv_error.V)
            {
                Finished();
                return;
            }
            else
            {
                compute_throttle();
                set_throttle(needed_throttle);
                status_line = $"remaining dV : {remaining_dv:n2} m/S";
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

        needed_throttle = remaining_full_burn_time * BurnManeuverSettings.burn_adjust.V;
    }

    public override void updateUI(VisualElement root_el, FullStatus st)
    {
        switch (mode)
        {
            case Mode.Waiting:
                st.Status("Waiting !", StatusLine.Level.Normal);
                st.Console(status_line);
                break;
            case Mode.Burning:
                st.Status("Burning !", StatusLine.Level.Warning);
                st.Console(status_line);
                if (maneuver.BurnRequiredDV >= 0)
                    st.Progess(remaining_dv / maneuver.BurnRequiredDV, $"{remaining_dv:n1} m/s");
                    
                st.Console(StrTool.DurationToString(remaining_full_burn_time));
                break;
        }

        if (K2D2Settings.debug_mode.V)
        {
            if (maneuver == null) return;
            st.Console("-----------------");
            //st.Console($"start_dt {Tools.remainingStartTime(maneuver)}");
            //st.Console($"end_dt {Tools.remainingEndTime(maneuver)}");

            st.Console($"BurnRequiredDV {maneuver.BurnRequiredDV}");
            st.Console($"angle {angle}");
            st.Console($"sign {sign}");
            st.Console($"remaining_dv {remaining_dv}");
            st.Console($"remaining_full_burn_time {remaining_full_burn_time}");
            st.Console($"needed_throttle {needed_throttle}");

            //burn_dV.onGUI();
        }
    }
}
