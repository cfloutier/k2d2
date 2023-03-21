using System;
using BepInEx.Logging;
using K2D2.KSPService;
using KSP.Game;
using KSP.Map;
using KSP.Sim;
using KSP.Sim.impl;
using KSP.Sim.Maneuver;
using KSP2FlightAssistant.KSPService;
using KSP2FlightAssistant.MathLibrary;

namespace K2D2.KSPService
{
    public class Maneuver
    {

        private VesselComponent _vesselComponent;
        private GameInstance Game { get; set; }
        
        public KSPVessel kspVessel { get; set; }
        
        public Maneuver(GameInstance game)
        {
            
            Game = game;
            kspVessel = new KSPVessel(game);
            _vesselComponent = kspVessel.GetActiveSimVessel();

        }
        
        
        // Functions------------------------------------------------------------------------------------------------------
        
        public double CircularizeOrbitApoapsis()
        {
            
            double gravitation = _vesselComponent.Orbit.ReferenceBodyConstants.StandardGravitationParameter;
            double circularizedVelocity= VisVivaEquation.CalculateVelocity(_vesselComponent.Orbit.Apoapsis, _vesselComponent.Orbit.Apoapsis,
                _vesselComponent.Orbit.Apoapsis, gravitation);
            
            double apoapsisVelocity = VisVivaEquation.CalculateVelocity(_vesselComponent.Orbit.Apoapsis, _vesselComponent.Orbit.Apoapsis,
                _vesselComponent.Orbit.Periapsis, gravitation);

            double deltaV = circularizedVelocity - apoapsisVelocity;


            Vector3d burnVector = ProgradeBurnVector(deltaV);
            CreateManeuverNode(burnVector, 180);
            return deltaV;
        }

        public double CircularizeOrbitPeriapsis()
        {
            double gravitation = _vesselComponent.Orbit.ReferenceBodyConstants.StandardGravitationParameter;
            double circularizedVelocity = VisVivaEquation.CalculateVelocity(_vesselComponent.Orbit.Periapsis,
                _vesselComponent.Orbit.Periapsis,
                _vesselComponent.Orbit.Periapsis, gravitation);

            double periapsisVelocity = VisVivaEquation.CalculateVelocity(_vesselComponent.Orbit.Periapsis,
                _vesselComponent.Orbit.Apoapsis,
                _vesselComponent.Orbit.Periapsis, gravitation);

            double deltaV = periapsisVelocity - circularizedVelocity;
            
            Vector3d burnVector = RetrogradeBurnVector(deltaV);
            CreateManeuverNode(burnVector, 0);
            return deltaV;
        }


        // Maneuver Service----------------------------------------------------------------------------------------------
        
        public void AddManeuverNode(ManeuverNodeData maneuverNodeData)
        {
            Game.SpaceSimulation.Maneuvers.AddNodeToVessel(maneuverNodeData);
            
            MapCore mapCore = null;
            Game.Map.TryGetMapCore(out mapCore);
            
            mapCore.map3D.ManeuverManager.CreateGizmoForLocation(maneuverNodeData);
        }
        
        /// <summary>
        /// Creates a maneuver node at a given true anomaly
        /// </summary>
        /// <param name="burnVector"></param>
        /// <param name="TrueAnomaly"></param>
        public void CreateManeuverNode(Vector3d burnVector, double TrueAnomaly)
        {
            double TrueAnomalyRad = TrueAnomaly * Math.PI / 180;
            double UT = _vesselComponent.Orbit.GetUTforTrueAnomaly(TrueAnomalyRad,0);
            
            ManeuverNodeData maneuverNodeData = new ManeuverNodeData(kspVessel.GetGlobalIDActiveVessel(),true, UT );
            IPatchedOrbit orbit = _vesselComponent.Orbit;
            
            orbit.PatchStartTransition = PatchTransitionType.Maneuver;
            
            maneuverNodeData.SetManeuverState((PatchedConicsOrbit)orbit);
          
            maneuverNodeData.BurnVector = burnVector;
            AddManeuverNode(maneuverNodeData);
        }
        

        
        
        
        // Burning Vectors----------------------------------------------------------------------------------------------

        /// <summary>
        /// Burn Vector for a Prograde Maneuver(0,0,1 )* magnitude
        /// </summary>
        /// <param name="magnitude"></param>
        /// <returns>Prograde Burn Vector </returns>
        public Vector3d ProgradeBurnVector(double magnitude)
        {
            return new Vector3d(0,0,magnitude);;
        }
        
        /// <summary>
        /// Burn Vector for a Retrograde Maneuver(0,0,-1) * magnitude
        /// </summary>
        /// <param name="magnitude"></param>
        /// <returns>Retrograde Burn Vector</returns>
        public Vector3d RetrogradeBurnVector(double magnitude)
        {
            return new Vector3d(0,0,-magnitude);
        }
        
        /// <summary>
        /// Burn Vector for a Normal Maneuver(0,1,0) * magnitude
        /// </summary>
        /// <param name="magnitude"></param>
        /// <returns>Normal </returns>
        public Vector3d NormalBurnVector(double magnitude)
        {
            return new Vector3d(0,magnitude,0);;
        }
        
        public Vector3d AntiNormalBurnVector(double magnitude)
        {
            return new Vector3d(0,-magnitude,0);;
        }
        
        public Vector3d RadialOutBurnVector(double magnitude)
        {
            return new Vector3d(magnitude,0,0);;
        }
        
        public Vector3d RadialInBurnVector(double magnitude)
        {
            return new Vector3d(-magnitude,0,0);;
        }
        
