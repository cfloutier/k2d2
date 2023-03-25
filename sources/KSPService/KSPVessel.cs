using KSP.Game;
using KSP.Sim;
using KSP.Sim.impl;
using KSP.Sim.State;
using System;
using BepInEx.Logging;
using KSP.Api;
using KSP.Sim;
using KSP.Sim.impl;
using KSP.Sim.Maneuver;
using KSP.Sim.State;
using KSP.Game;
using KSP.Map;
using KSP2FlightAssistant.KSPService;
using KSP2FlightAssistant.MathLibrary;
using UnityEngine;

namespace K2D2.KSPService
{
    public class KSPVessel
    {
        public GameInstance Game { get; set; }


        private TelemetryDataProvider telemetryDataProvider;

        public VesselComponent VesselComponent;
        private FlightCtrlState flightCtrlState;
        private VesselDataProvider VesselDataProvider;




        public KSPVessel(GameInstance Game)
        {
            this.Game = Game;
            VesselComponent = GetActiveSimVessel();
            VesselDataProvider = this.Game.ViewController.DataProvider.VesselDataProvider;

            telemetryDataProvider = this.Game.ViewController.DataProvider.TelemetryDataProvider;


        }
        
        public void Update()
        {
            VesselComponent = GetActiveSimVessel();
            VesselDataProvider = this.Game.ViewController.DataProvider.VesselDataProvider;

            telemetryDataProvider = this.Game.ViewController.DataProvider.TelemetryDataProvider;
        }

        //==================================================================================================================

        /// <summary>
        /// Retrieve the active vessel from the game
        /// </summary>
        /// <returns> VesselComponent</returns>
        public VesselComponent GetActiveSimVessel()
        {
            return Game.ViewController.GetActiveSimVessel();
        }


        // Available Instructions===========================================================================================


        // Values
        public void SetThrottle(float throttle)
        {
            flightCtrlState.mainThrottle = throttle;
        }

        public void SetPitch(float pitch)
        {
            flightCtrlState.pitch = pitch;
        }

        public void SetRoll(float roll)
        {
            flightCtrlState.roll = roll;
        }

        public void SetYaw(float yaw)
        {
            flightCtrlState.yaw = yaw;
        }


        public void SetWheelSteer(float wheelSteer)
        {
            flightCtrlState.wheelSteer = wheelSteer;
        }


        public void SetWheelThrottle(float wheelThrottle)
        {
            flightCtrlState.wheelThrottle = wheelThrottle;
        }


        // Trims

        public void SetPitchTrim(float pitchTrim)
        {
            flightCtrlState.pitchTrim = pitchTrim;
        }

        public void SetYawTrim(float yawTrim)
        {
            flightCtrlState.yawTrim = yawTrim;
        }

        public void SetRollTrim(float rollTrim)
        {
            flightCtrlState.rollTrim = rollTrim;
        }

        public void SetWheelSteerTrim(float wheelSteerTrim)
        {
            flightCtrlState.wheelSteerTrim = wheelSteerTrim;
        }

        // Input

        public void SetInputPitch(float inputPitch)
        {
            flightCtrlState.inputPitch = inputPitch;
        }

        public void SetInputRoll(float inputRoll)
        {
            flightCtrlState.inputRoll = inputRoll;
        }

        public void SetInputYaw(float inputYaw)
        {
            flightCtrlState.inputYaw = inputYaw;
        }






        //==================================================================================================================

        /// <summary>
        /// Start a set of instructions. Has to be called before any other instruction.
        /// See Instruction Set for all available instructions.
        /// Wipes the previous instruction set.
        /// Fetches the VesselComponent.
        /// To Execute the instruction set call ExecuteControlInstruction()
        /// </summary>
        public void StartControlInstruction()
        {
            // Get the active Vessel
            VesselComponent = GetActiveSimVessel();
            // Create a new Flight Instruction set of typ: FlightCtrlState
            flightCtrlState = new FlightCtrlState()
            {
                mainThrottle = VesselComponent.flightCtrlState.mainThrottle,
            };
        }

        /// <summary>
        /// Executes the instruction set.
        /// </summary>

