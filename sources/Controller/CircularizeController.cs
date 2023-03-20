using System;
using System.Collections.Generic;
using AwesomeTechnologies.VegetationSystem;
using BepInEx.Logging;
using JetBrains.Annotations;
using UnityEngine;
using KSP.Game;
using K2D2.KSPService;
using K2D2;
using KSP.Map;
using KSP2FlightAssistant.MathLibrary;


namespace K2D2.Controller
{
    public class CircularizeController : BaseController
    {
        ManualLogSource logger;
        public static CircularizeController Instance { get; set; }
        public GameInstance Game { get; set; }
        
        
        private bool _circularize = false;

        private KSPVessel _vessel;

        public CircularizeController(ManualLogSource logger)
        {
            Instance = this;

            this.logger = logger;
            logger.LogMessage("CircularizeController !");
            
            buttons.Add(new Switch());
            //buttons.Add(new Button());*/
            
            applicableStates.Add(GameState.FlightView);
            applicableStates.Add(GameState.Map3DView);
        }
       
        
        /*public override void Reinitialize()
        {
            logger.LogMessage("CircularizeController Reinitialize !");
            Game = Tools.Game();
            _vessel = new KSPVessel(Game);
        }*/
        
        public override void Update()
        {
            //logger.LogMessage("CircularizeController Reinitialize !");
            Game = Tools.Game();
            _vessel = new KSPVessel(Game);
        }

        public override void onGUI()
        {


            if(_vessel==null)
            {
                _vessel = new KSPVessel(Game);
                return;
            }

            GUILayout.Label(
                $"Altitude {_vessel.GetDisplayAltitude()}");
            
            GUILayout.Label($"Periapsis: {_vessel.getPeriapsis().ToString()}");
            GUILayout.Label($"Apoapsis: {_vessel.getApoapsis().ToString()}");
            GUILayout.Label($"Current Orbit Height: {_vessel.getCurrenOrbitHeight().ToString()}");
            GUILayout.Label($"Current Orbit Speed: {_vessel.getCurrentOrbitSpeed().ToString()}");
            GUILayout.Label($"Planetary Mass: {VisVivaEquation.CalculateGravitation(_vessel.getCurrenOrbitHeight(), _vessel.getApoapsis(), _vessel.getPeriapsis(), _vessel.getCurrentOrbitSpeed()).ToString()}");
            GUILayout.Label($"ZUP Vector: {_vessel.VesselComponent.Orbit.GetFrameVelAtUTZup(_vessel.VesselComponent.Orbit.GetUTforTrueAnomaly(180,0)).x}");
            GUILayout.Label($"ZUP Vector: {_vessel.VesselComponent.Orbit.GetFrameVelAtUTZup(_vessel.VesselComponent.Orbit.GetUTforTrueAnomaly(180,0)).y}");
            GUILayout.Label($"ZUP Vector: {_vessel.VesselComponent.Orbit.GetFrameVelAtUTZup(_vessel.VesselComponent.Orbit.GetUTforTrueAnomaly(180,0)).z}");
            GUILayout.Label($"Longitude of Ascending Node: {_vessel.VesselComponent.Orbit.longitudeOfAscendingNode}");
            GUILayout.Label($"Inclination: {_vessel.VesselComponent.Orbit.inclination}");
            

            if (GUILayout.Button("Circularize Node", Styles.button, GUILayout.Height(40)))
            {
                _circularize = !_circularize;
            }
            
            if (_circularize)
            {
               // Vector3d myBurnVector = new Vector3d(0, 100, 100);
                //_vessel.CreateManeuverNode(myBurnVector,180);//Game.UniverseModel.UniversalTime+100
                _vessel.CircularizeOrbit();
                _circularize = !_circularize;
            }
            Run();
        }
    }
}