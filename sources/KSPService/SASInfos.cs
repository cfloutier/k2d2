using System;
using KSP.Sim;
using KSP.Sim.impl;
using KSP.Sim.Maneuver;
using KSP.Sim.State;
using KSP.Game;

using System.Reflection;

namespace K2D2
{
    public class SASInfos
    {
        public static VesselAutopilot currentAutoPilot()
        {
            return VesselInfos.currentVessel()?.Autopilot;
        }

        public static VesselSAS currentSas()
        {
            return VesselInfos.currentVessel()?.Autopilot?.SAS;
        }

        public static double getSasResponsePC()
        {
            if (currentSas() == null)
                return 0;


            var my_obj = Reflex.GetInstanceField(typeof(VesselSAS), currentSas(), "sasResponse");
            return  ((Vector3d) my_obj).magnitude * 100;
        }

        public static Vector3d geSASAngularDelta()
        {
            if (currentSas() == null)
                return Vector3d.zero;

            var my_obj = Reflex.GetInstanceField(typeof(VesselSAS), currentSas(), "angularDelta");
            return GeneralTools.correctEuler(((Vector3d)  my_obj));
        }

        public static TelemetryComponent getTelemetry()
        {
            if (currentAutoPilot() == null)
                return null;

            var my_obj = Reflex.GetInstanceField(typeof(VesselAutopilot), currentAutoPilot(), "_telemetry");
            return my_obj as TelemetryComponent;
        }
    }

}

