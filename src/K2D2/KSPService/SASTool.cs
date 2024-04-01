
using KSP.Sim;
using KSP.Sim.impl;

using KTools;

namespace K2D2
{
    public class SASTool
    {
        public static VesselAutopilot currentAutoPilot()
        {
            return K2D2_Plugin.Instance.current_vessel?.VesselComponent?.Autopilot;
        }

        public static VesselSAS currentSas()
        {
            return K2D2_Plugin.Instance.current_vessel?.VesselComponent?.Autopilot?.SAS;
        }

        public static double getSasResponsePC()
        {
            if (currentSas() == null)
                return 0;


            var my_obj = ReflexionTool.GetInstanceField(typeof(VesselSAS), currentSas(), "sasResponse");
            return ((Vector3d)my_obj).magnitude * 100;
        }

        public static Vector3d geSASAngularDelta()
        {
            if (currentSas() == null)
                return Vector3d.zero;

            var my_obj = ReflexionTool.GetInstanceField(typeof(VesselSAS), currentSas(), "angularDelta");
            return GeneralTools.correctEuler(((Vector3d)my_obj));
        }

        public static TelemetryComponent getTelemetry()
        {
            if (currentAutoPilot() == null)
                return null;

            var my_obj = ReflexionTool.GetInstanceField(typeof(VesselAutopilot), currentAutoPilot(), "_telemetry");
            return my_obj as TelemetryComponent;
        }

        public static void setAutoPilot(AutopilotMode mode)
        {
            var autopilot = currentAutoPilot();
            if (autopilot == null) return;
            if (autopilot.AutopilotMode == mode) return;
            autopilot.Enabled = true;
            autopilot.SetMode(mode);
        }
    }
}

