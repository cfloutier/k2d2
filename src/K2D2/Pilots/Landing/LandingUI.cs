
using BepInEx.Logging;
using K2D2.KSPService;
using K2UI.Tabs;
using K2UI;
using KSP.Messages;
using KSP.Sim;
using KSP.Sim.Maneuver;
using KTools;
using UnityEngine.UIElements;
using K2D2.UI;
using KSP.Game;
namespace K2D2.Landing;

class LandingUI : K2Page
{
    LandingPilot pilot;
    public LandingUI(LandingPilot pilot)
    {
        this.pilot = pilot;
        code = "landing";
    }

    public K2UI.Console context_infos;
    public FullStatus status_bar;

    public ToggleButton run_button;
    
    public Button touch_down;

    public override bool onInit()
    {
        context_infos = panel.Q<K2UI.Console>("context_infos");
        run_button = panel.Q<ToggleButton>("run");
        touch_down = panel.Q<Button>("touch_down");
        status_bar = new FullStatus(panel);

        run_button.listeners +=  v => 
        {
            pilot.isRunning = v;
            run_button.label = v ? "Stop" : "Brake";
        }; 

        touch_down.RegisterCallback<ClickEvent>(evt => {
            pilot.isRunning = true;
            pilot.setMode(LandingPilot.Mode.TouchDown);
        });

        return true;
    }

    public void updateContext()
    {
        string txt = $"Current Fall Speed : {pilot.current_falling_speed:n2} m/s";

        LandingSettings settings = pilot.settings;
        if (pilot.collision_detected)
        {
            txt += "\n<b>   Collision detected !<b>";

            if (settings.verbose_infos.V)
            {
                txt += $"\n Collision in {StrTool.DurationToString(pilot.adjusted_collision_UT - GeneralTools.Game.UniverseModel.UniverseTime)}";
                txt += $"\n speed collision {pilot.speed_collision:n2} m/s";
                txt += $"\n start_burn in <b>{StrTool.DurationToString(pilot.startBurn_UT - GeneralTools.Game.UniverseModel.UniverseTime)}</b>";
                txt += $"\n burn_duration {pilot.burn_duration:n2} s";
            }
        }
        else
        {
            txt += "\n<b>   No Collision detected<b>";
        }

        txt += $"current_V_speed : { StrTool.DistanceToString(pilot.altitude)}";

        if (settings.verbose_infos.V)
             txt +=$"Altitude : { StrTool.DistanceToString(pilot.altitude)}";
        
        context_infos.Set(txt);
    }

    public override bool onUpdateUI()
    {
        if (!base.onUpdateUI())
            return false;

        updateContext();
        status_bar.Reset();

        if (!pilot.collision_detected)
            return true;

        var state = GeneralTools.Game.GlobalGameState.GetState();
        if (state != GameState.FlightView)
        {
            status_bar.Console("Landing is only available in Fligh View");
            return true;
        }

        touch_down.Show(pilot.mode != LandingPilot.Mode.TouchDown);
        if (pilot.isRunning)
        {
            switch (pilot.mode)
            {
                default:
                case LandingPilot.Mode.Off: break;
                case LandingPilot.Mode.Pause:
                    status_bar.Status("Pause");
                    break;
                case LandingPilot.Mode.QuickWarp:
                    status_bar.Status("Quick Warp");
                    break;
                case LandingPilot.Mode.RotationWarp:
                    status_bar.Status("Rotating Warp", StatusLine.Level.Warning);
                    break;
                case LandingPilot.Mode.Waiting:
                    status_bar.Status($"Waiting : {StrTool.DurationToString(pilot.startBurn_UT - GeneralTools.Game.UniverseModel.UniverseTime)}");
                    break;
                case LandingPilot.Mode.Brake:
                    status_bar.Status($"Brake !", StatusLine.Level.Warning);
                    break;
                case LandingPilot.Mode.TouchDown:
                    status_bar.Status($"Touch Down...", StatusLine.Level.Warning);
                    break;
            }

            if (pilot.current_executor != null && !string.IsNullOrEmpty(pilot.current_executor.status_line))
                status_bar.Console(pilot.current_executor.status_line);
        }

        status_bar.Console($"Altitude : {StrTool.DistanceToString(pilot.altitude)}");

        //    UI_Tools.Console("SurfaceVelocity" + StrTool.VectorToString(SurfaceVelocity.vector));

        if (isRunning && pilot.burn_dV.burned_dV > 0)
            status_bar.Console($"Burned : {pilot.burn_dV.burned_dV:n1} m/s");
    
         return true;
    }

}