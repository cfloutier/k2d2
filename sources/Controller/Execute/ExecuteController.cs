
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

namespace K2D2.Controller
{

    /// base class for all Execute controller
    /// each Execute controller have a specific task like Auto warp or Auto Burn to achieve.
    /// * Start is called to init the pilot
    /// * Must implement some of the Update,LateUpdate...
    /// must set the finished when task is accomplished
    public class ExecuteController : BaseController
    {
        public bool finished = false;
        public string status_line = "";

        // called everytime the Pilot shoudl start
        public virtual void Start()
        {
            finished = false;
        }
    }

    public class ManeuvreController : ExecuteController
    {
        protected ManeuverNodeData maneuver = null;

        public void setManeuver(ManeuverNodeData maneuver)
        {
            this.maneuver = maneuver;
        }
    }
}