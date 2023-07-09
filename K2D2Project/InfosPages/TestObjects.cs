using K2D2.Controller;
using KTools.UI;
using UnityEngine;
using Shapes;
using KTools.Shapes;

using KSP.Sim.impl;
using KSP.Sim;

namespace K2D2.InfosPages;

class TestObjects : ComplexControler
{
    public static TestObjects Instance { get; set; }

     public TestObjects()
    {
        Instance = this;
        debug_mode_only = false;
        ShapeDrawer.Instance.shapes.Add(onDrawShape);
        name = "TestObjects";
    }

    bool visible = false;

    float radius = 1;

    float distance = 3;

    float thickness = 0.1f;


    float alpha = 1;

    ShapesBlendMode blendMode = ShapesBlendMode.Opaque;

    float SimpleSlider(string label, float value, float min, float max)
    {
        UI_Tools.Label($"{label} : {value}");
        value = UI_Tools.FloatSlider(value, 0, 10);
        return value;
    }

    public override void onGUI()
    {
        UI_Tools.Label("test Objects");

        visible = UI_Tools.BigToggleButton(visible, "show", "hide");
        if (visible)
        {
            radius = SimpleSlider("scale", radius, 0, 10);
            distance = SimpleSlider("distance", distance, 0, 10);
            thickness = SimpleSlider("thickness", thickness, 0, 10);
            alpha = SimpleSlider("alpha", alpha, 0, 1);

            blendMode = UI_Tools.EnumGrid<ShapesBlendMode>("blend", blendMode);
        }
    }

    void Log(string str)
    {
        K2D2_Plugin.logger.LogInfo(str);
    }


    void onDrawShape()
    {
        if (!visible)
            return;

        VesselComponent vessel_component = K2D2_Plugin.Instance.current_vessel.VesselComponent;
        DrawVesselCenter(vessel_component);
    }

    void DrawVesselCenter(VesselComponent vessel_component)
    {
        var frame = vessel_component.transform.coordinateSystem;
        Vector3 CenterOfMass = frame.ToLocalPosition(vessel_component.CenterOfMass);

        Log($"center of mass : { StrTool.Vector3ToString(CenterOfMass)}");


        Position center = vessel_component.CenterOfMass;
        Position start = vessel_component.CenterOfMass + vessel_component.transform.up * distance;
        Position end = start + vessel_component.transform.up * 10;

        Vector3 localStart = frame.ToLocalPosition(start);
        Vector3 localEnd = frame.ToLocalPosition(end);

        Vector3 direction = localEnd - localStart;

        //Draw.Sphere(CenterOfMass, scale, Color.red);

        var rot = Quaternion.LookRotation(direction);

        Color color = Color.red;
        color.a = alpha;

        Draw.Torus(blendMode, ThicknessSpace.Meters, ThicknessSpace.Meters, localStart, rot, radius, thickness, color);
        Draw.Line(blendMode, LineGeometry.Volumetric3D, LineEndCap.Round, ThicknessSpace.Meters, localStart, localEnd, color, color, thickness);

        //SpatialShapes.DrawTorus(forwardPoint, direction, radius, thickness, Color.cyan);
    }

}