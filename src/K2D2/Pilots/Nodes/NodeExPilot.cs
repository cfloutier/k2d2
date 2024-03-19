using BepInEx.Logging;
using K2D2.KSPService;
using KSP.Messages;
using KSP.Sim;
using KSP.Sim.Maneuver;
using KTools;

// using KTools.UI;
// using K2D2.InfosPages;
using UnityEngine;

namespace K2D2.Controller;

public class NodeExPilot : Pilot
{
    public ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.NodeExecute");

    public static NodeExPilot Instance { get; set; }

    public ManeuverNodeData next_maneuver_node = null;
    ManeuverNodeData execute_node = null;

    NodeExecuteSettings execute_settings = new NodeExecuteSettings();

    // Sub Pilots
    TurnTo turn;
    WarpTo warp;
    BurnManeuver burn;

    //  ExecuteController current_pilot = null;
    KSPVessel current_vessel;

    public SingleExecuteController current_executor = new SingleExecuteController();

    public NodeExPilot()
    {
        Instance = this;
        debug_mode_only = false;
        name = "Node";
        K2D2PilotsMgr.Instance.RegisterPilot("Node", this);

        sub_contollers.Add(current_executor);
        current_vessel = K2D2Plugin.Instance.current_vessel;

        GeneralTools.Game.Messages.Subscribe<VesselChangedMessage>(OnActiveVesselChanged);

        turn = new TurnTo();
        warp = new WarpTo();
        burn = new BurnManeuver();
    }

    public void OnActiveVesselChanged(MessageCenterMessage msg)
    {
        Stop();
    }

    public enum Mode
    {
        Off,
        Turn,
        Warp,
        Burn
    }

    public Mode mode = Mode.Off;

    public void setMode(Mode mode)
    {
        if (mode == this.mode)
            return;

        logger.LogInfo("setMode " + mode);  
        if (mode == Mode.Off)
        {
            TimeWarpTools.SetRateIndex(0, false);
            current_executor.setController(null);
            this.mode = mode;
            return;
        }

        if (next_maneuver_node == null)
            checkManeuver();

        if (next_maneuver_node == null)
        {
            Stop();
            return;
        }

        this.mode = mode;
        switch (mode)
        {
            case Mode.Off:
                current_executor.setController(null);
                break;
            case Mode.Turn:
                // start
                execute_node = next_maneuver_node;
                current_executor.setController(turn);
                turn.StartManeuver(execute_node);
                break;
            case Mode.Warp:
                if (!execute_settings.auto_warp.V)
                {
                    setMode(Mode.Burn);
                    return;
                }
                current_executor.setController(warp);
                warp.StartManeuver(execute_node);
                break;
            case Mode.Burn:
                current_executor.setController(burn);
                burn.StartManeuver(execute_node);
                break;
        }

        logger.LogInfo("setMode " + mode);
    }

    public bool canStart()
    {
        if (next_maneuver_node == null)
            return false;

        var dt = GeneralTools.remainingStartTime(next_maneuver_node);
        if (dt < 0)
        {
            return false;
        }

        return true;
    }

    public void nextMode()
    {
        if (mode == Mode.Off)
        {
            Start();
            return;
        }
        if (mode == Mode.Burn)
        {
            Stop();
            if (execute_settings.pause_on_end.V)
                TimeWarpTools.SetIsPaused(true);
            return;
        }

        var next = this.mode + 1;
        setMode(next);
    }

    // public override void onGUI()
    // {
    //     if (K2D2Plugin.Instance.settings_visible)
    //     {
    //         // default Settings UI
    //         K2D2Settings.onGUI();
    //         settingsUI();
    //         return;
    //     }

    //     // UI_Tools.Console($"mode : {mode}");
    //     if (!isRunning)
    //     {
    //         if (next_maneuver_node == null)
    //         {
    //             UI_Tools.Label("No Maneuver node.");
    //             TestFlightPlan.FPToolsUI();
    //             return;
    //         }

    //         if (!valid_maneuver)
    //         {
    //             UI_Tools.Label("Invalid Maneuver node.");
    //             UI_Tools.Console("Actually a KSP2 bug when loading scenaries. Please open map to fix it");
    //             return;
    //         }
    //     }

    //     if (execute_settings.show_node_infos)
    //     {
    //         node_infos();
    //     }

