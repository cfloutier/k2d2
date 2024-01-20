using KSP.Game;
using KSP.Sim;
using KSP.Sim.impl;
using KSP.Sim.Maneuver;
using KSP.Sim.State;
using UnityEngine;

namespace K2D2.KSPService
{
    public class KSPVessel
    {
        public GameInstance Game => GameManager.Instance == null ? null : GameManager.Instance.Game;

        private TelemetryDataProvider telemetryDataProvider;

        public VesselComponent VesselComponent;

        public VesselVehicle VesselVehicle;

        private FlightCtrlState flightCtrlState;
        private VesselDataProvider VesselDataProvider;

        public KSPVessel()
        {
            // VesselComponent = GetActiveSimVessel();
            // VesselVehicle = GetActiveSimVehicle();
            // VesselDataProvider = this.Game.ViewController.DataProvider.VesselDataProvider;
            // telemetryDataProvider = this.Game.ViewController.DataProvider.TelemetryDataProvider;
        }

        static KSPVessel _current;

        public static KSPVessel current
        {
            get
            {
                return _current;
            }
        }

        public void Update()
        {
            _current = this;
            VesselComponent = GetActiveSimVessel();
            VesselVehicle = GetActiveSimVehicle();
            if (Game == null || Game.ViewController == null) return;
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
            if (Game == null) return null;
            if (Game.ViewController == null) return null;
            if (!Game.ViewController.TryGetActiveSimVessel(out var vessel)) return null;
            return vessel as VesselComponent;
        }

        public VesselVehicle GetActiveSimVehicle()
        {
            if (Game == null) return null;
            if (Game.ViewController == null) return null;
            if (!Game.ViewController.TryGetActiveVehicle(out var vehicle)) return null;
            return vehicle as VesselVehicle;
        }


        ///////////////////////// from former Vessel Infos /////////////

        public CelestialBodyComponent currentBody()
        {
            if (VesselComponent == null) return null;
            return VesselComponent.mainBody;
        }

        public void SetThrottle(float throttle)
        {
            VesselVehicle = GetActiveSimVehicle();
            // WARNING can only be called from Update not FixedUpdate
            if (VesselVehicle == null) return;

            throttle = Mathf.Clamp01(throttle);

            var update = new FlightCtrlStateIncremental
            {
                mainThrottle = throttle
            };

            VesselVehicle.AtomicSet(update);
        }

        public Vector GetAngularSpeed()
        {
            if (VesselVehicle == null) return new Vector();

            return VesselVehicle.AngularVelocity.relativeAngularVelocity;
        }

        public Rotation GetRotation()
        {
            if (VesselVehicle == null) return new Rotation();
            return VesselVehicle.VehicleTelemetry.Rotation;
        }

        public ManeuverNodeData GetNextManeuveurNode()
        {
            var maneuvers = Game.SpaceSimulation?.Maneuvers;
            if (maneuvers == null) return null;

            var current_vehicle = VesselVehicle;
            if (current_vehicle == null) return null;

            var activeNodes = maneuvers.GetNodesForVessel(current_vehicle.Guid);
            ManeuverNodeData next_node = (activeNodes.Count() > 0) ? activeNodes[0] : null;
            return next_node;
        }

        public ManeuverPlanSolver GetPlanSolver()
        {
            ManeuverPlanSolver solver = null;
            solver = VesselComponent?.Orbiter?.ManeuverPlanSolver;
            return solver;
        }

        public void SetSpeedMode(SpeedDisplayMode mode)
        {
            if (VesselVehicle == null) return;
            VesselVehicle.SetSpeedDisplayMode(mode);
        }

        // Available Instructions===========================================================================================


        // Values
        // public void SetThrottle(float throttle)
        // {
        //     flightCtrlState.mainThrottle = throttle;
        // }


        public float X
        {
            get
            {
                if (VesselVehicle == null) return 0;
                return VesselVehicle.X;
            }
            set
            {
                if (VesselVehicle == null) return;

                value = Mathf.Clamp(value, -1, 1);

                var update = new FlightCtrlStateIncremental
                {
                    X = value
                };

                VesselVehicle.AtomicSet(update);
            }
        }
        public float Y
        {
            get
            {
                if (VesselVehicle == null) return 0;
                // Y is mapped to Z yes
                return VesselVehicle.Z;
            }
            set
            {
                if (VesselVehicle == null) return;
                value = Mathf.Clamp(value, -1, 1);

                var update = new FlightCtrlStateIncremental
                {
                    Z = value
                };

                VesselVehicle.AtomicSet(update);
            }
        }

        public float Z
        {
            get
            {
                if (VesselVehicle == null) return 0;
                // Z is mapped to Y
                return -VesselVehicle.Y;
            }
            set
            {
                if (VesselVehicle == null) return;

                value = Mathf.Clamp(value, -1, 1);

                var update = new FlightCtrlStateIncremental
                {
                    Y = -value
                };

                VesselVehicle.AtomicSet(update);
            }
        }

        public float Pitch
        {
            get
            {
                if (VesselVehicle == null) return 0;

                return VesselVehicle.pitch;
            }
            set
            {
                if (VesselVehicle == null) return;

                value = Mathf.Clamp(value, -1, 1);

                var update = new FlightCtrlStateIncremental
                {
                    pitch = value
                };

                VesselVehicle.AtomicSet(update);
            }
        }

        public float Roll
        {
            get
            {
                if (VesselVehicle == null) return 0;
                return VesselVehicle.roll;
            }
            set
            {
                if (VesselVehicle == null) return;

                value = Mathf.Clamp(value, -1, 1);

                var update = new FlightCtrlStateIncremental
                {
                    roll = value
                };

                VesselVehicle.AtomicSet(update);
            }
        }

