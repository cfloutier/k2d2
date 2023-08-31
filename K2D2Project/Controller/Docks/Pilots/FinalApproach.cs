using K2D2.KSPService;
using KSP.Sim;
using KSP.Sim.impl;
using KSP.Sim.Maneuver;
using KTools;
using KTools.UI;
using UnityEngine;
using static K2D2.Controller.DroneController;
using static KSP.Api.UIDataPropertyStrings.View;

namespace K2D2.Controller.Docks.Pilots;

/// <summary>
/// rotation used for docking
/// </summary>
public class FinalApproach : ExecuteController
{
   public FinalApproach(DockingTurnTo turnTo)
   {
        this.turnTo = turnTo;
   }

    KSPVessel current_vessel;
    DockingTurnTo turnTo = null;


    public PartComponent control_component = null;

    public PartComponent target_part;

    public void StartPilot(PartComponent target_part, PartComponent control_component)
    {
        base.Start();
        this.target_part = target_part;
        this.control_component = control_component;
        current_vessel = K2D2_Plugin.Instance.current_vessel;
        finished = false;
    }

    public override void Update()
    {
        finished = false;

        current_vessel.SetThrottle(0);

        // update target position and speed
        UpdatePosition();

        // compute trust
        computeRCSThrust();
    }


    public Vector diff_Position = new Vector();
    public Vector3 local_speed = new Vector3();
    public Vector3 local_target_pos = new Vector3();

    public Vector3 vessel_to_target = new Vector3();
    public Vector3 target_to_vessel = new Vector3();

    void UpdatePosition()
    {
        if (target_part != null)
        {
            var vessel = current_vessel.VesselComponent;
            if (vessel == null)
            {
                return;
            }


            // diff_Position = Position.Delta(target_part.CenterOfMass, control_component.CenterOfMass);
            var curent_vessel_frame = control_component.transform.coordinateSystem;

            var vessel_to_control = Matrix4x4D.TRS(
                curent_vessel_frame.ToLocalPosition(control_component.transform.Position),
                curent_vessel_frame.ToLocalRotation(control_component.transform.Rotation)).GetTranspose();

            vessel_to_target = vessel_to_control.TransformPoint( curent_vessel_frame.ToLocalPosition(target_part.CenterOfMass) );

            local_speed = vessel_to_control.TransformVector( curent_vessel_frame.ToLocalVector(vessel.TargetVelocity ) );
            // local_target_pos = vessel_to_control.TransformVector(curent_vessel_frame.ToLocalPosition(diff_Position));

            // target_to_vessel = vessel_to_control.TransformPoint(curent_vessel_frame.ToLocalPosition(control_component.CenterOfMass));

            if (last_TargetSpeed != Vector3.zero)
            {
                var delta = local_speed - last_TargetSpeed;
                current_acc = delta / Time.deltaTime;
            }

            last_TargetSpeed = local_speed;
        }
    }

    Vector3 rcsThrust = Vector3.zero;
    Vector3 last_TargetSpeed;
    Vector3 current_acc;

    void computeRCSThrust()
    {

        // Why do X Y Z does not fit the speed direction ? rotation of 90� X ...
        if (current_vessel.X != 0)
        {
            rcsThrust.x = Mathf.Lerp(rcsThrust.x, current_acc.x / current_vessel.X , Time.deltaTime * 1);
        }
        if (current_vessel.Y != 0)
        {
            rcsThrust.y = Mathf.Lerp(rcsThrust.y, current_acc.y / current_vessel.Y, Time.deltaTime * 1);
        }
        if (current_vessel.Z != 0)
        {
            rcsThrust.z = Mathf.Lerp(rcsThrust.z, current_acc.z / current_vessel.Z, Time.deltaTime * 1);
        }
    }



    public override void onGUI()
    {
        UI_Tools.Warning("FinalApproach");

        if (!turnTo.finished)
            turnTo.onGUI();

        //if (K2D2Settings.debug_mode)
        {
            UI_Tools.Label($"Target speedV {StrTool.Vector3ToString(local_speed)} ");

            if (target_part != null)
            {
                UI_Tools.Label($"local_target_pos {StrTool.Vector3ToString(local_target_pos)} ");
                UI_Tools.Label($"diff_Position {StrTool.Vector3ToString(diff_Position.vector)} ");
                UI_Tools.Label($"vessel_to_target {StrTool.Vector3ToString(vessel_to_target)} ");

                UI_Tools.Label($"dist {StrTool.DistanceToString(vessel_to_target.magnitude)} ");
            }

            UI_Tools.Label($"X {current_vessel.X} ");
            UI_Tools.Label($"Y {current_vessel.Y} ");
            UI_Tools.Label($"Z {current_vessel.Z} ");

            UI_Tools.Label($"current_acc {StrTool.Vector3ToString(current_acc)} ");
            UI_Tools.Label($"rcsThrust {StrTool.Vector3ToString(rcsThrust)} ");
        }
    }


}
