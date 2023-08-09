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

    public void DrawComponent(PartComponent part, VesselComponent main_vessel, Color color)
    {
        // var frame = part.transform.coordinateSystem;

        Position center = part.CenterOfMass;
        Position start = center + part.transform.up * settings.pos_gizmo;
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
}

