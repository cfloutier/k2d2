
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
                case InterfaceMode.Circularize:
                    SimpleManeuverController.Instance.onGUI();
                    break;
                case InterfaceMode.Landing:
                    LandingController.Instance.onGUI();
                    break;
                default:
                    break;
            }
        }
    }
}