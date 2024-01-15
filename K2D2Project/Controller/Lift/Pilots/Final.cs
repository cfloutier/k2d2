
using K2D2.KSPService;
using KSP.Sim.impl;
using KTools.UI;
using KTools;

namespace K2D2.Controller.Lift.Pilots;

/// <summary>
/// rotation used for docking
/// </summary>
public class FinalCircularize : ExecuteController
{
    AutoLiftSettings lift_settings = null;

    KSPVessel current_vessel;

    AutoLiftController lift;

    public FinalCircularize(AutoLiftController lift, AutoLiftSettings lift_settings)
    {
        current_vessel = K2D2_Plugin.Instance.current_vessel;
        this.lift = lift;
        this.lift_settings = lift_settings;
    }

    double wait_for = 10;
    double time_to_wait = -1;
    double remaining = -1;

    public override void Start()
    {
        base.Start();

        TimeWarpTools.SetRateIndex(0, false);
        current_vessel.SetThrottle(0);

        if (K2D2OtherModsInterface.fpLoaded)
        {
            time_to_wait = GeneralTools.Current_UT + wait_for;
            remaining = wait_for;
        }
        else
        {
            time_to_wait = -1;

            lift.EndLiftPilot(true, "Please install FlightPlan for the final Step...");
        }
    }

    void createCircleNode()
    {
        var current_time = GeneralTools.Game.UniverseModel.UniverseTime;

        if (current_vessel == null)
        {
            lift.EndLiftPilot(false, "error : no vessel");
            return;
        }

        PatchedConicsOrbit orbit = current_vessel.VesselComponent.Orbit;
        if (orbit == null)
        {
            lift.EndLiftPilot(false, "error : no orbit");
            return;
        }

        if (K2D2OtherModsInterface.instance.Circularize(current_time + orbit.TimeToAp, 0))
        {
            K2D2_Plugin.Instance.FlyNode();
            lift.EndLiftPilot(true, "Launched With FlightPlan !");
        }
        else
        {
            lift.EndLiftPilot(false, "Error Creating Node");
        }
        return;
    }

    public override void onGUI()
    {
        UI_Tools.Console("--------------");

        if (remaining >= 0)
            UI_Tools.Console($"Waiting : {StrTool.DurationToString(remaining)}");


        if (UI_Tools.Button("Final !"))
        {
            createCircleNode();
        }
    }

    public override void Update()
    {
        if (time_to_wait < 0)
            return;

        remaining = time_to_wait - GeneralTools.Current_UT;
        // if (remaining < 0)
        //     createCircleNode();
    }
}
