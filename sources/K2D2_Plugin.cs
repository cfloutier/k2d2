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
using JetBrains.Annotations;
using K2D2.Controller;
using K2D2.Models;
using K2D2.sources.Models;
using K2D2.KSPService;
using K2D2.sources.KSPService;
using Action = System.Action;

namespace K2D2
{
    [BepInDependency(SpaceWarpPlugin.ModGuid,SpaceWarpPlugin.ModVer)]
    [BepInPlugin(ModGuid, ModName, ModVer)]
    public class K2D2_Plugin : BaseSpaceWarpPlugin
    {
        public static K2D2_Plugin Instance { get; private set; }

        public const string ModGuid = "K2D2";
        public const string ModName = "K2D2";
        public const string ModVer = "0.5.1";

        #region Fields

        // Main.
        public static bool loaded = false;

        // Paths.
        private static string _assemblyFolder;
        private static string AssemblyFolder =>
            _assemblyFolder ?? (_assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

        private static string _settingsPath;
        private static string SettingsPath =>
            _settingsPath ?? (_settingsPath = Path.Combine(AssemblyFolder, "settings.json"));

        public static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2");
        // GUI.

        private bool drawUI = false;
        private Rect windowRect = Rect.zero;

        private PopUp _popUp = new PopUp();
        private PopUpContent _popUpContent;


        private static GameState[] validScenes = new[] { GameState.FlightView, GameState.Map3DView };

        //private static GameState last_game_state ;

        private static bool ValidScene()
        {
            if (GeneralTools.Game == null) return false;


            var state = GeneralTools.Game.GlobalGameState.GetState();
            bool is_valid = validScenes.Contains(state);
            if (!is_valid)
            {
                ResetControllers();
            }
            return is_valid;
        }

        // call on reset on controller, each on can reset it's status
        public static void ResetControllers()
        {
            if (!loaded) return;
            Instance.controllerManager.onReset();
        }

        // Controllers
        MainUI main_ui;

        public bool settings_visible = false;

        ControllerManager controllerManager = new ControllerManager();

        public KSPVessel current_vessel = new KSPVessel();

        public static string mod_id;

        public ManeuverManager maneuverManager = new ManeuverManager();

        private ManeuverProvider _maneuverProvider;

        #endregion

        public override void OnInitialized()
        {
            if (loaded)
            {
                Destroy(this);
            }
            Instance = this;

            Settings.Init(SettingsPath);
            mod_id = SpaceWarpMetadata.ModID;

            loaded = true;
            Instance = this;

            gameObject.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(gameObject);

            logger.LogMessage("building AutoExecuteManeuver");

            // Setups
            _maneuverProvider = new ManeuverProvider(ref maneuverManager,logger);

            // Add Controllers that inherit from BaseController here:
            controllerManager.AddController(new SimpleManeuverController(logger,ref _maneuverProvider));
            controllerManager.AddController(new AutoExecuteManeuver());
            controllerManager.AddController(new LandingController());

            // Add PopUp Tabs here:
            _popUpContent = new PopUpContent(ref _popUp);
            _popUp.AddPopUpContents("Active Maneuvers",new Action (()=>_popUpContent.DisplayManeuverList(ref maneuverManager)));

            main_ui = new MainUI(logger);

            Appbar.RegisterAppButton(
                "K2-D2",
                "BTN-K2D2Button",
                AssetManager.GetAsset<Texture2D>($"{SpaceWarpMetadata.ModID}/images/icon.png"),
                ToggleButton);
        }

        void Awake()
        {
         
        }

        void check_rect_pos()
        {
            if (windowRect == Rect.zero)
            {
                int x_pos = Settings.window_x_pos;
                int y_pos = Settings.window_y_pos;

                if (x_pos == -1)
                {
                    x_pos = 100;
                    y_pos = 50;
                }

                windowRect = new Rect(x_pos, y_pos, 500, 100);
            }

            if (windowRect.xMax > Screen.width)
            {
                var dx = Screen.width - windowRect.xMax;
                windowRect.x += dx;
            }
            if (windowRect.yMax > Screen.height)
            {
                var dy = Screen.height - windowRect.yMax;
                windowRect.y += dy;
            }
            if (windowRect.xMin < 0)
            {
                windowRect.x = 0;
            }
            if (windowRect.yMin < 0)
            {
                windowRect.y = 0;
            }
        }

        void save_rect_pos()
        {
            Settings.window_x_pos = (int)windowRect.xMin;
            Settings.window_y_pos = (int)windowRect.yMin;
        }

        void Update()
        {
            if (ValidScene())
            {
                // Debug.developerConsoleVisible = false;
                if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.O) )
                    ToggleButton(!drawUI);

                // Update Models
                current_vessel.Update();
                _maneuverProvider.Update();

                // Update Controllers
                controllerManager.UpdateControllers();

            }
        }

        void FixedUpdate()
        {
            if (ValidScene())
            {
                controllerManager.FixedUpdateControllers();
            }
        }

        private void LateUpdate()
        {
            if (ValidScene())
            {
                controllerManager.LateUpdateControllers();
            }
        }

        void OnGUI()
        {
            if(!ValidScene())
                return;


            if (drawUI)
            {
                GUI.skin = Skins.ConsoleSkin;

                check_rect_pos();
                Styles.Init();


                windowRect = GUILayout.Window(
                    GUIUtility.GetControlID(FocusType.Passive),
                    windowRect,
                    FillWindow,
                    "<color=#71DBDB>K2-D2</color>",
                    Styles.window,
                    GUILayout.Height(0),
                    GUILayout.Width(350));

                save_rect_pos();
            }
            if (_popUp.isPopupVisible)
            {
                _popUp.OnGUI();
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
            settings_visible = GUI.Toggle(new Rect(windowRect.width - 56, 4, 25, 25), settings_visible, Styles.gear, Styles.small_button);
            if(GUI.Button(new Rect(windowRect.width - 81, 4, 25, 25), "P", Styles.small_button))
                _popUp.isPopupVisible = !_popUp.isPopupVisible;

            GUI.Label(new Rect(9, 2, 29, 29), Styles.big_icon, Styles.icons_label);

            GUILayout.BeginVertical();

            main_ui.onGUI();

            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, 10000, 500));
        }
    }

}