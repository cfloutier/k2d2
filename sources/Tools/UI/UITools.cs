using UnityEngine;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using KSP.Game;

namespace K2D2
{
    public class TopButtons
    {
        static Rect position = Rect.zero;
        const int space = 25;

        /// <summary>
        /// Must be called before any Button call
        /// </summary>
        /// <param name="widthWindow"></param>
        static public void Init(float widthWindow)
        {
            position = new Rect(widthWindow - 5, 4, 23, 23);
        }

        static public bool Button(string txt)
        {
            position.x -= space;
            return GUI.Button(position, txt, Styles.small_button);
        }
        static public bool Button(Texture2D icon)
        {
            position.x -= space;
            return GUI.Button(position, icon, Styles.small_button);
        }

        static public bool Toggle(bool value, string txt)
        {
            position.x -= space;
            return GUI.Toggle(position, value, txt, Styles.small_button);
        }

        static public bool Toggle(bool value, Texture2D icon)
        {
            position.x -= space;
            return GUI.Toggle(position, value, icon, Styles.small_button);
        }
    }

    /// <summary>
    /// A set of simple tools for UI
    /// </summary>
    /// TODO : remove static, make it singleton
    public class UI_Tools
    {
        public static bool Toggle(bool is_on, string txt, string tooltip = null)
        {
            if (tooltip != null)
                return GUILayout.Toggle(is_on, new GUIContent(txt, tooltip), Styles.toggle);
            else 
                return GUILayout.Toggle(is_on, txt, Styles.toggle);
        }

        public static bool ToggleButton(bool is_on, string txt_off, string txt_on)
        {
            int width_bt = 200;
            int height_bt = 40;

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var txt = is_on ? txt_on : txt_off;

            is_on = GUILayout.Toggle(is_on, txt, Styles.big_button, GUILayout.Width(width_bt), GUILayout.Height(height_bt));

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            return is_on;
        }

        public static bool BigButton(string txt, bool is_on = false)
        {
            int width_bt = 200;
            int height_bt = 40;

            return GUILayout.Button(txt, GUILayout.Height(width_bt), GUILayout.Height(height_bt));
        }

        public static void Title(string txt)
        {
            GUILayout.Label($"<b>{txt}</b>", Styles.title);
        }

        public static void Label(string txt)
        {
            GUILayout.Label(txt, Styles.label);
        }

        public static void Console(string txt)
        {
            GUILayout.Label(txt, Styles.console_text);
        }

        public static int IntSlider(string txt, int value, int min, int max )
        {
            GUILayout.Label(txt + $" : {value}");
            value = (int) GUILayout.HorizontalSlider((int) value, min, max, Styles.slider_line, Styles.slider_node);
            return value;
        }

        public static float FloatSlider(string txt, float value, float min, float max, string unity_txt = "")
        {
            // simple float slider with a lavel value

            string content = txt + $" : {value:n2}" + unity_txt;

            GUILayout.Label(content, Styles.slider_text);
            value = GUILayout.HorizontalSlider( value, min, max, Styles.slider_line, Styles.slider_node);
            return value;
        }

        public static void Right_Left_Text(string right_txt, string left_txt)
        {
            // text aligned to right and left with a space in between
            GUILayout.BeginHorizontal();
            UI_Tools.Console(right_txt);
            GUILayout.FlexibleSpace();
            UI_Tools.Console(left_txt);
            GUILayout.EndHorizontal();
        }

        public static Dictionary<string, string> temp_dict = new Dictionary<string, string>();
        public static List<string> inputFields = new List<string>();
        static bool gameInputState = true;

        static public void CheckEditor()
        {
            if (gameInputState && inputFields.Contains(GUI.GetNameOfFocusedControl()))
            {
                // Logger.LogInfo($"[Flight Plan]: Disabling Game Input: Focused Item '{GUI.GetNameOfFocusedControl()}'");
                gameInputState = false;
                // game.Input.Flight.Disable();
                GameManager.Instance.Game.Input.Disable();
            }
            else if (!gameInputState && !inputFields.Contains(GUI.GetNameOfFocusedControl()))
            {
                // Logger.LogInfo($"[Flight Plan]: Enabling Game Input: FYI, Focused Item '{GUI.GetNameOfFocusedControl()}'");
                gameInputState = true;
                // game.Input.Flight.Enable();
                GameManager.Instance.Game.Input.Enable();
            }
        }

        /// Simple Integer Field. for the moment there is a trouble. keys are sent to KSP2 enven if focus is in the field
        public static int IntField(string name, int value, int min, int max, string tooltip)
        {
            string text = value.ToString();

            if (temp_dict.ContainsKey(name))
                // always use temp value
                text = temp_dict[name];

            if (!inputFields.Contains(name))
                inputFields.Add(name); 

            GUILayout.BeginHorizontal();

            GUI.SetNextControlName(name);
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


            if (!string.IsNullOrEmpty(tooltip))
            {
                GUILayout.Button(new GUIContent("?", tooltip), GUILayout.Width(20));
            }
            

            GUILayout.EndHorizontal();

            return result;
        }


    }



}