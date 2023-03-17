
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
    /// each pilot have a specific task like Auto warp or Auto Burn.
    /// * Start is called to init the pilot
    /// * onUpdate is called every frame
    /// * onGui is optionnal
    /// A pilot can be used with it's own ui or without.
    public class BasePilot
    {
        public bool finished = false;
        public string status_line = "";

        // called everytime the Pilot shoudl start
        public virtual void Start()
        {
            finished = false;
        }

        // Must be implemented
        // should check status and set finished to true when tasks is done
        public virtual void onUpdate()
        {
            throw new System.NotImplementedException();
        }

        public virtual void onGui()
        {
            // GUI is Optionnal 
        }
    }
}