        // -------------------------------------------------------------------------------------------------------------
        
        // Custom Functions---------------------------------------------------------------------------------------------
        
        public Vector3d GetOrbitalVelocityAtUT(double UT)
        {
            double inclination = _vesselComponent.Orbit.inclination;
            double longitudeOfAscendingNode = _vesselComponent.Orbit.longitudeOfAscendingNode;
            Vector3d normalVector = _vesselComponent.Orbit.GetRelativeOrbitNormal();//GetOrbitalNormalVector(UT, inclination, longitudeOfAscendingNode);
            Vector3d velocity = GetOrbitalPerifocalVelocityVector(UT, _vesselComponent.Orbit.eccentricity, _vesselComponent.Orbit.semiMajorAxis, _vesselComponent.Orbit.meanAnomalyAtEpoch, _vesselComponent.Orbit.ReferenceBodyConstants.StandardGravitationParameter);
            
            Vector3d orbitalVelocity = Vector3d.Cross(normalVector, velocity);
            return orbitalVelocity;
        }
        
        /// <summary>
        /// Returns the orbital normal vector in the ECI frame
        /// Use VesselComponent.Orbit.GetRelativeOrbitNormal() instead of this function if you can
        /// </summary>
        /// <param name="UT"></param>
        /// <param name="inclination"></param>
        /// <param name="longitudeOfAscendingNode"></param>
        /// <returns></returns>
        public Vector3d GetOrbitalNormalVector(double UT, double inclination, double longitudeOfAscendingNode)
        {
            // Calculate the normal vector components in the perifocal frame
            double nx = Math.Cos(inclination) * Math.Cos(longitudeOfAscendingNode);
            double ny = Math.Cos(inclination) * Math.Sin(longitudeOfAscendingNode);
            double nz = Math.Sin(inclination);

            // Convert the normal vector from the perifocal frame to the ECI frame
            double cosRAAN = Math.Cos(longitudeOfAscendingNode);
            double sinRAAN = Math.Sin(longitudeOfAscendingNode);
            double cosI = Math.Cos(inclination);
            double sinI = Math.Sin(inclination);
            double cosTA = Math.Cos(UT);
            double sinTA = Math.Sin(UT);

            double ex = cosRAAN * cosTA - sinRAAN * sinTA * cosI;
            double ey = sinRAAN * cosTA + cosRAAN * sinTA * cosI;
            double ez = sinTA * sinI;

            return new Vector3d(ex, ey, ez);
        }
        
        /// <summary>
        /// Returns the orbital velocity vector in the ECI frame (Earth-centered inertial)
        /// </summary>
        /// <param name="semiMajorAxis"></param>
        /// <param name="eccentricity"></param>
        /// <param name="trueAnomaly"></param>
        /// <param name="inclination"></param>
        /// <param name="longitudeOfAscendingNode"></param>
        /// <returns></returns>
        public Vector3d GetOrbitalPerifocalVelocityVector(double semiMajorAxis, double eccentricity, double trueAnomaly, double inclination, double longitudeOfAscendingNode)
        {
            // Calculate the magnitude of the velocity vector
            double r = semiMajorAxis * (1 - eccentricity * eccentricity) / (1 + eccentricity * Math.Cos(trueAnomaly));
            double gravitation = _vesselComponent.Orbit.ReferenceBodyConstants.StandardGravitationParameter;
            // Calculate the magnitude of the velocity vector
            double v = Math.Sqrt(gravitation * (2 / r - 1 / semiMajorAxis));

            // Calculate the velocity vector components in the perifocal frame
            double vx = v * Math.Sin(trueAnomaly);
            double vy = v * (Math.Cos(trueAnomaly) + eccentricity);
            double vz = 0;

            // Convert the velocity vector from the perifocal frame to the ECI frame
            double cosRAAN = Math.Cos(longitudeOfAscendingNode);
            double sinRAAN = Math.Sin(longitudeOfAscendingNode);
            double cosArgPeriapsis = Math.Cos(_vesselComponent.Orbit.argumentOfPeriapsis);
            double sinArgPeriapsis = Math.Sin(_vesselComponent.Orbit.argumentOfPeriapsis);
            double cosInclination = Math.Cos(inclination);
            double sinInclination = Math.Sin(inclination);

            double x = cosRAAN * cosArgPeriapsis - sinRAAN * sinArgPeriapsis * cosInclination;
            double y = sinRAAN * cosArgPeriapsis + cosRAAN * sinArgPeriapsis * cosInclination;
            double z = sinArgPeriapsis * sinInclination;

            Vector3d perifocalVelocity = new Vector3d(vx, vy, vz);
            QuaternionD rotation = new QuaternionD(
                -sinRAAN * cosArgPeriapsis - cosRAAN * sinArgPeriapsis * cosInclination,
                cosRAAN * cosArgPeriapsis - sinRAAN * sinArgPeriapsis * cosInclination,
                sinRAAN * sinInclination,
                
                sinRAAN * cosArgPeriapsis * cosInclination + cosRAAN * sinArgPeriapsis);
            
            Vector3d velocityVector = rotation * perifocalVelocity;
            return velocityVector;
        }
        
        // Logging Functions--------------------------------------------------------------------------------------------
        
        public void Log(ManualLogSource logger, string message)
        {
            logger.LogMessage(message);
        }
        
        // -------------------------------------------------------------------------------------------------------------
        
        public void deleteAllManeuvers()
        {
            throw new NotImplementedException();
        }
        


    }
}