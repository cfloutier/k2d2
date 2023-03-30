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
        
        public void ChangeOrbit(double periapsis, double apoapsis)
        {
            periapsis = _maneuver.AddRadiusOfBody(periapsis*1000);
            apoapsis = _maneuver.AddRadiusOfBody(apoapsis*1000);
            logger.LogMessage(apoapsis +" "+ periapsis);
            double currentPeriapsis = _maneuver.GetCurrentPeriapsis();
            double currentApoapsis = _maneuver.GetCurrentApoapsis();
            
            if (apoapsis < periapsis)
            {
                double temp = apoapsis;
                periapsis = apoapsis;
                apoapsis = temp;
            }
                
            logger.LogMessage(periapsis +" "+ apoapsis);
            if (!_maneuver.IsOrbitElliptic())
            {
                logger.LogMessage("Hyperbolic orbit");
                ManeuverManager.AddManeuver("Change Apoapsis",new Action(() => _maneuver.ChangeApoapsis(apoapsis)));

                if (apoapsis >currentPeriapsis)
                {
                    ManeuverManager.AddManeuver("Change Periapsis",new Action(() => _maneuver.ChangePeriapsis(periapsis)));
                }
                else
                {
                    ManeuverManager.AddManeuver("Change Apoapsis",new Action(() => _maneuver.ChangeApoapsis(periapsis)));
                }

                return;
            }
            
            if (_maneuver.IsApoapsisFirst())
            {
                logger.LogMessage("Apoapsis first");
                ManeuverManager.AddManeuver("Change Periapsis",new Action(() => _maneuver.ChangePeriapsis(periapsis)));
                if(periapsis>currentApoapsis)
                    ManeuverManager.AddManeuver("Change Periapapsis",new Action(() => _maneuver.ChangePeriapsis(apoapsis)));
                else
                    ManeuverManager.AddManeuver("Change Apoapsis",new Action(() => _maneuver.ChangeApoapsis(apoapsis)));

                return;

            }
            
            logger.LogMessage("Periapsis first");
            ManeuverManager.AddManeuver("Change Apoapsis",new Action(() => _maneuver.ChangeApoapsis(apoapsis)));
            if(apoapsis<currentPeriapsis)
                ManeuverManager.AddManeuver("Change Periapsis",new Action(() => _maneuver.ChangeApoapsis(periapsis)));
            else
                ManeuverManager.AddManeuver("Change Apoapsis",new Action(() => _maneuver.ChangePeriapsis(periapsis)));
        }
        
        
        
    }
}