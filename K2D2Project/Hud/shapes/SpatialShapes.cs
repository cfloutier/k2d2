using Shapes;
using UnityEngine;

namespace Hud.Shapes;

internal class SpatialShapes
{
    public static void DrawLine(Vector3 start, Vector3 end, Color color, float thickness, DashStyle dashStyle = null)
    {
        var blendMode = ShapesBlendMode.Opaque;
        var geometry = LineGeometry.Volumetric3D;
        var endCaps = LineEndCap.Square;
        var thicknessSpace = ThicknessSpace.Meters;

        Draw.Line(blendMode, geometry, endCaps, thicknessSpace, start, end, color, color, thickness, dashStyle);
    }

    public static void DrawCone(Vector3d pos, Vector3d normal, float radius, float length, Color color)
    {
        var blendMode = ShapesBlendMode.Opaque;
        var sizeSpace = ThicknessSpace.Meters;
        var rot = Quaternion.LookRotation(normal);
        var fillCap = true;

        Draw.Cone(blendMode, sizeSpace, pos, rot, radius, length, fillCap, color);
    }

    public static void DrawTorus(Vector3d pos, Vector3d normal, float radius, float thickness, Color color) 
    {
        var blendMode = ShapesBlendMode.Opaque;
        var spaceRadius = ThicknessSpace.Meters;
        var spaceThickness = ThicknessSpace.Meters;
        var rot = Quaternion.LookRotation(normal);
        
        Draw.Torus(blendMode, spaceRadius, spaceThickness, pos, rot, radius, thickness, color);
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
