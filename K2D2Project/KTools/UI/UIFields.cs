using System.Text.RegularExpressions;
using BepInEx.Logging;
using KSP.Game;
using UnityEngine;

namespace KTools.UI;

public class UI_Fields
{
    public static Dictionary<string, string> temp_dict = new Dictionary<string, string>();
    public static List<string> inputFields = new List<string>();
    static bool _inputState = true;


    public static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("KTools.UI_Fields");

    static public bool GameInputState
    {
        get { return _inputState; }
        set
        {
            if (_inputState != value)
            {
                logger.LogWarning("input mode changed");

                if (value)
                    GameManager.Instance.Game.Input.Enable();
                else
                    GameManager.Instance.Game.Input.Disable();
            }
            _inputState = value;
        }
    }

    static public void CheckEditor()
    {
        GameInputState = !inputFields.Contains(GUI.GetNameOfFocusedControl());
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

    public static float FloatField(string entryName, float value)
    {
        return (float)DoubleField(entryName, value);
    }

    public static float FloatMinMaxField(string entryName, float value, float min, float max)
    {
        value = (float)DoubleField(entryName, value);
        value = Mathf.Clamp(value, min, max);
        return value;
    }

    public static double DoubleField(string entryName, double value)
    {
        string text_value;
        if (temp_dict.ContainsKey(entryName))
            // always use temp value
            text_value = temp_dict[entryName];
        else
            text_value = value.ToString();

        if (!inputFields.Contains(entryName))
            inputFields.Add(entryName);

        Color normal = GUI.color;
        double num = 0;
        bool parsed = double.TryParse(text_value, out num);
        if (!parsed) GUI.color = Color.red;

        GUI.SetNextControlName(entryName);
        text_value = GUILayout.TextField(text_value, GUILayout.Width(100));

        GUI.color = normal;

        // save filtered temp value
        temp_dict[entryName] = text_value;
        if (parsed)
            return num;

        return value;
    }

    /// Simple Integer Field. for the moment there is a trouble. keys are sent to KSP2 events if focus is in the field
    public static int IntField(string entryName, string label, int value, int min, int max, string tooltip = "")
    {
        string text_value = value.ToString();

        if (temp_dict.ContainsKey(entryName))
            // always use temp value
            text_value = temp_dict[entryName];

        if (!inputFields.Contains(entryName))
            inputFields.Add(entryName);

        GUILayout.BeginHorizontal();

        if (!string.IsNullOrEmpty(label))
        {
            GUILayout.Label(label);
        }

        GUI.SetNextControlName(entryName);
        var typed_text = GUILayout.TextField(text_value, GUILayout.Width(100));
        typed_text = Regex.Replace(typed_text, @"[^\d-]+", "");

        // save filtered temp value
        temp_dict[entryName] = typed_text;

        int result = value;
        bool ok = true;
        if (!int.TryParse(typed_text, out result))
        {
            ok = false;
        }
        if (result < min)
        {
            ok = false;
            result = value;
        }
        else if (result > max)
        {
            ok = false;
            result = value;
        }

        if (!ok)
            GUILayout.Label("!!!", GUILayout.Width(30));

       

        GUILayout.EndHorizontal();
        return result;
    }
}
