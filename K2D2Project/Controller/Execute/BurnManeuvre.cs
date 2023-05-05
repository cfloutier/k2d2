
using BepInEx.Logging;
using UnityEngine;
using KSP.Sim;
using K2D2.KSPService;
using KSP.Sim.Maneuver;

using K2D2.UI;
using K2D2.Tools;

namespace K2D2.Controller;

class BurnManeuvreSettings
{

    public static float burn_adjust
    {
        get => GeneralSettings.sfile.GetFloat("warp.burn_adjust", 1.5f);
        set
        {             // value = Mathf.Clamp(0.1,)
            GeneralSettings.sfile.SetFloat("warp.burn_adjust", value);
        }
    }

    public static float max_dv_error
    {
        get => GeneralSettings.sfile.GetFloat("warp.max_dv_error", 0.1f);
        set
        {             // value = Mathf.Clamp(0.1,)
            GeneralSettings.sfile.SetFloat("warp.max_dv_error", value);
        }
    }

    static public bool rotate_during_burn
    {
        get => GeneralSettings.sfile.GetBool("land.rotate_during_burn", false);
        set
        {
            // value = Mathf.Clamp(value, 0 , 1);
            GeneralSettings.sfile.SetBool("land.rotate_during_burn", value);
        }
    }

    static public void onGUI()
    {
        burn_adjust = UI_Tools.FloatSliderTxt("Adjusting rate", burn_adjust, 0.5f, 2, "m/s", "Used during final adjust phase");
        UI_Tools.Right_Left_Text("Precise", "Quick");

        max_dv_error = UI_Tools.FloatSliderTxt("Precision", max_dv_error, 0.001f, 0.1f, "m/s", "max delta speed in final adjust phase", 3);

        rotate_during_burn = UI_Tools.Toggle(rotate_during_burn, "Rotate During burn", "Keep following Maneuver Node\ndirection during burn phase");
    }
}

public class BurnManeuvre : ExecuteController
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

        TimeWarpTools.SetRateIndex(0, false);

        if (finished)
            return;

        if (mode == Mode.Waiting)
        {
            start_dt = UT - GeneralTools.Game.UniverseModel.UniversalTime;
            if (start_dt > 0)
            {
                status_line = $"start in {StrTool.DurationToString(start_dt)}";
                set_throttle(0);
                return;
            }
            else
            {
                mode = Mode.Burning;

                if (BurnManeuvreSettings.rotate_during_burn)
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
            if (remaining_dv <= BurnManeuvreSettings.max_dv_error)
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

        needed_throttle = remaining_full_burn_time * BurnManeuvreSettings.burn_adjust;
    }

    public override void onGUI()
    {
        switch (mode)
        {
            case Mode.Waiting:
                UI_Tools.OK("Waiting !");
                UI_Tools.Console(status_line);
                break;
            case Mode.Burning:
                UI_Tools.Warning("Burning !");
                UI_Tools.ProgressBar(remaining_dv, 0, maneuver.BurnRequiredDV);
                UI_Tools.Console(StrTool.DurationToString(remaining_full_burn_time));
                break;
        }

        if (K2D2Settings.debug_mode)
        {
            if (maneuver == null) return;

            //UI_Tools.Console($"start_dt {Tools.remainingStartTime(maneuver)}");
            //UI_Tools.Console($"end_dt {Tools.remainingEndTime(maneuver)}");

            UI_Tools.Console($"BurnRequiredDV {maneuver.BurnRequiredDV}");
            UI_Tools.Console($"angle {angle}");
            UI_Tools.Console($"sign {sign}");
            UI_Tools.Console($"remaining_dv {remaining_dv}");
            UI_Tools.Console($"remaining_full_burn_time {remaining_full_burn_time}");
            UI_Tools.Console($"needed_throttle {needed_throttle}");

            //burn_dV.onGUI();
        }
    }
}
