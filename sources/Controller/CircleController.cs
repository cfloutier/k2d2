
using BepInEx.Logging;
using K2D2.Controller;
using K2D2.KSPService;
using KSP.Sim;
using KSP.Sim.impl;
using UnityEngine;
using KSP.Sim.Maneuver;
using System.Collections;
using KSP.Map;

namespace K2D2.Controller
{
    /// a simple test page to add the simple circle maneuvers node made by @mole
    class CircleController : BaseController
    {
        public ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.CircleController");

        ManeuverCreator maneuver_creator = new ManeuverCreator();

        public static CircleController Instance { get; set; }

        public CircleController()
        {
            Instance = this;
            debug_mode = false;
            Name = "Circle";
        }

        public override void Update()
        {
            maneuver_creator.Update();
        }

        public override void onGUI()
        {
            if (K2D2_Plugin.Instance.settings_visible)
            {
                Settings.onGUI();
                return;
            }

            if (UI_Tools.Button("Circularize At Ap"))
            {
                maneuver_creator.CircularizeOrbitApoapsis();
            }

            if (UI_Tools.Button("Circularize At Pe"))
            {
                maneuver_creator.CircularizeOrbitPeriapsis();
            }

            if (AutoExecuteManeuver.Instance.canStart())
            {
                if (UI_Tools.Button("Execute"))
                {
                    AutoExecuteManeuver.Instance.Start();
                }
            }
        }
    }
}