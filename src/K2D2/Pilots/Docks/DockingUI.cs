
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

    // public enum UI_Mode
    // {
    //     Main,
    //     Select_Control,
    //     Select_Target,
    //     Select_Dock,
    // }

    public PartComponent selected_control = null;

    // Vector2 scroll_pos = Vector2.zero;

    // const int max_nb_list = 15;
    // const int scroll_height = 300;
    // void startList(int num_el)
    // {
    //     if (num_el > max_nb_list)
    //         scroll_pos = GUILayout.BeginScrollView(scroll_pos, GUILayout.Height(scroll_height));
    //     else
    //         GUILayout.BeginVertical();
    // }

    // void endList(int num_el)
    // {
    //     if (num_el > max_nb_list)
    //         GUILayout.EndScrollView();
    //     else
    //         GUILayout.EndVertical();
    // }
    private VisualElement select_group, button_bars;
    private ToggleButton run_button;
    private Button main_brake, rcs_final_approach, cheat;
    private K2Toggle align_dock;
    private DropdownField control_from_drop, target_drop, dock_drop;
    public FullStatus status_bar;

    public override bool onInit()
    {
        select_group = panel.Q<VisualElement>("select_group");

        control_from_drop = panel.Q<DropdownField>("control_from_drop");
        control_from_drop.listenClick(buildControlList);
        control_from_drop.RegisterCallback<ChangeEvent<int>>(evt =>
                {
                    Debug.LogWarning("Control Selected " + evt.newValue);
                    // todo
                    //selected_control = part.component;
                    //pilot.current_vessel.VesselComponent.SetControlOwner(selected_control);
                });


        target_drop = panel.Q<DropdownField>("target_drop");
        target_drop.listenClick(buildTargetList);
        target_drop.RegisterCallback<ChangeEvent<string>>(evt =>
            Debug.LogWarning("Target Selected " + evt.newValue)

        // todo
        // pilot.current_vessel.VesselComponent.SetTargetByID(vessel.GlobalId);
        // pilot.current_vessel.VesselComponent.ClearTarget();
        );

        dock_drop = panel.Q<DropdownField>("dock_drop");
        dock_drop.listenClick(buildDockList);
        dock_drop.RegisterCallback<ChangeEvent<string>>(evt =>
            Debug.LogWarning("Dock Selected " + evt.newValue)

        // todo
        // if (selected_control != null)
        //     pilot.current_vessel.VesselComponent.SetTargetByID(selected_control.GlobalId);
        );

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

        status_bar = new FullStatus(panel);

        pilot.listenIsRunning(is_running =>
        {
            // Hide selection and run buttons
            select_group.Show(!is_running);
            main_brake.Show(!is_running);
            rcs_final_approach.Show(!is_running);

            // update the main button states
            run_button.Show(is_running);
            run_button.value = is_running;
            run_button.label = is_running ? "Stop" : "Start";
        });

        main_brake.listenClick(() =>
        {
            pilot.Mode = DockingPilot.PilotMode.MainThrustKillSpeed;
        });

        rcs_final_approach.listenClick(() =>
        {
            pilot.Mode = DockingPilot.PilotMode.RCSFinalApproach;
        });

        return true;
    }

    public override bool onUpdateUI()
    {
        if (!base.onUpdateUI())
            return false;

        if (pilot.sub_controler != null)
            pilot.sub_controler.updateUI(panel, status_bar);

        return true;
    }

    public ListPart control_parts = new ListPart();
    public void buildControlList()
    {
        control_parts.Clear();
        var main_component = pilot.control_component;
        if (main_component == null)
            return;

        control_parts = FindParts(pilot.current_vessel.VesselComponent, true, true);
        selected_control = pilot.control_component;

        List<string> part_names = new();
        foreach (NamedComponent part in control_parts.Parts)
            part_names.Append(part.name);

        control_from_drop.choices = part_names;
    }

    public List<VesselComponent> target_vessels = new();
    public void buildTargetList()
    {
        target_vessels.Clear();

        var body = pilot.current_vessel.currentBody();

        var allVessels = GameManager.Instance.Game.SpaceSimulation.UniverseModel.GetAllVessels();
        allVessels = GameManager.Instance.Game.SpaceSimulation.UniverseModel.GetAllVessels();
        allVessels.Remove(pilot.current_vessel.VesselComponent);
        allVessels.RemoveAll(v => v.IsDebris());
        allVessels.RemoveAll(v => v.mainBody != body);

        target_vessels = allVessels;

        if (target_vessels.Count < 1)
        {
            GUILayout.Label("No other vessels orbiting the planet");
        }

        List<string> vessel_names = new();
        foreach (var vessel in target_vessels)
            vessel_names.Append(vessel.Name);

        control_from_drop.choices = vessel_names;
    }

    public void buildDockList()
    {
        pilot.listDocks();
        List<string> part_names = new();
        foreach (NamedComponent part in pilot.docks.Parts)
            part_names.Append(part.name);

        dock_drop.choices = part_names;
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