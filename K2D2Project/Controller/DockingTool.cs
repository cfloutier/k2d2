
using K2D2.KSPService;
using KTools;
using KTools.UI;
using BepInEx.Logging;
using UnityEngine;


using KSP.Sim.impl;
using KSP.Game;
using KSP.Sim.ResourceSystem;
using KSP.UI.Binding;


namespace K2D2.Controller;


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

            var target = current_vessel.VesselVehicle.Target;
            GUILayout.BeginHorizontal();

            UI_Tools.Label("Target : ");
            string name_bt = "Select";
            if (target != null)
            {
                name_bt = target.Name;
            }

            if (UI_Tools.SmallButton(name_bt))
            {
                ui_mode = UI_Mode.Select_Target;
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
                    current_vessel.VesselComponent.TargetObject = vessel.SimulationObject;
                    ui_mode = UI_Mode.Main;
                }
            }

            if (allVessels.Count > 10)
                GUILayout.EndScrollView();
            else
                GUILayout.EndVertical();

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

    public override void Update()
    {

    }


}