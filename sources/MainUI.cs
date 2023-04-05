
using UnityEngine;
using KSP.Sim;
using BepInEx.Logging;
using SpaceWarp.API.Assets;
using KSP.Sim.DeltaV;

using System.Collections.Generic;

using K2D2.Controller;
using K2D2.InfosPages;
using KSP.Game;

namespace K2D2
{
    public class MainUI
    {
        public ManualLogSource logger;

        public enum InterfaceMode { ExeNode, Circularize, Landing, Orbit, SAS, Vessel  }

        private static string[] interfaceModes = { "Execute", "Circle", "landing" };
        private static string[] interfaceModes_debug = { "Execute", "Circle", "Landing", "Orbit", "SAS", "Vessel" };

        public MainUI(ManualLogSource src_logger)
        {
            logger = src_logger;
            logger.LogMessage("MainGUI");
        }


       

        public void onGUI()
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
                    AutoExecuteManeuver.Instance.onGUI();
                    break;
                case InterfaceMode.Circularize:
                    SimpleManeuverController.Instance.onGUI();
                    break;
                case InterfaceMode.Landing:
                    LandingController.Instance.onGUI();
                    break;

                case InterfaceMode.Orbit:
                    K2D2.InfosPages.OrbitInfos.onGUI();
                    break;
                case InterfaceMode.SAS: K2D2.InfosPages.SASInfos.onGUI(); break;
                case InterfaceMode.Vessel: VesselInfos.onGUI(); break;
                default:
                    break;
            }
        }

    }
}