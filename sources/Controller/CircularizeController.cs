using System;
using System.Collections.Generic;
using AwesomeTechnologies.VegetationSystem;
using BepInEx.Logging;
using JetBrains.Annotations;
using UnityEngine;
using KSP.Game;
using K2D2.KSPService;
using K2D2;


namespace K2D2.Controller
{
    public class CircularizeController : BaseController
    {
        ManualLogSource logger;
        public static CircularizeController Instance { get; set; }
        
        public bool circularize = false;
        public bool printMyNameB = false;
        public bool printYourNameB = false;






        public KSPVessel vessel;

        public CircularizeController(ManualLogSource logger)
        {
            Instance = this;

            this.logger = logger;
            logger.LogMessage("CircularizeController !");
            
            buttons.Add(new Switch());
            buttons.Add(new Button());
        }
        









        public override void onGUI()
        {

            if(vessel==null)
            {
                this.vessel = new KSPVessel(Tools.Game());
                return;
            }
            GUILayout.Label(
                $"Circularize Node {vessel.GetDisplayAltitude()}");


            //logger.LogMessage("CircularizeController onGui !");

            if (GUILayout.Button("Circularize Node", Styles.button, GUILayout.Height(40)))
            {
                circularize = !circularize;
            }
            Run();
        }
    }

}