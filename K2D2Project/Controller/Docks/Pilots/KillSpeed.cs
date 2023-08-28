using K2D2.KSPService;
using KSP.Sim;
using KSP.Sim.Maneuver;
using KTools;
using KTools.UI;
using UnityEngine;
using static K2D2.Controller.DroneController;
using static KSP.Api.UIDataPropertyStrings.View;

namespace K2D2.Controller.Docks.Pilots;

/// <summary>
/// rotation used for docking
/// </summary>
public class KillSpeed : ExecuteController
{
    TurnTo turnTo =  new TurnTo();
    KSPVessel current_vessel;
    BurndV burn_dV = new BurndV();

    public override void Start()
    {

        current_vessel = K2D2_Plugin.Instance.current_vessel;
        finished = false;
        turnTo.StartRetroSpeed();
    }

    public override void Update()
    {
        turnTo.Update();
        burn_dV.Update();

        finished = false;

        if (turnTo.finished)
        {
            // compute trust
            
            
            compute_Throttle();

        }
        else
            current_vessel.SetThrottle(0);
    }

    void compute_Throttle()
    {
        //float min_throttle = 0;

        var target_direction_factor = Mathf.Cos(turnTo.angle * Mathf.Deg2Rad);

        Vector target_vel = current_vessel.VesselComponent.TargetVelocity;
        float current_speed = (float)target_vel.magnitude * target_direction_factor;

        float remaining_full_burn_time = (float)(current_speed / burn_dV.full_dv);
        var wanted_throttle = Mathf.Clamp(remaining_full_burn_time, 0, 1);

        if (current_speed < 2)
        {
            finished = true;
            current_vessel.SetThrottle(0);
        }
        else
        {
            // no stop for gravity compensation
            current_vessel.SetThrottle(wanted_throttle);
        }
    }

    public override void onGUI()
    {
        UI_Tools.Warning("Kill Target Speed");



        turnTo.onGUI();
    }
}
