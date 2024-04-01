﻿using System.Collections;
using BepInEx.Logging;
using KSP.Game;
using KSP.Map;
using KSP.Sim;
using KSP.Sim.impl;
using KSP.Sim.Maneuver;
using KSP2FlightAssistant.MathLibrary;
using UnityEngine;


namespace K2D2.KSPService
{
    public class ManeuverCreator
    {
        // Fields-------------------------------------------------------------------------------------------------------

        #region fields

        private VesselComponent _vesselComponent;
        public GameInstance Game => GameManager.Instance == null ? null : GameManager.Instance.Game;

        public KSPVessel kspVessel { get; set; }

        public ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.CircleController");

        public ManeuverCreator()
        {

        }

        public void Update()
        {
            kspVessel = K2D2_Plugin.Instance.current_vessel;
            _vesselComponent = kspVessel.GetActiveSimVessel();
        }

        #endregion

        // Functions----------------------------------------------------------------------------------------------------

        #region CicrularizeOrbit

        public double CircularizeOrbitApoapsis()
        {
            PatchedConicsOrbit orbit = GetLastOrbit() as PatchedConicsOrbit;

            if (orbit.eccentricity >= 1)
            {
                logger.LogMessage("Apoapsis Circularization not possible for hyperbolic orbits");
                return CircularizeHyperbolicOrbit();
            }

            double gravitation = orbit.ReferenceBodyConstants.StandardGravitationParameter;
            double periapsis = orbit.Periapsis;
            double apoapsis = orbit.Apoapsis;

            double circularizedVelocity = VisVivaEquation.CalculateVelocity(apoapsis, apoapsis,
                apoapsis, gravitation);

            double apoapsisVelocity = VisVivaEquation.CalculateVelocity(apoapsis,
                apoapsis,
                periapsis,
                gravitation);

            double deltaV = circularizedVelocity - apoapsisVelocity;


            Vector3d burnVector = ProgradeBurnVector(deltaV);

            // ManeuverNodeController.NodeControl.CreateManeuverNodeAtUT(burnVector, GeneralTools.Game.UniverseModel.UniverseTime + orbit.TimeToAp ,0);
            CreateManeuverNode(burnVector, 180);
            return deltaV;
        }

        public double CircularizeOrbitPeriapsis()
        {
            PatchedConicsOrbit orbit = GetLastOrbit() as PatchedConicsOrbit;
            if (orbit.eccentricity >= 1)
            {
                logger.LogMessage("Periapsis Circularization not possible for hyperbolic orbits");
                return CircularizeHyperbolicOrbit();
            }

            double periapsis = orbit.Periapsis;
            double apoapsis = orbit.Apoapsis;
            double eccentricity = orbit.eccentricity;
            logger.LogMessage($"AP: {orbit.Apoapsis} PE: {orbit.Periapsis}");
            double gravitation = orbit.ReferenceBodyConstants.StandardGravitationParameter;
            //double gravitation = _vesselComponent.Orbit.ReferenceBodyConstants.StandardGravitationParameter;
            double circularizedVelocity = VisVivaEquation.CalculateVelocity(periapsis,
                periapsis,
                periapsis,
                gravitation);

            double periapsisVelocity = VisVivaEquation.CalculateVelocity(periapsis,
                apoapsis,
                periapsis,
                gravitation);

            double deltaV = periapsisVelocity - circularizedVelocity;

            Vector3d burnVector = RetrogradeBurnVector(deltaV);
            // ManeuverNodeController.NodeControl.CreateManeuverNodeAtUT(burnVector, GeneralTools.Game.UniverseModel.UniverseTime + orbit.TimeToPe, 0);
            CreateManeuverNode(burnVector, 0);
            return deltaV;
        }

        public double CircularizeHyperbolicOrbit()
        {
            PatchedConicsOrbit orbit = GetLastOrbit() as PatchedConicsOrbit;
            double gravitation = orbit.ReferenceBodyConstants.StandardGravitationParameter;
            double periapsis = orbit.Periapsis;
            double orbitalEnergy = orbit.OrbitalEnergy;

            double currentVelocity = VisVivaEquation.CalculateHyperbolicVelocity(periapsis,
                gravitation,
                orbitalEnergy);

            double newPeriapsisVelocity = VisVivaEquation.CalculateVelocity(periapsis,
                periapsis,
                periapsis,
                gravitation);

            double deltaV = newPeriapsisVelocity - currentVelocity;

            Vector3d burnVector = ProgradeBurnVector(deltaV);
            CreateManeuverNode(burnVector, 0);
            return deltaV;
        }

        #endregion

        #region ChangeOrbit



