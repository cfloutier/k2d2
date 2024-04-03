// This script attaches the tabbed menu logic to the game.
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

using K2UI;
using K2UI.Tabs;
using KTools;

namespace K2D2.UI.Tests
{
    public class TestDropField : MonoBehaviour
    {
      



        // public List<Panel> panels;
        private void OnEnable()
        {

        }

        DropdownField drop;

        private void Start()
        {
            UIDocument menu = GetComponent<UIDocument>();
            VisualElement root = menu.rootVisualElement;
           
            drop = root.Q<DropdownField>();


       
        }

        public void Update()
        {
           
        }
    }
}