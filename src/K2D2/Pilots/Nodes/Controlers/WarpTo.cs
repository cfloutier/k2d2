using BepInEx.Logging;
using KSP.Sim;
using KSP.Sim.Maneuver;
using KTools;
using UnityEngine.UIElements;
using K2D2.UI;
using K2D2.Controller;


namespace K2D2.Node;

class WarpToSettings
{
    // compute max warp index depending on remaining time
    public static int compute_wanted_warp_index(double time_left)
    {
        if (time_left < 0)
            return 0;

        double time_ratio = 1 + time_left / (10 - warp_speed.V);

        // adding 1 because x1 during the warp mode is a lame
        return TimeWarpTools.ratioToIndex((float)time_ratio);
    }

    public static ClampSetting<float> warp_speed = new("node_ex.warp.speed", 2, 0, 7);

    public static ClampSetting<int> warp_safe_duration = new("node_ex.warp.safe_duration", 10, 5, 120);
}

public class WarpTo : ExecuteController
{
    public ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.Controller.WarpTo");

    int wanted_warp_index = 0;
    ManeuverNodeData maneuver = null;
    public double UT;

    TurnTo turn_to = null;

    public bool check_direction = false;
    public bool add_safe_duration = true;

    public float max_angle;

    public int max_warp_index = -1;

    public K2D2.KSPService.KSPVessel current_vessel;

    public void StartManeuver(ManeuverNodeData node, bool check_direction = false)
    {
        maneuver = node;
        this.UT = node.Time;
        this.check_direction = check_direction;
        this.add_safe_duration = true;

        Start();

        if (check_direction)
        {
            turn_to = new TurnTo();
            turn_to.StartManeuver(node);
        }
    }

    public void Start_Ut(double UT)
    {
        maneuver = null;

        this.UT = UT;
        this.add_safe_duration = false;
        this.check_direction = false;
        this.max_angle = 0;

        Start();
    }

    public void Start_Retrograde(double UT, bool check_direction = false, float max_angle = 30)
    {
        maneuver = null;
        this.UT = UT;
        this.check_direction = check_direction;
        this.add_safe_duration = true;
        this.max_angle = max_angle;

        Start();

        if (check_direction)
        {
            turn_to = new TurnTo();
            turn_to.StartRetroGrade(SpeedDisplayMode.Surface);
        }
    }

    public override void Start()
    {
        finished = false;
        current_vessel = K2D2Plugin.Instance.current_vessel;
    }

    double dt;

    public override void Update()
    {
        finished = false;

        status_line = "";

        if (add_safe_duration)
        {
            var ut_modified = UT - WarpToSettings.warp_safe_duration.V;
            dt = ut_modified - GeneralTools.Game.UniverseModel.UniverseTime;
        }
        else
        {
            dt = UT - GeneralTools.Game.UniverseModel.UniverseTime;
        }

        if (dt < 0)
        {
            TimeWarpTools.SetRateIndex(0, false);
            finished = true;
            return;
        }

        if (check_direction)
        {
            turn_to.Update();
            status_line = $"Attitude Correction = {turn_to.angle:n2} ° < {max_angle}";
            if (TimeWarpTools.CurrentRateIndex > 0)
            {
                if (turn_to.angle > max_angle)
                {
                    TimeWarpTools.SetRateIndex(0, false);
                    return;
                }
            }
            else
            {
                if (turn_to.angle > 1)
                    return;
            }
        }

        wanted_warp_index = WarpToSettings.compute_wanted_warp_index(dt);
        // minimum is x4
        if (wanted_warp_index < 2)
            wanted_warp_index = 2;

        // max is x4
        if (current_vessel.VesselVehicle.IsInAtmosphere)
            if (wanted_warp_index > 2)
                wanted_warp_index = 2;

        if (current_vessel.GetApproxAltitude() < 3000)
            if (wanted_warp_index > 2)
                wanted_warp_index = 2;

        if (max_warp_index > 0)
            if (wanted_warp_index > max_warp_index)
                wanted_warp_index = max_warp_index;

        float wanted_rate = TimeWarpTools.indexToRatio(wanted_warp_index);
        TimeWarpTools.SetRateIndex(wanted_warp_index, false);
        status_line = $"End warp : {StrTool.DurationToString(dt)} | x{wanted_rate}";
        if (check_direction)
        {
            status_line += $"\nAttitude Correction = {turn_to.angle:n2} ° < {max_angle}";
        }
    }
    public override void updateUI(VisualElement root_el, FullStatus st)
    {
        st.Status("Time Warp");
        st.Console(status_line);

        if (K2D2Settings.debug_mode.V)
        {
            st.Console($"CurrentRateIndex {TimeWarpTools.CurrentRateIndex}");
            st.Console($"CurrentRate x{TimeWarpTools.CurrentRate}");
            st.Console($"index_rate x{TimeWarpTools.indexToRatio(TimeWarpTools.CurrentRateIndex)}");
        }
    }
}
