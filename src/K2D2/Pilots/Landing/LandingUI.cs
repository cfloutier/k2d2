

using BepInEx.Logging;
using K2D2.KSPService;
using K2D2.UI;
using K2UI;
using K2UI.Tabs;
using KSP.Game;
using KSP.Messages;
using KSP.Sim;
using KSP.Sim.Maneuver;
using UnityEngine.UIElements;
using KTools;



namespace K2D2.Landing;

class LandingUI : K2Page
{
    LandingPilot pilot;
    public LandingUI(LandingPilot pilot)
    {
        this.pilot = pilot;
        code = "landing";
    }

    public K2UI.Console landing_infos;
    public FullStatus status_bar;

    public ToggleButton run_button;

    public Button touch_down;


    public override bool onInit()
    {
        LandingSettings settings = pilot.settings;
        landing_infos = panel.Q<K2UI.Console>("landing_infos");
        settings.landing_context.listen(v => landing_infos.Show(v));

        run_button = panel.Q<ToggleButton>("run");
        touch_down = panel.Q<Button>("touch_down");
        status_bar = new FullStatus(panel);

        pilot.is_running_event += is_running => run_button.value = is_running;
        run_button.listeners += v =>
        {
            pilot.isRunning = v;
            run_button.label = v ? "Stop" : "Brake";
        };

        touch_down.listenClick(() =>
        {
            pilot.isRunning = true;
            pilot.setMode(LandingPilot.Mode.TouchDown);
        });

        pilot.settings.setupUI(pilot, settings_page);
        addSettingsResetButton("land");

        return true;
    }

    public void updateContext()
    {
        landing_infos.Set("<b>Landing Context</b>");
        landing_infos.Add($"Current Fall Speed : {pilot.current_falling_speed:n2} m/s");

        LandingSettings settings = pilot.settings;
        if (pilot.collision_detected)
        {
            landing_infos.Add("<b>Collision detected !</b>");
            landing_infos.Add($" Collision in {StrTool.DurationToString(pilot.adjusted_collision_UT - GeneralTools.Game.UniverseModel.UniverseTime)}");
            landing_infos.Add($" speed collision {pilot.speed_collision:n2} m/s");
            landing_infos.Add($" start_burn in <b>{StrTool.DurationToString(pilot.startBurn_UT - GeneralTools.Game.UniverseModel.UniverseTime)}</b>");
            landing_infos.Add($" burn_duration {pilot.burn_duration:n2} s");

            // landing_infos.Add( $"\ncurrent_V_speed : {StrTool.DistanceToString(pilot.altitude)}");
            // landing_infos.Add( $"\nAltitude : {StrTool.DistanceToString(pilot.altitude)}");
        }
        else
        {
            landing_infos.Add("<b>No Collision detected</b>");
        }
    }

    public override bool onUpdateUI()
    {
        if (!base.onUpdateUI())
            return false;

        if (pilot.settings.landing_context.V)
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
                    status_bar.Warning("Rotating Warp");
                    break;
                case LandingPilot.Mode.Waiting:
                    status_bar.Status($"Waiting : {StrTool.DurationToString(pilot.startBurn_UT - GeneralTools.Game.UniverseModel.UniverseTime)}");
                    break;
                case LandingPilot.Mode.Brake:
                    status_bar.Warning($"Brake !");
                    break;
                case LandingPilot.Mode.TouchDown:
                    status_bar.Warning($"Touch Down...");
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