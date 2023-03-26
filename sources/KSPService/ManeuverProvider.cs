using System;
using BepInEx.Logging;
using K2D2.KSPService;
using K2D2.sources.Models;

namespace K2D2.sources.KSPService
{
    public class ManeuverProvider
    {
        private ManeuverManager ManeuverManager { get; set; }
        private Maneuver _maneuver { get; set; }
        
        private ManualLogSource logger;
        
        public ManeuverProvider(ref ManeuverManager maneuverManager)
        {
            ManeuverManager = maneuverManager;
            _maneuver = new Maneuver();
        }
        
        public ManeuverProvider(ref ManeuverManager maneuverManager, ManualLogSource logger)
        {
            ManeuverManager = maneuverManager;
            _maneuver = new Maneuver(logger);
            this.logger = logger;
            
        }
        
        public void Update()
        {
            _maneuver.Update();
        }
        
        public void CircularizeApoapsis()
        {
            ManeuverManager.AddManeuver("Circularize Periapsis",new Action(() => _maneuver.CircularizeOrbitPeriapsis()));

        }
        
        public void CircularizePeriapsis()
        {
            ManeuverManager.AddManeuver("Circularize Apoapsis",new Action(() => _maneuver.CircularizeOrbitApoapsis()));
        }
        
        public void CircularizeHyperbolicOrbit()
        {
            ManeuverManager.AddManeuver("Circularize Hyperbolic Orbit",new Action(() => _maneuver.CircularizeHyperbolicOrbit()));
        }
        
        
        
        
        
    }
}