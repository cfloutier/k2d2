using System;
using System.Collections.Generic;
using K2D2.sources.Models;
using K2D2.sources.Models.BaseClasses;
using UnityEngine;

namespace K2D2
{
    public class PopUpContent
    {
        private Vector2 _scrollPositionManeuverList = Vector2.zero;
        private PopUp PopUp{get; set;}
        private double _scollViewManeuverWidth { get; set; }
        public PopUpContent(ref PopUp popUp)
        {
            this.PopUp = popUp;
            _scollViewManeuverWidth = PopUp.PopupRect.width-20;
            
        }
        public void DisplayManeuverList(ref ManeuverManager maneuverManager)
        {
            List<GuidTuple<FunctionObject>> allManeuvers = maneuverManager.GetManeuversAsGuidTupleList();
            

            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Maneuver List");
            GUILayout.EndHorizontal();
            
            // TODO: Make this a scrollable list
            _scrollPositionManeuverList = GUILayout.BeginScrollView(_scrollPositionManeuverList, GUILayout.Width((float)_scollViewManeuverWidth), GUILayout.Height(300));
            foreach (GuidTuple<FunctionObject> maneuver in allManeuvers)
            {
                GUILayout.BeginHorizontal();
                if(GUILayout.Button(maneuver.content.description))
                {
                    maneuverManager.RemoveManeuver(maneuver.guid);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Start Maneuver"))
            {
                maneuverManager.StartManeuver();
            }
            GUILayout.EndHorizontal();
        }
        
        
    }
}