using System;
using System.Collections.Generic;
using System.Globalization;
using MoonSharp.Interpreter.Tree.Statements;
using UnityEngine;

namespace KTools.UI
{
    /// <summary>
    /// A set of simple tools for UI
    /// </summary>
    /// TODO : remove static, make it singleton
    public class UI_Tools
    {
        public static void OnGUI()
        {
            ToolTipsManager.DrawToolTips();
            UI_Fields.OnGUI();
        }

        public static void OnUpdate()
        {
            HeadingSlider.onStaticUpdate();
        }

        public static int GetEnumValue<T>(T inputEnum) where T : struct, IConvertible
        {
            Type t = typeof(T);
            if (!t.IsEnum)
            {
                throw new ArgumentException("Input type must be an enum.");
            }

            return inputEnum.ToInt32(CultureInfo.InvariantCulture.NumberFormat);
        }

        public static TEnum EnumGrid<TEnum>(string label, TEnum value, int cols = 4) where TEnum : struct, Enum
        {
            int int_value = value.GetHashCode();
            string[] labels = Enum.GetNames(typeof(TEnum));
            UI_Tools.Label(label);
            int result = GUILayout.SelectionGrid(int_value, labels, cols, KBaseStyle.tab_normal);

            return (TEnum)Enum.ToObject(typeof(TEnum), result);
        }

        public static TEnum EnumGrid<TEnum>(string label, TEnum value, string[] labels) where TEnum : struct, Enum
        {
            int int_value = value.GetHashCode();
            UI_Tools.Label(label);
            int result = GUILayout.SelectionGrid(int_value, labels, labels.Length, KBaseStyle.tab_normal);

            return (TEnum)Enum.ToObject(typeof(TEnum), result);
        }

        public static bool Toggle(bool is_on, string txt, string tooltip = null)
        {
            if (tooltip != null)
                return GUILayout.Toggle(is_on, new GUIContent(txt, tooltip), KBaseStyle.toggle);
            else
                return GUILayout.Toggle(is_on, txt, KBaseStyle.toggle);
        }

        public static bool BigToggleButton(bool is_on, string txt_run, string txt_stop)
        {
            // int height_bt = 30;
            int min_width_bt = 150;

            var txt = is_on ? txt_stop : txt_run;
            is_on = GUILayout.Toggle(is_on, txt, KBaseStyle.big_button, GUILayout.MinWidth(min_width_bt));
            return is_on;
        }

        public static bool SmallToggleButton(bool is_on, string txt_run, string txt_stop)
        {
            // int height_bt = 30;
            int min_width_bt = 150;

            var txt = is_on ? txt_stop : txt_run;

            is_on = GUILayout.Toggle(is_on, txt, KBaseStyle.small_button, GUILayout.MinWidth(min_width_bt));

            return is_on;
        }

        public static bool BigButton(string txt)
        {
            // int height_bt = 30;
            int min_width_bt = 150;

            return GUILayout.Button(txt, KBaseStyle.big_button, GUILayout.MinWidth(min_width_bt));
        }

        public static bool Button(string txt)
        {
            return GUILayout.Button(txt, KBaseStyle.button);
        }

        public static bool SmallButton(string txt)
        {
            return GUILayout.Button(txt, KBaseStyle.small_button);
        }

        public static bool BigIconButton(string txt)
        {
            return GUILayout.Button(txt, KBaseStyle.bigicon_button);
        }

        public static bool ListButton(string txt)
        {
            return GUILayout.Button(txt, KBaseStyle.button, GUILayout.ExpandWidth(true));
        }

        public static bool miniToggle(bool value, string txt, string tooltip)
        {
            return GUILayout.Toggle(value, new GUIContent(txt, tooltip), KBaseStyle.small_button, GUILayout.Height(20));
        }

        public static bool miniButton(string txt, string tooltip = "")
        {
            return GUILayout.Button(new GUIContent(txt, tooltip), KBaseStyle.small_button, GUILayout.Height(20));
        }


        public static bool resetButton()
        {
            return GUILayout.Button("R", KBaseStyle.small_button, GUILayout.Height(20), GUILayout.Width(20));
        }

        public static bool ToolTipButton(string tooltip)
        {
            return GUILayout.Button(new GUIContent("?", tooltip), KBaseStyle.small_button, GUILayout.Height(20), GUILayout.Width(20));
        }

        static public bool BigIconButton(Texture2D icon)
        {
            return GUILayout.Button(icon, KBaseStyle.bigicon_button);
        }

        public static void Title(string txt)
        {
            GUILayout.Label($"<b>{txt}</b>", KBaseStyle.title);
        }

        public static void Value(string txt, float value)
        {
            GUILayout.Label(txt + " : " + value.ToString("0.##", CultureInfo.InvariantCulture), KBaseStyle.console_text);
        }

        public static void Value(string txt, double value)
        {
            GUILayout.Label(txt + " : " + value.ToString("0.##", CultureInfo.InvariantCulture), KBaseStyle.console_text);
        }

        public static void Value(string txt, int value)
        {
            GUILayout.Label(txt + " : " + value.ToString(), KBaseStyle.console_text);
        }

