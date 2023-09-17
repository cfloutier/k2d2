
using K2D2.KSPService;
using KSP.Sim;
using KSP.Sim.impl;

using KTools.UI;
using UnityEngine;

namespace K2D2.Controller.Docks.Pilots;

/// <summary>
/// rotation used for docking
/// </summary>
public class FinalApproach : ExecuteController
{
   public FinalApproach(DocksSettings settings, DockingTurnTo turnTo)
   {
        this.settings = settings;
        this.turnTo = turnTo;
   }

    KSPVessel current_vessel;
    DockingTurnTo turnTo = null;

    public PartComponent control_component = null;

    public PartComponent target_part;

    public DocksSettings settings;

    public void StartPilot( PartComponent target_part, PartComponent control_component)
    {
        base.Start();
        this.target_part = target_part;
        this.control_component = control_component;
        current_vessel = K2D2_Plugin.Instance.current_vessel;
        turnTo.StartDockAlign();

        Sub_mode = SubMode.Manual;
        Kill_X_Speed = Kill_Y_Speed = Kill_Z_Speed = false;
        center_axis = false;
        auto_forward = false;
        center_power = 1;

        finished = false;
    }

    public enum SubMode{
        Manual,
        Auto,
    }

    public SubMode _sub_mode = SubMode.Manual;
    public SubMode Sub_mode
    {
        get { return _sub_mode;}
        set {
                if (_sub_mode == value)
                    return;

                _sub_mode = value;
                switch ( _sub_mode)
                {
                    case SubMode.Auto:
                        // reset rcs thrust
                        forward_speed = 0;
                        break;
                }
            }
    }

    public Vector diff_Position = new Vector();
    public Vector3 local_speed = new Vector3();
    public Vector3 local_target_pos = new Vector3();
    public Vector3 vessel_to_target = new Vector3();
    

    public bool Kill_X_Speed = false;
    public bool Kill_Y_Speed = false;
    public bool Kill_Z_Speed = false;

    public override void Update()
    {
        finished = false;

        current_vessel.SetThrottle(0);

        // update target position and speed
        UpdatePosition();

        switch(_sub_mode)
        {
            case SubMode.Manual:
                RCKillSpeed();
                break;
            case SubMode.Auto:
                AutoMode();
                break;
        }
    }

    void RCKillSpeed()
    {
       if (Kill_X_Speed && current_vessel.X == 0)
            current_vessel.X = - local_speed.x * settings.pilot_power;

       if (Kill_Y_Speed && current_vessel.Y == 0)
            current_vessel.Y = - local_speed.y * settings.pilot_power;

       if (Kill_Z_Speed && current_vessel.Z == 0)
            current_vessel.Z = - local_speed.z * settings.pilot_power;
    }


    bool auto_forward = false;

    bool center_axis = false;
    float forward_speed = 0;

    float center_power = 0;

    float forward_wanted_speed;

    float right_wanted_speed;
    float up_wanted_speed;

