
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

        var vessel = current_vessel.VesselComponent;
        if (vessel == null)
        {
            return;
        }
        var target_vel = vessel.TargetVelocity;
        UI_Tools.Label($"Target speedV {StrTool.Vector3ToString(target_vel.vector)} ");

        if (target_part != null)
        {
            var diff = target_part.CenterOfMass - control_component.CenterOfMass;
            UI_Tools.Label($"rel pos {StrTool.Vector3ToString(diff.vector)} ");
            UI_Tools.Label($"dist {StrTool.DistanceToString(diff.magnitude)} ");
        }
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
        if (!dock_ui.drawShapes(shapes_drawer))
        {
            if (settings.show_gizmos)
            {
                // draw target
                if (target_part != null)
                {
                    shapes_drawer.DrawComponent(target_part, current_vessel.VesselComponent, settings.target_color);
                }

                // draw control
                shapes_drawer.DrawComponent(control_component, current_vessel.VesselComponent, settings.vessel_color);
            }
        }
    }
}