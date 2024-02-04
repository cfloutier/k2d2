using Shapes;
using UnityEngine;

namespace Hud.Shapes;

// XX can not draw a partial Torus only a partial Disc
// but Disc is limited too because it is 2D form : so it has no 3D thickness...
internal class SphereShapes
{
    private readonly Vector3 _position;
    private readonly float _radius;
    private readonly float _thickness;

    public SphereShapes(Vector3 position, float radius)
    {
        _position = position;
        _radius = radius;
        _thickness = radius / 200;
    }

    public void DrawWideTorus(Vector3 up, Vector3 right, Color[] quarterColors)
    {
        var wide = 1;

        SpatialShapes.DrawWideTorusQuarter(_position, up, right, quarterColors[0], _radius, _thickness, wide);
        SpatialShapes.DrawWideTorusQuarter(_position, -right, up, quarterColors[1], _radius, _thickness, wide);
        SpatialShapes.DrawWideTorusQuarter(_position, -up, -right, quarterColors[2], _radius, _thickness, wide);
        SpatialShapes.DrawWideTorusQuarter(_position, right, -up, quarterColors[3], _radius, _thickness, wide);
    }

    public void DrawGraduation(Vector3 direction, Color color, bool invert)
    {
        var inversion = invert ? -1 : 1;
        
        var start = _position + (direction * _radius);
        var end = start + 2 * _thickness * direction * inversion;

        SpatialShapes.DrawLine(start, end, color, _thickness * 3f);
    }
}
