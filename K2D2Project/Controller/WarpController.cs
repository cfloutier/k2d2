using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using BepInEx.Logging;
using K2D2.KSPService;
using KSP.Sim;
using KSP.Sim.impl;

using K2D2.Controller;
using SpaceGraphicsToolkit;
using VehiclePhysics;
using System;

using K2D2.UI;

namespace K2D2.Controller;

public class WarpController : ComplexControler
{
    public static WarpController Instance { get; set; }

    KSPVessel current_vessel;

    public WarpController()
    {
        // logger.LogMessage("LandingController !");
        current_vessel = K2D2_Plugin.Instance.current_vessel;

        Instance = this;
        debug_mode = false;
        Name = "Warp";
        warp = new WarpTo();
    }

    public override void onReset()
    {
        isActive = false;
    }

    bool _active = false;
    public override bool isActive
    {
        get { return _active; }
        set
        {
            if (value == _active)
                return;

            if (!value)
            {
                TimeWarpTools.SetRateIndex(0, false);
                _active = false;
            }
            else
            {
                // reset controller to desactivate other controllers.
                K2D2_Plugin.ResetControllers();
                _active = true;
            }
        }
    }

    WarpTo warp = new WarpTo();

    public override void Update()
    {
        if (!isActive) return;

        warp.Update();
        if (warp.finished)
            isActive = false;
    }

    public override void onGUI()
    {
        if (K2D2_Plugin.Instance.settings_visible)
        {
            Settings.onGUI();
            UI_Tools.Title("No Warp Settings");
            if (UI_Tools.BigButton("close"))
                K2D2_Plugin.Instance.settings_visible = false;
            return;
        }
        var orbit = current_vessel.VesselComponent.Orbit;
        UI_Tools.Console($"timeToTransition1 : {StrTool.DurationToString( orbit.timeToTransition1 )}");
        UI_Tools.Console($"timeToTransition2 : {StrTool.DurationToString( orbit.timeToTransition1 )}");
        UI_Tools.Console($"PatchStartTransition : {orbit.PatchStartTransition }");
        UI_Tools.Console($"PatchEndTransition : {orbit.PatchEndTransition }");

        bool go = UI_Tools.ToggleButton(isActive, "Warp to SOI Change", "Stop");
        if (go != isActive && go)
        {
            if (go)
                warp.Start_Ut(orbit.timeToTransition1 + GeneralTools.Game.UniverseModel.UniversalTime + 60);

            isActive = go;
        }
      


      //  if (UI_Tools.BigButton("close"))
       

    }


}
