using System;
using KSP.Sim;
using KSP.Sim.impl;
using KSP.Sim.Maneuver;
using KSP.Sim.State;
using KSP.Game;

using System.Reflection;

namespace K2D2
{
    public class Tools
    {

        static public string printVector(Vector3d vec)
        {
            return $"{vec.x:n2} {vec.y:n2} {vec.z:n2}";
        }

        static public string printDuration(double secs)
        {
            if (secs < 0)
            {
                secs = -secs;
                TimeSpan t = TimeSpan.FromSeconds(secs);

                return string.Format("- {0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                    t.Hours,
                    t.Minutes,
                    t.Seconds,
                    t.Milliseconds);
                }
            else
            {
                TimeSpan t = TimeSpan.FromSeconds(secs);

                return string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                    t.Hours,
                    t.Minutes,
                    t.Seconds,
                    t.Milliseconds);
            }
        }

        static public KSP.Game.GameInstance Game()
        {
            if (GameManager.Instance == null) return null;
            return GameManager.Instance.Game;
        }


        public static Vector3d correctEuler(Vector3d euler)
        {
            Vector3d result = euler;
            if (result.x > 180)
            {
                result.x -= 360;
            }
            if (result.y > 180)
            {
                result.y -= 360;
            }
            if (result.z > 180)
            {
                result.z -= 360;
            }

            return result;
        }

        public static double remainingStartTime(ManeuverNodeData node)
        {
            var dt = node.Time - Game().UniverseModel.UniversalTime;
            return dt;
        }

        public static double remainingEndTime(ManeuverNodeData node)
        {
            var dt = node.Time + node.BurnDuration - Game().UniverseModel.UniversalTime;
            return dt;
        }


        public static ManeuverNodeData getNextManeuveurNode()
        {
            var game = GameManager.Instance?.Game;
            if (game == null) return null;

            var manoeuvers = game.SpaceSimulation?.Maneuvers;
            if (manoeuvers == null) return null;

            var current_vehicle = VesselInfos.currentVehicle();
            if (current_vehicle == null) return null;

            var activeNodes = manoeuvers.GetNodesForVessel(current_vehicle.Guid);
            ManeuverNodeData next_node = (activeNodes.Count() > 0) ? activeNodes[0] : null;
            return next_node;
        }

    }

    public class Reflex
    {

        /// <summary>
        /// Uses reflection to get the field value from an object.
        /// </summary>
        ///
        /// <param name="type">The instance type.</param>
        /// <param name="instance">The instance object.</param>
        /// <param name="fieldName">The field's name which is to be fetched.</param>
        ///
        public static object GetInstanceField(Type type, object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            object value = field.GetValue(instance);
            return value;
        }
    }

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
            return Tools.correctEuler(((Vector3d)  my_obj));
        }

        public static TelemetryComponent getTelemetry()
        {
            if (currentAutoPilot() == null)
                return null;

            var my_obj = Reflex.GetInstanceField(typeof(VesselAutopilot), currentAutoPilot(), "_telemetry");
            return my_obj as TelemetryComponent;
        }

    }

    class VesselInfos
    {
        static public VesselComponent currentVessel()
        {
            return Tools.Game().ViewController?.GetActiveSimVessel();
        }
        static public VesselVehicle currentVehicle()
        {
            if (Tools.Game().ViewController == null) return null;
            if (!Tools.Game().ViewController.TryGetActiveVehicle(out var vehicle)) return null;
            return vehicle as VesselVehicle;
        }

        static public CelestialBodyComponent currentBody()
        {
            if (currentVessel() == null) return null;
            return currentVessel().mainBody;
        }

        public static void SetThrottle(float throttle)
        {
            var active_Vehicle = currentVehicle();
            if (active_Vehicle == null) return;

            var update = new FlightCtrlStateIncremental
            {
                mainThrottle = throttle
            };

            active_Vehicle.AtomicSet(update);
        }



        public static Vector GetAngularSpeed()
        {
            var active_Vehicle = currentVehicle();
            return active_Vehicle.AngularVelocity.relativeAngularVelocity;
        }

        public static Rotation GetRotation()
        {
            var active_Vehicle = currentVehicle();
            return active_Vehicle.VehicleTelemetry.Rotation;
        }
    }

    public class TimeWarpTools
    {
        public static TimeWarp time_warp()
        {
            return GameManager.Instance?.Game?.ViewController?.TimeWarp;
        }

        public static float indexToRatio(int index)
        {
            var levels = time_warp().GetWarpRates();
            if (index < 0 || index >= levels.Length) return 0f;

            return levels[index].TimeScaleFactor;
        }

        public static int ratioToIndex(float ratio)
        {
            var levels = time_warp().GetWarpRates();
            for (int index = 0; index < levels.Length; index++ )
            {
                float factor = levels[index].TimeScaleFactor;
                if (ratio < factor)
                    return index;
            }

            return levels.Length -1;
        }
    }

}

