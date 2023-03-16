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

namespace COSMAT
{
    [BepInDependency(SpaceWarpPlugin.ModGuid,SpaceWarpPlugin.ModVer)]
    [BepInPlugin(ModGuid, ModName, ModVer)]
    public class AutoExecuteNode : BaseSpaceWarpPlugin
    {
        public static AutoExecuteNode Instance { get; set; }

        public const string ModGuid = "AutoExecuteNode";
        public const string ModName = "AutoExecuteNode";
        public const string ModVer = "0.1.0";

        #region Fields

        // Main.
        public static bool loaded = false;
        public static AutoExecuteNode instance;

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
        AutoExecuteManeuver auto_execute_maneuver;

        public static string mod_id;

        #endregion


        public override void OnInitialized()
        {
            if (loaded)
            {
                Destroy(this);
            }

            logger = BepInEx.Logging.Logger.CreateLogSource("AutoExecuteNode");

            mod_id = SpaceWarpMetadata.ModID;

            loaded = true;
            instance = this;

            gameObject.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(gameObject);

            logger.LogMessage("building AutoExecuteManeuver");
            auto_execute_maneuver = new AutoExecuteManeuver(logger);
            main_ui = new MainUI(logger);
            Settings.Init(SettingsPath, logger);


            Appbar.RegisterAppButton(
                // "C.O.S.M.A.T",
                "Auto-Node",
                "BTN-AutoExecuteNodeButton",
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
                    // "<color=#00D346>C.O.S.M.A.T</color>",
                    "<color=#00D346>Auto-Node</color>",
                    Styles.window,
                    GUILayout.Height(0),
                    GUILayout.Width(350));
            }
        }

        public void ToggleButton(bool toggle)
        {
            drawUI = toggle;
            GameObject.Find("BTN-AutoExecuteNodeButton")?.GetComponent<UIValue_WriteBool_Toggle>()?.SetValue(toggle);
        }


        private void FillWindow(int windowID)
        {
            GUILayout.BeginVertical();

            if (GUI.Button(new Rect(windowRect.width - 30, 4, 25, 25), "X", Styles.small_button))
                ToggleButton(false);

            main_ui.onGui();

            GUILayout.EndVertical();
        }
    }

}