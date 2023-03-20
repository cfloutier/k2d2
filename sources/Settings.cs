using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using BepInEx.Logging;
using Newtonsoft.Json;

namespace K2D2
{
    public class K2D2Settings
    {
        public MainUI.InterfaceMode current_mode = MainUI.InterfaceMode.ExeNode;

        public bool debug_mode = false;
    }

    public class Settings
    {
        public static SettingsFile s_settings_file = null;
        public static string s_settings_path;

        private static ManualLogSource logger;

        public static void Init(string settings_path)
        {
            s_settings_file = new SettingsFile(settings_path);
        }

        // each setting is defined by an accessor pointing on s_settings_file
        // the convertion to type is made here
        // this way we can have any kind of settings without hard work 




        public static bool debug_mode
        {
            get => s_settings_file.GetBool("debug_mode", false);
            set {  s_settings_file.SetBool("debug_mode", value); }
        }

        public static MainUI.InterfaceMode current_interface_mode
        {
            get { return s_settings_file.GetEnum< MainUI.InterfaceMode> ("interface_setting", MainUI.InterfaceMode.ExeNode); }
            set{  s_settings_file.SetEnum< MainUI.InterfaceMode>("interface_setting", value); }
        }

        public static int warp_speed
        {
            get => s_settings_file.GetInt("warp.speed", 1);
            set {  s_settings_file.SetInt("warp.speed", value); }
        }

        public static int warp_safe_duration
        {
            get => s_settings_file.GetInt("warp.safe_duration", 10);
            set {
                if (value < 5) value = 5;
                s_settings_file.SetInt("warp.safe_duration", value); }
        }
    }

    public class SettingsUI
    {
        public static void onGui()
        {
            GUILayout.Label("Settings", Styles.title);

            GUILayout.Label("Debug mode open work in progress features and verbose information.", Styles.console);
            Settings.debug_mode = GUILayout.Toggle(Settings.debug_mode, "debug mode");

            GUILayout.Label("Warp", Styles.title);


            GUILayout.Label("Warp Speed ", Styles.console);
            Settings.warp_speed = (int) GUILayout.HorizontalSlider((int) Settings.warp_speed, 1, 10);
            GUILayout.Label("(1 : quick, 10 slow) ", Styles.console);

            GUILayout.Label("Warp Speed ", Styles.console);
            Settings.warp_safe_duration = UI_Tools.LayoutIntField(Settings.warp_safe_duration);
            GUILayout.Label("nb seconds before launch ", Styles.console);

        }
    }

}




