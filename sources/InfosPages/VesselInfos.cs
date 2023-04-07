

using UnityEngine;
using KSP.Sim;

namespace K2D2.InfosPages
{
    class VesselInfos
    {
        public static void onGUI()
        {
            var vehicle = K2D2_Plugin.Instance.current_vessel.VesselVehicle;
            var vessel_component = K2D2_Plugin.Instance.current_vessel.VesselComponent;

            if (vehicle == null)
            {
                GUILayout.Label("No vessel !");
                return;
            }

            GUILayout.Label($"mainThrottle {vehicle.mainThrottle}");
            GUILayout.Label($"pitch {vehicle.pitch:n3} yaw {vehicle.yaw:n3} roll {vehicle.roll:n3}");

            GUILayout.Space(6);

            GUILayout.Label($"AltitudeFromTerrain {vehicle.AltitudeFromTerrain:n2} m");
            GUILayout.Label($"Corrected alt : {K2D2_Plugin.Instance.current_vessel.GetApproxAltitude()} m");
            GUILayout.Label($"Landed : {vessel_component.Landed}");

            GUILayout.Label($"Lat {vehicle.Latitude:n2} Lon {vehicle.Longitude:n2}");
            GUILayout.Label($"IsInAtmosphere {vehicle.IsInAtmosphere}");


            GUILayout.Label($"Body {K2D2_Plugin.Instance.current_vessel.currentBody()}");

            //var body = K2D2_Plugin.Instance.current_vessel.currentBody();
            //var coord = body.coordinateSystem;
            //var body_location = Rotation.Reframed(vehicle.Rotation, coord);

            //  GUILayout.Label($"coordinate_system {vehicle.Rotation.coordinateSystem}");
            //GUILayout.Label($"body_location {StrTool.VectorToString(body_location.localRotation.eulerAngles)}");
        }
    }
}