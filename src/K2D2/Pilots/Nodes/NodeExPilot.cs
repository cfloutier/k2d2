using BepInEx.Logging;
using K2D2.Controller;
using K2D2.KSPService;
using K2UI;
using KSP.Messages;
using KSP.Sim;
using KSP.Sim.Maneuver;
using KTools;

// using KTools.UI;
// using K2D2.InfosPages;
using UnityEngine;

namespace K2D2.Node;

public class NodeExPilot : Pilot
{
    public ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.NodeExecute");

    public static NodeExPilot Instance { get; set; }

    public ManeuverNodeData next_maneuver_node = null;
    public ManeuverNodeData execute_node = null;

    public NodeExSettings settings = new NodeExSettings();

    // Sub Pilots
    TurnTo turn;
    WarpTo warp;
    BurnManeuver burn;

    //  ExecuteController current_pilot = null;
    KSPVessel current_vessel;

    public SingleExecuteController current_executor = new SingleExecuteController();

    NodeExUI ui;

    public NodeExPilot()
    {
        Instance = this;
        debug_mode_only = false;

        _page = ui = new NodeExUI(this);

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
                if (!settings.auto_warp.V)
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
            if (settings.pause_on_end.V)
                TimeWarpTools.SetIsPaused(true);
            return;
        }

        var next = this.mode + 1;
        setMode(next);
    }

    public void UpdateUI()
    {
        ui.status_bar.Reset();

        var st = ui.status_bar;
        if (!isRunning)
        {
            if (next_maneuver_node == null)
            {
                st.Status("No Maneuver node.", StatusLine.Level.Normal);
                return;
            }

            if (!valid_maneuver)
            {
                st.Status("No Maneuver node.", StatusLine.Level.Warning);
                st.Console("Actually a KSP2 bug when loading scenaries. Please open map to fix it");               
                return;
            }

            if (!isRunning && !canStart())
            {
                st.Status("No valid Maneuver node found", StatusLine.Level.Warning);
                return;
            }
        } 
        else
            current_executor.updateUI(page.panel, st);
    }

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
            switch (settings.start_mode.V)
            {
                case NodeExSettings.StartMode.precise:
                    UT = execute_node.Time;
                    break;
                case NodeExSettings.StartMode.half_duration:
                    UT = execute_node.Time - execute_node.BurnDuration / 2;
                    break;
                case NodeExSettings.StartMode.constant:
                    UT = execute_node.Time - settings.start_before.V;
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

        UpdateUI();
    }

  

    public override bool isRunning
    {
        get { return mode != Mode.Off; }
        set
        {
            if (value == base.isRunning)
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

            // send call backs
            base.isRunning = value; 
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

    internal string ApiStatus()
    {
        string status = "";
        
        if (next_maneuver_node == null)
        {
            status = "No Maneuver Node";
        }
        else
        {
            if (!valid_maneuver) status = "Invalid Maneuver Node";
            // else if (!NodeExecute.Instance.canStart()) status = "No Future Maneuver Node";
            else if (isRunning)
            {
                if (mode == Mode.Off) status = "Off";
                else if (mode == Mode.Turn)
                {
                    status = $"Turning: {current_executor.status_line}";
                    // report angle deviation?
                }
                else if (mode == Mode.Warp)
                {
                    status = $"Warping: {current_executor.status_line}";
                    // report time to node?
                }
                else if (mode == Mode.Burn)
                {
                    if (Game.UniverseModel.UniverseTime < next_maneuver_node.Time)
                    {
                        status = $"Waiting to Burn: {current_executor.status_line}";
                    }
                    else
                    {
                        status = $"Burning: {current_executor.status_line}";
                        // report burn remaining (delta-v?)
                    }
                }
            }
            else status = "Done";
            // else status = "Unknown";
        }
        return status;
    }
}