    void AutoMode()
    {
        current_vessel.Y = (forward_speed - local_speed.y) * settings.pilot_power;

        float max_speed = 1 + vessel_to_target.magnitude / 10;

        if (auto_forward)
        {
            forward_wanted_speed = Mathf.Sign( vessel_to_target.y) * Mathf.Sqrt( Mathf.Abs(center_power/10 * vessel_to_target.y ));
            forward_wanted_speed += 0.05f; // final touch speed !!
            forward_wanted_speed = Mathf.Clamp(forward_wanted_speed, -max_speed, max_speed);
            current_vessel.Y = (forward_wanted_speed - local_speed.y) * settings.pilot_power;
        }

        if (center_axis)
        {
            right_wanted_speed = Mathf.Sign( vessel_to_target.x) * Mathf.Sqrt( Mathf.Abs(center_power/10 * vessel_to_target.x ));
            right_wanted_speed = Mathf.Clamp(right_wanted_speed, -max_speed, max_speed);
            current_vessel.X = (right_wanted_speed - local_speed.x) * settings.pilot_power;

            up_wanted_speed = Mathf.Sign( vessel_to_target.z) * Mathf.Sqrt(Mathf.Abs( center_power/10 * vessel_to_target.z ));
            up_wanted_speed = Mathf.Clamp(up_wanted_speed, -max_speed, max_speed);
            current_vessel.Z = (up_wanted_speed - local_speed.z) * settings.pilot_power;
        }
    }

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
                curent_vessel_frame.ToLocalRotation(control_component.transform.Rotation)).GetInverse();

            vessel_to_target = vessel_to_control.TransformPoint( curent_vessel_frame.ToLocalPosition( target_part.CenterOfMass ) );

            local_speed = vessel_to_control.TransformVector( curent_vessel_frame.ToLocalVector(vessel.TargetVelocity ) );

            if (last_TargetSpeed != Vector3.zero)
            {
                var delta = local_speed - last_TargetSpeed;
                current_acc = delta / Time.deltaTime;
            }

            last_TargetSpeed = local_speed;
        }
    }

    Vector3 rcsThrust = Vector3.one;
    Vector3 last_TargetSpeed;
    Vector3 current_acc;

    void computeRCSThrust()
    {
        // Why do X Y Z does not fit the speed direction ? rotation of 90° X ...
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

        // if (!turnTo.finished)
        //     turnTo.onGUI();

        Sub_mode = UI_Tools.EnumGrid<SubMode>("mode", Sub_mode, 2);


        switch (Sub_mode)
        {
            case SubMode.Manual:
                GUILayout.Label("RCS - Kill Speed");
                GUILayout.BeginHorizontal();
                Kill_X_Speed = UI_Tools.Toggle(Kill_X_Speed, "Hor.");
                Kill_Z_Speed = UI_Tools.Toggle(Kill_Z_Speed, "Vert.");
                Kill_Y_Speed = UI_Tools.Toggle(Kill_Y_Speed, "Depth.");
                bool is_all = Kill_X_Speed && Kill_Z_Speed && Kill_Y_Speed;
                bool _is_all = UI_Tools.Toggle(is_all, "All");
                if (is_all != _is_all)
                {
                    Kill_X_Speed = Kill_Z_Speed = Kill_Y_Speed = _is_all;
                }
                GUILayout.EndHorizontal();
                break;
            case SubMode.Auto:
                // UI_Tools.Label($"Auto Mode");

                UI_Tools.Label("Axis : ");
                center_axis = UI_Tools.Toggle(center_axis, "Center axis");
                if (center_axis || auto_forward)
                {
                    center_power = UI_Tools.FloatSliderTxt("Pilot - speed adapt", center_power, 0, 5);

                    if (K2D2Settings.debug_mode)
                    {
                        UI_Tools.Label("Wanted speeds : ");
                        if (auto_forward)
                            UI_Tools.FloatSliderTxt("forward", forward_wanted_speed, -10, 10);

                        UI_Tools.FloatSliderTxt("right", right_wanted_speed, -10, 10);
                        UI_Tools.FloatSliderTxt("up ", up_wanted_speed,-10, 10);
                    }
                }

                UI_Tools.Label("Forward : ");
                auto_forward = UI_Tools.Toggle(auto_forward, "auto approach");
                if (!auto_forward)
                {
                    UI_Tools.Label($"Forward speed : {forward_speed:n2}");
                    GUILayout.BeginHorizontal();
                    forward_speed = UI_Tools.FloatSlider(forward_speed, -10, 10);

                    if (UI_Tools.resetButton())
                        forward_speed = 0;

                    GUILayout.EndHorizontal();
                }

                break;
        }

        if (K2D2Settings.debug_mode)
        {
             UI_Tools.Console($"speed {StrTool.Vector3ToString(local_speed)} ");
            // UI_Tools.Console($"Rcs Thrust {StrTool.Vector3ToString(rcsThrust)} ");
            UI_Tools.Console($"Control {current_vessel.X:n2} {current_vessel.Y:n2} {current_vessel.Z:n2}");
            UI_Tools.Label($"Pos {StrTool.Vector3ToString(vessel_to_target)} ");
        }


        //if (K2D2Settings.debug_mode)
        // {
        //     UI_Tools.Label($"Target speedV {StrTool.Vector3ToString(local_speed)} ");

        //     if (target_part != null)
        //     {
        //
        //         UI_Tools.Label($"diff_Position {StrTool.Vector3ToString(diff_Position.vector)} ");
        //         UI_Tools.Label($"vessel_to_target {StrTool.Vector3ToString(vessel_to_target)} ");

        //         UI_Tools.Label($"dist {StrTool.DistanceToString(vessel_to_target.magnitude)} ");
        //     }
        //     UI_Tools.Label($"current_acc {StrTool.Vector3ToString(current_acc)} ");
        //     UI_Tools.Label($"rcsThrust {StrTool.Vector3ToString(rcsThrust)} ");
        // }
    }

    public void drawShapes(DockShape shape_drawer)
    {
        drawTargetPosLines(shape_drawer);
    }

    public void drawTargetPosLines(DockShape shape_drawer)
    {
        // draw directions of position in the local control frame
        if (control_component == null)
            return;

        //L.Log("Final Approach Draw Shapes");

        if (settings.show_gizmos)
        {
            Position center = control_component.CenterOfMass;
            Position start = center + control_component.transform.up * settings.pos_grid;

            Vector3 pos = vessel_to_target;

            // start with X
            Position XPos = start + control_component.transform.right * pos.x;

            // Z is the Y command
            Position Center_Plane = XPos + control_component.transform.forward * pos.z;

            // y is the z command
            Position ZPos = Center_Plane + control_component.transform.up * pos.y;


            float len_lines = 100;

            // vertical lines
            shape_drawer.Drawline(Center_Plane ,
                                Center_Plane + control_component.transform.forward*len_lines,
                                current_vessel.VesselComponent, Color.green);

            shape_drawer.Drawline(Center_Plane ,
                                Center_Plane + control_component.transform.back*len_lines,
                                current_vessel.VesselComponent, Color.green);

            // horizontal line
            shape_drawer.Drawline(Center_Plane,
                                Center_Plane + control_component.transform.left*len_lines,
                                current_vessel.VesselComponent, Color.blue);

            shape_drawer.Drawline(Center_Plane,
                                Center_Plane + control_component.transform.right*len_lines,
                                current_vessel.VesselComponent, Color.blue);
            // depth line
            shape_drawer.Drawline(Center_Plane, ZPos, current_vessel.VesselComponent, Color.yellow);
        }
    }
}
