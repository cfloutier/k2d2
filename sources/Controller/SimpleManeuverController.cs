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
using K2D2.sources.Models;
using KSP.Map;
using KSP2FlightAssistant.MathLibrary;
using KSP.Sim.Maneuver;


namespace K2D2.Controller
{
    public class SimpleManeuverController : ButtonController
    {

        ManualLogSource logger;
        public Rect windowRect { get; set; }
        
        public static SimpleManeuverController Instance { get; set; }
        // public GameInstance Game { get; set; }


        //private bool _circularizeApoapsis = false, _circularizePeriapsis = false, _hohmannTransfer = false;
        private string distanceHohmannS ="0", timeHohmannS="0";
        double distanceHohmann = 0, timeHohmann = 0;
        string periapsisS = "0", apoapsisS = "0";
        
        

        public ManeuverManager ManeuverManager { get; set; }
        private Maneuver _maneuver;
        
        public SimpleManeuverController(ref ManeuverManager maneuverManager)
        {
            Instance = this;
            ManeuverManager = maneuverManager;

            applicableStates.Add(GameState.FlightView);
            applicableStates.Add(GameState.Map3DView);
            

        }
        
        public SimpleManeuverController(ManualLogSource logger, ref ManeuverManager maneuverManager):this(ref maneuverManager)
        {
            this.logger = logger;
            logger.LogMessage("SimpleManeuverController !");
        }
        

        public override void Update()
        {
            _maneuver = new Maneuver(Game, logger);
        }

        public override void onGUI()
        {
            if (_maneuver == null)
            {
                return;
            }
/*
            if (Settings.debug_mode)
            {
                DebugInformation();
            }

*/

            if (GUILayout.Button("Circularize Node in Apoapsis"))
            {
                try
                {
                    
                    ManeuverManager.AddManeuver("Circularize Apoapsis",new Action(() => _maneuver.CircularizeOrbitApoapsis()));
                    //double deltaV = _maneuver.CircularizeOrbitApoapsis();
                    //GUILayout.Label($"Required dV: {deltaV}");
                    
                }
                catch (Exception e)
                {
                    logger.LogError("Apoapsis Error");
                    logger.LogError(e.Message);
                }
                return;
            }
            

            if (GUILayout.Button("Circularize Node in Periapsis"))
            {
                try
                {
                    
                    ManeuverManager.AddManeuver("Circularize Periapsis",new Action(() => _maneuver.CircularizeOrbitPeriapsis()));
                    //double deltaV = _maneuver.CircularizeOrbitPeriapsis();
                    //GUILayout.Label($"Required dV: {deltaV}");
                   
                }
                catch (Exception e)
                {
                    logger.LogError("Periapsis Error");
                    logger.LogError(e.Message);
                }

                return;
            }
            
            if(GUILayout.Button("Start Maneuver"))
            {
                ManeuverManager.StartManeuver();
                
                
            }
            foreach (string description in ManeuverManager.GetDescriptionOfAllManeuvers())
            {
                GUILayout.Label(description);
            }

            GUILayout.BeginHorizontal();
            GUILayout.BeginScrollView(new Vector2(0, 0), new GUIStyle(), new GUIStyle());
            foreach (string description in ManeuverManager.GetDescriptionOfAllManeuvers())
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(description);
                GUILayout.EndHorizontal();

            }
            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();

                /*
                GUILayout.Label("Hohmann Transfer Distance (km):");
                distanceHohmannS = GUILayout.TextField(distanceHohmannS);
                GUILayout.Label("Hohmann Transfer Time (s):");
                timeHohmannS = GUILayout.TextField(timeHohmannS);
    
    
    
                if (GUILayout.Button("Hohmann Transfer"))
                {
                    distanceHohmann = GeneralTools.GetNumberString(distanceHohmannS);
                    timeHohmann = GeneralTools.GetNumberString(timeHohmannS);
                    if (distanceHohmann < 0 || timeHohmann < 0)
                    {
                        GUILayout.Label("Invalid input");
                        logger.LogError("Invalid input: Hohmann Transfer");
                        return;
                    }
    
                    try
                    {
    
                        
                        double UT = timeHohmann + Game.UniverseModel.UniversalTime;
                        logger.LogMessage("Hohmann Time: " + UT);
    
                        //double deltaV = _maneuver.ChangePeriapsis(Math.Abs(distanceHohmann));//_maneuver.HohmannTransfer(UT, distance);
    
                        //GUILayout.Label($"Required dV: {deltaV}");
                        
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e.Message);
                    }
                }
                
                GUILayout.Label("Orbit Periapsis (km):");
                periapsisS = GUILayout.TextField(periapsisS);
                GUILayout.Label("Orbit Apoapsis (km):");
                apoapsisS = GUILayout.TextField(apoapsisS);
    
                if (GUILayout.Button("Set Orbit"))
                {
                    double periapsis = GeneralTools.GetNumberString(periapsisS);
                    double apoapsis = GeneralTools.GetNumberString(apoapsisS);
                    
                    if (periapsis < 0 || apoapsis < 0)
                    {
                        GUILayout.Label("Invalid input");
                        logger.LogError("Invalid input: Set Orbit");
                        return;
                    }
                    
                    if (periapsis > apoapsis)
                    {
                        GUILayout.Label("Invalid input");
                        logger.LogError("Invalid input: Set Orbit");
                        return;
                    }
    
                    try
                    {
                        _maneuver.ChangeApoapsis(2500000);
                        _maneuver.ChangePeriapsis(1500000);
    
                        
                        //_maneuver.CreateOrbit(apoapsis, periapsis);
    
    
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e.Message);
                    }
                    
                }
                */
            //PopUp();
            Run();
        }


        /*public void DebugInformation()
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

        public void PopUpAwake()
        {
            windowRect = new Rect(0, 0, 200, 400);
        }
        
        public void PopUp()
        {
            windowRect = GUILayout.Window(GUIUtility.GetControlID(FocusType.Passive), (Rect)windowRect, PopOpContent, "<color=#00D346>K2-D2</color>", Styles.window, GUILayout.Height(200), GUILayout.Width(400));
        }

        

        public void PopOpContent(int windowID)
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Label("A pop up window");
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }*/
    }
}