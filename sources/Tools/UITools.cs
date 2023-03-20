using UnityEngine;
using System.Text.RegularExpressions; 

namespace K2D2
{
    /// <summary>
    /// A set of simple tools for UI
    /// </summary>
    public class UI_Tools
    {
        public static int LayoutIntSlider(string txt, int value, int min, int max )
        {
            GUILayout.Label(txt + $" : {value}");
            value = (int) GUILayout.HorizontalSlider((int) value, min, max);
            return value;
        }


        public static int LayoutIntField(int value)
        {
            string text = value.ToString();

            text = GUILayout.TextField(text);
            text = Regex.Replace(text, @"[^a-zA-Z0-9 ]", "");

            return int.Parse(text);
        }
    }

}