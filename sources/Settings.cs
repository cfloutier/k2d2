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

        public static MainUI.InterfaceMode current_interface_mode
        {
            get { return s_settings_file.GetEnum< MainUI.InterfaceMode> ("interface_setting", MainUI.InterfaceMode.ExeNode); }
            set{  s_settings_file.SetEnum< MainUI.InterfaceMode>("interface_setting", value); }
        }

        public static int warp_speed
        {
            get => s_settings_file.GetInt("warp.speed", 5);
            set {  s_settings_file.SetInt("warp.speed", value); }
        }

        public static int warp_safe_duration
        {
            get => s_settings_file.GetInt("warp.safe_duration", 10);
            set {
                if (value < 5) value = 5;
                s_settings_file.SetInt("warp.safe_duration", value);
            }
        }

        public static float burn_adjust
        {
            get => s_settings_file.GetFloat("warp.burn_adjust", 1.5f);
            set {             // value = Mathf.Clamp(0.1,)
                s_settings_file.SetFloat("warp.burn_adjust", value); }
        }

        public static float max_dv_error
        {
            get => s_settings_file.GetFloat("warp.max_dv_error", 0.1f);
            set {             // value = Mathf.Clamp(0.1,)
                s_settings_file.SetFloat("warp.max_dv_error", value); }
        }
    }


    public class SettingsUI
    {
        public static void onGUI()
        { 
            GUILayout.BeginHorizontal();
            UI_Tools.Title("// Settings");

            GUILayout.FlexibleSpace();

            // VERSION
            GUILayout.Label($"v{K2D2_Plugin.ModVer}", Styles.console_text);

            GUILayout.EndHorizontal();
            Settings.debug_mode = UI_Tools.Toggle(Settings.debug_mode, 
                "debug mode", "Debug mode open\nWIP features and verbose informations.");
        }
    }

}




