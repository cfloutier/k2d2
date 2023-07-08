using UnityEngine;

namespace Hud.Shapes;

internal class NavballColors
{
    public Color Background { get; } = new Color(1, 1, 1, 0.01f);

    public Color Sky { get; } = Color100(21, 64, 78);
    public Color Ground { get; } = Color100(67, 42, 19);

    public Color SkyIndicator { get; } = Color100(5, 16, 20);
    public Color HorizontalIndicator { get; } = Color100(8, 20, 54);
    public Color GroundIndicator { get; } = Color100(17, 10, 5);

    public Color North { get; } = Color100(80, 16, 0);
    public Color South { get; } = Color100(2, 36, 0);
    public Color East { get; } = Color100(8, 20, 54);
    public Color West { get; } = Color100(8, 20, 54);

    public Color HorizonEdge { get; } = Color.grey;

    public Color Up { get; } = Color100(80, 66, 0);
    public Color Control { get; } = Color100(100, 22, 0);

    public Color Prograde { get; } = Color100(0, 100, 40);
    public Color Normal { get; } = Color100(47, 14, 47);
    public Color Radial { get; } = Color100(1, 54, 54);

    public Color Target { get; } = Color.grey;
    public Color Maneuver { get; } = Color100(100, 82, 59);

    private static Color Color100(int r, int g, int b, double a = 1)
    {
        var scale = 100f;
        return new Color(r / scale, g / scale, b / scale, (float)a);
    }
}