    //     if (!isRunning && !canStart())
    //     {
    //         UI_Tools.Label("No valid Maneuver node found");
    //         return;
    //     }

    //     GUILayout.BeginHorizontal();
    //     isRunning = UI_Tools.BigToggleButton(isRunning, "Run", "Stop");

    //     execute_settings.pause_on_end = GUILayout.Toggle(execute_settings.pause_on_end, 
    //                             new GUIContent(KBaseStyle.pause, "Auto-Pause when the Node is executed"),
    //                              KBaseStyle.big_button_warning, GUILayout.Width(40));

    //     GUILayout.EndHorizontal();
    //     // call the current UI
    //     current_executor.onGUI();
    // }

    public bool valid_maneuver = false;

    public bool checkManeuver()
    {
        next_maneuver_node = current_vessel.GetNextManeuveurNode();
        valid_maneuver = false;
        if (next_maneuver_node == null)
        {
            Stop();
            return false;
        }

        double ut;

        var plan_solver = current_vessel.GetPlanSolver();
        if (plan_solver == null)
        {
            Stop();
            return false;
        }

        // check that the maneuver is well declared.
        Vector velocity_after_maneuver = plan_solver.GetVelocityAfterFirstManeuver(out ut);
        if (ut == 0)
        {
            // error
            Stop();
            return false;
        }

        valid_maneuver = true;
        return true;
    }

    public override void Update()
    {
        checkManeuver();
        base.Update();

         if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.O))
            isRunning = !isRunning;

        if (isRunning)
        {
            if (execute_node == null)
            {
                Stop();
                return;
            }

            //stagingController.CheckStaging();
            double UT = 0;
            switch (execute_settings.start_mode.Value)
            {
                case NodeExecuteSettings.StartMode.precise:
                    UT = execute_node.Time;
                    break;
                case NodeExecuteSettings.StartMode.half_duration:
                    UT = execute_node.Time - execute_node.BurnDuration / 2;
                    break;
                case NodeExecuteSettings.StartMode.constant:
                    UT = execute_node.Time - execute_settings.start_before.V;
                    break;
            }

            warp.UT = UT;
            burn.UT = UT;
        }
        else
        {
           
        }
       
        if (current_executor.finished)
        {
            // auto next
            nextMode();
        }
    }

    // public FoldOut accordion = new FoldOut();

    // void settingsUI()
    // {
    //     if (accordion.Count == 0)
    //     {
    //         // accordion.addChapter("Staging", StagingSettings.settings_UI);
    //         accordion.addChapter("Execute", execute_settings.settings_UI);
    //         accordion.addChapter("Turn", TurnToSettings.onGUI);
    //         accordion.addChapter("Warp", execute_settings.warp_ui);
    //         accordion.addChapter("Burn", BurnManeuverSettings.onGUI);

    //         accordion.singleChapter = true;
    //     }

    //     accordion.OnGUI();
    // }

    // void node_infos()
    // {
    //     UI_Tools.Title("Node Infos");
    //     ManeuverNodeData node = null;
    //     if (isRunning)
    //         node = execute_node;
    //     else
    //         node = next_maneuver_node;

    //     if (node == null)
    //         return;

    //     var dt = GeneralTools.remainingStartTime(node);
    //     UI_Tools.Label($"Node in <b>{StrTool.DurationToString(dt)}</b>");
    //     UI_Tools.Label($"dV {node.BurnRequiredDV:n2} m/s");
    //     UI_Tools.Label($"Duration {StrTool.DurationToString(node.BurnDuration)}");

    //     if (K2D2Settings.debug_mode)
    //     {
    //         if (dt < 0)
    //         {
    //             UI_Tools.Label("In The Past");
    //             return;
    //         }
    //     }
    // }


    public override bool isRunning
    {
        get { return mode != Mode.Off; }
        set
        {
            if (isRunning == value)
                return;
            
            if (!value)
            {
                // stop
                if (current_vessel != null)
                    current_vessel.SetThrottle(0);

                setMode(Mode.Off);          
            }
            else
            {
                // reset controller to desactivate other controllers.
                K2D2Plugin.ResetControllers();
                TimeWarpTools.SetIsPaused(false);
                
                setMode(Mode.Turn);
            }
        }
    }

    public void Start()
    {
        isRunning = true;       
    }

    public void Stop()
    {
        isRunning = false;
    }

    public override void onReset()
    {
        Stop();
    }

}
