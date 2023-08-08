
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

namespace K2D2.Controller;

public class DockingAssist : ComplexControler
{
    public static DockingAssist Instance { get; set; }
    public ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.DockingTool");

    public KSPVessel current_vessel;
    public PartComponent control_component = null;
    public VesselComponent target_vessel;
    public PartComponent target_part;

    public int target_dock_num = -1;
    public SimulationObjectModel last_target;

    public DockTools.ListPart docks = new DockTools.ListPart();
    public DocksSettings settings = new DocksSettings();

    public DockingAssist()
    {
        Instance = this;
        debug_mode_only = false;
        name = "Dock";

        current_vessel = K2D2_Plugin.Instance.current_vessel;

        shapes_drawer = new DockShape(settings);
        dock_ui = new DockingUI(this);
    }

    DockShape shapes_drawer;

    
    DockingUI dock_ui;

    public override void onGUI()
    {
        dock_ui.onGUI();
    }

    public void listDocks()
    {
        docks = DockTools.FindParts(target_vessel, false, true);
    }

    public override void Update()
    {
        // logger.LogInfo($"target is {current_vessel.VesselComponent.TargetObject}");
        if (current_vessel.VesselComponent == null)
        {
            control_component = null;
            return;
        }

        control_component = current_vessel.VesselComponent.GetControlOwner();

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
                
            }
            else if (last_target.IsPart)
            {
                // dock selected
                target_part = last_target.Part;
                if (docks.Count == 0 && target_vessel != null)
                    listDocks();

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
        if (dock_ui.ui_mode == DockingUI.UI_Mode.Select_Dock)
        {
            foreach(NamedComponent part in docks.Parts)
            {
                Color color = settings.dock_color;
                if (part.component == target_part)
                {
                    color = settings.selected_color;
                }

                shapes_drawer.DrawPartComponent(part.component, current_vessel.VesselComponent, color);
            }

            //shape.draw_dockPort(part, current_vessel.VesselComponent);
           // shapes_drawer.DrawVesselCenter(current_vessel.VesselComponent);
        }
        else if (settings.show_gizmos)
        {
            if (target_part != null)
            {
                shapes_drawer.DrawPartComponent(target_part, current_vessel.VesselComponent, settings.selected_color);
            }

            shapes_drawer.DrawPartComponent(control_component, current_vessel.VesselComponent, settings.vessel_color);
        }
    }
}