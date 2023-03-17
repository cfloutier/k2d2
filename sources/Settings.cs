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
        public static K2D2Settings settings { get; set; }
        public static string settings_path;

        private static ManualLogSource logger;

        public static void Init(string settings_path, ManualLogSource logger )
        {
            Settings.settings_path = settings_path;
            Settings.logger = logger;
            Load();
        }

        public static bool debug_mode
         {
            get => Settings.settings.debug_mode;
            set {
                    if (value == Settings.settings.debug_mode) return;
                    Settings.settings.debug_mode = value;
                    Settings.Save();
            }
        }


        static public void Save()
        {
            try 
            {
                File.WriteAllText(settings_path, JsonConvert.SerializeObject(settings));
            }
            catch (Exception ex)
            {
                logger.LogError($"Save exception {ex}");
            }
        }

        static void Load()
        {
            try
            {
                settings = JsonConvert.DeserializeObject<K2D2Settings>(File.ReadAllText(settings_path));
            }

            catch (FileNotFoundException ex)
            {
                Settings.logger.LogWarning($"Load exception {ex}");
                settings = new K2D2Settings();
            }
            catch (Exception ex)
            {
                Settings.logger.LogError($"Save exception {ex}");
                settings = new K2D2Settings();
            }
        }
    }

}




