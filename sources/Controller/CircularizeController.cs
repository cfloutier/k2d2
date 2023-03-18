using System;
using System.Collections.Generic;
using AwesomeTechnologies.VegetationSystem;
using BepInEx.Logging;
using JetBrains.Annotations;
using UnityEngine;
using KSP.Game;
using K2D2.KSPService;
using K2D2;
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
            
            /*buttons.Add(new Switch());
            buttons.Add(new Button());*/
            
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
            logger.LogMessage("CircularizeController Reinitialize !");
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
                $"Circularize Node {_vessel.GetDisplayAltitude()}");
            
            GUILayout.Label($"Periapsis: {_vessel.getPeriapsis().ToString()}");
            GUILayout.Label($"Apoapsis: {_vessel.getApoapsis().ToString()}");
            GUILayout.Label($"Current Orbit Height: {_vessel.getCurrenOrbitHeight().ToString()}");
            GUILayout.Label($"Current Orbit Speed: {_vessel.getCurrentOrbitSpeed().ToString()}");
            GUILayout.Label($"Planetary Mass: {VisVivaEquation.CalculateGravitation(_vessel.getCurrenOrbitHeight(), _vessel.getApoapsis(), _vessel.getPeriapsis(), _vessel.getCurrentOrbitSpeed()).ToString()}");

            

            if (GUILayout.Button("Circularize Node", Styles.button, GUILayout.Height(40)))
            {
                _circularize = !_circularize;
            }
            Run();
        }
    }
}