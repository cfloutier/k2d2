
using K2D2.KSPService;
using KSP.Sim.impl;
using KTools.UI;
using KTools;
using KSP.Sim.Maneuver;
using UnityEngine;


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

    string status_msg = "";

    public override void Start()
    {
        base.Start();

        TimeWarpTools.SetRateIndex(0, false);
        current_vessel.SetThrottle(0);

        if (lift_settings.pause_on_final)
            TimeWarpTools.SetIsPaused(true);

        if (!K2D2OtherModsInterface.fpLoaded)
        {        
            lift.EndLiftPilot(true, "Please install FlightPlan for the final Step...");
        }

        status_msg = "";
    }

    PatchedConicsOrbit getOrbit()
    {
        if (current_vessel == null)
        {
            status_msg = "error : no vessel";
            return null;
        }

        PatchedConicsOrbit orbit = current_vessel.VesselComponent.Orbit;
        if (orbit == null)
        {
            status_msg = "error : no orbit";
            return null;
        }

        return orbit;
    }

    void createApNode()
    {
        var current_time = GeneralTools.Game.UniverseModel.UniverseTime;

        var orbit = getOrbit();
        if (orbit == null)
            return;

        lift.logger.LogMessage($"Circularize TimeToAp = {orbit.TimeToAp}");
        if (!K2D2OtherModsInterface.instance.Circularize(current_time + orbit.TimeToAp, 0))
        {    
            status_msg = "Error Creating Node";
        }
        
        return;
    }

    void createNowNode()
    {
        var current_time = GeneralTools.Game.UniverseModel.UniverseTime;
        if (!K2D2OtherModsInterface.instance.Circularize(current_time + 30, 0))
        {    
            status_msg = "Error Creating Node";
        }
        
        return;
    }
    


    public override void onGUI()
    {
        // if (UI_Tools.BigButton("Pause"))
        // {
        //     TimeWarpTools.SetIsPaused(true);
        // }
            

        GUILayout.BeginHorizontal();

        if (UI_Tools.Button("Create Node at AP"))
        {
            removeAllNodes();
            createApNode();
        }

        if (UI_Tools.Button("Create Node Now"))
        {
            removeAllNodes();
            createNowNode();
        }

        GUILayout.EndHorizontal();

        if (AutoExecuteManeuver.Instance.current_maneuver_node != null)
        {
            if (UI_Tools.Button("Execute"))
            {
                TimeWarpTools.SetIsPaused(false);
                AutoExecuteManeuver.Instance.Start();
            }
        }

        if (!string.IsNullOrEmpty(status_msg))
        {
            UI_Tools.Warning(status_msg);
        }
    }

    void removeAllNodes()
    {
        ManeuverPlanComponent maneuvers_component = current_vessel?.VesselComponent?.SimulationObject.FindComponent<ManeuverPlanComponent>();
        if (maneuvers_component == null)
        {
            lift.logger.LogWarning("no ManeuverPlanComponent");
            return;
        }
        List<ManeuverNodeData> nodes = maneuvers_component.GetNodes();
        if (nodes == null)
        {
            lift.logger.LogWarning("no ManeuverPlanComponent");
            return;
        }
            
        maneuvers_component.RemoveNodes(nodes);
    }

    public override void Update()
    {
       
    }
}
