using Shapes;
using UnityEngine;

namespace K2D2.Controller;
using KTools.UI;
using KTools;
using KSP.Sim.impl;
using KSP.Sim;
using K2D2.Controller.Docks;
using KSP.Iteration.UI.Binding;
using static KSP.Api.UIDataPropertyStrings.View.Vessel.Stages;
using RTG;

public class DockShape
{
    DocksSettings settings = null;
    public DockShape(DocksSettings settings)
    {
        this.settings = settings;
    }

    ShapesBlendMode blendMode = ShapesBlendMode.Additive;

    //ColorEditor color_editor = new ColorEditor();

    public void DrawComponent(PartComponent part, VesselComponent main_vessel, Color color, bool torus, bool line)
    {
        // var frame = part.transform.coordinateSystem;

        Position center = part.CenterOfMass;
        Position start = center;
        Position end = start + part.transform.up * settings.length_line;

        var local_frame = main_vessel.transform.coordinateSystem;

        Vector3 localStart = local_frame.ToLocalPosition(start);
        Vector3 localEnd = local_frame.ToLocalPosition(end);

        Vector3 direction = localEnd - localStart;

        var rot = Quaternion.LookRotation(direction.normalized);

        color.a = settings.sfx_blur;

        float radius = (float) (part.PartData.PartSizeDiameter/2);;

        if (torus)
            Draw.Torus(blendMode, ThicknessSpace.Meters, ThicknessSpace.Pixels, localStart, rot, radius, settings.thickness_circle, color);
        if (line)
            Draw.Line(blendMode, LineGeometry.Volumetric3D, LineEndCap.Round, ThicknessSpace.Pixels, localStart, localEnd, color, color, settings.thickness_line);
    }

    public void drawAxis(PartComponent part, VesselComponent main_vessel)
    {
        Position center = part.CenterOfMass;
        Position start = center + part.transform.up*settings.pos_grid;

        var local_frame = main_vessel.transform.coordinateSystem;
        Vector3 localStart = local_frame.ToLocalPosition(start);

        //var rot = Quaternion.LookRotation(direction.normalized);
        Vector3 Y_Dir = local_frame.ToLocalVector( part.transform.up);
        Vector3 X_Dir = local_frame.ToLocalVector( part.transform.right);
        Vector3 Z_Dir = local_frame.ToLocalVector( part.transform.forward);

        var rot = Quaternion.LookRotation(Y_Dir);

        // center
        Draw.Torus(blendMode, ThicknessSpace.Pixels, ThicknessSpace.Pixels, localStart, rot, 10, settings.thickness_line / 0.8f, Color.white);

        DrawLocalArrow(localStart, localStart + Y_Dir* settings.length_line, Color.yellow);
        DrawLocalArrow(localStart, localStart + X_Dir* settings.length_line, Color.blue);
        DrawLocalArrow(localStart, localStart + Z_Dir* settings.length_line, Color.green);
    }

    void DrawLocalArrow(Vector3 localStart, Vector3 localEnd, Color color)
    {
        Quaternion localDirection = Quaternion.LookRotation(localEnd - localStart);

        Draw.Line(blendMode, LineGeometry.Volumetric3D, LineEndCap.Round, ThicknessSpace.Pixels,
                localStart, localEnd, color, color, settings.thickness_line);
        Draw.Cone(blendMode, ThicknessSpace.Pixels,
                localEnd, localDirection, settings.thickness_line*3, settings.thickness_line * 3, true, color);
    }


    public void DrawSpeed(PartComponent center_part, VesselComponent main_vessel, Vector speed, Color color)
    {
        Position center = center_part.CenterOfMass;
        color.a = settings.sfx_blur;
        Position start = center + center_part.transform.up * settings.pos_grid;

       // Position end = start + speed;

        var local_frame = main_vessel.transform.coordinateSystem;


        //var rot = Quaternion.LookRotation(direction.normalized);
        Vector3 X_Dir = local_frame.ToLocalVector( center_part.transform.right);
        Vector3 Y_Dir = local_frame.ToLocalVector( center_part.transform.up);
        Vector3 Z_Dir = local_frame.ToLocalVector( center_part.transform.forward);

        Vector3 localSpeed = local_frame.ToLocalVector( speed );

        float x_value = localSpeed.Dot(X_Dir);
        float y_value = localSpeed.Dot(Y_Dir);
        float z_value = localSpeed.Dot(Z_Dir);


        Vector3 localStart = local_frame.ToLocalPosition(start);

        Vector3 localSpeed_Plane = localStart + x_value * X_Dir + z_value * Z_Dir;
        Vector3 local_forward = localSpeed_Plane + y_value*Y_Dir;

        DrawLocalArrow(localStart, localSpeed_Plane, Color.cyan);
        DrawLocalArrow(localSpeed_Plane, local_forward, Color.red);
    }

    public void Drawline(Position start, Position end, VesselComponent main_vessel, Color color)
    {
        var local_frame = main_vessel.transform.coordinateSystem;


        Vector3 localStart = local_frame.ToLocalPosition(start);
        Vector3 localEnd = local_frame.ToLocalPosition(end);
        color.a = settings.sfx_blur;

        Draw.Line(blendMode, LineGeometry.Volumetric3D, LineEndCap.Round, ThicknessSpace.Pixels,
                localStart, localEnd, color, color, settings.thickness_line);
    }


}

