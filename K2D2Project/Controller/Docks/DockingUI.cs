
using K2D2.KSPService;
using KTools.UI;
using BepInEx.Logging;
using UnityEngine;

using KSP.Sim.impl;
using KSP.Game;
using KSP.Sim;
using KSP;
using KSP.Sim.Definitions;

using K2D2.Controller.Docks;
using static VehiclePhysics.VPReplay;

using JetBrains.Annotations;
using KSP.Iteration.UI.Binding;
using LibNoise.Modifiers;

using System.Diagnostics.Tracing;
using static K2D2.Controller.Docks.DockTools;
using KTools.Shapes;
using static KSP.Api.UIDataPropertyStrings.View.Vessel.Stages;

namespace K2D2.Controller.Docks;

// TODO : ajouter le dessin des docks ici
class DockingUI
{
    public DockingUI(DockingAssist pilot)
    {
        this.pilot = pilot;
        settings = pilot.settings;
    }

    DockingAssist pilot;
    public DocksSettings settings;

    public enum UI_Mode
    {
        Main,
        Select_Control,
        Select_Target,
        Select_Dock,
    }

    public UI_Mode _ui_mode = UI_Mode.Main;
    public UI_Mode ui_mode
    {
        get { return _ui_mode; }
        set
        {
            switch (value)
            {
                case UI_Mode.Select_Control:
                    buildControlList();
                    break;
                case UI_Mode.Select_Dock:
                    pilot.listDocks();
                    selected_component = pilot.target_part;
                    break;
                case UI_Mode.Select_Target:
                    break;

            }

            _ui_mode = value;
        }
    }

    public PartComponent selected_component = null;


    Vector2 scroll_pos = Vector2.zero;

    const int max_nb_list = 15;
    const int scroll_height = 300;
    void startList(int num_el)
    {
        if (num_el > max_nb_list)
            scroll_pos = GUILayout.BeginScrollView(scroll_pos, GUILayout.Height(scroll_height));
        else
            GUILayout.BeginVertical();
    }

    void endList(int num_el)
    {
        if (num_el > max_nb_list)
            GUILayout.EndScrollView();
        else
            GUILayout.EndVertical();
    }


    public ListPart control_parts = new ListPart();
    public void buildControlList()
    {
        control_parts.Clear();
        var main_component = pilot.control_component;
        if (main_component == null)
            return;

        control_parts = FindParts(pilot.current_vessel.VesselComponent, true, true);
        selected_component = pilot.control_component;
    }


    /// <summary>
    /// UI line for selecting control dock
    /// </summary>
    void controlLineUI()
    {
        if (pilot.control_component == null)
            return;

        GUILayout.BeginHorizontal();

        UI_Tools.Label($"Control from : ");
        if (UI_Tools.Button(pilot.control_component.Name))
        {
            ui_mode = UI_Mode.Select_Control;
        }

        GUILayout.EndHorizontal();
        GUILayout.Space(5);
    }

