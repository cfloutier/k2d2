
using K2D2.KSPService;
using KTools.UI;
using BepInEx.Logging;
using UnityEngine;

using KSP.Sim.impl;
using KSP.Game;
using KSP.Sim;
using KSP;

using K2D2.Controller.Docks;

namespace K2D2.Controller;

using JetBrains.Annotations;
using KTools;
using Shapes;

public class DockingAssist : ComplexControler
{
    public static DockingAssist Instance { get; set; }
    public ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.DockingTool");

    KSPVessel current_vessel;

    public DockingAssist()
    {
        Instance = this;
        debug_mode_only = false;
        name = "Dock";

        current_vessel = K2D2_Plugin.Instance.current_vessel;

        shape = new DockShape();
    }

    DockShape shape;

    UI_Mode ui_mode = UI_Mode.Main;

    Vector2 scroll_pos = Vector2.one;

    public override void onGUI()
    {
        if (K2D2_Plugin.Instance.settings_visible)
        {
            // default Settings UI
            K2D2Settings.onGUI();
            // settingsUI();
            return;
        }

        if (ui_mode == UI_Mode.Main)
        {
            UI_Tools.Title("Docking Tools");
            GUILayout.BeginHorizontal();

            UI_Tools.Label("Target : ");

            if (UI_Tools.SmallButton(target_name))
            {
                ui_mode = UI_Mode.Select_Target;
            }
            if (target_vessel != null)
            {
                if (UI_Tools.SmallButton("Dock"))
                {
                    ui_mode = UI_Mode.Select_Dock;
                }
            }

            GUILayout.EndHorizontal();
        }
        else if (ui_mode == UI_Mode.Select_Target)
        {
            UI_Tools.Title("Select Target");

            var body = current_vessel.currentBody();

            var allVessels = GameManager.Instance.Game.SpaceSimulation.UniverseModel.GetAllVessels();
            allVessels = GameManager.Instance.Game.SpaceSimulation.UniverseModel.GetAllVessels();
            allVessels.Remove(current_vessel.VesselComponent);
            allVessels.RemoveAll(v => v.IsDebris());
            allVessels.RemoveAll(v => v.mainBody != body);

            if (allVessels.Count < 1)
            {
                GUILayout.Label("No other vessels orbiting the planet");
            }

            if (allVessels.Count > 10)
                scroll_pos = GUILayout.BeginScrollView(scroll_pos);
            else
                GUILayout.BeginVertical();

            foreach (var vessel in allVessels)
            {
                if (UI_Tools.SmallButton(vessel.Name))
                {
                    current_vessel.VesselComponent.SetTargetByID(vessel.GlobalId);
                    current_vessel.VesselComponent.TargetObject = vessel.SimulationObject;
                    ui_mode = UI_Mode.Main;
                }
            }

            if (allVessels.Count > 10)
                GUILayout.EndScrollView();
            else
                GUILayout.EndVertical();

            if (UI_Tools.SmallButton("None"))
            {
                ui_mode = UI_Mode.Main;
                current_vessel.VesselComponent.ClearTarget();
            }

            if (UI_Tools.SmallButton("Cancel"))
            {
                ui_mode = UI_Mode.Main;
            }
        }
        else if (ui_mode == UI_Mode.Select_Dock)
        {
            UI_Tools.Title("Select Dock");
            if (target_vessel == null)
            {
                ui_mode = UI_Mode.Main;
                return;
            }
            UI_Tools.Console("vessel : " + target_vessel.Name);

            // foreach(var part in docks)
            // {
            //     UI_Tools.Console(part.Name + " - " + part.Type.Name);
            // }

            shape.draw_ui();

            if (UI_Tools.SmallButton("Cancel"))
            {
                ui_mode = UI_Mode.Main;
            }
        }
    }

    enum UI_Mode
    {
        Main,
        Select_Target,
        Select_Dock,
    }

    string target_name = "None";
    VesselComponent target_vessel;
    PartComponent target_part;
    public SimulationObjectModel last_target;

    public List<PartComponent> docks = new List<PartComponent>();

    public override void Update()
    {
        // logger.LogInfo($"target is {current_vessel.VesselComponent.TargetObject}");
        if (current_vessel.VesselComponent == null)
            return;

        if (last_target != current_vessel.VesselComponent.TargetObject)
        {
            logger.LogInfo($"changed target is {current_vessel.VesselComponent.TargetObject}");

            last_target = current_vessel.VesselComponent.TargetObject;

            if (last_target == null)
            {
                target_vessel = null;
                target_name = "None";
                docks.Clear();
            }
            else if (last_target.IsCelestialBody)
            {
                target_vessel = null;
                target_name = last_target.Name;
                docks.Clear();
            }
            else if (last_target.IsVessel)
            {
                // logger.LogInfo(last_target);
                target_name = last_target.Name;
                target_vessel = last_target.Vessel;
                docks = DockTools.ListDocks(target_vessel);
            }
            else if (last_target.IsPart)
            {
                target_part = last_target.Part;

                PartOwnerComponent owner = target_part.PartOwner;
                if (owner.SimulationObject.IsVessel)
                {
                    target_vessel = last_target.Vessel;
                    target_name = target_vessel.Name +"." + last_target.Name;
                }
            }
            else
            {
                target_vessel = null;
                target_name = "None";
            }
        }
    }

