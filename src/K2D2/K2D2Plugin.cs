using System.Reflection;
using BepInEx;
using JetBrains.Annotations;
using SpaceWarp;
using SpaceWarp.API.Assets;
using SpaceWarp.API.Mods;
using SpaceWarp.API.UI.Appbar;
using K2D2.UI;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;
using BepInEx.Logging;
using KTools;
using K2D2.KSPService;
using KSP.Game;
using KSP.Sim.ResourceSystem;

using KSP.Messages;
using K2D2.Controller;

namespace K2D2;

class L
{
    public static void Log(string txt)
    {
        K2D2Plugin.logger.LogInfo(txt);
    }

    public static void Vector3(string label, Vector3 value)
    {
        K2D2Plugin.logger.LogInfo(label + " : " + StrTool.Vector3ToString(value));
    }
}

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
public class K2D2Plugin : BaseSpaceWarpPlugin
{
    // Useful in case some other mod wants to use this mod a dependency
    [PublicAPI] public const string ModGuid = MyPluginInfo.PLUGIN_GUID;
    [PublicAPI] public const string ModName = MyPluginInfo.PLUGIN_NAME;
    [PublicAPI] public const string ModVer = MyPluginInfo.PLUGIN_VERSION;

    /// Singleton instance of the plugin class
    [PublicAPI] public static K2D2Plugin Instance { get; set; }

    // AppBar button IDs
    internal const string ToolbarFlightButtonID = "BTN-K2D2Flight";
    internal const string ToolbarOabButtonID = "BTN-K2D2OAB";
    internal const string ToolbarKscButtonID = "BTN-K2D2KSC";

    public static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2");

    public KSPVessel current_vessel = new KSPVessel();

    static bool loaded = false;

    /// <summary>
    /// Runs when the mod is first initialized.
    /// </summary>
    public override void OnInitialized()
    {
        base.OnInitialized();

        Instance = this;

        // Load all the other assemblies used by this mod
        LoadAssemblies();
        new K2D2PilotsMgr();
        SettingsFile.Init("K2d2_settings.json");
        

        gameObject.hideFlags = HideFlags.HideAndDontSave;
        DontDestroyOnLoad(gameObject);
        RegisterMessages();

        // create staging 
        new StagingController();

        // Load the UI from the asset bundle
        var myFirstWindowUxml = AssetManager.GetAsset<VisualTreeAsset>(
            // The case-insensitive path to the asset in the bundle is composed of:
            // - The mod GUID:
            $"{ModGuid}/" +
            // - The name of the asset bundle:
            "K2D2_ui/" +
            // - The path to the asset in your Unity project (without the "Assets/" part)
            "UI/K2D2_UI/Main_K2D2_Window.uxml"
        );

        // Create the window options object
        var windowOptions = new WindowOptions
        {
            // The ID of the window. It should be unique to your mod.
            WindowId = "K2D2",
            // The transform of parent game object of the window.
            // If null, it will be created under the main canvas.
            Parent = null,
            // Whether or not the window can be hidden with F2.
            IsHidingEnabled = true,
            // Whether to disable game input when typing into text fields.
            DisableGameInputForTextFields = true,
            MoveOptions = new MoveOptions
            {
                // Whether or not the window can be moved by dragging.
                IsMovingEnabled = true,
                // Whether or not the window can only be moved within the screen bounds.
                CheckScreenBounds = true
            }
        };

        // Create the window
        var k2d2_window = Window.Create(windowOptions, myFirstWindowUxml);
        // Add a controller for the UI to the window's game object
        var k2d2_window_controller = k2d2_window.gameObject.AddComponent<MyFirstWindowController>();

        // Register Flight AppBar button
        Appbar.RegisterAppButton(
            ModName,
            ToolbarFlightButtonID,
            AssetManager.GetAsset<Texture2D>($"{ModGuid}/images/icon.png"),
            isOpen => k2d2_window_controller.IsWindowOpen = isOpen
        );

        // Register OAB AppBar Button
        // Appbar.RegisterOABAppButton(
        //     ModName,
        //     ToolbarOabButtonID,
        //     AssetManager.GetAsset<Texture2D>($"{ModGuid}/images/icon.png"),
        //     isOpen => myFirstWindowController.IsWindowOpen = isOpen
        // );

        // Register KSC AppBar Button
        // Appbar.RegisterKSCAppButton(
        //     ModName,
        //     ToolbarKscButtonID,
        //     AssetManager.GetAsset<Texture2D>($"{ModGuid}/images/icon.png"),
        //     () => myFirstWindowController.IsWindowOpen = !myFirstWindowController.IsWindowOpen
        // );

        loaded = true;
    }



    /// <summary>
    /// Loads all the assemblies for the mod.
    /// </summary>
    private static void LoadAssemblies()
    {
        Debug.Log("Trying to load K2D2 Assemblies");
        // Load the Unity project assembly
        var currentFolder = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory!.FullName;
        var unityAssembly = Assembly.LoadFrom(Path.Combine(currentFolder, "K2D2.Unity.dll"));
        // Register any custom UI controls from the loaded assembly
         Debug.Log("assemblie : "+  unityAssembly);
        CustomControls.RegisterFromAssembly(unityAssembly);
    }

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

    void Update()
    {
        // main_ui?.Update();

        Debug.developerConsoleVisible = false;
        // Update Models (even on non valid scenes)
        current_vessel.Update();

        if (K2D2OtherModsInterface.instance == null)
        {
            var other_mods = new K2D2OtherModsInterface();
            other_mods.CheckModsVersions();
        }

        // if (ValidScene())
        // {
        //     // Debug.developerConsoleVisible = false;
        //     if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.O))
        //         ToggleAppBarButton(!drawUI);

        //     _maneuverProvider.Update();
        //     StagingController.Instance.Update();

        //     if (!StagingController.Instance.is_staging)
        //     {
        //         // Update Controllers only if staging is not in progress
        //         controllerManager.UpdateControllers();
        //     }   

        //     UI_Tools.OnUpdate();
        // }
    }

      // call on reset on controller, each on can reset it's status
    public static void ResetControllers()
    {
        if (!loaded) return;
        StagingController.Instance.onReset();
        Instance.controllerManager.onReset(); 
    }

    public bool settings_visible = false;

    ControllerManager controllerManager = new ControllerManager();

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

    private void RegisterMessages()
    {
        Game.Messages.Subscribe<GameStateChangedMessage>(msg =>
        {
            var message = (GameStateChangedMessage)msg;

            // if (message.CurrentState == GameState.FlightView)
            // {
            //     ShapeDrawer.Instance.can_draw = true;
            // }
            // else if (message.PreviousState == GameState.FlightView)
            // {
            //     ShapeDrawer.Instance.can_draw = false;
            // }
        });

        Game.Messages.Subscribe<VesselChangedMessage>(msg =>
        {
            var message = (VesselChangedMessage)msg;
            ResetControllers();
        });
    }
}
