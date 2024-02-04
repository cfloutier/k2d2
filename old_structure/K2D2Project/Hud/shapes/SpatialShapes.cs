using Shapes;
using UnityEngine;

namespace Hud.Shapes;

internal class SpatialShapes
{
    public static void DrawLine(Vector3 start, Vector3 end, Color color, float thickness, DashStyle dashStyle = null)
    {
        Draw.BlendMode = ShapesBlendMode.Opaque;
        Draw.LineGeometry = LineGeometry.Volumetric3D;;
        Draw.LineEndCaps = LineEndCap.Square;
        Draw.LineThicknessSpace = ThicknessSpace.Meters;
        if (dashStyle != null)
            Draw.LineDashStyle = dashStyle;
        else
            Draw.LineDashStyle = DashStyle.DefaultDashStyleLine;
        Draw.LineDashStyle = dashStyle;

        Draw.Line(start, end, thickness, color);
    }

    public static void DrawCone(Vector3d pos, Vector3d normal, float radius, float length, Color color)
    {
        Draw.BlendMode = ShapesBlendMode.Opaque;
        Draw.ConeSizeSpace = ThicknessSpace.Meters;

        var rot = Quaternion.LookRotation(normal);
        var fillCap = true;

        Draw.Cone(pos, rot, radius, length, fillCap, color);
    }

    public static void DrawTorus(Vector3d pos, Vector3d normal, float radius, float thickness, Color color) 
    {
        var rot = Quaternion.LookRotation(normal);
        Draw.BlendMode = ShapesBlendMode.Opaque;
        Draw.TorusRadiusSpace = ThicknessSpace.Meters;
        Draw.TorusThicknessSpace = ThicknessSpace.Meters;
        
        Draw.Torus( pos, rot, radius, thickness, color);
    }

    // XX can not draw a partial Torus only a partial Disc
    // but Disc is limited too because it is 2D form : so it has no 3D thickness and spatial ordering...
    // here is a workaround
    public static void DrawTorusQuarter(Vector3d pos, Vector3 vertical, Vector3 horizontal, Color color, float radius, float thickness, int steps = 18)
    {
        // TODO memoize cos and sin
        for (int i = 0; i < steps; i++)
        {
            var stepRad = Mathf.PI / (steps * 2);

            var currentAngle = i * stepRad;
            var nextAngle = (i + 1) * stepRad;

            var currentRadius = Mathf.Cos(currentAngle) * radius;
            var nextRadius = Mathf.Cos(nextAngle) * radius;

            var currentOffset = Mathf.Sin(currentAngle) * radius;
            var nextOffset = Mathf.Sin(nextAngle) * radius;

            var start = pos + (horizontal * currentRadius) + (vertical * currentOffset);
            var end = pos + (horizontal * nextRadius) + (vertical * nextOffset);
            SpatialShapes.DrawLine(start, end, color, thickness);
        }
    }

    // Torus has a circular section, here is a workaround to increase width without height increase
    public static void DrawWideTorusQuarter(Vector3d pos, Vector3 vertical, Vector3 horizontal, Color color, float radius, float thickness, int wide) {
        for (int i = 0 - wide; i <= wide; i++)
        {
            var localRadius = radius - i * thickness;
            SpatialShapes.DrawTorusQuarter(pos, vertical, horizontal, color, localRadius, thickness);
        }
    }

}
