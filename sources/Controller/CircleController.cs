
using BepInEx.Logging;
using K2D2.Controller;
using K2D2.KSPService;
using KSP.Sim;
using KSP.Sim.impl;
using UnityEngine;
using KSP.Sim.Maneuver;
using System.Collections;
using KSP.Map;

namespace K2D2.Controller
{
    /// a simple test page to add the simple circle maneuvers node made by @mole
    class CircleController : BaseController
    {
        public ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.CircleController");

        ManeuverCreator maneuver_creator = new ManeuverCreator();

        public static CircleController Instance { get; set; }

        public CircleController()
        {
            Instance = this;
            debug_mode = false;
            Name = "Circle";
        }

        public override void Update()
        {
            maneuver_creator.Update();
        }

        IEnumerator add_Test_Node()
        {
            var current_vessel = K2D2_Plugin.Instance.current_vessel;
            PatchedConicsOrbit orbit = current_vessel.VesselComponent.Orbit;

            ManeuverPlanComponent activeVesselPlan = current_vessel.VesselComponent?.SimulationObject?.FindComponent<ManeuverPlanComponent>();
            if (activeVesselPlan == null)
            {
                 yield return null;
            }

          
            double burnUT = GeneralTools.Current_UT + orbit.TimeToAp;

            logger.LogInfo($"UT {StrTool.DurationToString(burnUT)}");

            logger.LogInfo($"UT {StrTool.DurationToString(burnUT)}");
            logger.LogInfo($"UTs {burnUT}");

            Vector3 burnDv = new Vector3(0,0,100);

            var SimulationObject = current_vessel.VesselComponent.SimulationObject;

            // Create Node
            ManeuverNodeData nodeData = new ManeuverNodeData(SimulationObject.GlobalId, false, burnUT);
            orbit.PatchEndTransition = PatchTransitionType.Maneuver;
            nodeData.SetManeuverState((PatchedConicsOrbit)orbit);

            nodeData.BurnVector = burnDv;

            // Add the node to the vessel's orbit
           // SimulationObject.ManeuverPlan.AddNode(nodeData, true);

            Game.SpaceSimulation.Maneuvers.AddNodeToVessel(nodeData);

            yield return new WaitForFixedUpdate();

            MapCore mapCore = null;
            Game.Map.TryGetMapCore(out mapCore);
            if (mapCore)
            {
                mapCore.map3D.ManeuverManager.GetNodeDataForVessels();
                mapCore.map3D.ManeuverManager.UpdatePositionForGizmo(nodeData.NodeID);
                // mapCore.map3D.ManeuverManager.UpdateAll();
                // mapCore.map3D.ManeuverManager.RemoveAll();
            }
        }

        public override void onGUI()
        {
            if (K2D2_Plugin.Instance.settings_visible)
            {
                Settings.onGUI();
                return;
            }

            if (UI_Tools.Button("test"))
            {
                K2D2_Plugin.Instance.StartCoroutine(add_Test_Node());
            }

            if (UI_Tools.Button("test Ap"))
            {
                maneuver_creator.CircularizeOrbitApoapsis();
            }

            if (UI_Tools.Button("test Pe"))
            {
                maneuver_creator.CircularizeOrbitPeriapsis();
            }



        }
    }
}