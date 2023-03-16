
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using KSP.Sim;
using KSP.Sim.impl;
using KSP.Sim.Maneuver;
using KSP.Game;
using KSP.Sim.ResourceSystem;



using BepInEx.Logging;
using KSP.ScriptInterop.impl.moonsharp;

namespace K2D2
{
    /// base class for all pilot tools
    // it includes update call and gui
    public class BasePilot
    {
        public bool finished = false;
        public string status_line = "";

        public virtual void Start()
        {
            finished = false;
        }

        public virtual void onUpdate()
        {
            throw new System.NotImplementedException();
        }

        public virtual void onGui()
        {
            throw new System.NotImplementedException();
        }
    }
}