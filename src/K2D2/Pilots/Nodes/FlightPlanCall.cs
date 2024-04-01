using K2UI;
using KSP.Sim.impl;
using KTools;
using UnityEngine;
using UnityEngine.UIElements;

namespace K2D2.Node;

public class FlightPlanCall
{
    public FlightPlanCall(NodeExPilot pilot)
    {
        this.pilot = pilot;
    }

    public static PatchedConicsOrbit getOrbit()
    {
        var current_vessel = K2D2_Plugin.Instance.current_vessel;
        if (current_vessel == null)
        {
            // UI_Tools.Error("no vessel");
            return null;
        }

        if (current_vessel.VesselComponent == null)
        {
            //UI_Tools.Error("no vessel component");
            return null;
        }

        PatchedConicsOrbit orbit = current_vessel.VesselComponent.Orbit;
        return orbit; 
    }
  
    public bool Circularize(double burnUT, double burnOffsetFactor = -0.5)
    {
        if (K2D2OtherModsInterface.instance.Circularize(burnUT, burnOffsetFactor))
        {
            return true;
        }
        return false;
    }

    public void CircularizeAtAp()
    {
        var orbit = getOrbit();
        if (orbit == null)
        {
            Debug.LogError("No Orbit");
            return;
        }

        var current_time = GeneralTools.Game.UniverseModel.UniverseTime;
        Circularize(current_time + orbit.TimeToAp, 0);
    }
    public void CircularizeAtPe()
    {
        var orbit = getOrbit();
        if (orbit == null)
        {
            Debug.LogError("No Orbit");
            return;
        }

        var current_time = GeneralTools.Game.UniverseModel.UniverseTime;
        Circularize(current_time + orbit.TimeToAp, 0);
    }

    public void CircularizeNow()
    {
        var current_time = GeneralTools.Game.UniverseModel.UniverseTime;
        Circularize(current_time + 30, 0);
    }

    public Group flight_plan_bar;
    public Label please_install_fp;
    public VisualElement add_node_buttons_group;
    private NodeExPilot pilot;

    public void initUI(VisualElement panel)
    {
         // flight pan items
        flight_plan_bar = panel.Q<Group>("flight_plan_bar");
        please_install_fp = panel.Q<Label>("please_install_fp");
        add_node_buttons_group = panel.Q<VisualElement>("add_node_buttons_group");

        panel.Q<Button>("circle_at_ap").listenClick(CircularizeAtAp);
        panel.Q<Button>("circle_at_pe").listenClick(CircularizeAtPe);
        panel.Q<Button>("circle_now").listenClick(CircularizeNow);
    }

    public void updateUI()
    {
        bool no_maneuver = pilot.next_maneuver_node == null;
        flight_plan_bar.Show(no_maneuver);
        if (no_maneuver)
        {
            var fpLoaded = K2D2OtherModsInterface.fpLoaded;
            please_install_fp.Show(!fpLoaded);
            add_node_buttons_group.Show(fpLoaded);
        }
    }

}