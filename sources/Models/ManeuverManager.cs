using System;

namespace K2D2.sources.Models
{
    public class ManeuverManager
    {
        private FunctionQueue _customQueue;
        
        public ManeuverManager()
        {
            _customQueue = new FunctionQueue();
            
        }
        
        public void AddManeuver(Delegate innerfunc, params object[] parameters)
        {
            _customQueue.Add(innerfunc, parameters);
        }
        
        
        public void StartManeuver()
        {
            if (_customQueue.HasNext())
            {
                _customQueue.Pop();
                return;
            }
        }
        
    }
}