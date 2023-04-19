
using UnityEngine;
using KSP.Sim;
using BepInEx.Logging;
using SpaceWarp.API.Assets;
using KSP.Sim.DeltaV;

using System.Collections.Generic;
using System;
using K2D2.Controller;
using K2D2.InfosPages;
using KSP.Game;

namespace K2D2
{
    public class MainTabs
    {
        public static bool TabButton(bool is_current, bool isActive, string txt)
        {
            GUIStyle style = isActive ? Styles.tab_active : Styles.tab_normal;
            return GUILayout.Toggle(is_current, txt, style);
        }

        public static int DrawTabs(int current, string [] interfaceModes, bool[] is_actives, int max_line = 3)
        {
            current = GeneralTools.ClampInt(current, 0, interfaceModes.Length -1);
            GUILayout.BeginHorizontal();

            int result = current;


            int index_in_line = 0;

            for (int index = 0 ; index < interfaceModes.Length; index++)
            {
                if (index_in_line >= max_line)
                {
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    index_in_line = 0;
                }

                bool is_current = current == index;
                if (TabButton(is_current, is_actives[index], interfaceModes[index]))
                {
                    if (!is_current)
                        result = index;
                }

                index_in_line++;
            }

            GUILayout.EndHorizontal();
            return result;
        }
    }

    public class MainUI
    {
        public ManualLogSource logger;

        private static string[] interfaceModes = { "Execute", "Landing", "V-Speed", };
        private static string[] interfaceModes_debug = { "Execute", "Landing", "V-Speed", "Lift", "Attitude", "Navigation", "Orbit", "SAS", "Vessel" };

        bool init_done = false;
        public List<BaseController> pages = new List<BaseController>();

        public MainUI(ManualLogSource src_logger)
        {
            logger = src_logger;
            logger.LogMessage("MainGUI");
        }

        public bool[] active_pages
        {
            get {
                bool[] result = new bool[pages.Count];
                for (int i = 0 ; i < pages.Count; i++)
                    result[i] = pages[i].isActive;
                return result;
            }
        }

        public void onGUI()
        {
            if (!init_done)
            {
                pages.Add(AutoExecuteManeuver.Instance);
                pages.Add(LandingController.Instance);
                pages.Add(VSpeedController.Instance);

                pages.Add(AutoLiftController.Instance);
                pages.Add(AttitudeController.Instance);
                pages.Add(SimpleManeuverController.Instance);
                pages.Add(new OrbitInfos());
                pages.Add(new K2D2.InfosPages.SASInfos());
                pages.Add(new VesselInfos());

                Settings.current_interface_mode  = GeneralTools.ClampInt(Settings.current_interface_mode, 0, pages.Count-1);
                pages[Settings.current_interface_mode].ui_visible = true;
            }

            string [] pages_str = Settings.debug_mode ? interfaceModes_debug : interfaceModes;
            int result = MainTabs.DrawTabs( Settings.current_interface_mode, pages_str, active_pages, 4);
            if (result != Settings.current_interface_mode)
            {
                pages[Settings.current_interface_mode].ui_visible = false;
                Settings.current_interface_mode = result;
                pages[result].ui_visible = true;
            }
        
            pages[result].onGUI();
        }
    }
}