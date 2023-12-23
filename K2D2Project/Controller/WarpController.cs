using K2D2.KSPService;
using KTools;
using KTools.UI;

namespace K2D2.Controller;


public class WarpControllerSettings
{
    public int delta_time
    {
        get => KBaseSettings.sfile.GetInt("drone.delta_time", -60);
        set
        {
            KBaseSettings.sfile.SetInt("drone.delta_time", value);
        }
    }
}




// could be a BaseController?

public class WarpController : ComplexController
{
    public static WarpController Instance { get; set; }

    KSPVessel current_vessel;


    WarpControllerSettings settings = new WarpControllerSettings();

    public WarpController()
    {
        // logger.LogMessage("LandingController !");
        current_vessel = K2D2_Plugin.Instance.current_vessel;

        Instance = this;
        debug_mode_only = false;
        name = "Warp";
        warp = new WarpTo();
    }

    public override void onReset()
    {
        isRunning = false;
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
                TimeWarpTools.SetRateIndex(0, false);
                _active = false;
            }
            else
            {
                // reset controller to desactivate other controllers.
                K2D2_Plugin.ResetControllers();
                _active = true;
            }
        }
    }

    WarpTo warp = new WarpTo();

    public override void Update()
    {
        if (!isRunning) return;

        warp.Update();

        if (warp.finished)
            isRunning = false;
    }

    public override void onGUI()
    {
        UI_Tools.Title("Warp to SOI");

        // if (K2D2_Plugin.Instance.settings_visible)
        // {
        //     K2D2Settings.onGUI();

            

        //     if (UI_Tools.BigButton("close"))
        //         K2D2_Plugin.Instance.settings_visible = false;

        //     return;
        // }

        if (isRunning)
        {
            isRunning = UI_Tools.BigToggleButton(isRunning, "Warping", "Stop");
            warp.onGUI();
            return;
        }

        settings.delta_time = UI_Tools.IntSlider("Delta time", (int)settings.delta_time, -120, 120, "s");

        var orbit = current_vessel.VesselComponent.Orbit;
        if (orbit.PatchEndTransition == KSP.Sim.PatchTransitionType.Encounter ||
            orbit.PatchEndTransition == KSP.Sim.PatchTransitionType.Escape)
        {
            var end_duration = orbit.EndUT - GeneralTools.Game.UniverseModel.UniverseTime;
            UI_Tools.Console($"Next transition is : {orbit.PatchEndTransition}");
            UI_Tools.Console($"SOI Change in : {StrTool.DurationToString(end_duration)}");

            string txt_delta = settings.delta_time > 0 ? $"(+{settings.delta_time}s)" : $"({settings.delta_time}s)";

            bool go = UI_Tools.BigToggleButton(isRunning, "Warp to SOI Change " + txt_delta, "Stop");
            if (isRunning != go)
            {
                if (go)
                {
                    warp.Start_Ut(orbit.EndUT + settings.delta_time);
                    isRunning = true;
                }
            }
        }
    }
}
