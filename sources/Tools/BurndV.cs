using UnityEngine;

using System;
using KSP.Sim;
using KSP.Sim.impl;
using KSP.Sim.Maneuver;
using KSP.Sim.State;
using KSP.Game;
using KSP.Sim.DeltaV;

using System.Collections.Generic;

using System.Reflection;

namespace K2D2
{
    /// Simple class used to compute Burn DV
    public class BurndV
    {

        public BurndV()
        {

        }

        public float burned_dV = 0;

        public Vector3 actual_thrust;
        public float actual_dv
        {
            get
            {
                var vessel = VesselInfos.currentVessel();
                if (vessel == null) return 0;

                var totalMass = vessel.totalMass;
                return (float) ( actual_thrust.magnitude / totalMass );
            }
        }

        public void FixedUpdate()
        {
            Compute_FullThrust();
            burned_dV += actual_dv * Time.fixedDeltaTime;
        }

        public void Compute_FullThrust()
        {
            var vessel = VesselInfos.currentVessel();
            if (vessel == null) return;
            VesselDeltaVComponent delta_v = vessel.VesselDeltaV;
            if (delta_v == null) return;
           
            actual_thrust = Vector3.zero;
            List<DeltaVEngineInfo> engineInfos = delta_v.EngineInfo;
            for (int i = 0; i < engineInfos.Count; i++)
            {
                DeltaVEngineInfo engineInfo = engineInfos[i];

                actual_thrust += ComputeEngine_ActualThrust(engineInfo);
            }
        }

        // it is the way engine burning is computed in KSP
        public bool Engine_Running(DeltaVEngineInfo engine_info)
        {
            return (engine_info.Engine.EngineIgnited && engine_info.Engine.RequestedMassFlow > 0f);
        }

        public Vector3 ComputeEngine_ActualThrust(DeltaVEngineInfo engineInfo)
        {
            if (Engine_Running(engineInfo))
            {
                return engineInfo.ThrustVectorActual;

            }
            return Vector3.zero;

        }

        public void onGUI()
        {
            VesselDeltaVComponent delta_v = VesselInfos.currentVessel().VesselDeltaV;
            if (delta_v == null)
            {
                GUILayout.Label("NO VesselDeltaVComponent");
                return;
            }
            List<DeltaVEngineInfo> engineInfos = delta_v.EngineInfo;

            GUILayout.Label($"nb_engines   {engineInfos.Count}  ");

            GUILayout.Label($"actual_thrust  {Tools.printVector(actual_thrust)}  ");
            GUILayout.Label($"actual_dv  {actual_dv:n2}  ");
            GUILayout.Label($"burned_dV  {burned_dV:n2}  ");

            if (GUILayout.Button("Reset"))
                burned_dV = 0;
        }
    }
}