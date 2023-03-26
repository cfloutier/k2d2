using System;
using KSP.Sim;
using KSP.Sim.impl;
using KSP.Sim.Maneuver;
using KSP.Sim.State;
using KSP.Game;

using System.Reflection;

namespace K2D2
{
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

    // TODO : to merge with KSPVessel
    class VesselInfos
    {
        static public VesselComponent currentVessel()
        {
            return GeneralTools.Game.ViewController?.GetActiveSimVessel();
        }

        static public VesselVehicle currentVehicle()
        {
            if (GeneralTools.Game.ViewController == null) return null;
            if (!GeneralTools.Game.ViewController.TryGetActiveVehicle(out var vehicle)) return null;
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


}