        public void ChangePeriapsis(double OrbitDistance)
        {
            PatchedConicsOrbit orbit = GetLastOrbit() as PatchedConicsOrbit;


            double periapsis = orbit.Periapsis;
            double apoapsis = orbit.Apoapsis;
            double eccentricity = orbit.eccentricity;
            double gravitation = orbit.ReferenceBodyConstants.StandardGravitationParameter;

            if (eccentricity >= 1)
            {
                logger.LogMessage("Periapsis Change not possible for hyperbolic orbits");
                return;
            }

            double currentPeriapsisVelocity = VisVivaEquation.CalculateVelocity(apoapsis,
                apoapsis,
                periapsis,
                gravitation);

            double newPeriapsisVelocity = VisVivaEquation.CalculateVelocity(apoapsis,
                apoapsis,
                OrbitDistance,
                gravitation);

            double deltaV = newPeriapsisVelocity - currentPeriapsisVelocity;

            Vector3d burnVector = ProgradeBurnVector(deltaV);
            CreateManeuverNode(burnVector, 180);
        }

        public void ChangeApoapsis(double OrbitDistance)
        {
            PatchedConicsOrbit orbit = GetLastOrbit() as PatchedConicsOrbit;


            double periapsis = orbit.Periapsis;
            double apoapsis = orbit.Apoapsis;
            double eccentricity = orbit.eccentricity;
            double newEccentricity = (OrbitDistance - periapsis) / (OrbitDistance + periapsis);

            double gravitation = orbit.ReferenceBodyConstants.StandardGravitationParameter;
            double deltaV = 0;
            double currentApoapsisVelocity;

            if (eccentricity >= 1)
            {
                currentApoapsisVelocity = VisVivaEquation.CalculateHyperbolicVelocity(periapsis,
                    gravitation,
                    orbit.OrbitalEnergy);
            }
            else
            {
                currentApoapsisVelocity = VisVivaEquation.CalculateVelocity(periapsis,
                    apoapsis,
                    periapsis,
                    gravitation);
            }

            double newApoapsisVelocity = VisVivaEquation.CalculateVelocity(periapsis,
                OrbitDistance,
                periapsis,
                gravitation);

            deltaV = newApoapsisVelocity - currentApoapsisVelocity;


            Vector3d burnVector = ProgradeBurnVector(deltaV);
            CreateManeuverNode(burnVector, 0);
        }

        #endregion

        #region InterplanetaryTransfer
        public double HohmannTransfer(double UT, double OrbitDistance)
        {
            CircularizeOrbitApoapsis();
            double deltaV = 0;
            return deltaV;
        }

        #endregion

        // Internal Maneuver Services-----------------------------------------------------------------------------------

        #region Internal Maneuver Services

        private IPatchedOrbit GetLastOrbit()
        {
            List<ManeuverNodeData> patchList =
                Game.SpaceSimulation.Maneuvers.GetNodesForVessel(kspVessel.GetGlobalIDActiveVessel());

            logger.LogMessage(patchList.Count);

            if (patchList.Count == 0)
            {
                logger.LogMessage(_vesselComponent.Orbit);
                return _vesselComponent.Orbit;
            }

            logger.LogMessage(patchList[patchList.Count - 1].ManeuverTrajectoryPatch);
            IPatchedOrbit orbit = patchList[patchList.Count - 1].ManeuverTrajectoryPatch;

            return orbit;
        }

        /// <summary>
        /// Creates a maneuver node at a given true anomaly
        /// </summary>
        /// <param name="burnVector"></param>
        /// <param name="TrueAnomaly"></param>
        private void CreateManeuverNode(Vector3d burnVector, double TrueAnomaly)
        {
            K2D2_Plugin.Instance.StartCoroutine(CreateManeuverNode_Co(burnVector, TrueAnomaly));
        }

