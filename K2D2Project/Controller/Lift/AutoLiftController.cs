using K2D2.KSPService;
using KSP.Messages.PropertyWatchers;
using KSP.Sim;
using KSP.Sim.impl;
using KTools;
using KTools.UI;
using Steamworks;
using UnityEngine;

namespace K2D2.Controller;





public class AutoLiftController : ComplexController
{
    public enum LiftStatus
    {
        Off,
        Ascent,
        Coasting,
        Adjust,
        Circularize
    }


    public static AutoLiftController Instance { get; set; }

    AutoLiftSettings lift_settings = null;
    LiftAscentPath ascentPath = null;

    KSPVessel current_vessel;

    float wanted_elevation;

    public AutoLiftController()
    {
        // logger.LogMessage("LandingController !");
        current_vessel = K2D2_Plugin.Instance.current_vessel;

        lift_settings = new AutoLiftSettings();
        ascentPath = new LiftAscentPath(lift_settings);

        Instance = this;
        debug_mode_only = false;
        name = "Lift";
        K2D2PilotsMgr.Instance.RegisterPilot("Lift", this);
    }

    public override void onReset()
    {
        isRunning = false;
    }

    LiftStatus _status = LiftStatus.Off;
    LiftStatus status
    {
        get { return _status; }
        set
        {
            if (_status == value)
                return;
            _status = value;
            switch (value)
            {
                case LiftStatus.Off:
                    {
                        // stop
                        if (current_vessel != null)
                            current_vessel.SetThrottle(0);
                    }
                    break;
                case LiftStatus.Ascent:
                    {
                        SASTool.setAutoPilot(AutopilotMode.StabilityAssist);
                        last_ap_km = 0;
                        ap_km = 0;
                        delta_ap_per_second = 0;
                        wanted_elevation = -90;
                    }
                    break;
                case LiftStatus.Coasting:
                    current_vessel.SetThrottle(0);
                    turn_to = new TurnTo();
                    turn_to.StartProGrade(SpeedDisplayMode.Surface);
                    TimeWarpTools.SetRateIndex(0, false);
                    break;
                case LiftStatus.Adjust:
                    current_vessel.SetThrottle(0);
                    TimeWarpTools.SetRateIndex(0, false);
                    break;
                case LiftStatus.Circularize:
                    {
                        TimeWarpTools.SetRateIndex(0, false);
                        if (K2D2OtherModsInterface.fpLoaded)
                        {
                            createCircleNode();
                        }
                        else
                        {
                            EndLiftPilot(true, "Please install FlightPlan for the final Step...");
                        }
                    }
                    break;
            }
        }
    }


    void createCircleNode()
    {
        var current_time = GeneralTools.Game.UniverseModel.UniverseTime;

        var current_vessel = K2D2_Plugin.Instance.current_vessel;
        if (current_vessel == null)
        {
            EndLiftPilot(false, "error : no vessel");
            return;
        }

        PatchedConicsOrbit orbit = current_vessel.VesselComponent.Orbit;
        if (orbit == null)
        {
            EndLiftPilot(false, "error : no orbit");
            return;
        }

        if (K2D2OtherModsInterface.instance.Circularize(current_time + orbit.TimeToAp, 0))
        {
            K2D2_Plugin.Instance.FlyNode();
            EndLiftPilot(true, "Launched With FlightPlan !");
        }
        else
        {
            EndLiftPilot(false, "Error Creating Node");
        }
        return;
    }



    public override bool isRunning
    {
        get { return _status != LiftStatus.Off; }
        set
        {
            if (value == isRunning)
                return;

            if (!value)
            {
                status = LiftStatus.Off;
            }
            else
            {
                // reset controller to desactivate other controllers.
                K2D2_Plugin.ResetControllers();
                OnStartController();
            }
        }
    }

    void OnStartController()
    {
        status = LiftStatus.Ascent;
    }

    Vector3d direction = Vector3d.zero;

    public void applyDirection()
    {
        var autopilot = current_vessel.Autopilot;

        if (autopilot == null)
            return;

        // force autopilot
        autopilot.Enabled = true;

        var telemetry = SASTool.getTelemetry();

        var up = telemetry.HorizonUp;

        direction = QuaternionD.Euler(-wanted_elevation, lift_settings.heading, 0) * Vector3d.forward;

        Vector direction_vector = new Vector(up.coordinateSystem, direction);

        autopilot.SAS.lockedMode = false;
        autopilot.SAS.SetTargetOrientation(direction_vector, false);
    }