        public void ExecuteControlInstruction()
        {
            // Send the new flight instructions to the Vessel
            VesselComponent.SetFlightControlState(flightCtrlState);
            // Sync the VesselDataProvider and VesselComponent
            VesselDataProvider.SyncTo(VesselComponent);
            // Execute the instruction
            Game.ViewController.DataProvider.ActiveVessel.SetValueInternal(VesselDataProvider);
        }

        // =================================================================================================================

        // AutoPilot========================================================================================================
        // Note: No Instruction set needed can be called as is

        /// <summary>
        /// Enables or disables the SAS
        /// 
        /// </summary>
        /// <param name="sas"></param>
        public void SetSAS(bool sas)
        {
            Game.ViewController.DataProvider.TelemetryDataProvider.SASRetrograde.SetValueInternal(sas);
        }


        // Telemetry Data===================================================================================================

        /// <summary>
        /// Retrieve the current activation value of yaw
        /// </summary>
        public double GetYaw()
        {
            return Game.ViewController.GetActiveSimVessel().flightCtrlState.yaw;
        }


        /// <summary>
        /// Retrieve the current activation value of pitch
        /// </summary>
        public double GetPitch()
        {
            return Game.ViewController.GetActiveSimVessel().flightCtrlState.pitch;
        }

        /// <summary>
        /// Retrieve the current activation value of roll
        /// </summary>
        public double GetRoll()
        {
            return Game.ViewController.GetActiveSimVessel().flightCtrlState.roll;
        }

        /// <summary>
        /// Retrieve the current activation value of throttle
        /// </summary>
        public double GetThrottle()
        {
            return Game.ViewController.GetActiveSimVessel().flightCtrlState.mainThrottle;
        }

        /// <summary>
        /// Retrieve the current activation value of wheelSteer
        /// </summary>
        public double GetWheelSteer()
        {
            return Game.ViewController.GetActiveSimVessel().flightCtrlState.wheelSteer;
        }

        /// <summary>
        /// Retrieve the current activation value of wheelThrottle
        /// </summary>
        public double GetWheelThrottle()
        {
            return Game.ViewController.GetActiveSimVessel().flightCtrlState.wheelThrottle;
        }

        /// <summary>
        /// Retrieve the current activation value of pitchTrim
        /// </summary>
        public double GetPitchTrim()
        {
            return Game.ViewController.GetActiveSimVessel().flightCtrlState.pitchTrim;
        }

        /// <summary>
        /// Retrieve the current activation value of yawTrim
        /// </summary>
        public double GetYawTrim()
        {
            return Game.ViewController.GetActiveSimVessel().flightCtrlState.yawTrim;
        }

        /// <summary>
        /// Retrieve the current activation value of rollTrim
        /// </summary>
        public double GetRollTrim()
        {
            return Game.ViewController.GetActiveSimVessel().flightCtrlState.rollTrim;
        }

        /// <summary>
        /// Retrieve the current activation value of wheelSteerTrim
        /// </summary>
        public double GetWheelSteerTrim()
        {
            return Game.ViewController.GetActiveSimVessel().flightCtrlState.wheelSteerTrim;
        }

        /// <summary>
        /// Retrieve the current activation value of inputPitch
        /// </summary>
        public double GetInputPitch()
        {
            return Game.ViewController.GetActiveSimVessel().flightCtrlState.inputPitch;
        }

        /// <summary>
        /// Retrieve the current activation value of inputRoll
        /// </summary>
        public double GetInputRoll()
        {
            return Game.ViewController.GetActiveSimVessel().flightCtrlState.inputRoll;
        }

        /// <summary>
        /// Retrieve the current activation value of inputYaw
        /// </summary>
        public double GetInputYaw()
        {
            return Game.ViewController.GetActiveSimVessel().flightCtrlState.inputYaw;
        }


        // =================================================================================================================


        // General Vessel Data===================================================================================================

        public AltimeterDisplayMode GetAltimeterDisplayMode()
        {
            return Game.ViewController.DataProvider.TelemetryDataProvider.AltimeterDisplayMode.GetValue();
        }

