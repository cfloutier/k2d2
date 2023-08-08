
using K2D2.KSPService;
using KTools.UI;
using BepInEx.Logging;
using UnityEngine;

using KSP.Sim.impl;
using KSP.Game;
using KSP.Sim;
using KSP;

using K2D2.Controller.Docks;
using static VehiclePhysics.VPReplay;

namespace K2D2.Controller;

using JetBrains.Annotations;
using KSP.Iteration.UI.Binding;

using LibNoise.Modifiers;

using System.Diagnostics.Tracing;

public class DockingAssist : ComplexControler
{
    public static DockingAssist Instance { get; set; }
    public ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.DockingTool");

    KSPVessel current_vessel;

    DocksSettings settings = new DocksSettings();

    public DockingAssist()
    {
        Instance = this;
        debug_mode_only = false;
        name = "Dock";

        current_vessel = K2D2_Plugin.Instance.current_vessel;

        shapes_drawer = new DockShape(settings);
    }

    DockShape shapes_drawer;

    UI_Mode ui_mode = UI_Mode.Main;
    Vector2 scroll_pos = Vector2.one;

    void mainGUI()
    {
        UI_Tools.Title("Docking Tools");
        GUILayout.BeginHorizontal();

        UI_Tools.Label("Target : ");

        string target_name = "None";
        if (target_vessel != null)
        {
            target_name = target_vessel.Name;
        }

        if (UI_Tools.SmallButton(target_name))
        {
            ui_mode = UI_Mode.Select_Target;
        }
        if (target_vessel != null)
        {
            string bt_label = "Dock";

            if (target_part != null)
            {
                bt_label = "Dock #" + target_dock_num + " " + target_part.PartData.sizeCategory;
            }

            if (UI_Tools.SmallButton(bt_label))
            {
                ui_mode = UI_Mode.Select_Dock;
            }
        }

        GUILayout.EndHorizontal();



        settings.show_gizmos = UI_Tools.BigToggleButton(settings.show_gizmos, "Show A.R", "Hide A.R");

        if (target_vessel != null)
        {
            if (UI_Tools.Button("Go go lazy"))
            {
                if (target_vessel.Guid == current_vessel.VesselComponent.Guid)
                    return;

                Game.SpaceSimulation.Lua.TeleportToRendezvous(
                    current_vessel.VesselComponent.Guid,
                    target_vessel.Guid,
                    30,
                    0, 0, 0, 0, 0);
            }
        }

        // shapes_drawer.StyleGUI();
    }

    void selectTargetGUI()
    {
        UI_Tools.Title("Select Target");

        var body = current_vessel.currentBody();

        var allVessels = GameManager.Instance.Game.SpaceSimulation.UniverseModel.GetAllVessels();
        allVessels = GameManager.Instance.Game.SpaceSimulation.UniverseModel.GetAllVessels();
        allVessels.Remove(current_vessel.VesselComponent);
        allVessels.RemoveAll(v => v.IsDebris());
        allVessels.RemoveAll(v => v.mainBody != body);

        if (allVessels.Count < 1)
        {
            GUILayout.Label("No other vessels orbiting the planet");
        }

        if (allVessels.Count > 10)
            scroll_pos = GUILayout.BeginScrollView(scroll_pos);
        else
            GUILayout.BeginVertical();

        foreach (var vessel in allVessels)
        {
            if (UI_Tools.Button(vessel.Name))
            {
                current_vessel.VesselComponent.SetTargetByID(vessel.GlobalId);
                current_vessel.VesselComponent.TargetObject = vessel.SimulationObject;
                ui_mode = UI_Mode.Main;
            }
        }

        if (allVessels.Count > 10)
            GUILayout.EndScrollView();
        else
            GUILayout.EndVertical();

        if (UI_Tools.SmallButton("None"))
        {
            ui_mode = UI_Mode.Main;
            current_vessel.VesselComponent.ClearTarget();
        }

        if (UI_Tools.SmallButton("Cancel"))
        {
            ui_mode = UI_Mode.Main;
        }
    }

    void selectDockGUI()
    {
        UI_Tools.Title("Select Dock");
        if (target_vessel == null)
        {
            ui_mode = UI_Mode.Main;
            return;
        }
        UI_Tools.Console("vessel : " + target_vessel.Name);

        int num = 0;

        foreach (var part in docks)
        {
            num += 1;

            string name = "#" + num + " - " + part.PartData.sizeCategory;
            GUIStyle style = KBaseStyle.small_button;
            if (part == target_part)
            {
                GUILayout.Toggle(true, name, style );
            }
            else
            {
                if (GUILayout.Toggle(false, name, style ))
                {
                    current_vessel.VesselComponent.SetTargetByID(part.GlobalId);
                }
            }
        }

        // drawing options
        // shapes_drawer.draw_ui();

        if (UI_Tools.Button("Ok"))
        {
            ui_mode = UI_Mode.Main;
        }
    }

    public override void onGUI()
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
        else if (ui_mode == UI_Mode.Select_Target)
        {
            selectTargetGUI();
        }
        else if (ui_mode == UI_Mode.Select_Dock)
        {
            selectDockGUI();
        }
    }

    enum UI_Mode
    {
        Main,
        Select_Target,
        Select_Dock,
    }

    VesselComponent target_vessel;
    PartComponent target_part;
    int target_dock_num = -1;
    public SimulationObjectModel last_target;

    public List<PartComponent> docks = new List<PartComponent>();

    public override void Update()
    {
        // logger.LogInfo($"target is {current_vessel.VesselComponent.TargetObject}");
        if (current_vessel.VesselComponent == null)
            return;

        // update the dock list when target change
        if (last_target != current_vessel.VesselComponent.TargetObject)
        {
            logger.LogInfo($"changed target is {current_vessel.VesselComponent.TargetObject}");

            last_target = current_vessel.VesselComponent.TargetObject;
            target_part = null;
            target_dock_num = -1;

            if (last_target == null)
            {
                target_vessel = null;
                docks.Clear();
            }
            else if (last_target.IsCelestialBody)
            {
                docks.Clear();
            }
            else if (last_target.IsVessel)
            {
                // logger.LogInfo(last_target);
                target_vessel = last_target.Vessel;
                docks = DockTools.ListDocks(target_vessel);
            }
            else if (last_target.IsPart)
            {
                // dock selected
                target_part = last_target.Part;
                if (docks.Count == 0)
                    docks = DockTools.ListDocks(target_vessel);

                target_dock_num = docks.IndexOf(last_target.Part) + 1;

                PartOwnerComponent owner = target_part.PartOwner;
                if (owner.SimulationObject.IsVessel)
                {
                    target_vessel = owner.SimulationObject.Vessel;
                }
            }
            else
            {
                target_vessel = null;
            }
        }
    }

    public void drawShapes()
    {
        // logger.LogInfo("drawShapes");
        if (ui_mode == UI_Mode.Select_Dock)
        {
            foreach(var part in docks)
            {
                Color color = settings.dock_color;
                if (part == target_part)
                {
                    color = settings.selected_color;
                }

                shapes_drawer.DrawDockPort(part, current_vessel.VesselComponent, color);
            }

            //shape.draw_dockPort(part, current_vessel.VesselComponent);
           // shapes_drawer.DrawVesselCenter(current_vessel.VesselComponent);
        }
        else if (settings.show_gizmos)
        {
            if (target_part != null)
            {
                shapes_drawer.DrawDockPort(target_part, current_vessel.VesselComponent, settings.selected_color);
            }

            shapes_drawer.DrawVesselCenter(current_vessel.VesselComponent, settings.vessel_color);
        }
    }
}