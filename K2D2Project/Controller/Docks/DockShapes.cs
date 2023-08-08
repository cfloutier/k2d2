using Shapes;
using UnityEngine;

namespace K2D2.Controller;
using KTools.UI;
using KTools;
using KSP.Sim.impl;
using KSP.Sim;
using K2D2.Controller.Docks;

class DockShape
{
    DocksSettings settings = null;

    public DockShape(DocksSettings settings)
    {
        this.settings = settings;
    }

    ShapesBlendMode blendMode = ShapesBlendMode.Additive;

    //ColorEditor color_editor = new ColorEditor();

    public void DrawPartComponent(PartComponent part, VesselComponent main_vessel, Color color)
    {
        // var frame = part.transform.coordinateSystem;

        Position center = part.CenterOfMass;
        Position start = center + part.transform.up * settings.pos_gizmo_dock;
        Position end = start + part.transform.up * settings.length_line;

        var local_frame = main_vessel.transform.coordinateSystem;

        Vector3 localStart = local_frame.ToLocalPosition(start);
        Vector3 localEnd = local_frame.ToLocalPosition(end);

        Vector3 direction = localEnd - localStart;

        var rot = Quaternion.LookRotation(direction.normalized);

        color.a = settings.sfx_blur;

        float radius = (float) (part.PartData.PartSizeDiameter/2);;

        Draw.Torus(blendMode, ThicknessSpace.Meters, ThicknessSpace.Pixels, localStart, rot, radius, settings.thickness_circle, color);
        Draw.Line(blendMode, LineGeometry.Volumetric3D, LineEndCap.Round, ThicknessSpace.Pixels, localStart, localEnd, color, color, settings.thickness_line);
    }

    public void DrawVesselCenter(VesselComponent vessel_component, Color color)
    {
        var frame = vessel_component.transform.coordinateSystem;
        Vector3 CenterOfMass = frame.ToLocalPosition(vessel_component.CenterOfMass);

        float distance = 3;

        Position center = vessel_component.CenterOfMass;
        Position start = vessel_component.CenterOfMass + vessel_component.transform.up * distance;
        Position end = start + vessel_component.transform.up * settings.length_line;

        Vector3 localStart = frame.ToLocalPosition(start);
        Vector3 localEnd = frame.ToLocalPosition(end);

        Vector3 direction = localEnd - localStart;

        //Draw.Sphere(CenterOfMass, scale, Color.red);

        var rot = Quaternion.LookRotation(direction);

        color.a = settings.sfx_blur;

        float radius_torus = 1;

        //L.Vector3("position vessel", localStart);

        Draw.Torus(blendMode, ThicknessSpace.Meters, ThicknessSpace.Pixels, localStart, rot, radius_torus, settings.thickness_circle, color);
        Draw.Line(blendMode, LineGeometry.Volumetric3D, LineEndCap.Round, ThicknessSpace.Pixels, localStart, localEnd, color, color, settings.thickness_line);

        //SpatialShapes.DrawTorus(forwardPoint, direction, radius, thickness, Color.cyan);
    }
}