        public double GetDisplayAltitude()
        {
            AltimeterDisplayMode altimeterDisplayMode = GetAltimeterDisplayMode();
            return telemetryDataProvider.GetAltitudeDisplayValue(altimeterDisplayMode);
        }


        /// <summary>
        /// Returns the velocity of the vessel that is displayed HUD.
        /// </summary>
        public double GetDisplaySpeed()
        {
            SpeedDisplayMode speedDisplayMode =
                Game.ViewController.DataProvider.TelemetryDataProvider.SpeedDisplayMode.GetValue();
            return telemetryDataProvider.GetSpeedDisplayValue(speedDisplayMode);
        }

        /// <summary>
        /// Can be Target, Surface or Orbit.
        /// </summary>
        public SpeedDisplayMode GetSpeedDisplayMode()
        {
            return Game.ViewController.DataProvider.TelemetryDataProvider.SpeedDisplayMode.GetValue();
        }
        
        public double getApoapsis()
        {
            return VesselComponent.Orbit.Apoapsis;
        }
        
        public double getPeriapsis()
        {
            return VesselComponent.Orbit.Periapsis;
        }
        
        public double getCurrenOrbitHeight()
        {
            return VesselComponent.Orbit.radius;

        }
        
        public double getCurrentOrbitSpeed()
        {
            return VesselComponent.Orbit.orbitalSpeed;
        }
        
        public double getEccentricity()
        {
            return VesselComponent.Orbit.eccentricity;
        }
        
        public double getInclination()
        {
            return VesselComponent.Orbit.inclination;
        }
        
        public Vector getOrbitalVelocity()
        {
            return VesselComponent.OrbitalVelocity;
        }
        
        
        

        // =================================================================================================================
        
        public IGGuid GetGlobalIDActiveVessel()
        {
            return VesselComponent.SimulationObject.GlobalId;
        }
        
        
        // Maneuver Node===================================================================================================
        public void AddManeuverNode(ManeuverNodeData maneuverNodeData)
        {
            Game.SpaceSimulation.Maneuvers.AddNodeToVessel(maneuverNodeData);
            MapCore mapCore = null;
            Game.Map.TryGetMapCore(out mapCore);
            
            mapCore.map3D.ManeuverManager.CreateGizmoForLocation(maneuverNodeData);
            //Game.SpaceSimulation.
            //VesselComponent.OnManeuverNodePositionChanged();
            //Game.SpaceSimulation.Maneuvers.
            //AddComponent()
        }
        
        public void CreateManeuverNode(Vector3d burnVector, double TrueAnomaly)
        {
            double TrueAnomalyRad = TrueAnomaly * Math.PI / 180;
            double UT = VesselComponent.Orbit.GetUTforTrueAnomaly(TrueAnomalyRad,0);
            
            ManeuverNodeData maneuverNodeData = new ManeuverNodeData(GetGlobalIDActiveVessel(),true, UT );
            IPatchedOrbit orbit = VesselComponent.Orbit;
            
            orbit.PatchStartTransition = PatchTransitionType.Maneuver;
            
            maneuverNodeData.SetManeuverState((PatchedConicsOrbit)orbit);
          
            maneuverNodeData.BurnVector = burnVector;
            AddManeuverNode(maneuverNodeData);
        }
        
        
        
        public void CircularizeOrbit(ManualLogSource logger)
        {
            double gravitation = VesselComponent.Orbit.ReferenceBodyConstants.StandardGravitationParameter;
            double circularizedVelocity= VisVivaEquation.CalculateVelocity(VesselComponent.Orbit.Apoapsis, VesselComponent.Orbit.Apoapsis,
                VesselComponent.Orbit.Apoapsis, gravitation);
            
            double apoapsisVelocity = VisVivaEquation.CalculateVelocity(VesselComponent.Orbit.Apoapsis, VesselComponent.Orbit.Apoapsis,
                VesselComponent.Orbit.Periapsis, gravitation);
            double UT = VesselComponent.Orbit.GetUTforTrueAnomaly(180,0);
            double deltaV = circularizedVelocity - apoapsisVelocity;
            logger.LogMessage(deltaV);

            Vector3d burnVector = new Vector3d(0,0,deltaV);
            CreateManeuverNode(burnVector, 180);
        }
    }
}