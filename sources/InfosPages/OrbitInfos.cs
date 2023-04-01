
using BepInEx.Logging;
using K2D2.KSPService;
using KSP.Sim;
using KSP.Sim.impl;
using UnityEngine;

namespace K2D2.InfosPages
{
    class OrbitInfos
    {
        public static void onGUI()
        {

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
            UI_Tools.Console($"period {GeneralTools.DurationToString(orbit.period)}");


            if (orbit.PatchEndTransition == PatchTransitionType.Collision)
            {
                var dt = GeneralTools.Game.UniverseModel.UniversalTime - orbit.collisionPointUT;
                UI_Tools.Console($"collision in  {GeneralTools.DurationToString(dt)}");
            }

            GUILayout.Label("todo");
        }
    }
}