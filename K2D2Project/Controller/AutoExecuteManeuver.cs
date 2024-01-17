using BepInEx.Logging;
using K2D2.KSPService;
using KSP.Messages;
using KSP.Sim;
using KSP.Sim.Maneuver;
using KTools;
using KTools.UI;
using K2D2.InfosPages;

namespace K2D2.Controller;

public class ExecuteSettings
{
    public bool show_node_infos
    {
        get => KBaseSettings.sfile.GetBool("execute.show_node_infos", true);
        set
        {
            // value = Mathf.Clamp(value, 0 , 1);
            KBaseSettings.sfile.SetBool("execute.show_node_infos", value);
        }
    }

    public bool auto_warp
    {
        get => KBaseSettings.sfile.GetBool("execute.auto_warp", true);
        set
        {
            // value = Mathf.Clamp(value, 0 , 1);
            KBaseSettings.sfile.SetBool("execute.auto_warp", value);
        }
    }


    public enum StartMode { precise, constant, half_duration }
    private static string[] StartMode_Labels = { "T0", "before", "mid-duration" };
    public StartMode start_mode
    {
        get => KBaseSettings.sfile.GetEnum<StartMode>("execute.start_mode", StartMode.precise);
        set
        {
            // value = Mathf.Clamp(value, 0 , 1);
            KBaseSettings.sfile.SetEnum<StartMode>("execute.start_mode", value);
        }
    }

    public float start_before
    {
        get => KBaseSettings.sfile.GetFloat("execute.start_before", 1);
        set
        {
            // value = Mathf.Clamp(value, 0 , 1);
            KBaseSettings.sfile.SetFloat("execute.start_before", value);
        }
    }

    public void settings_UI()
    {
        show_node_infos = UI_Tools.Toggle(show_node_infos, "Show Nodes Infos");

        start_mode = UI_Tools.EnumGrid<StartMode>("Start Burn at :", start_mode, StartMode_Labels);

        if (start_mode == StartMode.constant)
        {
            start_before = UI_Tools.FloatSliderTxt("Start before T0", start_before, 0, 10, "s");
        }
    }

    public void warp_ui()
    {
        auto_warp = UI_Tools.Toggle(auto_warp, "Auto Warp", "warp time before burn");
        if (auto_warp)
            WarpToSettings.onGUI();
    }
}

public class AutoExecuteManeuver : ComplexController
{
    public ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.AutoExecuteManeuver");

    public static AutoExecuteManeuver Instance { get; set; }

    public ManeuverNodeData current_maneuver_node = null;
    ManeuverNodeData execute_node = null;

    ExecuteSettings execute_settings = new ExecuteSettings();

    // Sub Pilots
    TurnTo turn;
    WarpTo warp;
    BurnManeuver burn;

    //  ExecuteController current_pilot = null;
    KSPVessel current_vessel;

    public SingleExecuteController current_executor = new SingleExecuteController();

    public AutoExecuteManeuver()
    {
        Instance = this;
        debug_mode_only = false;
        name = "Node";
        K2D2PilotsMgr.Instance.RegisterPilot("Node", this);

        sub_contollers.Add(current_executor);
        current_vessel = K2D2_Plugin.Instance.current_vessel;

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

        this.mode = mode;

        if (mode == Mode.Off)
        {
            TimeWarpTools.SetRateIndex(0, false);
            current_executor.setController(null);
            return;
        }

        switch (mode)
        {
            case Mode.Off:
                current_executor.setController(null);
                break;
            case Mode.Turn:
                if (current_maneuver_node == null)
                    checkManeuver();

                if (current_maneuver_node == null)
                {
                    isRunning = false;
                    return;
                }

                execute_node = current_maneuver_node;
                current_executor.setController(turn);
                turn.StartManeuver(execute_node);
                break;
            case Mode.Warp:
                if (!execute_settings.auto_warp)
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
        if (current_maneuver_node == null)
            return false;

        var dt = current_maneuver_node.Time - GeneralTools.Game.UniverseModel.UniverseTime;
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
            return;
        }

        var next = this.mode + 1;
        setMode(next);
    }




