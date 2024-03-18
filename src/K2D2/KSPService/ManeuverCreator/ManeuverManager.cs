using System;
using System.Collections.Generic;
using K2D2.sources.Models.BaseClasses;

namespace K2D2.sources.Models
{
    public class ManeuverManager
    {
        private FunctionQueue _customQueue;
        
        public ManeuverManager()
        {
            _customQueue = new FunctionQueue();
            
        }
        
        public void AddManeuver(Delegate innerFunction, params object[] parameters)
        {
            _customQueue.Add(innerFunction, parameters);
        }
        
        public void AddManeuver(string description, Delegate innerFunction, params object[] parameters)
        {
            _customQueue.Add(description,innerFunction, parameters);
        }
        
        public void AddManeuver(string description, DetailsObject detailsObject, Delegate innerFunction, params object[] parameters)
        {
            _customQueue.Add(description, detailsObject, innerFunction, parameters);
        }
        
        
        public string StartManeuver()
        {
            return _customQueue.HasNext() ? _customQueue.PopAndRun() : "No more Maneuvers";
        }
        
        public List<FunctionObject> GetManeuversAsList()
        {
            return _customQueue.ToList();
        }
        
        public List<GuidTuple<FunctionObject>> GetManeuversAsGuidTupleList()
        {
            return _customQueue.ToGuidList();
        }

        public List<string> GetDescriptionOfAllManeuvers()
        {
            return _customQueue.ViewQueue();
        }
        

        
        public bool HasNext()
        {
            return _customQueue.HasNext();
        }
        
        public void Clear()
        {
            _customQueue.Clear();
        }
        
        public void RemoveManeuver(Guid guid)
        {
            _customQueue.RemoveElement(guid);
        }
        
        public void RemoveManeuverAndAllAfter(Guid guid)
        {
            _customQueue.RemoveElementAndAllAfter(guid);
        }

        
    }
}