    void selectControlGUI()
    {
        UI_Tools.Title("Select Control Part");
        if (control_parts.Count == 0)
        {
            GUILayout.Label("No control");
            return;
        }

        startList(control_parts.Count);

        foreach (NamedComponent part in control_parts.Parts)
        {
            //L.Log("*"+part.name);

            GUIStyle style = KBaseStyle.small_button;
            if (part.component == selected_component)
            {
                GUILayout.Toggle(true, part.name, style);
            }
            else
            {
                if (GUILayout.Toggle(false, part.name, style))
                {
                    selected_component = part.component;
                }
            }
        }

        endList(control_parts.Count);

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();

        if (UI_Tools.SmallButton("Cancel"))
        {
            ui_mode = UI_Mode.Main;
        }
        if (UI_Tools.SmallButton("OK"))
        {
            if (selected_component != null)
                pilot.current_vessel.VesselComponent.SetControlOwner(selected_component);

            ui_mode = UI_Mode.Main;
        }

        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// UI Line for selecting target
    /// </summary>
    void targetLineUI()
    {

        GUILayout.BeginHorizontal();
        UI_Tools.Label("Target : ");

        string target_name = "None";
        if (pilot.target_vessel != null)
        {
            target_name = pilot.target_vessel.Name;
        }

        if (UI_Tools.SmallButton(target_name))
        {
            ui_mode = UI_Mode.Select_Target;
        }

        if (pilot.target_vessel != null)
        {
            string bt_label = "Dock";

            if (pilot.target_part != null)
            {
                bt_label = "Dock #" + pilot.target_dock_num + " " + pilot.target_part.PartData.sizeCategory;
            }

            if (UI_Tools.SmallButton(bt_label))
            {
                ui_mode = UI_Mode.Select_Dock;
            }
        }

        GUILayout.EndHorizontal();
    }

    void selectTargetGUI()
    {
        UI_Tools.Title("Select Target");

        var body = pilot.current_vessel.currentBody();

        var allVessels = GameManager.Instance.Game.SpaceSimulation.UniverseModel.GetAllVessels();
        allVessels = GameManager.Instance.Game.SpaceSimulation.UniverseModel.GetAllVessels();
        allVessels.Remove(pilot.current_vessel.VesselComponent);
        allVessels.RemoveAll(v => v.IsDebris());
        allVessels.RemoveAll(v => v.mainBody != body);

        if (allVessels.Count < 1)
        {
            GUILayout.Label("No other vessels orbiting the planet");
        }

        startList(allVessels.Count);

        foreach (var vessel in allVessels)
        {
            if (UI_Tools.Button(vessel.Name))
            {
                pilot.current_vessel.VesselComponent.SetTargetByID(vessel.GlobalId);
                pilot.current_vessel.VesselComponent.TargetObject = vessel.SimulationObject;
                ui_mode = UI_Mode.Main;
            }
        }

        endList(allVessels.Count);

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();

        if (UI_Tools.SmallButton("None"))
        {
            ui_mode = UI_Mode.Main;
            pilot.current_vessel.VesselComponent.ClearTarget();
        }

        if (UI_Tools.SmallButton("Cancel"))
        {
            ui_mode = UI_Mode.Main;
        }

        GUILayout.EndHorizontal();
    }

    void selectDockGUI()
    {
        UI_Tools.Title("Select Dock");
        if (pilot.target_vessel == null)
        {
            ui_mode = UI_Mode.Main;
            return;
        }
        UI_Tools.Console("vessel : " + pilot.target_vessel.Name);

        int num = 0;

        startList(pilot.docks.Count);

        foreach ( NamedComponent part in pilot.docks.Parts)
        {
            num += 1;
            GUIStyle style = KBaseStyle.small_button;
            if (part.component == selected_component)
            {
                GUILayout.Toggle(true, part.name, style );
            }
            else
            {
                if (GUILayout.Toggle(false, part.name, style ))
                {
                    selected_component = part.component;
                }
            }
        }

        // drawing options
        // shapes_drawer.draw_ui();

        endList(pilot.docks.Count);

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();

        if (UI_Tools.Button("Cancel"))
        {
            ui_mode = UI_Mode.Main;
        }

        if (UI_Tools.Button("Ok"))
        {
            if (selected_component != null)
                pilot.current_vessel.VesselComponent.SetTargetByID(selected_component.GlobalId);

            ui_mode = UI_Mode.Main;
        }

        GUILayout.EndHorizontal();
    }

    void cheatUI()
    {
        if (pilot.target_vessel != null)
        {
            if (UI_Tools.Button("Go go lazy"))
            {
                if (pilot.target_vessel.Guid == pilot.current_vessel.VesselComponent.Guid)
                    return;

                pilot.Game.SpaceSimulation.Lua.TeleportToRendezvous(
                    pilot.current_vessel.VesselComponent.Guid,
                    pilot.target_vessel.Guid,
                    30,
                    0, 0, 0, 0, 0);
            }
        }
    }


    public void MainOptionsLine()
    {
        GUILayout.BeginHorizontal();
        settings.show_gizmos = UI_Tools.SmallToggleButton(settings.show_gizmos, "Show A.R", "Hide A.R");

        if (pilot.target_part != null)
        {
            bool is_active = pilot.turnTo.mode == Pilots.DockingTurnTo.Mode.TargetDock;

            bool is_on = UI_Tools.SmallToggleButton(is_active, "Align Dock", "Stop Align Dock");
            if (is_active != is_on)
            {
                if (is_on)
                {
                    pilot.turnTo.StartDockAlign();
                }
                else
                {
                    pilot.turnTo.mode = Pilots.DockingTurnTo.Mode.Off;
                }
            }
        }
        GUILayout.EndHorizontal();
    }

    void mainGUI()
    {
        UI_Tools.Title("Docking Tools");

        if (pilot.isRunning)
        {
            MainOptionsLine();
            if (!UI_Tools.BigToggleButton(true, "Start", "Stop"))
            {
                pilot.isRunning = false;
            }
            return;
        }

        controlLineUI();
        targetLineUI();
        MainOptionsLine();

        if (pilot.target_part != null)
        {
            if (UI_Tools.BigToggleButton(false, "Main Thrust Brake", "-"))
            {
                pilot.Mode = DockingAssist.PilotMode.MainThrustKillSpeed;
            }

            if (UI_Tools.BigToggleButton(false, "RCS Final Approach", "-"))
            {
                pilot.Mode = DockingAssist.PilotMode.RCSFinalApproach;
            }
        }

     //   cheatUI();
        // shapes_drawer.StyleGUI();
    }

    public bool onGUI()
    {
        if (K2D2_Plugin.Instance.settings_visible)
        {
            // default Settings UI
            K2D2Settings.onGUI();
            settings.StyleGUI();
            return false;
        }

        switch(ui_mode)
        {
            case UI_Mode.Main: mainGUI(); 
                return true;
            case UI_Mode.Select_Control: selectControlGUI(); 
                return false;
            case UI_Mode.Select_Target: selectTargetGUI(); 
                return false;
            case UI_Mode.Select_Dock: selectDockGUI();
                return false;
        }

        return false;
    }

    public bool drawShapes(DockShape shapes_drawer)
    {
        // logger.LogInfo("drawShapes");
        if (ui_mode == UI_Mode.Select_Control)
        {
            foreach (NamedComponent part in control_parts.Parts)
            {
                Color color = settings.unselected_color;
                if (part.component == selected_component)
                {
                    color = settings.vessel_color;
                }

                shapes_drawer.DrawComponent(part.component, pilot.current_vessel.VesselComponent, color, true, true);
            }

            return true;
        }
        if (ui_mode == UI_Mode.Select_Dock)
        {
            foreach (NamedComponent part in pilot.docks.Parts)
            {
                Color color = settings.unselected_color;
                if (part.component == selected_component)
                {
                    color = settings.target_color;
                }

                shapes_drawer.DrawComponent(part.component, pilot.current_vessel.VesselComponent, color, true, true);
            }

            return true;
        }


        return false;
    }

}