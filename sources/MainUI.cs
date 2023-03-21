
using UnityEngine;
using KSP.Sim;
using BepInEx.Logging;
using SpaceWarp.API.Assets;
using K2D2.Controller;

namespace K2D2
{
    public class MainUI
    {
        public ManualLogSource logger;

        #region interfaces modes

        public enum InterfaceMode { ExeNode, SAS, Vessel, Circularize }
        private static string[] interfaceModes = { "Auto Execute", "SAS Infos", "Vessel Infos", "Circularization" };

        #endregion

        

        public MainUI(ManualLogSource src_logger)
        {
            logger = src_logger;
            logger.LogMessage("MainGUI");
        }

        public void onGui()
        {
            if (VesselInfos.currentVessel() == null)
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label("No active vessel.", Styles.error);
                GUILayout.FlexibleSpace();
                return;
            }

            if (VesselInfos.currentBody() == null)
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label("No active body.", Styles.error);
                GUILayout.FlexibleSpace();

                return;
            }

            // ############# Uncomment this to get tabs in UI ###################

            if (Settings.debug_mode){
                // Mode selection.
                Settings.current_interface_mode = (InterfaceMode)GUILayout.SelectionGrid((int)Settings.current_interface_mode, interfaceModes, 3);
                // #############  ###################
            }
            else
                //Settings.current_interface_mode = InterfaceMode.ExeNode;
                Settings.current_interface_mode = (InterfaceMode)GUILayout.SelectionGrid((int)Settings.current_interface_mode, interfaceModes, 3);



            // return;
            // Draw one of the modes.
            switch (Settings.current_interface_mode)
            {
                case InterfaceMode.ExeNode:
                    if (AutoExecuteManeuver.Instance != null)
                        AutoExecuteManeuver.Instance.onGUI();
                    else
                        logger.LogError("Missing AutoExecuteManeuver");
                    break;
                case InterfaceMode.SAS: SASInfos(); break;
                case InterfaceMode.Vessel: VesselInfo(); break;
                case InterfaceMode.Circularize: 
                    CircularizeController.Instance.onGUI(); 
                    break;
                
                default:
                    break;
            }
        }

        public static void SASInfos()
        {
            var sas = VesselInfos.currentVessel().Autopilot.SAS;
            if (sas == null)
            {
                GUILayout.Label("NO SAS");
                return;
            }

            GUILayout.Label($"sas.dampingMode {sas.dampingMode}");
            GUILayout.Label($"sas.ReferenceFrame {sas.ReferenceFrame}");
            GUILayout.Label($"sas.AutoTune {sas.AutoTune}");
            GUILayout.Label($"sas.lockedMode {sas.lockedMode}");
            GUILayout.Label($"sas.LockedRotation {Tools.printVector(sas.LockedRotation.eulerAngles)}");

            GUILayout.Label($"sas.TargetOrientation {Tools.printVector(sas.TargetOrientation)}");
            GUILayout.Label($"sas.PidLockedPitch {sas.PidLockedPitch}");
            GUILayout.Label($"sas.PidLockedRoll {sas.PidLockedRoll}");
            GUILayout.Label($"sas.PidLockedYaw {sas.PidLockedYaw}");
        }

        void VesselInfo()
        {
            var vehicle = VesselInfos.currentVehicle();

            if (vehicle == null)
            {
                GUILayout.Label("NO vessel");
                return;
            }

            GUILayout.Label($"mainThrottle {vehicle.mainThrottle}");
            GUILayout.Label($"pitch {vehicle.pitch:n3} yaw {vehicle.yaw:n3} roll {vehicle.roll:n3}");

            GUILayout.Label($"AltitudeFromTerrain {vehicle.AltitudeFromTerrain:n2}");
            GUILayout.Label($"Lat {vehicle.Latitude:n2} Lon {vehicle.Longitude:n2}");
            GUILayout.Label($"IsInAtmosphere {vehicle.IsInAtmosphere}");
            GUILayout.Label($"LandedOrSplashed {vehicle.LandedOrSplashed}");

            var body = VesselInfos.currentBody();
            var coord = body.coordinateSystem;
            var body_location = Rotation.Reframed(vehicle.Rotation, coord);

            GUILayout.Label($"coordinate_system {vehicle.Rotation.coordinateSystem}");
            GUILayout.Label($"body_location {Tools.printVector(body_location.localRotation.eulerAngles)}");
        }
    }
}