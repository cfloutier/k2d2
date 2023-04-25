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
    public class Settings
    {
        public static SettingsFile s_settings_file = null;
        public static string s_settings_path;

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

        public static bool auto_next
        {
            get => s_settings_file.GetBool("auto_next", true);
            set {  s_settings_file.SetBool("auto_next", value); }
        }

        public static int window_x_pos
        {
            get => s_settings_file.GetInt("window_x_pos", 70);
            set { s_settings_file.SetInt("window_x_pos", value); }
        }

        public static int window_y_pos
        {
            get => s_settings_file.GetInt("window_y_pos", 50);
            set { s_settings_file.SetInt("window_y_pos", value); }
        }

        public static int current_interface_mode
        {
            get { return s_settings_file.GetInt("interface_setting", 0); }
            set{  s_settings_file.SetInt("interface_setting", value); }
        }

        public static void onGUI()
        {
            GUILayout.BeginHorizontal();
            UI_Tools.Title("// Settings");

            GUILayout.FlexibleSpace();
            // VERSION
            UI_Tools.Console($"v{K2D2_Plugin.ModVer}");

            if (UI_Tools.Button("back"))
                K2D2_Plugin.Instance.settings_visible = false;

            GUILayout.EndHorizontal();
            Settings.debug_mode = UI_Tools.Toggle(Settings.debug_mode,
                "debug mode", "Debug mode open\nWIP features and verbose informations.");
        }
    }



}




