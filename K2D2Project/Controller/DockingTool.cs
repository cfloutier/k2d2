
using K2D2.KSPService;
using KTools.UI;
using BepInEx.Logging;
using UnityEngine;

using KSP.Sim.impl;
using KSP.Game;
using KSP.Sim;

namespace K2D2.Controller;

using Shapes;



public class DockingTool : ComplexControler
{
    public static DockingTool Instance { get; set; }
    public ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.DockingTool");


    KSPVessel current_vessel;

    public DockingTool()
    {
        Instance = this;
        debug_mode_only = false;
        name = "Dock";

        current_vessel = K2D2_Plugin.Instance.current_vessel;
    }


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
            UI_Tools.Console("vessel : " + target_vessel.Name);

            foreach(var part in docks)
            {
                UI_Tools.Console(part.Name + " - " + part.Type.Name);
            }

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
                PartOwnerComponent owner = last_target.PartOwner;
                listDocks(owner);
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
        if (ui_mode == UI_Mode.Select_Dock)
        {
            foreach(var part in docks)
            {
                draw_dockPort(part);
            }
        }
    }




    void draw_dockPort(PartComponent part)
    {
        var frame = part.transform.coordinateSystem;

        float delta_pos = 1;
        float length = 100;
        float alpha = 5;

        Position center = part.CenterOfMass;
        Position start = center + part.transform.up * delta_pos;
        Position end = start + part.transform.up * length;

        var local_frame = current_vessel.VesselComponent.transform.coordinateSystem;

        Vector3 localStart = local_frame.ToLocalPosition(start);
        Vector3 localEnd = local_frame.ToLocalPosition(end);

        Vector3 direction = localEnd - localStart;

        var blendMode = ShapesBlendMode.Additive;

        //Draw.Sphere(CenterOfMass, scale, Color.red);

        var rot = Quaternion.LookRotation(direction.normalized);

        Color color = Color.red;
        color.a = alpha;

        Draw.Torus(blendMode, ThicknessSpace.Meters, ThicknessSpace.Meters, localStart, rot, 10, 0.1f, color);
        Draw.Line(blendMode, LineGeometry.Volumetric3D, LineEndCap.Round, ThicknessSpace.Meters, localStart, localEnd, color, color, 0.1f);
    }

    void listDocks(PartOwnerComponent owner)
    {
        docks.Clear();

        foreach(var part in owner.Parts)
        {
            if (part.Name.ToLower().Contains("dock"))
                docks.Add(part);
        }
    }
}