    float current_altitude_km = 0;
    float ap_km = 0;
    float last_ap_km = 0;
    float delta_ap_per_second;
    bool show_profile = false;

    void computeValues()
    {
        if (current_vessel.VesselComponent == null)
            return;

        PatchedConicsOrbit orbit = current_vessel.VesselComponent.Orbit;
        ap_km = (float)(orbit.Apoapsis - orbit.referenceBody.radius) / 1000;
        current_altitude_km = (float)(current_vessel.GetSeaAltitude() / 1000);

        if (last_ap_km == 0)
        {
            last_ap_km = ap_km;
        }
        else
        {
            float delta_ap = ap_km - last_ap_km;
            last_ap_km = ap_km;
            if (status == LiftStatus.Ascent)
            {
                // compute delta_ap_per_second only on ascent
                float throttle = (float)current_vessel.GetThrottle();
                if (throttle > 0.1f && Time.deltaTime != 0)
                {
                    float new_delta_ap_per_second = delta_ap / (Time.deltaTime * throttle);
                    delta_ap_per_second = Mathf.Lerp(delta_ap_per_second, new_delta_ap_per_second, 0.1f);
                }
                else
                    delta_ap_per_second = 0;
            }
        }

        wanted_elevation = ascentPath.compute_elevation(current_altitude_km);
    }

    // used during coasting
    double densityAtm = 0;
    double duration_to_atm = 0;

    TurnTo turn_to = null;

    bool result_ok = false;
    string end_status;

    void EndLiftPilot(bool result_ok, string end_status)
    {
        this.result_ok = result_ok;
        this.end_status = end_status;
        status = LiftStatus.Off;
    }


    public WarpTo warp_to = new WarpTo();

    public override void Update()
    {
        // if (!isRunning && !ui_visible) return;
        if (current_vessel == null) return;

        if (isRunning || ui_visible)
            computeValues();

        if (!isRunning)
            return;

        switch (status)
        {
            case LiftStatus.Coasting:
                {
                    CelestialBodyComponent mainBody = K2D2_Plugin.Instance.current_vessel.currentBody();
                    var maxAtmosphereAltitude_km = (float)(mainBody.atmosphereDepth / 1000);
                    if (lift_settings.destination_Ap_km < maxAtmosphereAltitude_km)
                    {
                        EndLiftPilot(false, "Warning Ap is under Atm. limit");
                        return;
                    }

                    var altitude = (float)current_vessel.GetApproxAltitude() / 1000;
                    densityAtm = mainBody.GetPressure(altitude * 1000);

                    // compute time to reaching altitude.
                    float V_Speed = (float)current_vessel.VesselVehicle.VerticalSpeed;
                    var delta_alt = maxAtmosphereAltitude_km - altitude;

                    if (delta_alt < 0)
                    {
                        // reached 
                        status = LiftStatus.Adjust;
                        return;
                    }

                    // warp until end
                    duration_to_atm = delta_alt / V_Speed;
                    var wanted_warp_index = WarpToSettings.compute_wanted_warp_index(duration_to_atm);
                    TimeWarpTools.SetRateIndex(wanted_warp_index+1, false);
                }
                break;
            case LiftStatus.Adjust:
                {
                    float remaining_Ap = lift_settings.destination_Ap_km - ap_km;
                    if (remaining_Ap <= lift_settings.destination_Ap_km * 0.001f) // we stop at 0.1% of dest AP
                    {
                        status = LiftStatus.Circularize;
                        return;
                    }

                    SASTool.setAutoPilot(AutopilotMode.Prograde);

                    turn_to.Update();
                    if (!turn_to.finished)
                    {
                        // adjust direction
                        current_vessel.SetThrottle(0);
                        return;
                    }

                    if (delta_ap_per_second == 0)
                    {
                        // set default ap per second to high value to get a smooth adjust
                        delta_ap_per_second = 10;
                    }

                    wanted_throttle = remaining_Ap / delta_ap_per_second;
                    wanted_throttle = wanted_throttle / 2;

                    if (wanted_throttle > lift_settings.max_throttle)
                        wanted_throttle = lift_settings.max_throttle;

                    current_vessel.SetThrottle(wanted_throttle);
                }
                break;
            case LiftStatus.Ascent:
                {

                    applyDirection();
                    float remaining_Ap = lift_settings.destination_Ap_km - ap_km;
                    if (remaining_Ap <= lift_settings.destination_Ap_km * 0.01f) // we stop at 0.1% of dest AP
                    {
                        status = LiftStatus.Coasting;
                        return;
                    }
                    else
                    {
                        if (delta_ap_per_second <= 0)
                        {
                            wanted_throttle = lift_settings.max_throttle;
                        }
                        else
                        {
                            wanted_throttle = remaining_Ap / delta_ap_per_second;
                            if (wanted_throttle > lift_settings.max_throttle)
                                wanted_throttle = lift_settings.max_throttle;
                        }

                        current_vessel.SetThrottle(wanted_throttle);
                    }
                }
                break;
        }
    }

