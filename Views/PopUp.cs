using System;
using System.Collections.Generic;
using SpaceWarp.API.UI;
using UniLinq;
using UnityEngine;

namespace K2D2
{
    public class PopUp
    {
        public Rect PopupRect { get; set; }

        public bool isPopupVisible = false;
        private int currentContentIndex = 0;


        public FunctionObject currentDisplayContent { get; set; }

        //private static Dictionary<string, Delegate> _popUpContent = new Dictionary<string, Delegate>();
        private Dictionary<string, FunctionObject> _popUpContent = new Dictionary<string, FunctionObject>();

        
        public void AddPopUpContents(string tabDescription, Delegate guiFunction, params object[] parameters )
        {
            FunctionObject contentFunctionObject = new FunctionObject(guiFunction, parameters);
            AddPopUpContents(tabDescription, contentFunctionObject);
            
        }
        
        private void AddPopUpContents(string tabDescription, FunctionObject guiFunctionObject)
        {
            _popUpContent.Add(tabDescription, guiFunctionObject);
        }


        public PopUp()
        {
            
            PopupRect = new Rect(Screen.width / 2 - 175, Screen.height / 2 - 250, 350, 500);
        }

        public void TogglePopup(bool toggle)
        {
            isPopupVisible = toggle;
        }

        public void OnGUI()
        {
            if (isPopupVisible)
            {
                GUI.skin = Skins.ConsoleSkin;
                Styles.Init();

                PopupRect = GUILayout.Window(
                    GUIUtility.GetControlID(FocusType.Passive),
                    PopupRect,
                    FillPopup,
                    "<color=#00D346>K2-D2</color>",
                    Styles.window,
                    GUILayout.Height(0),
                    GUILayout.Width(350));
            }
        }

        private static void DefaultPopup()
        {
            GUILayout.Label("K2-D2 PopUp");
        }
        
        private static void TestPopup()
        {
            GUILayout.Label("Test PopUp");
        }


        public void DrawPopup()
        {
            currentDisplayContent.Run();
        }

        private void FillPopup(int windowID)
        {
            // Print the icon
            GUI.Label(new Rect(15, 2, 29, 29), Styles.big_icon, Styles.icons_label);

            if (GUI.Button(new Rect(PopupRect.width - 30, 4, 25, 25), "X", Styles.small_button))
                TogglePopup(false);

            GUILayout.BeginHorizontal();

            // TODO: Make this less stupid
            string[] keys = _popUpContent.Keys.ToArray();
            currentContentIndex = GUILayout.SelectionGrid(currentContentIndex, keys, _popUpContent.Count());
            string key = keys[currentContentIndex];
            currentDisplayContent = _popUpContent[key];

            GUILayout.EndHorizontal();

            GUILayout.BeginVertical();
            if (currentDisplayContent != null)
                DrawPopup();
            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, 10000, 500));
        }
    }
}