
using UnityEngine;
using KSP.Sim;
using BepInEx.Logging;
using SpaceWarp.API.Assets;
using KSP.Sim.DeltaV;

using System.Collections.Generic;

using K2D2.Controller;

namespace K2D2
{
    public class MainUI
    {
        public ManualLogSource logger;

        #region interfaces modes


        public enum InterfaceMode { ExeNode, Circularize, Landing, Orbit, SAS, Vessel  }

        private static string[] interfaceModes = { "Execute Node", "Circularization" };
        private static string[] interfaceModes_debug = { "Execute", "Circle", "Landing", "Orbit", "SAS", "Vessel" };

        #endregion

        public MainUI(ManualLogSource src_logger)
        {
            logger = src_logger;
            logger.LogMessage("MainGUI");
        }

        public void onGui()
        {
            if (Settings.debug_mode)
                // Mode selection debug
                Settings.current_interface_mode = (InterfaceMode)GUILayout.SelectionGrid((int)Settings.current_interface_mode,
                                                                                    interfaceModes_debug, 4);
            else
                Settings.current_interface_mode = (InterfaceMode)GUILayout.SelectionGrid((int)Settings.current_interface_mode,
                                                                        interfaceModes, interfaceModes.Count());

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
                    SimpleManeuverController.Instance.onGUI();
                    break;
<<<<<<< HEAD
=======
                case InterfaceMode.Landing:
                    LandingController.Instance.onGUI();
                    break;

                case InterfaceMode.Orbit:
                    K2D2.InfosPages.OrbitInfos.onGUI();
                    break;
                case InterfaceMode.SAS: K2D2.InfosPages.SASInfos.onGUI(); break;
                case InterfaceMode.Vessel: VesselInfos.onGUI(); break;
>>>>>>> d0e33bb... added Landing WIP
                default:
                    break;
            }
        }

        public static void SASInfos()
        {
            var current_vessel = K2D2_Plugin.Instance.current_vessel.VesselComponent;

            var sas = current_vessel.Autopilot.SAS;
            if (sas == null)
            {
                GUILayout.Label("NO SAS");
                return;
            }

            GUILayout.Label($"sas.dampingMode {sas.dampingMode}");
            GUILayout.Label($"sas.ReferenceFrame {sas.ReferenceFrame}");
            GUILayout.Label($"sas.AutoTune {sas.AutoTune}");
            GUILayout.Label($"sas.lockedMode {sas.lockedMode}");
            GUILayout.Label($"sas.LockedRotation {GeneralTools.VectorToString(sas.LockedRotation.eulerAngles)}");

            GUILayout.Label($"sas.TargetOrientation {GeneralTools.VectorToString(sas.TargetOrientation)}");
            GUILayout.Label($"sas.PidLockedPitch {sas.PidLockedPitch}");
            GUILayout.Label($"sas.PidLockedRoll {sas.PidLockedRoll}");
            GUILayout.Label($"sas.PidLockedYaw {sas.PidLockedYaw}");
        }

        void VesselInfo()
        {
            var vehicle = K2D2_Plugin.Instance.current_vessel.VesselVehicle;

            if (vehicle == null)
            {
                GUILayout.Label("No vessel !");
                return;
            }

            GUILayout.Label($"mainThrottle {vehicle.mainThrottle}");
            GUILayout.Label($"pitch {vehicle.pitch:n3} yaw {vehicle.yaw:n3} roll {vehicle.roll:n3}");

            GUILayout.Space(6);

            GUILayout.Label($"AltitudeFromTerrain {vehicle.AltitudeFromTerrain:n2}");
            GUILayout.Label($"Lat {vehicle.Latitude:n2} Lon {vehicle.Longitude:n2}");
            GUILayout.Label($"IsInAtmosphere {vehicle.IsInAtmosphere}");
            GUILayout.Label($"LandedOrSplashed {vehicle.LandedOrSplashed}");

            var body = K2D2_Plugin.Instance.current_vessel.currentBody();
            var coord = body.coordinateSystem;
            var body_location = Rotation.Reframed(vehicle.Rotation, coord);

            GUILayout.Label($"coordinate_system {vehicle.Rotation.coordinateSystem}");
            GUILayout.Label($"body_location {GeneralTools.VectorToString(body_location.localRotation.eulerAngles)}");
        }
    }
}