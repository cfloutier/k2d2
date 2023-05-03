
using K2D2.Controller;
using KSP.Sim;
using KSP.Sim.impl;

using K2D2.UI;

namespace K2D2.InfosPages;

class OrbitInfos : BaseController
{

    public OrbitInfos()
    {
        debug_mode = true;
        Name = "Orbit Infos";
    }


    public override void onGUI()
    {
        if (K2D2_Plugin.Instance.settings_visible)
        {
            Settings.onGUI();
            return;
        }


        var current_vessel = K2D2_Plugin.Instance.current_vessel;
        PatchedConicsOrbit orbit = current_vessel.VesselComponent.Orbit;

        UI_Tools.Title("// Orbit Infos");

        if (orbit.referenceBody != null)
            UI_Tools.Console($"ref Body {orbit.referenceBody.Name}");
        else
            UI_Tools.Console($"no ref body");


        UI_Tools.Console($"eccentricity {orbit.eccentricity:n3}");
        UI_Tools.Console($"inclination {orbit.inclination:n3}");
        UI_Tools.Console($"semiMajorAxis {orbit.semiMajorAxis:n3}");
        UI_Tools.Console($"longitudeOfAscendingNode {orbit.longitudeOfAscendingNode:n3}");
        UI_Tools.Console($"argumentOfPeriapsis {orbit.argumentOfPeriapsis:n3}");
        UI_Tools.Console($"meanAnomalyAtEpoch {orbit.meanAnomalyAtEpoch:n3}");
        UI_Tools.Console($"epoch {orbit.epoch:n3}");
        UI_Tools.Console($"period {StrTool.DurationToString(orbit.period)}");

        if (orbit.PatchEndTransition == PatchTransitionType.Collision)
        {
            var dt = GeneralTools.Game.UniverseModel.UniversalTime - orbit.collisionPointUT;
            UI_Tools.Console($"collision in  {StrTool.DurationToString(dt)}");
        }
        else if (orbit.PatchEndTransition == PatchTransitionType.Escape)
        {
            UI_Tools.Console($"Leaving SOI in {StrTool.DurationToString(orbit.timeToTransition2)}");
        }
        else if (orbit.PatchEndTransition == PatchTransitionType.Encounter)
        {
            UI_Tools.Console($"Entering SOI in {StrTool.DurationToString(orbit.timeToTransition2)}");
        }
    }
}
