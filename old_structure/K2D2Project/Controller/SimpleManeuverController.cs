﻿
using BepInEx.Logging;
using UnityEngine;
using ManeuverProvider = K2D2.sources.KSPService.ManeuverProvider;

#pragma warning disable CS0414

using KTools;

namespace K2D2.Controller;

public class SimpleManeuverController : ButtonController
{

    ManualLogSource logger;
    public Rect windowRect { get; set; }

    public static SimpleManeuverController Instance { get; set; }

    //private bool _circularizeApoapsis = false, _circularizePeriapsis = false, _hohmannTransfer = false;
    private string distanceHohmannS = "0", timeHohmannS = "0";
    double distanceHohmann = 0, timeHohmann = 0;
    string periapsisS = "0", apoapsisS = "0";

    private ManeuverProvider _maneuverProvider;

    public SimpleManeuverController(ref ManeuverProvider maneuverProvider)
    {
        debug_mode_only = true;
        name = "Simple Maneuver Controller";
        //K2D2PilotsMgr.Instance.RegisterPilot("Circularize", this);


        _maneuverProvider = maneuverProvider;
        Instance = this;
    }

    public SimpleManeuverController(ManualLogSource logger, ref ManeuverProvider maneuverProvider) : this(ref maneuverProvider)
    {
        this.logger = logger;
        logger.LogMessage("SimpleManeuverController !");
    }


    public override void Update()
    {
    }

    public override void onGUI()
    {
        if (K2D2_Plugin.Instance.settings_visible)
        {
            K2D2Settings.onGUI();
            return;
        }

        if (_maneuverProvider == null)
        {
            return;
        }

        if (GUILayout.Button("Circularize Node in Apoapsis"))
        {
            _maneuverProvider.CircularizeApoapsis();
            if (!K2D2Settings.debug_mode)
                _maneuverProvider.ManeuverManager.StartManeuver();
        }


        if (GUILayout.Button("Circularize Node in Periapsis"))
        {
            _maneuverProvider.CircularizePeriapsis();
            if (!K2D2Settings.debug_mode)
                _maneuverProvider.ManeuverManager.StartManeuver();
        }

        if (GUILayout.Button("Circularize Hyperbolic Orbit"))
        {
            _maneuverProvider.CircularizeHyperbolicOrbit();
            if (!K2D2Settings.debug_mode)
                _maneuverProvider.ManeuverManager.StartManeuver();
        }


        GUILayout.Label("Periapsis (km):");
        periapsisS = GUILayout.TextField(periapsisS);
        GUILayout.Label("Apoapsis (km):");
        apoapsisS = GUILayout.TextField(apoapsisS);

        if (GUILayout.Button("Change Orbit"))
        {
            logger.LogMessage(GeneralTools.GetNumberString(periapsisS));
            logger.LogMessage(GeneralTools.GetNumberString(apoapsisS));
            _maneuverProvider.ChangeOrbit(GeneralTools.GetNumberString(periapsisS), GeneralTools.GetNumberString(apoapsisS));
            if (!K2D2Settings.debug_mode)
                _maneuverProvider.ManeuverManager.StartManeuver();
            return;
        }

        GUILayout.Label("Hohmann Transfer Distance (km):");
        distanceHohmannS = GUILayout.TextField(distanceHohmannS);
        GUILayout.Label("Hohmann Transfer Time (s):");
        timeHohmannS = GUILayout.TextField(timeHohmannS);



        if (GUILayout.Button("Hohmann Transfer"))
        {
            distanceHohmann = GeneralTools.GetNumberString(distanceHohmannS);
            timeHohmann = GeneralTools.GetNumberString(timeHohmannS);
            if (distanceHohmann < 0 || timeHohmann < 0)
            {
                GUILayout.Label("Invalid input");
                logger.LogError("Invalid input: Hohmann Transfer");
                return;
            }

            try
            {


                double UT = timeHohmann + Game.UniverseModel.UniverseTime;
                logger.LogMessage("Hohmann Time: " + UT);

                //double deltaV = _maneuver.ChangePeriapsis(Math.Abs(distanceHohmann));//_maneuver.HohmannTransfer(UT, distance);

                //GUILayout.Label($"Required dV: {deltaV}");

            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
            }
        }

        GUILayout.Label("Orbit Periapsis (km):");
        periapsisS = GUILayout.TextField(periapsisS);
        GUILayout.Label("Orbit Apoapsis (km):");
        apoapsisS = GUILayout.TextField(apoapsisS);

        if (GUILayout.Button("Set Orbit"))
        {
            double periapsis = GeneralTools.GetNumberString(periapsisS);
            double apoapsis = GeneralTools.GetNumberString(apoapsisS);

            if (periapsis < 0 || apoapsis < 0)
            {
                GUILayout.Label("Invalid input");
                logger.LogError("Invalid input: Set Orbit");
                return;
            }

            if (periapsis > apoapsis)
            {
                GUILayout.Label("Invalid input");
                logger.LogError("Invalid input: Set Orbit");
                return;
            }

            try
            {
                // _maneuver.ChangeApoapsis(2500000);
                // _maneuver.ChangePeriapsis(1500000);


                //_maneuver.CreateOrbit(apoapsis, periapsis);


            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
            }

        }

        //Run();
    }
}
