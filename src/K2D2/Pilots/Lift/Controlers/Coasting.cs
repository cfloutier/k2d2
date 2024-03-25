
using K2D2.KSPService;
using KSP.Sim;
using KSP.Sim.impl;
using KTools;
using UnityEngine;

namespace K2D2.Controller.Lift.Pilots;

/// <summary>
/// rotation used for docking
/// </summary>
public class Coasting : ExecuteController
{
    LiftSettings lift_settings = null;

    KSPVessel current_vessel;

    LiftController lift;

    public Coasting(LiftController lift, LiftSettings lift_settings)
    {
        current_vessel = K2D2Plugin.Instance.current_vessel;
        this.lift = lift;
        this.lift_settings = lift_settings;
    }

    TurnTo turn_to = null;

    // used during coasting
    double densityAtm = 0;
    double duration_to_atm = 0;
    public float current_altitude_km = 0;

    public override void Start()
    {
        base.Start();

        turn_to = new TurnTo();
        turn_to.StartProGrade(SpeedDisplayMode.Orbit);

        current_vessel.SetThrottle(0);
        TimeWarpTools.SetRateIndex(0, false);
        turn_to = new TurnTo();
        turn_to.StartProGrade(SpeedDisplayMode.Surface);
    }

    public override void updateUI(FullStatus st)
    {
        st.Status("Coasting");

        st.Console($"Altitude = {current_altitude_km:n2} km");
        st.Console($"Atm Density = {densityAtm:n2}");

        if (!turn_to.finished)
            st.Console(turn_to.status_line);
        else
            st.Console($"End warp : {StrTool.DurationToString(duration_to_atm)} x{TimeWarpTools.CurrentRate}");
    }

    public override void Update()
    {
        if (!lift_settings.coasting_warp.V)
            lift.NextMode();

        current_altitude_km = (float)(current_vessel.GetSeaAltitude() / 1000);
        finished = false;

        CelestialBodyComponent mainBody = K2D2Plugin.Instance.current_vessel.currentBody();
        var maxAtmosphereAltitude_km = (float)(mainBody.atmosphereDepth / 1000);
        if (lift_settings.destination_Ap_km.V < maxAtmosphereAltitude_km)
        {
            lift.EndLiftPilot(false, "Warning Ap is under Atm. limit");
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
            finished = true;
            return;
        }

        turn_to.Update();
        if (!turn_to.finished)
        {
            TimeWarpTools.SetRateIndex(0, false);
            return;
        }

        // warp until end
        duration_to_atm = delta_alt / V_Speed;
        var wanted_warp_index = WarpToSettings.compute_wanted_warp_index(duration_to_atm);
        TimeWarpTools.SetRateIndex(wanted_warp_index + 1, false);
    }
}
