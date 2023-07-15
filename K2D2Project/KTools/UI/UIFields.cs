using System.Collections.Generic;
using System.Globalization;
using KSP.Game;
using UnityEngine;

namespace KTools.UI
{
    internal class DoubleField
    {
        string current_text_Value;
        string entryName;
        double current_value;
        double last_parsed_value;
        public bool focus = false;
        bool valid = false;

        public float width = 100;

        string format = "0.##";

        public DoubleField(string entryName, double value, int precision, float width)
        {
            this.entryName = entryName;
            this.width = width;

            format = "0.";
            for (int i = 0; i < precision; i++)
            {
                format += "#";
            }

            current_text_Value = value.ToString(format, CultureInfo.InvariantCulture);
            current_value = value;
        }

        bool need_validate = false;

        public void Validate()
        {
            // UI_Fields.logger.LogInfo("Validate " + valid);
            if (valid)
            {
                need_validate = true;
            }
            else
            {
                // reset current value to the previous one
                current_text_Value = current_value.ToString(format, CultureInfo.InvariantCulture);
            }
        }

        public double OnGUI(double value)
        {
            Color normal = GUI.color;

            if (Event.current.type == EventType.Repaint && need_validate)
            {
                current_value = last_parsed_value;
                need_validate = false;
            }
            else
            {
                if (!focus)
                {
                    if (value != current_value)
                    {
                        current_text_Value = value.ToString(format, CultureInfo.InvariantCulture);
                        current_value = value;
                    }
                }
                else
                {
                    double num = value;
                    valid = double.TryParse(current_text_Value, NumberStyles.Any, CultureInfo.InvariantCulture, out num);
                    if (!valid)
                    {
                        GUI.color = Color.red;
                    }
                    else
                    {
                        GUI.color = Color.yellow;
                        last_parsed_value = num;
                    }
                }
            }

            GUI.SetNextControlName(entryName);
            string new_current_text_Value = GUILayout.TextField(current_text_Value, GUILayout.Width(width));

            current_text_Value = new_current_text_Value;
            // GUILayout.Label(current_text_Value);

            if (focus)
            {
                if ((Event.current.type == EventType.KeyDown) && (Event.current.character == '\n'))
                {
                    Validate();
                    GUI.FocusControl("");
                }
            }

            GUI.color = normal;
            return current_value;
        }
    }


    public class UI_Fields
    {
        internal static Dictionary<string, DoubleField> fields_dict = new Dictionary<string, DoubleField>();

        static bool _inputState = true;

        //public static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("KTools.UI_Fields");

        static public bool GameInputState
        {
            get { return _inputState; }
            set
            {
                if (_inputState != value)
                {
                    //logger.LogWarning("input mode changed");

                    if (value)
                        GameManager.Instance.Game.Input.Enable();
                    else
                        GameManager.Instance.Game.Input.Disable();
                }
                _inputState = value;
            }
        }

        static DoubleField current_focus = null;

        // check editor focus and un set  Game Input, check enter key
        static public void OnGUI()
        {
            if (Event.current.type == EventType.Layout)
            {
                // logger.LogInfo("focus on " + GUI.GetNameOfFocusedControl());
                bool isFocused = fields_dict.ContainsKey(GUI.GetNameOfFocusedControl());
                if (isFocused)
                {
                    var focus = fields_dict[GUI.GetNameOfFocusedControl()];
                    if (current_focus != focus)
                    {
                        if (current_focus != null)
                        {
                            current_focus.Validate();
                            current_focus.focus = false;
                        }
                        current_focus = focus;
                        current_focus.focus = true;
                    }
                }
                else
                {
                    if (current_focus != null)
                    {
                        current_focus.focus = false;
                        current_focus.Validate();
                    }
                    current_focus = null;
                }

                GameInputState = !isFocused;
            }
        }

        public static int IntFieldLine(string entryName, string label, int value, int min, int max, string postfix, string tooltip = "")
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label(label);

            value = IntMinMaxField(entryName, value, min, max);
            GUILayout.Label(postfix);

            if (!string.IsNullOrEmpty(tooltip))
            {
                UI_Tools.ToolTipButton(tooltip);
            }

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();

            return value;
        }

        public static int IntMinMaxField(string entryName, int value, int min, int max)
        {
            int new_value = (int)DoubleField(entryName, value);

            if (new_value != value)
            {
                if (new_value < min) new_value = min;
                if (new_value > max) new_value = max;
            }

            return new_value;
        }

        public static float FloatField(string entryName, float value, int precision = 2, float width = 50)
        {
            return (float)DoubleField(entryName, value, precision, width);
        }

        public static float FloatMinMaxField(string entryName, float value, float min, float max, int precision = 2, float width = 50)
        {
            value = (float)DoubleField(entryName, value, precision, width);
            value = Mathf.Clamp(value, min, max);
            return value;
        }

        public static double DoubleField(string entryName, double value, int precision = 2, float width = 50)
        {
            DoubleField field = null;

            if (fields_dict.ContainsKey(entryName))
                // always use temp value
                field = fields_dict[entryName];
            else
            {
                field = new DoubleField(entryName, value, precision, width);
                fields_dict[entryName] = field;
            }

            value = field.OnGUI(value);
            return value;
        }
    }
}