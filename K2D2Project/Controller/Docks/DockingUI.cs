
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

    public UI_Mode ui_mode = UI_Mode.Main;


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
            buildControlList();
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
            if (UI_Tools.Button(part.name))
            {
                pilot.current_vessel.VesselComponent.SetControlOwner(part.component);
                ui_mode = UI_Mode.Main;
            }
        }

        endList(control_parts.Count);

        GUILayout.Space(10);

        if (UI_Tools.SmallButton("Cancel"))
        {
            ui_mode = UI_Mode.Main;
        }
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
                pilot.listDocks();
                ui_mode = UI_Mode.Select_Dock;
            }
        }

        GUILayout.EndHorizontal();
    }

    void mainGUI()
    {
        UI_Tools.Title("Docking Tools");

        controlLineUI();
        targetLineUI();

        settings.show_gizmos = UI_Tools.BigToggleButton(settings.show_gizmos, "Show A.R", "Hide A.R");

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

        // shapes_drawer.StyleGUI();
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
            if (part.component == pilot.target_part)
            {
                GUILayout.Toggle(true, part.name, style );
            }
            else
            {
                if (GUILayout.Toggle(false, part.name, style ))
                {
                    pilot.current_vessel.VesselComponent.SetTargetByID(part.component.GlobalId);
                }
            }
        }

        // drawing options
        // shapes_drawer.draw_ui();

        endList(pilot.docks.Count);

        GUILayout.Space(10);

        if (UI_Tools.Button("Ok"))
        {
            ui_mode = UI_Mode.Main;
        }
    }

    public void onGUI()
    {
        if (K2D2_Plugin.Instance.settings_visible)
        {
            // default Settings UI
            K2D2Settings.onGUI();
            settings.StyleGUI();
            return;
        }

        if (ui_mode == UI_Mode.Main)
        {
            mainGUI();
        }
        if (ui_mode == UI_Mode.Select_Control)
        {
            selectControlGUI();
        }
        else if (ui_mode == UI_Mode.Select_Target)
        {
            selectTargetGUI();
        }
        else if (ui_mode == UI_Mode.Select_Dock)
        {
            selectDockGUI();
        }
    }

}