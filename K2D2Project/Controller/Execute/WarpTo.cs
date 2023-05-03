
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using KSP.Sim.impl;
using KSP.Sim.Maneuver;

using BepInEx.Logging;
using System;

using K2D2.UI;

namespace K2D2.Controller;


class WarpToSettings
{

    public static float warp_speed
    {
        get => Settings.s_settings_file.GetFloat("warp.speed", 2);
        set
        {
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

    public static void onGUI()
    {
        warp_speed = UI_Tools.FloatSliderTxt("Warp Speed", warp_speed, 0, 7, "", "Warp adjust rate");
        UI_Tools.Right_Left_Text("Safe", "Quick");

        warp_safe_duration = UI_Fields.IntField("warp_safe_duration", "Before Burn Time", warp_safe_duration, 5, int.MaxValue,
            "Nb seconds in x1 before next phase (min:5)");
    }
}

public class WarpTo : ExecuteController
{
    public ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.Controller.WarpTo");

    int wanted_warp_index = 0;
    ManeuverNodeData maneuver = null;
    public double UT;

    TurnTo turn_to = null;

    public bool check_direction = false;

    public float max_angle;

    public int max_warp_index = -1;


    public K2D2.KSPService.KSPVessel current_vessel;

    public void StartManeuver(ManeuverNodeData node, bool check_direction = false)
    {
        maneuver = node;
        this.UT = node.Time;
        this.check_direction = check_direction;

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
        this.check_direction = false;
        this.max_angle = 0;
        Start();
    }

    public void Start_Retrograde(double UT, bool check_direction = false, float max_angle = 30)
    {
        maneuver = null;
        this.UT = UT;
        this.check_direction = check_direction;
        this.max_angle = max_angle;

        Start();

        if (check_direction)
        {
            turn_to = new TurnTo();
            turn_to.StartSurfaceRetroGrade();
        }
    }

    public override void Start()
    {
        finished = false;
        current_vessel = K2D2_Plugin.Instance.current_vessel;
    }

    double dt;

    public override void Update()
    {
        finished = false;

        status_line = "";

        var ut_modified = UT - WarpToSettings.warp_safe_duration;
        dt = ut_modified - GeneralTools.Game.UniverseModel.UniversalTime;

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

        wanted_warp_index = compute_wanted_warp_index(dt);
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


    int compute_wanted_warp_index(double dt)
    {
        if (dt < 0)
            return 0;

        double time_ratio = 1 + dt / (10 - WarpToSettings.warp_speed);

        // adding 1 because x1 during the warp mode is a lame
        return TimeWarpTools.ratioToIndex((float)time_ratio);
    }

    public override void onGUI()
    {
        UI_Tools.OK("Time Warp");
        UI_Tools.Console(status_line);

        if (Settings.debug_mode)
        {
            UI_Tools.Console($"CurrentRateIndex {TimeWarpTools.CurrentRateIndex}");
            UI_Tools.Console($"CurrentRate x{TimeWarpTools.CurrentRate}");
            UI_Tools.Console($"index_rate x{TimeWarpTools.indexToRatio(TimeWarpTools.CurrentRateIndex)}");
        }
    }
}
