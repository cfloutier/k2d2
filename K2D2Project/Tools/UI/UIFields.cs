using UnityEngine;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using KSP.Game;
using BepInEx.Logging;

namespace K2D2.UI;

public class UI_Fields
{
    public static Dictionary<string, string> temp_dict = new Dictionary<string, string>();
    public static List<string> inputFields = new List<string>();
    static bool _inputState = true;


    public static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.MainUI");

    static public bool GameInputState
    {
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

    /// Simple Integer Field. for the moment there is a trouble. keys are sent to KSP2 events if focus is in the field
    public static int IntField(string name, string label, int value, int min, int max, string tooltip = "")
    {
        string text_value = value.ToString();

        if (temp_dict.ContainsKey(name))
            // always use temp value
            text_value = temp_dict[name];

        if (!inputFields.Contains(name))
            inputFields.Add(name);

        GUILayout.BeginHorizontal();

        GUI.SetNextControlName(name);
        GUILayout.Label(label);
        var typed_text = GUILayout.TextField(text_value, GUILayout.Width(100));
        typed_text = Regex.Replace(typed_text, @"[^\d-]+", "");

        // save filtered temp value
        temp_dict[name] = typed_text;

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
            GUILayout.Label("!!!");

        if (!string.IsNullOrEmpty(tooltip))
        {
            UI_Tools.ToolTipButton(tooltip);
        }

        GUILayout.EndHorizontal();
        return result;
    }
}



