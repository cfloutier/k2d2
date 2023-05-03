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

public class FindSecrets : BaseController
{
    KSPVessel current_vessel;

    public FindSecrets()
    {
        // logger.LogMessage("LandingController !");
        current_vessel = K2D2_Plugin.Instance.current_vessel;
        debug_mode_only = true;
        name = "Secrets";
    }

    public override void Update()
    {

    }

    bool lockedMode = false;
    bool reset = false;

    public override void onGUI()
    {
        var body = current_vessel.VesselComponent.mainBody;
        var transforms = body.transform.children;
        foreach (var child in transforms)
        {
            var pos = child.Position;
            var direction = pos - current_vessel.VesselComponent.transform.Position;
            var Upcoords = current_vessel.VesselVehicle.Up.coordinateSystem;

            var Surface_Dir = Vector.Reframed(direction, Upcoords).vector;
            var North = Vector.Reframed(current_vessel.VesselVehicle.North, Upcoords).vector;
            var Up = current_vessel.VesselVehicle.Up.vector;
            
            // var Up = Vector.Reframed(current_vessel.VesselVehicle.Up, Upcoords).vector;
            var heading = (float)-Vector3d.SignedAngle(Surface_Dir.normalized, North, Up);
            GUILayout.BeginHorizontal();
            UI_Tools.Console($"head. {heading}°");
            UI_Tools.Console($"dist. {StrTool.DistanceToString( Surface_Dir.magnitude)}");
            
            GUILayout.EndHorizontal();
            
        }


    }


}