        public static void Value(string txt, Vector3d value)
        {
            GUILayout.Label(txt + " : " + StrTool.Vector3ToString(value), KBaseStyle.console_text);
        }

        public static void Value(string txt, Vector2 value)
        {
            GUILayout.Label(txt + " : " + StrTool.Vector2ToString(value), KBaseStyle.console_text);
        }

        public static void Label(string txt)
        {
            GUILayout.Label(txt, KBaseStyle.label);
        }

        public static void OK(string txt)
        {
            GUILayout.Label(txt, KBaseStyle.phase_ok);
        }

        public static void Warning(string txt)
        {
            GUILayout.Label(txt, KBaseStyle.phase_warning);
        }

        public static void Error(string txt)
        {
            GUILayout.Label(txt, KBaseStyle.phase_error);
        }

        public static void Console(string txt)
        {
            GUILayout.Label(txt, KBaseStyle.console_text);
        }

        public static void Mid(string txt)
        {
            GUILayout.Label(txt, KBaseStyle.mid_text);
        }

        public static float LabelSlider(string label, float value, float min, float max, float width_label = 150)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{label} : {value:0.##}", GUILayout.Width(width_label));
            value = FloatSlider(value, min, max);
            GUILayout.EndHorizontal();

            return value;
        }

        public static int IntSlider(string txt, int value, int min, int max, string postfix = "", string tooltip = "")
        {
            string content = txt + $" : {value} " + postfix;

            GUILayout.Label(content, KBaseStyle.slider_text);
            GUILayout.BeginHorizontal();
            value = (int)GUILayout.HorizontalSlider((int)value, min, max);
            if (value < min) value = min;
            if (value > max) value = max;

            if (!string.IsNullOrEmpty(tooltip))
            {
                UI_Tools.ToolTipButton(tooltip);
            }
            GUILayout.EndHorizontal();
            return value;
        }

        public static float ElevationSlider(string ui_code, float value, string tooltip = "")
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Elevation (°)");

            //GUILayout.FlexibleSpace();
            value = RepeatButton.OnGUI(ui_code + ".elevation_minus", " - ", value, -0.2f);
            value = UI_Fields.FloatField(ui_code + ".elevation_field", value, 1, 50);
            value = RepeatButton.OnGUI(ui_code + ".att.elevation_plus", " + ", value, 0.2f);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            value = FloatSlider(value, -90, 90);

            return value;
        }

        public static float HeadingControl(string ui_code, float value, string tooltip = "")
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Heading (°)");
            value = RepeatButton.OnGUI(ui_code + ".heading_minus", " - ", value, -0.2f);
            value = UI_Fields.FloatField(ui_code + ".heading_field", value, 1, 50);
            value = RepeatButton.OnGUI(ui_code + ".heading_plus", " + ", value, 0.2f);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            value = HeadingSlider.onGUI(ui_code + ".heading_slider", value, true);

            return value;
        }


        public static void Separator()
        {
            GUILayout.Box("", KBaseStyle.separator);
        }

        public static void ProgressBar(double value, double min, double max)
        {
            ProgressBar((float)value, (float)min, (float)max);
        }

        public static void ProgressBar(float value, float min, float max)
        {
            var ratio = Mathf.InverseLerp(min, max, value);

            GUILayout.Box("", KBaseStyle.progress_bar_empty, GUILayout.ExpandWidth(true));
            var lastrect = GUILayoutUtility.GetLastRect();

            lastrect.width = Mathf.Clamp(lastrect.width * ratio, 4, 10000000);
            GUI.Box(lastrect, "", KBaseStyle.progress_bar_full);
        }

        public static float FloatSlider(float value, float min, float max, string tooltip = "")
        {
            // simple float slider
            GUILayout.BeginHorizontal();
            float new_value = GUILayout.HorizontalSlider(value, min, max);

            value = new_value;
            if (!string.IsNullOrEmpty(tooltip))
            {
                UI_Tools.ToolTipButton(tooltip);
            }
            GUILayout.EndHorizontal();

            value = Mathf.Clamp(value, min, max);
            return value;
        }

        public static float FloatSliderTxt(string txt, float value, float min, float max, string postfix = "", string tooltip = "", int precision = 2)
        {
            // simple float slider with a printed value
            string value_str = value.ToString("N" + precision);

            string content = $"{txt} : {value_str} {postfix}";

            GUILayout.Label(content, KBaseStyle.slider_text);
            value = FloatSlider(value, min, max, tooltip);
            return value;
        }

        public static void Right_Left_Text(string right_txt, string left_txt)
        {
            // text aligned to right and left with a space in between
            GUILayout.BeginHorizontal();
            UI_Tools.Mid(right_txt);
            GUILayout.FlexibleSpace();
            UI_Tools.Mid(left_txt);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
        }

        public static Vector2 BeginScrollView(Vector2 scrollPos, int height)
        {
            return GUILayout.BeginScrollView(scrollPos, false, true,
                // GUILayout.MinWidth(250),
                GUILayout.Height(height));
        }

    }

}