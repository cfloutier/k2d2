using K2D2.Controller.Lift.Pilots;
using K2D2.KSPService;
using KSP.Messages.PropertyWatchers;
using KSP.Sim;
using KSP.Sim.impl;
using KTools;
using KTools.UI;
using Steamworks;
using UnityEngine;
using BepInEx.Logging;

namespace K2D2.Controller;

public class LiftController : ComplexController
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

    public static LiftController Instance { get; set; }

    LiftSettings lift_settings = null;
    LiftAscentPath ascent_path = null;

    KSPVessel current_vessel;


    Vector3d direction = Vector3d.zero;
    bool show_profile = false;

    // sub pilots
    Ascent ascent = null;
    Adjust adjust = null;
    Coasting coasting = null;   
    FinalCircularize final_circularize;

    ExecuteController current_subpilot = null;

    public LiftController()
    {
        current_vessel = K2D2_Plugin.Instance.current_vessel;

        lift_settings = new LiftSettings();
        ascent_path = new LiftAscentPath(lift_settings);

        ascent = new Ascent(lift_settings, ascent_path);
        adjust = new Adjust(lift_settings, ascent);
        coasting = new Coasting(this, lift_settings);
        final_circularize = new FinalCircularize(this, lift_settings);
        
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
    LiftStatus status
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
                K2D2_Plugin.ResetControllers();
                OnStartController();
            }
        }
    }

    void OnStartController()
    {
        status = LiftStatus.Ascent;
    }

    bool result_ok = false;
    string end_status;

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


    public override void onGUI()
    {
        
        if (K2D2_Plugin.Instance.settings_visible)
        {
            K2D2Settings.onGUI();
            lift_settings.onGUI();
            K2D2Settings.CloseUI();
            return;
        }

        //UI_Tools.Title("Lift Pilot");


        if (show_profile)
        {
            lift_settings.destination_Ap_km = UI_Fields.IntFieldLine("lift.destination_Ap_km", "Ap Altitude", lift_settings.destination_Ap_km, 0, Int32.MaxValue, "km");

            GUILayout.BeginHorizontal();
            // GUILayout.Label("5° Alt");
            GUILayout.Label($"5° Alt. : {lift_settings.end_rotate_altitude_km:n0} km", KBaseStyle.slider_text);
            lift_settings.end_rotate_ratio = UI_Tools.FloatSlider(lift_settings.end_rotate_ratio, 0, 1);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"45° Alt. : {lift_settings.mid_rotate_altitude_km:n0} km", KBaseStyle.slider_text);
            lift_settings.mid_rotate_ratio = UI_Tools.FloatSlider(lift_settings.mid_rotate_ratio, 0, 1);
            GUILayout.EndHorizontal();

            lift_settings.start_altitude_km = UI_Fields.IntFieldLine("lift.start_altitude_km", "90° Altitude", lift_settings.start_altitude_km, 0, Int32.MaxValue, "km");
            ascent_path.drawProfile(ascent.current_altitude_km);

            lift_settings.heading = UI_Tools.HeadingControl("lift.heading", lift_settings.heading);

            GUILayout.BeginHorizontal();
            if (UI_Tools.miniButton("Hide profile"))
            {
                show_profile = false;
            }

            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.BeginHorizontal();
            if (UI_Tools.miniButton("Show Profile"))
            {
                show_profile = true;
            }

            GUILayout.EndHorizontal();
        }

        lift_settings.max_throttle = UI_Tools.FloatSliderTxt("Max Throttle", lift_settings.max_throttle, 0, 1);

        isRunning = UI_Tools.BigToggleButton(isRunning, "Start", "Stop");

        if (isRunning)
        {
            UI_Tools.Warning($"Status : {status}");


            if (current_subpilot != null)
                current_subpilot.onGUI();
        }
        else
        {
            if (!string.IsNullOrEmpty(end_status))
            {
                if (result_ok)
                {
                    UI_Tools.OK("Final status : " + end_status);
                }
                else
                {
                    UI_Tools.Warning("Final status : " + end_status);
                }
            }
        }
    }
}