    float wanted_throttle = 0;

    public override void onGUI()
    {
        UI_Tools.Title("Lift Pilot");

        if (K2D2_Plugin.Instance.settings_visible)
        {
            K2D2Settings.onGUI();
            return;
        }

        if (show_profile)
        {
            lift_settings.destination_Ap_km = UI_Fields.IntFieldLine("lift.destination_Ap_km", "Ap Altitude", lift_settings.destination_Ap_km, 0, Int32.MaxValue, "km");

            GUILayout.BeginHorizontal();
            // GUILayout.Label("5° Alt");
            GUILayout.Label($"5° Alt. : {lift_settings.end_rotate_altitude_km:n0} km", KBaseStyle.slider_text);
            lift_settings.end_rotate_ratio = UI_Tools.FloatSlider(lift_settings.end_rotate_ratio, 0, 1);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"45° Alt. : {lift_settings.mid_rotate_altitude_km:n0} km", KBaseStyle.slider_text);
            lift_settings.mid_rotate_ratio = UI_Tools.FloatSlider(lift_settings.mid_rotate_ratio, 0, 1);
            GUILayout.EndHorizontal();

            lift_settings.start_altitude_km = UI_Fields.IntFieldLine("lift.start_altitude_km", "90° Altitude", lift_settings.start_altitude_km, 0, Int32.MaxValue, "km");
            ascentPath.drawProfile(current_altitude_km);

            lift_settings.heading = UI_Tools.HeadingControl("lift.heading", lift_settings.heading);

            GUILayout.BeginHorizontal();
            if (UI_Tools.miniButton("Hide"))
            {
                show_profile = false;
            }

            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.BeginHorizontal();
            if (UI_Tools.miniButton("Show Profile"))
            {
                show_profile = true;
            }

            GUILayout.EndHorizontal();
        }

        lift_settings.max_throttle = UI_Tools.FloatSliderTxt("Max Throttle", lift_settings.max_throttle, 0, 1);

        isRunning = UI_Tools.BigToggleButton(isRunning, "Start", "Stop");

        if (isRunning)
        {
            UI_Tools.Warning($"Status : {status}");

            switch (status)
            {
                case LiftStatus.Ascent:
                    UI_Tools.Console($"Altitude = {current_altitude_km:n2} km");
                    UI_Tools.Console($"Apoapsis Alt. = {ap_km:n2} km");

                    UI_Tools.Console($"Inclination = {wanted_elevation:n2} °");

                    UI_Tools.Console($"Last delta ap. = {delta_ap_per_second:n2} km/s");
                    UI_Tools.Console($"wanted_throttle. = {wanted_throttle:n2}");
                    break;
                case LiftStatus.Adjust:
                    UI_Tools.Console($"Altitude = {current_altitude_km:n2} km");
                    UI_Tools.Console($"Apoapsis Alt. = {ap_km:n2} km");

                    if (!turn_to.finished)
                    {
                        UI_Tools.Console(turn_to.status_line);
                        return;
                    }
                    UI_Tools.Console($"wanted_throttle. = {wanted_throttle:n2}");
                    break;
                case LiftStatus.Coasting:
                    UI_Tools.Console($"Altitude = {current_altitude_km:n2} km");
                    UI_Tools.Console($"Atm Density = {densityAtm:n2}");
                    UI_Tools.Console($"End warp : {StrTool.DurationToString(duration_to_atm)} x{TimeWarpTools.CurrentRate}");
                    break;
                case LiftStatus.Circularize:
                    UI_Tools.Console("--------------");
                    if (UI_Tools.Button("Final !"))
                    {
                        createCircleNode();
                    }

                    break;
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(end_status))
            {
                if (result_ok)
                {
                    UI_Tools.OK(end_status);
                }
                else
                {
                    UI_Tools.Warning(end_status);
                }
            }
        }
    }
}
