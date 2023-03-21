using System;
using System.Collections.Generic;
using System.Reflection.Emit;
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


        private bool _circularizeApoapsis,_circularizePeriapsis = false;


        private Maneuver _maneuver;

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


        public override void Update()
        {
            //logger.LogMessage("CircularizeController Reinitialize !");
            Game = Tools.Game();

            _maneuver = new Maneuver(Game);
        }

        public override void onGUI()
        {
            if (_maneuver == null)
            {
                return;
            }

            /*if (Game.GlobalGameState.GetState() != GameState.Map3DView)
            {
                GUILayout.Label("Map must be opened to use this feature");
                return;
            }*/


            if (Settings.debug_mode)
            {
                GUILayout.Label(
                    $"Debug Mode: {Settings.debug_mode}");
                GUILayout.Label(
                    $"Altitude {_maneuver.kspVessel.GetDisplayAltitude()}");

                GUILayout.Label($"Periapsis: {_maneuver.kspVessel.getPeriapsis().ToString()}");
                GUILayout.Label($"Apoapsis: {_maneuver.kspVessel.getApoapsis().ToString()}");
                GUILayout.Label($"Current Orbit Height: {_maneuver.kspVessel.getCurrenOrbitHeight().ToString()}");
                GUILayout.Label($"Current Orbit Speed: {_maneuver.kspVessel.getCurrentOrbitSpeed().ToString()}");
                GUILayout.Label(
                    $"Planetary Mass: {VisVivaEquation.CalculateGravitation(_maneuver.kspVessel.getCurrenOrbitHeight(), _maneuver.kspVessel.getApoapsis(), _maneuver.kspVessel.getPeriapsis(), _maneuver.kspVessel.getCurrentOrbitSpeed()).ToString()}");
                GUILayout.Label(
                    $"ZUP Vector: {_maneuver.kspVessel.VesselComponent.Orbit.GetFrameVelAtUTZup(_maneuver.kspVessel.VesselComponent.Orbit.GetUTforTrueAnomaly(180, 0)).x}");
                GUILayout.Label(
                    $"ZUP Vector: {_maneuver.kspVessel.VesselComponent.Orbit.GetFrameVelAtUTZup(_maneuver.kspVessel.VesselComponent.Orbit.GetUTforTrueAnomaly(180, 0)).y}");
                GUILayout.Label(
                    $"ZUP Vector: {_maneuver.kspVessel.VesselComponent.Orbit.GetFrameVelAtUTZup(_maneuver.kspVessel.VesselComponent.Orbit.GetUTforTrueAnomaly(180, 0)).z}");
                GUILayout.Label(
                    $"Longitude of Ascending Node: {_maneuver.kspVessel.VesselComponent.Orbit.longitudeOfAscendingNode}");
                GUILayout.Label($"Inclination: {_maneuver.kspVessel.VesselComponent.Orbit.inclination}");
            }

            
            if (GUILayout.Button("Circularize Node in Apoapsis", Styles.button, GUILayout.Height(40)))
            {
                _circularizeApoapsis = !_circularizeApoapsis;
            }

            if (_circularizeApoapsis)
            {
                try
                {
                    double deltaV = _maneuver.CircularizeOrbitApoapsis();
                    GUILayout.Label($"Required dV: {deltaV}");
                    _circularizeApoapsis = !_circularizeApoapsis;
                }
                catch (Exception e)
                {
                    logger.LogError(e.Message);
                }

                return;
            }
            
            
            if (GUILayout.Button("Circularize Node in Periapsis", Styles.button, GUILayout.Height(40)))
            {
                _circularizePeriapsis = !_circularizePeriapsis;
            }

            if (!_circularizePeriapsis)
            {
                try
                {
                    double deltaV = _maneuver.CircularizeOrbitPeriapsis();
                    GUILayout.Label($"Required dV: {deltaV}");
                    _circularizePeriapsis = !_circularizePeriapsis;
                }
                catch (Exception e)
                {
                    logger.LogError(e.Message);
                }
                
            }

            Run();
        }
    }
}