using K2D2.KSPService;
using KTools;
using KTools.UI;

namespace K2D2.Controller;

public class WarpController : ComplexControler
{
    public static WarpController Instance { get; set; }

    KSPVessel current_vessel;

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
        if (K2D2_Plugin.Instance.settings_visible)
        {
            K2D2Settings.onGUI();
            UI_Tools.Title("No Warp Settings");
            if (UI_Tools.BigButton("close"))
                K2D2_Plugin.Instance.settings_visible = false;
            return;
        }
        var orbit = current_vessel.VesselComponent.Orbit;

        if (orbit.PatchEndTransition == KSP.Sim.PatchTransitionType.Encounter ||
            orbit.PatchEndTransition == KSP.Sim.PatchTransitionType.Escape)
        {
            UI_Tools.Console($"SOI Change in : {StrTool.DurationToString(orbit.timeToTransition1)}");

            bool go = UI_Tools.BigToggleButton(isRunning, "Warp to SOI Change", "Stop");
            if (isRunning != go)
            {
                if (go)
                {
                    warp.Start_Ut(orbit.timeToTransition1 + GeneralTools.Game.UniverseModel.UniversalTime + 120);
                    isRunning = true;
                }
                else 
                {
                    isRunning = false;
                }
            }

        }
        else
        {
            // TODO : DBG why this does not set to false ??????
            isRunning = false;
            UI_Tools.Console("No SOI change");
        }

        //  if (UI_Tools.BigButton("close"))


    }


}
