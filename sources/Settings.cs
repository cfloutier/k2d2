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
        public static void onGui()
        {
            GUILayout.Label("Settings", Styles.title);

            GUILayout.Label("Debug mode open work in progress features and verbose information.", Styles.small_dark_text);
            Settings.debug_mode = GUILayout.Toggle(Settings.debug_mode, "debug mode");

            if (Settings.debug_mode)
            {
                GUILayout.Label("Auto_Execute Next phase.", Styles.small_dark_text);
                Settings.auto_next = GUILayout.Toggle(Settings.auto_next, "Auto Next Phase");
            }
            else
                Settings.auto_next = true;

            GUILayout.Label("Warp", Styles.title);

            Settings.warp_speed = UI_Tools.IntSlider("Warp Speed" , Settings.warp_speed, 1, 10);
            GUILayout.Label("(1 : quick, 10 slow) ", Styles.small_dark_text);

            GUILayout.Label("Safe time", Styles.small_dark_text);

            Settings.warp_safe_duration = UI_Tools.IntField("warp_safe_duration", Settings.warp_safe_duration, 5, int.MaxValue);
            GUILayout.Label("nb seconds in x1 before launch (min:5)", Styles.small_dark_text);

            GUILayout.Label("Burn", Styles.title);

            Settings.burn_adjust = UI_Tools.FloatSlider("burn_adjust", Settings.burn_adjust, 0, 2);
            GUILayout.Label("adjusting rate", Styles.small_dark_text);

            Settings.max_dv_error = UI_Tools.FloatSlider("max_dv_error", Settings.max_dv_error, 0.001f, 2);
            GUILayout.Label("accepted dV difference (m/s)", Styles.small_dark_text);

            // VERSION
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label($"v{K2D2_Plugin.ModVer}", Styles.small_dark_text);

            GUILayout.EndHorizontal();
        }
    }

}




