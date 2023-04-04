
using UnityEngine;

namespace K2D2
{
    public class Styles
    {
        private static bool guiLoaded = false;

        public static GUIStyle box, window, error, warning;

        public static GUIStyle small_button;
        public static GUIStyle big_button;

        public static GUIStyle label, console_text, phase_ok, phase_warning, phase_error;

        public static GUIStyle toggle;

        public static GUIStyle slider_line, slider_node, slider_text;

        public static GUIStyle icons_label, title;

        public static Texture2D gear, icon, big_icon;

        public static void Init()
        {
            if (!guiLoaded)
            {
                GetStyles();
            }
        }


        private static void setAllFromNormal(GUIStyle style)
        {
            style.hover = style.normal;
            style.active = style.normal;
            style.focused = style.normal;
            style.onNormal = style.normal;
            style.onHover = style.normal;
            style.onActive = style.normal;
            style.onFocused = style.normal;
        }

        private static void setFromOn(GUIStyle style)
        {
            style.onHover = style.onNormal;
            style.onActive = style.onNormal;
            style.onFocused = style.onNormal;
        }



        public static void GetStyles()
        {
            if (box != null)
                return;

            box = GUI.skin.GetStyle("Box");
            // Set the background color of the Box
            box.normal.background = Texture2D.whiteTexture;
            // Set the font size of the Box
            box.fontSize = 20;
            // Set the alignment of the Box
            box.alignment = TextAnchor.MiddleCenter;

            // WINDOW

            // Define the GUIStyle for the window
            window = new GUIStyle(GUI.skin.window);

            window.border = new RectOffset(25, 25, 35, 25);
            window.margin = new RectOffset(83, 43, 20, 20);
            window.padding = new RectOffset(20, 13, 44, 17);
            window.overflow = new RectOffset(0, 0, 0, 0);

            window.fontSize = 20;
            window.contentOffset = new Vector2(31, -40);

            slider_line = new GUIStyle(GUI.skin.horizontalSlider);
            slider_line.normal.background = AssetsLoader.loadIcon("Slider");
            setAllFromNormal(slider_line);
            slider_line.border = new RectOffset(5, 5, 0, 0);

            slider_line.border = new RectOffset(12, 14, 0, 0);
            slider_line.fixedWidth = 0;
            slider_line.fixedHeight = 21;
            slider_line.margin = new RectOffset(0, 0, 2, 5);

            slider_node = new GUIStyle(GUI.skin.horizontalSliderThumb);
            slider_node.normal.background = AssetsLoader.loadIcon("SliderNode");
            setAllFromNormal(slider_node);
            slider_node.border = new RectOffset(0, 0, 0, 0);
            slider_node.fixedWidth = 21;
            slider_node.fixedHeight = 21;

            // Set the background color of the window
            window.normal.background = AssetsLoader.loadIcon("window");
            window.normal.textColor = Color.black;
            setAllFromNormal(window);
            window.alignment = TextAnchor.UpperLeft;
            window.stretchWidth = true;

            // Small Button
            small_button = new GUIStyle(GUI.skin.GetStyle("Button"));
            small_button.normal.background = AssetsLoader.loadIcon("Button-normal");
            setAllFromNormal(small_button);
            small_button.active.background = AssetsLoader.loadIcon("Button-over");
            setFromOn(small_button);
         
            small_button.border = new RectOffset(8, 10, 8, 10);
            small_button.padding = new RectOffset(4, 4, 4, 6);
            small_button.overflow = new RectOffset(0, 0, 0, 2);
            small_button.fontSize = 14;
            small_button.alignment = TextAnchor.MiddleCenter;

            big_button = new GUIStyle(GUI.skin.GetStyle("Button"));
            big_button.normal.background = AssetsLoader.loadIcon("BigButton_Normal");
            big_button.hover.background = AssetsLoader.loadIcon("BigButton_hover");
            
            //setAllFromNormal(big_button);
            big_button.hover.background = AssetsLoader.loadIcon("BigButton_hover");
            big_button.onNormal.background = AssetsLoader.loadIcon("BigButton_on");
            setFromOn(big_button);
            big_button.onHover.background = AssetsLoader.loadIcon("BigButton_on_hover");

            small_button.border = new RectOffset(12, 12, 5, 5);
            small_button.padding = new RectOffset(10, 10, 10, 10);
            small_button.overflow = new RectOffset(0, 0, 0, 0);
            small_button.fontSize = 40;
            small_button.alignment = TextAnchor.MiddleCenter;

            // Toggle Button
            toggle = new GUIStyle(GUI.skin.GetStyle("Button"));
            toggle.normal.background = AssetsLoader.loadIcon("Toggle_Off");
            toggle.normal.textColor = ColorTools.parseColor("#C0C1E2");


            setAllFromNormal(toggle);
            toggle.onNormal.background = AssetsLoader.loadIcon("Toggle_On");
            toggle.onNormal.textColor = ColorTools.parseColor("#C0E2DC");
            setFromOn(toggle);
            toggle.fixedHeight = 32;
            toggle.stretchWidth = false;

            toggle.border = new RectOffset(45, 5, 5, 5);
            toggle.padding = new RectOffset(34, 16, 0, 0);
            toggle.overflow = new RectOffset(0, 0, 0, 2);

            // GEAR icon
            gear = AssetsLoader.loadIcon("gear");
            icon = AssetsLoader.loadIcon("icon");
            big_icon = AssetsLoader.loadIcon("big_icon");

            icons_label = new GUIStyle(GUI.skin.GetStyle("Label"));
            icons_label.border = new RectOffset(0, 0, 0, 0);
            icons_label.padding = new RectOffset(0, 0, 0, 0);
            icons_label.margin = new RectOffset(0, 0, 0, 0);
            icons_label.overflow = new RectOffset(0, 0, 0, 0);


            error = new GUIStyle(GUI.skin.GetStyle("Label"));
            warning = new GUIStyle(GUI.skin.GetStyle("Label"));
            error.normal.textColor = Color.red;
            warning.normal.textColor = Color.yellow;
            //labelColor = GUI.skin.GetStyle("Label").normal.textColor;

            phase_ok = new GUIStyle(GUI.skin.GetStyle("Label"));
            phase_ok.normal.textColor = ColorTools.parseColor("#00BC16");
            phase_ok.fontSize = 20;

            phase_warning = new GUIStyle(GUI.skin.GetStyle("Label"));
            phase_warning.normal.textColor = ColorTools.parseColor("#BC9200");
            phase_warning.fontSize = 20;

            phase_error = new GUIStyle(GUI.skin.GetStyle("Label"));
            phase_error.normal.textColor = ColorTools.parseColor("#B30F0F");
            phase_error.fontSize = 20;

            console_text = new GUIStyle(GUI.skin.GetStyle("Label"));
            console_text.normal.textColor = ColorTools.parseColor("#9395D4");
            console_text.fontSize = 15;
            console_text.padding = new RectOffset(0, 0, 0, 0);
            console_text.margin = new RectOffset(0, 0, 0, 0);

            slider_text = new GUIStyle(console_text);
            slider_text.margin = new RectOffset(10, 0, 0, 0);
            slider_text.contentOffset = new Vector2(8, 5);

            label = new GUIStyle(GUI.skin.GetStyle("Label"));
            label.fontSize = 17;
            label.margin = new RectOffset(0, 0, 0, 0);
            label.padding = new RectOffset(0, 0, 0, 0);

            title = new GUIStyle();
            title.normal.textColor = ColorTools.parseColor("#C0C1E2");
            title.fontSize = 19;

            guiLoaded = true;
        }
    }

}