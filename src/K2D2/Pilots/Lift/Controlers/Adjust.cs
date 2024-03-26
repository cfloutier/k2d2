
using K2D2.KSPService;
using KSP.Sim;
// using KSP.Sim.impl;
// using UnityEngine;
using UnityEngine.UIElements;

namespace K2D2.Controller.Lift.Pilots;

/// <summary>
/// rotation used for docking
/// </summary>
public class Adjust : ExecuteController
{
    LiftSettings lift_settings = null;
    Ascent ascent = null;

    TurnTo turn_to = null;
    KSPVessel current_vessel;

    float wanted_throttle = 0;

    public Adjust(LiftSettings lift_settings, Ascent ascent)
    {
        current_vessel = K2D2Plugin.Instance.current_vessel;
        this.lift_settings = lift_settings;
        this.ascent = ascent;
    }

    public override void Start()
    {
        current_vessel.SetThrottle(0);
        TimeWarpTools.SetRateIndex(0, false);
        turn_to = new TurnTo();
        turn_to.StartProGrade(SpeedDisplayMode.Surface);
    }

    public override void updateUI(VisualElement root_el, FullStatus st)
    {
        st.Console($"Altitude = {ascent.current_altitude_km:n2} km");
        st.Console($"Apoapsis Alt. = {ascent.ap_km:n2} km");

        if (!turn_to.finished)
            st.Console(turn_to.status_line);       
        else
            st.Console($"wanted_throttle. = {wanted_throttle:n2}");
    }

    public override void Update()
    {
        if (!lift_settings.adjust.V)
        {
            finished = true;
            return;
        }


        finished = false;
        ascent.computeValues(false);

        float remaining_Ap = lift_settings.destination_Ap_km.V - ascent.ap_km;
        if (remaining_Ap <= lift_settings.end_adjust_error) // we stop at 0.1% of dest AP
        {
            finished = true;
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

        float delta_ap_per_second = ascent.delta_ap_per_second;

        if (delta_ap_per_second == 0)
        {
            // set default ap per second to high value to get a smooth adjust
            delta_ap_per_second = 5;
        }

        wanted_throttle = remaining_Ap / delta_ap_per_second;
        // wanted_throttle = wanted_throttle / 2;

        if (wanted_throttle > lift_settings.max_throttle.V)
            wanted_throttle = lift_settings.max_throttle.V;

        current_vessel.SetThrottle(wanted_throttle);
    }
}
