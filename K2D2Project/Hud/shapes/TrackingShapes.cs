using Shapes;
using UnityEngine;

namespace Hud.Shapes;

internal class TrackingShapes
{
    internal class Thickness
    {
        public readonly float Torus;
        
        public readonly float MajorLine;
        
        public readonly float MinorLine;

        public Thickness(float radius) 
        {
            Torus = radius / 100;
            MajorLine = radius / 100;
            MinorLine = radius / 200;
        }
    }


    private readonly Vector3 _position;
    private readonly float _radius;
    private readonly Thickness _thickness;

    public TrackingShapes(Vector3 position, float radius)
    {
        _position = position;
        _radius = radius;
        _thickness = new Thickness(radius);
    }

    public void DrawHeading(Vector3? direction, Color color, bool invert = false)
    {
        if (direction is null)
        {
            return;
        }

        DrawHeadingLine(direction, color, true, invert);
        DrawHeadingCone(direction, color, true, invert);

        DrawHeadingLine(- direction, color, false, invert);
        DrawHeadingCone(- direction, color, false, invert);
    }

    public void DrawReference(Vector3? direction, Color color)
    {
        if (direction is null)
        {
            return;
        }

        var scaledDirection = ScaleDirection(direction, 1.01f);

        SpatialShapes.DrawTorus(_position + scaledDirection, direction.Value, _radius / 25, _thickness.Torus, color);
        SpatialShapes.DrawTorus(_position - scaledDirection, direction.Value, _radius / 50, _thickness.Torus, color);
    }

    public void DrawSelection(Vector3? direction, Color color)
    {
        if (direction is null)
        {
            return;
        }

        var scaledDirection = ScaleDirection(direction, 0.99f);

        SpatialShapes.DrawTorus(_position + scaledDirection, direction.Value, _radius / 25, _thickness.Torus, color);
        SpatialShapes.DrawTorus(_position - scaledDirection, direction.Value, _radius / 50, _thickness.Torus, color);
    }

    private void DrawHeadingLine(Vector3? direction, Color color, bool major, bool invert)
    {
        var start = _position;
        var end = _position + ScaleDirection(direction);

        var thickness = major ? _thickness.MajorLine : _thickness.MinorLine;

        //  ensure that dahsSize + dashSpace is the same for major and minor heading
        var dashSize = major ? _radius / 20 : _radius / 50;
        var dashSpace = _radius / 7 - dashSize;
        var dashOffset = invert ? 0 : 0.5f;
        var dashStyle = new DashStyle(dashSize, dashSpace, dashOffset)
        {
            space = DashSpace.Meters
        };

        SpatialShapes.DrawLine(start, end, color, thickness, dashStyle);
    }

    private void DrawHeadingCone(Vector3? direction, Color color, bool major, bool invert)
    {
        var scaledDirection = ScaleDirection(direction);
        var inversion = invert ? 1 : -1;

        var radius = major ? _radius / 25 : _radius / 50;
        var length = major ? _radius / 10 : _radius / 20;

        var pos = _position + scaledDirection + (direction.Value * inversion * length);
        var normal = -direction.Value * inversion;

        SpatialShapes.DrawCone(pos, normal, radius, length, color);
    }

    private Vector3 ScaleDirection(Vector3? direction, float offset = 1)
    {
        return direction.Value.normalized * _radius * offset;
    }

}
