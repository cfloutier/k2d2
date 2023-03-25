
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

        // Must be implemented, called every frame
        // should check status and set finished to true when tasks is done
        public virtual void onUpdate()
        {
            throw new System.NotImplementedException();
        }


        // Fixed Update is called every physic frame
        public virtual void FixedUpdate()
        {

            // FixedUpdate is Optionnal
        }

        public virtual void LateUpdate()
        {
            // LateUpdate is Optionnal
        }

        public virtual void onGui()
        {
            // GUI is Optionnal
        }
    }


    public class ManeuvrePilot : BasePilot
    {
        protected ManeuverNodeData maneuver = null;

        public void setManeuver(ManeuverNodeData maneuver)
        {
            this.maneuver = maneuver;
        }
    }
}