        public float Yaw
        {
            get
            {
                if (VesselVehicle == null) return 0;
                return VesselVehicle.yaw;
            }
            set
            {
                if (VesselVehicle == null) return;

                value = Mathf.Clamp(value, -1, 1);

                var update = new FlightCtrlStateIncremental
                {
                    yaw = value
                };

                VesselVehicle.AtomicSet(update);
            }
        }

        public float PitchTrim
        {
            get
            {
                if (VesselVehicle == null) return 0;
                return VesselVehicle.pitchTrim;
            }
            set
            {
                if (VesselVehicle == null) return;

                value = Mathf.Clamp(value, -1, 1);

                var update = new FlightCtrlStateIncremental
                {
                    pitchTrim = value
                };

                VesselVehicle.AtomicSet(update);
            }
        }

        public float RollTrim
        {
            get
            {
                if (VesselVehicle == null) return 0;
                return VesselVehicle.rollTrim;
            }
            set
            {
                if (VesselVehicle == null) return;

                value = Mathf.Clamp(value, -1, 1);

                var update = new FlightCtrlStateIncremental
                {
                    rollTrim = value
                };

                VesselVehicle.AtomicSet(update);
            }
        }

        public float YawTrim
        {
            get
            {
                if (VesselVehicle == null) return 0;
                return VesselVehicle.yawTrim;
            }
            set
            {
                if (VesselVehicle == null) return;

                value = Mathf.Clamp(value, -1, 1);

                var update = new FlightCtrlStateIncremental
                {
                    yawTrim = value
                };

                VesselVehicle.AtomicSet(update);
            }
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


        public VesselAutopilot Autopilot => VesselComponent == null ? null : VesselComponent.Autopilot;

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
            return VesselComponent.flightCtrlState.yaw;
        }


        /// <summary>
        /// Retrieve the current activation value of pitch
        /// </summary>
        public double GetPitch()
        {
            return VesselComponent.flightCtrlState.pitch;
        }

        /// <summary>
        /// Retrieve the current activation value of roll
        /// </summary>
        public double GetRoll()
        {
            return VesselComponent.flightCtrlState.roll;
        }

        /// <summary>
        /// Retrieve the current activation value of throttle
        /// </summary>
        public double GetThrottle()
        {
            return VesselComponent.flightCtrlState.mainThrottle;
        }

        /// <summary>
        /// Retrieve the current activation value of wheelSteer
        /// </summary>
        public double GetWheelSteer()
        {
            return VesselComponent.flightCtrlState.wheelSteer;
        }

        /// <summary>
        /// Retrieve the current activation value of wheelThrottle
        /// </summary>
        public double GetWheelThrottle()
        {
            return VesselComponent.flightCtrlState.wheelThrottle;
        }

        /// <summary>
        /// Retrieve the current activation value of pitchTrim
        /// </summary>
        public double GetPitchTrim()
        {
            return VesselComponent.flightCtrlState.pitchTrim;
        }

        /// <summary>
        /// Retrieve the current activation value of yawTrim
        /// </summary>
        public double GetYawTrim()
        {
            return VesselComponent.flightCtrlState.yawTrim;
        }

        /// <summary>
        /// Retrieve the current activation value of rollTrim
        /// </summary>
        public double GetRollTrim()
        {
            return VesselComponent.flightCtrlState.rollTrim;
        }

        /// <summary>
        /// Retrieve the current activation value of wheelSteerTrim
        /// </summary>
        public double GetWheelSteerTrim()
        {
            return VesselComponent.flightCtrlState.wheelSteerTrim;
        }

        /// <summary>
        /// Retrieve the current activation value of inputPitch
        /// </summary>
        public double GetInputPitch()
        {
            return VesselComponent.flightCtrlState.inputPitch;
        }

        /// <summary>
        /// Retrieve the current activation value of inputRoll
        /// </summary>
        public double GetInputRoll()
        {
            return VesselComponent.flightCtrlState.inputRoll;
        }

        /// <summary>
        /// Retrieve the current activation value of inputYaw
        /// </summary>
        public double GetInputYaw()
        {
            return VesselComponent.flightCtrlState.inputYaw;
        }


        // =================================================================================================================


        // General Vessel Data===================================================================================================

        public AltimeterDisplayMode GetAltimeterDisplayMode()
        {
            return Game.ViewController.DataProvider.TelemetryDataProvider.AltimeterDisplayMode.GetValue();
        }


        public bool Landed()
        {
            return VesselComponent.Landed;
        }

        /// Correct the altitude unsing the vessel radius
        public double GetApproxAltitude()
        {
            var altitude_from_ground = GetGlobalGroundAltitude();
            // VesselComponent.SimulationObject.objVesselBehavior.ShowCenterOfMass;
            // var center = VesselComponent.SimulationObject.objVesselBehavior.BoundingBox.center.y - VesselComponent.SimulationObject.objVesselBehavior.BoundingBox.extents.z;
            //var bounding_sphere = VesselComponent.SimulationObject.objVesselBehavior.BoundingSphere;
            var result = altitude_from_ground - VesselComponent.SimulationObject.objVesselBehavior.BoundingSphere.radius;
            return result;
        }

        public double GetGroundAltitude()
        {
            return telemetryDataProvider.GetAltitudeDisplayValue(AltimeterDisplayMode.GroundLevel);
        }


        public double GetSeaAltitude()
        {
            return telemetryDataProvider.GetAltitudeDisplayValue(AltimeterDisplayMode.SeaLevel);
        }

        public double GetGlobalGroundAltitude()
        {
            return Math.Min(GetSeaAltitude(), GetGroundAltitude());
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

    }
}