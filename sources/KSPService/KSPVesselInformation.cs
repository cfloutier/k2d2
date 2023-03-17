using KSP.Game;
using KSP.Sim;
using UnityEngine;

namespace KSP2FlightAssistant.KSPService
{
    public class KSPVesselInformation
    {
        public TelemetryDataProvider TelemetryDataProvider { get; set; }
        public bool IsInitialized = false;
        
        //Game.ViewController.DataProvider.TelemetryDataProvider.NAVBallRotation.GetValue().z
            
        public KSPVesselInformation()
        {
            
        }
        
        public void Initialize(GameInstance game)
        {
            TelemetryDataProvider = game.ViewController.DataProvider.TelemetryDataProvider;
            
            IsInitialized = true;
        }
        
        public void Destroy()
        {
            TelemetryDataProvider = null;
            
            IsInitialized = false;
        }
        
        public Vector3 GetManeuverNodeVector()
        {
            return TelemetryDataProvider.ManeuverMarkerVector.GetValue();

        }
        

        
    }
}