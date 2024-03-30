
using K2D2.KSPService;
using KTools;
using UnityEngine;
using BepInEx.Logging;
using UnityEngine.UIElements;
using K2D2.Controller;
using K2D2.Node;

namespace K2D2.Lift;

public class LiftPilot : Pilot
{
    public enum LiftStatus
    {
        Off,
        Ascent,
        Coasting,
        Adjust,
        Circularize
    }

    public ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.Lift");

    public static LiftPilot Instance { get; set; }

    internal  LiftSettings settings;

    internal  LiftAscentPath ascent_path = null;

    KSPVessel current_vessel;

    Vector3d direction = Vector3d.zero;

    // sub pilots
    internal Ascent ascent = null;
    Adjust adjust = null;
    Coasting coasting = null;   
    FinalCircularize final_circularize;

    public ExecuteController current_subpilot = null;

    public LiftPilot()
    {
        settings = new LiftSettings();
        ascent_path = new LiftAscentPath(settings);
        _panel = new LiftUI(this);

        current_vessel = K2D2Plugin.Instance.current_vessel;
       
        ascent = new Ascent(settings, ascent_path);
        adjust = new Adjust(settings, ascent);
        coasting = new Coasting(this, settings);
        final_circularize = new FinalCircularize(this, settings);
        
        Instance = this;

        debug_mode_only = false;
        name = "Lift";
        K2D2PilotsMgr.Instance.RegisterPilot("Lift", this);
    }

    public override void onReset()
    {
        isRunning = false;
    }

    LiftStatus _status = LiftStatus.Off;
    internal LiftStatus status
    {
        get { return _status; }
        set
        {
            if (_status == value)
                return;
            _status = value;
            switch (value)
            {
                case LiftStatus.Off:
                    {
                        // stop
                        if (current_vessel != null)
                            current_vessel.SetThrottle(0);

                        current_subpilot = null;
                    }
                    break;
                case LiftStatus.Ascent:
                    current_subpilot = ascent;
                    break;
                case LiftStatus.Coasting:
                    current_subpilot = coasting;
                    break;
                case LiftStatus.Adjust:
                    current_subpilot = adjust;
                    break;
                case LiftStatus.Circularize:
                    current_subpilot = final_circularize;
                    break;
            }

            if (current_subpilot != null)
                current_subpilot.Start();
        }
    }

    public override bool isRunning
    {
        get { return _status != LiftStatus.Off; }
        set
        {
            if (value == isRunning)
                return;

            if (!value)
            {
                status = LiftStatus.Off;
            }
            else
            {
                // reset controller to desactivate other controllers.
                K2D2Plugin.ResetControllers();
                OnStartController();
            }

            // send call backs
            base.isRunning = value; 
        }
    }

    void OnStartController()
    {
        status = LiftStatus.Ascent;
    }

    internal bool result_ok = false;
    internal string end_status;

    public void EndLiftPilot(bool result_ok, string end_status)
    {
        this.result_ok = result_ok;
        this.end_status = end_status;
        status = LiftStatus.Off;
    }

    public WarpTo warp_to = new WarpTo();


    public void NextMode()
    {
        status = status.Next();
    }

    public override void Update()
    {
        // if (!isRunning && !ui_visible) return;
        if (current_vessel == null) return;

        if (isRunning || ui_visible)
            ascent.computeValues(status == LiftStatus.Ascent);

        if (!isRunning)
            return;

        if (current_subpilot != null)
        {
            current_subpilot.Update();
            if (current_subpilot.finished)
                NextMode();
        }
    }
}
