﻿using System;
using System.Collections.Generic;

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
        
        
        public string StartManeuver()
        {
            return _customQueue.HasNext() ? _customQueue.PopAndRun() : "No more Maneuvers";
        }
        
        public List<string> GetDescriptionOfAllManeuvers()
        {
            return _customQueue.ViewQueue();
        }
        
        public bool HasNext()
        {
            return _customQueue.HasNext();
        }
        
    }
}