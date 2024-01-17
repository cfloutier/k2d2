

using K2D2.Controller;
using KTools;
using KTools.UI;
using KSP.Sim.impl;
using UnityEngine;

namespace K2D2.InfosPages;

class TestFlightPlan : BaseController
{
    public TestFlightPlan()
    {
        debug_mode_only = true;
        name = "Flight Plan";
    }

    public override bool isActive
    {
        get
        {
            return K2D2OtherModsInterface.fpLoaded;
        }
    }

    public static bool Circularize(double burnUT, double burnOffsetFactor = -0.5)
    {
        if (K2D2OtherModsInterface.instance.Circularize(burnUT, burnOffsetFactor))
        {
          
            return true;
        }
        return false;
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

    public override void onGUI()
    {
        FPToolsUI();
    }

    public static void FPToolsUI()
    {
        var fpLoaded = K2D2OtherModsInterface.fpLoaded;

        if (!fpLoaded)
        {
            UI_Tools.Warning("Flight Plan not detected, install this fantastic Mod to create Nodes");
            return;
        }

        UI_Tools.Console("Flight Plan is available !");
        var orbit = getOrbit();
        if (orbit == null)
        {
            UI_Tools.Error("no orbit");
            return;
        }

        GUILayout.BeginHorizontal();
        UI_Tools.Label("Circle ");

        if (UI_Tools.Button("in 30s"))
        {
            var current_time = GeneralTools.Game.UniverseModel.UniverseTime;
            Circularize(current_time + 30, 0);
        }
        if (UI_Tools.Button("at AP"))
        {
            var current_time = GeneralTools.Game.UniverseModel.UniverseTime;
            Circularize(current_time + orbit.TimeToAp, 0);
        }
        if (UI_Tools.Button("at PE"))
        {
            var current_time = GeneralTools.Game.UniverseModel.UniverseTime;
            Circularize(current_time + orbit.TimeToPe, 0);
        }

        GUILayout.EndHorizontal();
    }
}
