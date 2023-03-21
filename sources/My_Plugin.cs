using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;

using UnityEngine;

using KSP.Game;
using KSP.Sim.ResourceSystem;
using KSP.UI.Binding;

using BepInEx;
using SpaceWarp;
using SpaceWarp.API;
using SpaceWarp.API.Mods;
using SpaceWarp.API.Assets;
using SpaceWarp.API.UI;
using SpaceWarp.API.UI.Appbar;
using BepInEx.Logging;

namespace K2D2
{
    [BepInDependency(SpaceWarpPlugin.ModGuid,SpaceWarpPlugin.ModVer)]
    [BepInPlugin(ModGuid, ModName, ModVer)]
    public class K2D2_Plugin : BaseSpaceWarpPlugin
    {
        public static K2D2_Plugin Instance { get; set; }

        public const string ModGuid = "K2D2";
        public const string ModName = "K2D2";
        public const string ModVer = "0.3.0";

        #region Fields

        // Main.
        public static bool loaded = false;
        public static K2D2_Plugin instance;

        // Paths.
        private static string _assemblyFolder;
        private static string AssemblyFolder =>
            _assemblyFolder ?? (_assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

        private static string _settingsPath;
        private static string SettingsPath =>
            _settingsPath ?? (_settingsPath = Path.Combine(AssemblyFolder, "settings.json"));

        public ManualLogSource logger;
        // GUI.

        private bool drawUI = false;
        private Rect windowRect;
        private int windowWidth = 500;
        private int windowHeight = 700;

        private static GameState[] validScenes = new[] { GameState.FlightView, GameState.Map3DView };

        private static bool ValidScene()
        {
            if (Tools.Game() == null) return false;
            var state = Tools.Game().GlobalGameState.GetState();
            return validScenes.Contains(state);
        }

        MainUI main_ui;

        public bool settings_visible = false;

        AutoExecuteManeuver auto_execute_maneuver;

        public static string mod_id;

        #endregion

        public override void OnInitialized()
        {
            if (loaded)
            {
                Destroy(this);
            }

            Settings.Init(SettingsPath);

            logger = BepInEx.Logging.Logger.CreateLogSource("K2D2");

            mod_id = SpaceWarpMetadata.ModID;

            loaded = true;
            instance = this;

            gameObject.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(gameObject);

            logger.LogMessage("building AutoExecuteManeuver");
            auto_execute_maneuver = new AutoExecuteManeuver(logger);
            main_ui = new MainUI(logger);

            Appbar.RegisterAppButton(
                "K2-D2",
                "BTN-K2D2Button",
                AssetManager.GetAsset<Texture2D>($"{SpaceWarpMetadata.ModID}/images/icon.png"),
                ToggleButton);
        }

        void Awake()
        {
            windowRect = new Rect((Screen.width * 0.7f) - (windowWidth / 2), (Screen.height / 2) - (windowHeight / 2), 0, 0);
        }

        void Update()
        {
            if (ValidScene())
            {
                if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.O) )
                    ToggleButton(!drawUI);

                if (auto_execute_maneuver != null)
                    auto_execute_maneuver.Update();
            }
        }

        void OnGUI()
        {
            if (drawUI && ValidScene())
            {

                GUI.skin = Skins.ConsoleSkin;
                Styles.Init();

                windowRect = GUILayout.Window(
                    GUIUtility.GetControlID(FocusType.Passive),
                    windowRect,
                    FillWindow,
                    "<color=#00D346>K2-D2</color>",
                    Styles.window,
                    GUILayout.Height(0),
                    GUILayout.Width(350));
            }
        }

        public void ToggleButton(bool toggle)
        {
            drawUI = toggle;
            GameObject.Find("BTN-K2D2Button")?.GetComponent<UIValue_WriteBool_Toggle>()?.SetValue(toggle);
        }


        private void FillWindow(int windowID)
        {
            if (GUI.Button(new Rect(windowRect.width - 30, 4, 25, 25), "X", Styles.small_button))
                ToggleButton(false);

            // settings button
            if (GUI.Button(new Rect(windowRect.width - 56, 4, 25, 25), Styles.gear, Styles.small_button))
                settings_visible = !settings_visible;

            GUI.Label(new Rect(22, 2, 24, 24), Styles.icon, Styles.icons_label);

            GUILayout.BeginVertical();

            if (settings_visible)
            {
                SettingsUI.onGui();
            }
            else
            {
                main_ui.onGui();
            }

            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, 10000, 500));
        }
    }

}