        private IEnumerator CreateManeuverNode_Co(Vector3d burnVector, double TrueAnomaly)
        {
            PatchedConicsOrbit referencedOrbit = _vesselComponent.Orbit;

            double TrueAnomalyRad = TrueAnomaly * Math.PI / 180;
            double UT = referencedOrbit.GetUTforTrueAnomaly(TrueAnomalyRad, 0);

            var SimulationObject = _vesselComponent.SimulationObject;

            // Create Node
            ManeuverNodeData nodeData = new ManeuverNodeData(SimulationObject.GlobalId, false, UT);
            referencedOrbit.PatchEndTransition = PatchTransitionType.Maneuver;
            nodeData.SetManeuverState((PatchedConicsOrbit)referencedOrbit);

            nodeData.BurnVector = burnVector;

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




        private VesselComponent activeVessel
        {
            get
            {

                return KSPVessel.current.VesselComponent;
            }
        }



        #endregion

        // Logging------------------------------------------------------------------------------------------------------

        #region Logging

        public void Log(ManualLogSource logger, string message)
        {
            logger.LogMessage(message);
        }

        public void LogOrbit()
        {
            logger.LogMessage("================= Orbit Log =================");
            PatchedConicsOrbit orbit = GetLastOrbit() as PatchedConicsOrbit;
            logger.LogMessage($"AP: {orbit.Apoapsis} PE: {orbit.Periapsis}");
            logger.LogMessage($"SemiMajorAxis: {orbit.semiMajorAxis} SemiMinorAxis: {orbit.SemiMinorAxis}");
            logger.LogMessage($"Eccentricity: {orbit.eccentricity} ");
            logger.LogMessage($"Inclination: {orbit.inclination} ArgumentOfPeriapsis: {orbit.argumentOfPeriapsis}");
            logger.LogMessage("epoch: " + orbit.epoch);
            logger.LogMessage("referenceBody: " + orbit.referenceBody);
        }

        #endregion

        // Special Burning Vectors--------------------------------------------------------------------------------------

        #region Special Burning Vectors

        /// <summary>
        /// Burn Vector for a Prograde Maneuver(0,0,1 )* magnitude
        /// </summary>
        /// <param name="magnitude"></param>
        /// <returns>Prograde Burn Vector3d </returns>
        public Vector3d ProgradeBurnVector(double magnitude)
        {
            return new Vector3d(0, 0, magnitude);
            ;
        }

        /// <summary>
        /// Burn Vector for a Retrograde Maneuver(0,0,-1) * magnitude
        /// </summary>
        /// <param name="magnitude"></param>
        /// <returns>Retrograde Burn Vector3d</returns>
        public Vector3d RetrogradeBurnVector(double magnitude)
        {
            return new Vector3d(0, 0, -magnitude);
        }

        /// <summary>
        /// Burn Vector for a Normal Maneuver(0,1,0) * magnitude
        /// </summary>
        /// <param name="magnitude"></param>
        /// <returns>Normal Burn Vector3d</returns>
        public Vector3d NormalBurnVector(double magnitude)
        {
            return new Vector3d(0, magnitude, 0);
            ;
        }

        /// <summary>
        /// Burn Vector for a AntiNormal Maneuver(0,-1,0) * magnitude
        /// </summary>
        /// <param name="magnitude"></param>
        /// <returns>Anti Normal Burn Vector3d </returns>
        public Vector3d AntiNormalBurnVector(double magnitude)
        {
            return new Vector3d(0, -magnitude, 0);
            ;
        }

        /// <summary>
        /// Radial Out Burn Vector(1,0,0) * magnitude
        /// </summary>
        /// <param name="magnitude"></param>
        /// <returns>Radial Out Burn Vector3d</returns>
        public Vector3d RadialOutBurnVector(double magnitude)
        {
            return new Vector3d(magnitude, 0, 0);
            ;
        }

        /// <summary>
        /// Radial In Burn Vector(-1,0,0) * magnitude
        /// </summary>
        /// <param name="magnitude"></param>
        /// <returns>Radial In Burn Vector3d</returns>
        public Vector3d RadialInBurnVector(double magnitude)
        {
            return new Vector3d(-magnitude, 0, 0);
        }

        #endregion

        // Custom Functions---------------------------------------------------------------------------------------------

        #region Custom Functions

        public bool IsApoapsisFirst()
        {
            PatchedConicsOrbit orbit = GetLastOrbit() as PatchedConicsOrbit;
            double timePE = orbit.GetUTforTrueAnomaly(0, 0);
            double timeAP = orbit.GetUTforTrueAnomaly(Math.PI, 0);
            return timePE > timeAP;
        }

        public bool IsOrbitElliptic()
        {
            PatchedConicsOrbit orbit = GetLastOrbit() as PatchedConicsOrbit;
            return orbit.eccentricity < 1;
        }

        public double AddRadiusOfBody(double radius)
        {
            PatchedConicsOrbit orbit = GetLastOrbit() as PatchedConicsOrbit;
            return radius + orbit.ReferenceBodyConstants.Radius;
        }

        public double GetCurrentPeriapsis()
        {
            PatchedConicsOrbit orbit = GetLastOrbit() as PatchedConicsOrbit;
            return orbit.Periapsis;
        }

        public double GetCurrentApoapsis()
        {
            PatchedConicsOrbit orbit = GetLastOrbit() as PatchedConicsOrbit;
            return orbit.Apoapsis;
        }

        public Vector3d GetOrbitalVelocityAtUT(double UT)
        {
            double inclination = _vesselComponent.Orbit.inclination;
            double longitudeOfAscendingNode = _vesselComponent.Orbit.longitudeOfAscendingNode;
            Vector3d
                normalVector =
                    _vesselComponent.Orbit
                        .GetRelativeOrbitNormal(); //GetOrbitalNormalVector(UT, inclination, longitudeOfAscendingNode);
            Vector3d velocity = GetOrbitalPerifocalVelocityVector(UT, _vesselComponent.Orbit.eccentricity,
                _vesselComponent.Orbit.semiMajorAxis, _vesselComponent.Orbit.meanAnomalyAtEpoch,
                _vesselComponent.Orbit.ReferenceBodyConstants.StandardGravitationParameter);

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
        public Vector3d GetOrbitalPerifocalVelocityVector(double semiMajorAxis, double eccentricity, double trueAnomaly,
            double inclination, double longitudeOfAscendingNode)
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

        #endregion

        // Currently Not Implemented Functions--------------------------------------------------------------------------

        #region Unimplemented Functions

        public void deleteAllManeuvers()
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}