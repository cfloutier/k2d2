
using K2D2.KSPService;

using BepInEx.Logging;
using UnityEngine;

using KSP.Sim.impl;
using KSP.Game;
using static K2D2.Controller.Docks.DockTools;
using K2UI.Tabs;
using UnityEngine.UIElements;
using K2UI;
using K2D2.UI;
using KSP.UI.Binding;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using static K2D2.Controller.Docks.DockTools.NamedComponent;

namespace K2D2.Controller.Docks;

// TODO : ajouter le dessin des docks ici
class DockingUI : K2Page
{
    public DockingUI(DockingPilot pilot)
    {
        this.pilot = pilot;
        code = "dock";
    }

    DockingPilot pilot;

    public PartComponent selected_control = null;

    private VisualElement select_group, button_bars;

    public K2UI.Console context;
    private ToggleButton run_button;
    private Button main_brake, rcs_final_approach, cheat;
    private K2Toggle align_dock;

    public FullStatus st;

    public override bool onInit()
    {
        select_group = panel.Q<VisualElement>("select_group");
        select_group.Show(false);

        // TODO Selection target
        // select_target_ui = SelectTargetUI(this.pilot, panel)
        // select_target_ui.onInitUI();

        context = panel.Q<K2UI.Console>("context");
    
        button_bars = panel.Q<VisualElement>("button_bars");
        run_button = button_bars.Q<ToggleButton>("run");
        main_brake = button_bars.Q<Button>("main_brake");
        align_dock = button_bars.Q<K2Toggle>("align_dock");
        align_dock.RegisterCallback<ChangeEvent<bool>>(is_on =>
        {
            if (is_on.newValue)
                pilot.turnTo.StartDockAlign();
            else
                pilot.turnTo.mode = Pilots.DockingTurnTo.Mode.Off;
        });

        cheat = button_bars.Q<Button>("cheat");
        cheat.listenClick(() => onCheat());

        rcs_final_approach = button_bars.Q<Button>("rcs_final_approach");

        st = new FullStatus(panel);

        pilot.listenIsRunning(is_running =>
        {
            // Hide selection and run buttons
            // select_group.Show(!is_running);
            main_brake.Show(!is_running);
            rcs_final_approach.Show(!is_running);

            // update the main button states
            run_button.Show(is_running);
            run_button.Value = is_running;
            run_button.label = is_running ? "Stop" : "Start";
        });

        run_button.listen((v) => {
            if (!v)
                pilot.isRunning = false;
        });

        main_brake.listenClick(() =>
        {
            pilot.Mode = DockingPilot.PilotMode.MainThrustKillSpeed;
        });

        rcs_final_approach.listenClick(() =>
        {
            pilot.Mode = DockingPilot.PilotMode.RCSFinalApproach;
        });

        pilot.final_approach_pilot.onInitUI(panel, settings_page);

        addSettingsResetButton("lift");

        return true;
    }

    void updateContext()
    {
        context.Set("<b>Context</b>");
        context.Add("Control : " + ListPart.formatComponent( pilot.current_vessel.VesselComponent, pilot.control_component ));
        context.Add("Target : " + ListPart.formatComponent( pilot.target_vessel, pilot.target_part ));
    }

    public override bool onUpdateUI()
    {  
        if (!base.onUpdateUI())
            return false;

        st.Reset();
        // update the align_dock
        align_dock.value = pilot.turnTo.isDockAlign;

        updateContext();
        pilot.final_approach_pilot.Hide();
        if (pilot.sub_controler != null)
            pilot.sub_controler.updateUI(panel, st);

        return true;
    }

    void onCheat()
    {
        if (pilot.target_vessel == null) return;

        if (pilot.target_vessel.Guid == pilot.current_vessel.VesselComponent.Guid)
            return;

        pilot.Game.SpaceSimulation.Lua.TeleportToRendezvous(
            pilot.current_vessel.VesselComponent.Guid,
            pilot.target_vessel.Guid,
            30,
            0, 0, 0, 0, 0);
    }

    // public bool drawShapes(DockShape shapes_drawer)
    // {
    //     // logger.LogInfo("drawShapes");
    //     if (ui_mode == UI_Mode.Select_Control)
    //     {
    //         foreach (NamedComponent part in control_parts.Parts)
    //         {
    //             Color color = settings.unselected_color;
    //             if (part.component == selected_component)
    //             {
    //                 color = settings.vessel_color;
    //             }

    //             shapes_drawer.DrawComponent(part.component, pilot.current_vessel.VesselComponent, color, true, true);
    //         }

    //         return true;
    //     }
    //     if (ui_mode == UI_Mode.Select_Dock)
    //     {
    //         foreach (NamedComponent part in pilot.docks.Parts)
    //         {
    //             Color color = settings.unselected_color;
    //             if (part.component == selected_component)
    //             {
    //                 color = settings.target_color;
    //             }

    //             shapes_drawer.DrawComponent(part.component, pilot.current_vessel.VesselComponent, color, true, true);
    //         }

    //         return true;
    //     }


    //     return false;
    // }

}