    public override void onGUI()
    {
        if (K2D2_Plugin.Instance.settings_visible)
        {
            // default Settings UI
            K2D2Settings.onGUI();
            settingsUI();
            return;
        }

        if (!isRunning)
        {
            if (current_maneuver_node == null)
            {





                UI_Tools.Label("No Maneuver node.");

                TestFlightPlan.FPToolsUI();
                return;
            }

            if (!valid_maneuver)
            {
                UI_Tools.Label("Invalid Maneuver node.");
                UI_Tools.Console("Actually a KSP2 bug when loading scenaries. Please open map to fix it");
                return;
            }
        }

        if (execute_settings.show_node_infos)
        {
            node_infos();
        }

        if (!isRunning && !canStart())
        {
            UI_Tools.Label("No Maneuver node in the future");
            return;
        }

        isRunning = UI_Tools.BigToggleButton(isRunning, "Run", "Stop");

        current_executor.onGUI();
        if (!K2D2Settings.auto_next)
        {
            UI_Tools.Label($"finished {current_executor.finished}");
            if (!current_executor.finished)
            {
                if (UI_Tools.SmallButton("Next /!\\"))
                    nextMode();
            }
            else
            {
                if (UI_Tools.SmallButton("Next"))
                    nextMode();
            }
        }
    }

    public bool valid_maneuver = false;

    public bool checkManeuver()
    {
        current_maneuver_node = current_vessel.GetNextManeuveurNode();
        valid_maneuver = false;
        if (current_maneuver_node == null)
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
        
        if (isRunning)
        {
            if (execute_node == null)
                execute_node = current_maneuver_node;

            if (execute_node == null)
                return;

            //stagingController.CheckStaging();
            double UT = 0;
            switch (execute_settings.start_mode)
            {
                case ExecuteSettings.StartMode.precise:
                    UT = execute_node.Time;
                    break;
                case ExecuteSettings.StartMode.half_duration:
                    UT = execute_node.Time - execute_node.BurnDuration / 2;
                    break;
                case ExecuteSettings.StartMode.constant:
                    UT = execute_node.Time - execute_settings.start_before;
                    break;
            }

            warp.UT = UT;
            burn.UT = UT;
        }

        if (current_executor.finished && K2D2Settings.auto_next)
        {
            // auto next
            nextMode();
        }
    }


    public FoldOut accordion = new FoldOut();

    void settingsUI()
    {

        if (K2D2Settings.debug_mode)
        {
            K2D2Settings.auto_next = UI_Tools.Toggle(K2D2Settings.auto_next, "Auto Next Phase", "Debug Mode : Need to press next");
        }
        else
            K2D2Settings.auto_next = true;

        if (accordion.Count == 0)
        {
            // accordion.addChapter("Staging", StagingSettings.settings_UI);
            accordion.addChapter("Execute", execute_settings.settings_UI);
            accordion.addChapter("Turn", TurnToSettings.onGUI);
            accordion.addChapter("Warp", execute_settings.warp_ui);
            accordion.addChapter("Burn", BurnManeuverSettings.onGUI);

            accordion.singleChapter = true;
        }

        accordion.OnGUI();
    }

    void node_infos()
    {
        UI_Tools.Title("// Node Infos");
        ManeuverNodeData node = null;
        if (isRunning)
            node = execute_node;
        else
            node = current_maneuver_node;

        if (node == null)
            return;

        var dt = GeneralTools.remainingStartTime(node);
        UI_Tools.Label($"Node in <b>{StrTool.DurationToString(dt)}</b>");
        UI_Tools.Label($"dV {current_maneuver_node.BurnRequiredDV:n2} m/s");
        UI_Tools.Label($"Duration {StrTool.DurationToString(current_maneuver_node.BurnDuration)}");

        if (K2D2Settings.debug_mode)
        {
            if (dt < 0)
            {
                UI_Tools.Label("In The Past");
                return;
            }
        }
    }


    bool _active = false;
    public override bool isRunning
    {
        get { return _active; }
        set
        {
            if (value == _active)
                return;

            if (!value)
            {
                // stop
                if (current_vessel != null)
                    current_vessel.SetThrottle(0);

                setMode(Mode.Off);
                _active = false;
            }
            else
            {
                // reset controller to desactivate other controllers.
                K2D2_Plugin.ResetControllers();
                _active = true;

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
