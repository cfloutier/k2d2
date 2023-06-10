using System.Collections.Generic;
using System.Globalization;
using KSP.Game;
using UnityEngine;

using System.Text.RegularExpressions;
using BepInEx.Logging;

namespace KTools.UI;

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
        for (int i = 0; i < precision ; i++)
        {
            format += "#";
        }

        current_text_Value = value.ToString(format, CultureInfo.InvariantCulture);
        current_value = value;
    }

    bool need_validate = false;

    public void Validate()
    {
        if (valid)
        {
            need_validate = true;
            GUI.FocusControl("");
        }
    }

    public double OnGUI(double value)
    {
        Color normal = GUI.color;

        if (Event.current.type == EventType.Repaint && need_validate)
        {
            value = last_parsed_value;
            need_validate = false;
        }
       
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
            bool parsed = double.TryParse(current_text_Value, NumberStyles.Any, CultureInfo.InvariantCulture, out num);
            if (!parsed)
            {
                GUI.color = Color.red;
                valid = false;
            }
            else
            {
                valid = true;
                GUI.color = Color.yellow;
                last_parsed_value = current_value = num;
            }
        }

        GUI.SetNextControlName(entryName);
        current_text_Value = GUILayout.TextField(current_text_Value, KBaseStyle.field, GUILayout.Width(width));

       // GUILayout.Label(current_text_Value);


        GUI.color = normal;
        return value;
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
        bool isFocused = fields_dict.ContainsKey(GUI.GetNameOfFocusedControl());
        if (isFocused)
        {
            var focus = fields_dict[GUI.GetNameOfFocusedControl()];
            if (current_focus != focus)
            {
                if (current_focus != null)
                {
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
            }
            current_focus = null;
        }

        if (current_focus != null)
        {
            if ((Event.current.type == EventType.KeyDown) && (Event.current.character == '\n'))
            {
                current_focus.Validate();
            }
        }

        GameInputState = !isFocused;
    }

    public static int IntFieldLine(string entryName, string label, int value, int min, int max, string postfix, string tooltip = "")
    {
        GUILayout.BeginHorizontal();

        UI_Tools.Label(label);
        GUILayout.FlexibleSpace();

        value = IntMinMaxField(entryName, value, min, max);
        UI_Tools.Label(postfix);
        if (!string.IsNullOrEmpty(tooltip))
        {
            UI_Tools.ToolTipButton(tooltip);
        }

        GUILayout.EndHorizontal();

        return value;
    }

    public static int IntMinMaxField(string entryName, int value, int min, int max)
    {
        value = (int)DoubleField(entryName, value);
        if (value < min) value = min;
        if (value > max) value = max;
        return value;
    }

    public static float FloatField(string entryName, float value, int precision = 2, float width = 100)
    {
        return (float)DoubleField(entryName, value, precision, width);
    }

    public static float FloatMinMaxField(string entryName, float value, float min, float max, int precision = 2, float width = 100)
    {
        value = (float)DoubleField(entryName, value, precision, width);
        value = Mathf.Clamp(value, min, max);
        return value;
    }

    public static double DoubleField(string entryName, double value, int precision = 2, float width = 100)
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
