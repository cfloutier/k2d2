using System;
using KSP.Sim;
using KSP.Sim.impl;
using KSP.Sim.Maneuver;
using KSP.Sim.State;
using KSP.Game;

using System.Reflection;

namespace K2D2
{
    // TODO : to merge with KSPVessel
    class VesselInfos__
    {
        static public VesselComponent currentVessel()
        {
            return GeneralTools.Game.ViewController?.GetActiveSimVessel();
        }

        static public VesselVehicle currentVehicle()
        {
            if (GeneralTools.Game.ViewController == null) return null;
            if (!GeneralTools.Game.ViewController.TryGetActiveVehicle(out var vehicle)) return null;
            return vehicle as VesselVehicle;
        }

    }


}

