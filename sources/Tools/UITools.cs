using UnityEngine;
using System.Text.RegularExpressions; 
using System.Collections.Generic;

namespace K2D2
{
    /// <summary>
    /// A set of simple tools for UI
    /// </summary>
    public class UI_Tools
    {
        public static bool BigButton(string txt, bool is_on = false)
        {
            int width_bt = 200;
            int height_bt = 40;

            return GUILayout.Button(txt, GUILayout.Height(width_bt), GUILayout.Height(height_bt));
        }

        public static int IntSlider(string txt, int value, int min, int max )
        {
            GUILayout.Label(txt + $" : {value}");
            value = (int) GUILayout.HorizontalSlider((int) value, min, max);
            return value;
        }

        public static float FloatSlider(string txt, float value, float min, float max )
        {
            GUILayout.Label(txt + $" : {value:n2}");
            value = GUILayout.HorizontalSlider( value, min, max);
            return value;
        }

        public static Dictionary<string, string> temp_dict = new Dictionary<string, string>();

        /// Simple Integer Field. for the moment there is a trouble. keys are sent to KSP2 enven if focus is in the field
        public static int IntField(string name, int value, int min, int max)
        {
            string text = value.ToString();

            if (temp_dict.ContainsKey(name))
                // always use temp value
                text = temp_dict[name];

            GUILayout.BeginHorizontal();

            var typed_text = GUILayout.TextField(text);
            typed_text = Regex.Replace(typed_text, @"[^\d-]+", "");

            // save filtered temp value
            temp_dict[name] = typed_text;

            int result = value;
            bool ok = true;
            if (!int.TryParse(typed_text, out result))
            {
                ok = false;
            }
            if (result < min) {
                ok = false;
                result = value;
            }
            else if (result > max) {
                ok = false;
                result = value;
            }

            if (ok)
                GUILayout.Label("", GUILayout.Width(30));
            else
                GUILayout.Label("X", GUILayout.Width(30));

            GUILayout.EndHorizontal();

            return result;
        }

        // public static int LayoutIntField(int value)
        // {
        //     string text = value.ToString();

        //     text = GUILayout.TextField(text);
        //     text = Regex.Replace(text, @"[^a-zA-Z0-9 ]", "");

        //     return int.Parse(text);
        // }


    }

   

}