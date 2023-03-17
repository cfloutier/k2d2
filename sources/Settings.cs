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

    }

}