    public void drawShapes()
    {
        // logger.LogInfo("drawShapes");
        if (ui_mode == UI_Mode.Select_Dock)
        {
            foreach(var part in docks)
            {
                shape.draw_dockPort(part, current_vessel.VesselComponent);
            }

            //shape.draw_dockPort(part, current_vessel.VesselComponent);

            shape.DrawVesselCenter(current_vessel.VesselComponent);
        }
    }
}

public class ColorEditor
{
    public float h= 0;
    public float s = 1;
    public float v = 1;

    public void draw_ui(string label)
    {
        GUILayout.Label(label, KBaseStyle.console_text);
        h = UI_Tools.LabelSlider("Hue", h, 0, 1 );
        s = UI_Tools.LabelSlider("Sat", s, 0, 1 );
        v = UI_Tools.LabelSlider("Val", v, 0, 1 );

        UI_Tools.Console(ColorTools.formatColorHtml(color));
    }

    public Color color
    {
        get
        {
            return ColorTools.FromHSV(h, s ,v, 1);
        }
    }
}

class DockShape
{
    ShapesBlendMode blendMode = ShapesBlendMode.Additive;

    float delta_pos = 1;
    float length = 15;

    float radius = 1;

    float alpha = 3.5f;

    float thickness_torus = 0.15f;
    float thickness_line = 0.15f;

    ColorEditor color_editor = new ColorEditor();

    Color dock_color = ColorTools.parseColor("#CB5B00");
    Color vessel_color = ColorTools.parseColor("#00FF34");

    public void draw_ui()
    {
        delta_pos = UI_Tools.LabelSlider("delta_pos", delta_pos, -10, 10 );

        length = UI_Tools.LabelSlider("length", length, 0, 100 );
        radius = UI_Tools.LabelSlider("radius", radius, 0, 100 );

        alpha = UI_Tools.LabelSlider("alpha", alpha, 0, 10 );
        thickness_line = UI_Tools.LabelSlider("thickness_line", thickness_line, 0, 0.5f);

        color_editor.draw_ui("Color");
    }

    public void draw_dockPort(PartComponent part, VesselComponent main_vessel)
    {
        // var frame = part.transform.coordinateSystem;

        Position center = part.CenterOfMass;
        Position start = center + part.transform.up * delta_pos;
        Position end = start + part.transform.up * length;

        var local_frame = main_vessel.transform.coordinateSystem;

        Vector3 localStart = local_frame.ToLocalPosition(start);
        Vector3 localEnd = local_frame.ToLocalPosition(end);

        Vector3 direction = localEnd - localStart;

        //Draw.Sphere(CenterOfMass, scale, Color.red);

        var rot = Quaternion.LookRotation(direction.normalized);

        Color color = dock_color;
        color.a = alpha;

        float radius = (float) (part.PartData.PartSizeDiameter/2);;

        Draw.Torus(blendMode, ThicknessSpace.Meters, ThicknessSpace.Meters, localStart, rot, radius, thickness_torus, color);
        Draw.Line(blendMode, LineGeometry.Volumetric3D, LineEndCap.Round, ThicknessSpace.Meters, localStart, localEnd, color, color, thickness_line);
    }

    public void DrawVesselCenter(VesselComponent vessel_component)
    {
        var frame = vessel_component.transform.coordinateSystem;
        Vector3 CenterOfMass = frame.ToLocalPosition(vessel_component.CenterOfMass);

        // Log($"center of mass : { StrTool.Vector3ToString(CenterOfMass)}");

        // float radius = 1;

        float distance = 3;

        // float thickness = 0.1f;

        // float alpha = 1;

       // ShapesBlendMode blendMode = ShapesBlendMode.Opaque;

        Position center = vessel_component.CenterOfMass;
        Position start = vessel_component.CenterOfMass + vessel_component.transform.up * distance;
        Position end = start + vessel_component.transform.up * length;

        Vector3 localStart = frame.ToLocalPosition(start);
        Vector3 localEnd = frame.ToLocalPosition(end);

        Vector3 direction = localEnd - localStart;

        //Draw.Sphere(CenterOfMass, scale, Color.red);

        var rot = Quaternion.LookRotation(direction);

        Color color = color_editor.color;
        color.a = alpha;

        //L.Vector3("position vessel", localStart);

        Draw.Torus(blendMode, ThicknessSpace.Meters, ThicknessSpace.Meters, localStart, rot, radius, thickness_torus, color);
        Draw.Line(blendMode, LineGeometry.Volumetric3D, LineEndCap.Round, ThicknessSpace.Meters, localStart, localEnd, color, color, thickness_torus);

        //SpatialShapes.DrawTorus(forwardPoint, direction, radius, thickness, Color.cyan);
    }
}

