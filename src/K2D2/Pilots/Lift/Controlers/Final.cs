
using K2D2.KSPService;
using KSP.Sim.impl;
using KTools;
using KSP.Sim.Maneuver;
using UnityEngine;
using UnityEngine.UIElements;
using K2UI;
using K2D2.UI;
using K2D2.Controller;
using K2D2.Node;

namespace K2D2.Lift;

/// <summary>
/// rotation used for docking
/// </summary>
public class FinalCircularize : ExecuteController
{
    LiftSettings lift_settings = null;

    KSPVessel current_vessel;

    LiftPilot lift;

    public FinalCircularize(LiftPilot lift, LiftSettings lift_settings)
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

        if (lift_settings.pause_on_final.V)
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

    internal VisualElement final_grp;
    Button create_ap, create_now, run;

    public override void updateUI(VisualElement root_el, FullStatus st)
    {
        // if (UI_Tools.BigButton("Pause"))
        // {
        //     TimeWarpTools.SetIsPaused(true);
        // }

        if (create_ap == null)
        {
            final_grp = root_el.Q<VisualElement>("final_grp");
            create_ap = root_el.Q<Button>("create_ap");

            create_ap.listenClick( () => {
                    removeAllNodes();
                    createApNode();  
                });

            create_now = root_el.Q<Button>("create_now");

            create_now.listenClick( () => {
                    removeAllNodes();
                    createNowNode();  
                });

            run = root_el.Q<Button>("run");
            run.listenClick( () => {
                NodeExPilot.Instance.Start();
            });
        }

        final_grp.Show(true);

        if (!string.IsNullOrEmpty(status_msg))
        {
            st.Warning(status_msg);
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
