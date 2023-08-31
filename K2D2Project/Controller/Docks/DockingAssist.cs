
using K2D2.KSPService;
using KTools.UI;
using BepInEx.Logging;
using UnityEngine;

using KSP.Sim.impl;
using KSP.Sim;
using K2D2.Controller.Docks;

using static K2D2.Controller.Docks.DockTools;
using K2D2.Controller.Docks.Pilots;
using static KSP.Api.UIDataPropertyStrings.View;

namespace K2D2.Controller;

public class DockingAssist : SingleExecuteController
{
    public static DockingAssist Instance { get; set; }
    public ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.DockingTool");

    public KSPVessel current_vessel;
    public PartComponent control_component = null;
    public VesselComponent target_vessel;
    public PartComponent target_part;

    public int target_dock_num = -1;
    public SimulationObjectModel last_target;

    public ListPart docks = new ListPart();
    public DocksSettings settings = new DocksSettings();


    public enum PilotMode
    {
        Off,
        KillSpeed,
        FinalApproach,
    }

    PilotMode _mode = PilotMode.Off;
    public PilotMode Mode
    {
        get
        {
            return _mode;
        }
        set
        {
            _mode = value;
            switch(_mode)
            {
                case PilotMode.KillSpeed:
                    setController(kill_speed_pilot);
                    kill_speed_pilot.Start();
                    break;
                case PilotMode.FinalApproach:
                    setController(final_approach_pilot);
                    final_approach_pilot.StartPilot(target_part, control_component);
                    break;
                case PilotMode.Off:
                    setController(null);
                    break;
            }
        }
    }

    KillSpeed kill_speed_pilot = null;
    FinalApproach final_approach_pilot = null;

    public override bool isRunning
    {
        get { return Mode != PilotMode.Off; }
        set
        {
            if (isRunning && value)
                return;

            if (!value)
            {
                // stop
                if (current_vessel != null)
                    current_vessel.SetThrottle(0);

                Mode = PilotMode.Off;
            }
        }
    }

    public DockingAssist()
    {
        Instance = this;
        debug_mode_only = false;
        name = "Dock";

        current_vessel = K2D2_Plugin.Instance.current_vessel;

        shapes_drawer = new DockShape(settings);
        dock_ui = new DockingUI(this);
        kill_speed_pilot = new KillSpeed(turnTo);
        final_approach_pilot = new FinalApproach(turnTo);
    }

    DockShape shapes_drawer;

    DockingUI dock_ui;

    public DockingTurnTo turnTo = new DockingTurnTo();

    public override void onGUI()
    {
        dock_ui.onGUI();

        var vessel = current_vessel.VesselComponent;
        if (vessel == null)
        {
            return;
        }

        if (sub_controler != null)
            sub_controler.onGUI();

    }

    public void listDocks()
    {
        docks = FindParts(target_vessel, false, true);
    }

    public override void Update()
    {
        turnTo.Update();

        // logger.LogInfo($"target is {current_vessel.VesselComponent.TargetObject}");
        if (current_vessel.VesselComponent == null)
        {
            control_component = null;
            return;
        }

        if (sub_controler != null)
        {
            sub_controler.Update();
            if (sub_controler.finished)
            {
                isRunning = false;
            }
        }

        control_component = current_vessel.VesselComponent.GetControlOwner();

        // update the dock list when target change
        if (last_target != current_vessel.VesselComponent.TargetObject)
        {
            // logger.LogInfo($"changed target is {current_vessel.VesselComponent.TargetObject}");

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

        if (target_part != null)
        {
            var vessel = current_vessel.VesselComponent;
            if (vessel == null)
            {
                return;
            }


            diff_Position = Position.Delta(target_part.CenterOfMass, control_component.CenterOfMass);
            var curent_vessel_frame = control_component.transform.coordinateSystem;
            var vessel_to_control = Matrix4x4D.TRS(
                curent_vessel_frame.ToLocalPosition(control_component.transform.Position),
                curent_vessel_frame.ToLocalRotation(control_component.transform.Rotation)).GetInverse();

            vessel_to_target = vessel_to_control.TransformPoint( curent_vessel_frame.ToLocalPosition(target_part.CenterOfMass) );

            local_speed = vessel_to_control.TransformVector( curent_vessel_frame.ToLocalVector( vessel.TargetVelocity ) );
            target_to_vessel = vessel_to_control.TransformPoint( curent_vessel_frame.ToLocalPosition(control_component.CenterOfMass));
        }
    }

    public Vector diff_Position = new Vector();
    public Vector3 local_speed = new Vector3();

    public Vector3 vessel_to_target = new Vector3();
    public Vector3 target_to_vessel = new Vector3();

    public void drawShapes()
    {
        if (!dock_ui.drawShapes(shapes_drawer))
        {

            var vessel = current_vessel.VesselComponent;
            if (settings.show_gizmos)
            {
                // draw target
                if (target_part != null)
                {
                    shapes_drawer.DrawComponent(target_part, vessel, settings.target_color);
                    shapes_drawer.DrawSpeed(control_component, vessel, vessel.TargetVelocity, Color.red);
                }

                // draw control
                shapes_drawer.DrawComponent(control_component, vessel, settings.vessel_color);
            }
        }
    }
}