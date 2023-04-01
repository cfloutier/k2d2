
using UnityEngine;
using KSP.Sim;
using BepInEx.Logging;
using SpaceWarp.API.Assets;

namespace K2D2
{
    public class Styles
    {
        private static bool guiLoaded = false;

        public static GUIStyle box, window, error, warning, small_button;

        public static GUIStyle console_text, phase_ok, phase_warning, phase_error;

        public static GUIStyle slider_line, slider_node;

        public static GUIStyle icons_label, title;

        public static Texture2D gear, icon, big_icon;
        public static Color labelColor;

        public static void Init()
        {
            if (!guiLoaded)
            {
                GetStyles();
            }
        }

        // BEPEXVersion
        public static Texture2D loadIcon(string path)
        {
           var imageTexture = AssetManager.GetAsset<Texture2D>($"{K2D2_Plugin.mod_id}/images/{path}.png");

            //   Check if the texture is null
            if (imageTexture == null)
            {
                // Print an error message to the Console
                Debug.LogError("Failed to load image texture from path: " + path);

                // Print the full path of the resource
                Debug.Log("Full resource path: " + Application.dataPath + "/" + path);

                // Print the type of resource that was expected
                Debug.Log("Expected resource type: Texture2D");
            }

            return imageTexture;
        }

        // Unity_Editor_Version
        // static public Texture2D loadIcon(string path)
        // {
        //     var imageTexture = Resources.Load<Texture2D>(path);

        //     // Check if the texture is null
        //     if (imageTexture == null)
        //     {
        //         // Print an error message to the Console
        //         Debug.LogError("Failed to load image texture from path: " + path);

        //         // Print the full path of the resource
        //         Debug.Log("Full resource path: " + Application.dataPath + "/" + path);

        //         // Print the type of resource that was expected
        //         Debug.Log("Expected resource type: Texture2D");
        //     }

        //     return imageTexture;
        // }

        private static void resetToNormal(GUIStyle style)
        {
            style.hover = style.normal;
            style.active = style.normal;
            style.focused = style.normal;
            style.onNormal = style.normal;
            style.onHover = style.normal;
            style.onActive = style.normal;
            style.onFocused = style.normal;
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
            slider_line.normal.background = loadIcon("Slider");
            resetToNormal(slider_line);
            slider_line.border = new RectOffset(5, 5, 0, 0);

            slider_line.border = new RectOffset(12, 12, 0, 0);
            slider_line.fixedWidth = 0;
            slider_line.fixedHeight = 21;
            slider_line.margin = new RectOffset(0, 0, 10, 10);

            slider_node = new GUIStyle(GUI.skin.horizontalSliderThumb);
            slider_node.normal.background = loadIcon("SliderNode");
            resetToNormal(slider_node);
            slider_node.border = new RectOffset(0, 0, 0, 0);
            slider_node.fixedWidth = 21;
            slider_node.fixedHeight = 21;

            // Set the background color of the window
            window.normal.background = loadIcon("window");
            window.normal.textColor = Color.black;
            resetToNormal(window);
            window.alignment = TextAnchor.UpperLeft;
            window.stretchWidth = true;


            // Small Button
            small_button = new GUIStyle(GUI.skin.GetStyle("Button"));
            small_button.normal.background = loadIcon("Button-normal");
            resetToNormal(small_button);
            small_button.hover.background = loadIcon("Button-over");
            small_button.active.background = loadIcon("Button-active");
            small_button.onNormal.background = loadIcon("Button-active");
            small_button.border = new RectOffset(8, 10, 8, 10);
            small_button.margin = new RectOffset(0, 0, 0, 5);
            small_button.padding = new RectOffset(5, 5, 5, 5);
            small_button.overflow = new RectOffset(0, 0, 0, 2);
            small_button.fontSize = 15;

            // GEAR icon
            gear = loadIcon("gear");
            icon = loadIcon("icon");
            big_icon = loadIcon("big_icon");

            icons_label = new GUIStyle(GUI.skin.GetStyle("Label"));
            icons_label.border = new RectOffset(0, 0, 0, 0);
            icons_label.padding = new RectOffset(0, 0, 0, 0);
            icons_label.margin = new RectOffset(0, 0, 0, 0);
            icons_label.overflow = new RectOffset(0, 0, 0, 0);


            error = new GUIStyle(GUI.skin.GetStyle("Label"));
            warning = new GUIStyle(GUI.skin.GetStyle("Label"));
            error.normal.textColor = Color.red;
            warning.normal.textColor = Color.yellow;
            labelColor = GUI.skin.GetStyle("Label").normal.textColor;

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
            console_text.normal.textColor = ColorTools.parseColor("#6ADFDF");
            console_text.fontSize = 15;
            console_text.padding = new RectOffset(0, 0, 0, 0);
            console_text.margin = new RectOffset(0, 0, 0, 0);

            title = new GUIStyle(GUI.skin.GetStyle("Label"));
            title.normal.textColor = ColorTools.parseColor("#8AECEC");
            title.fontSize = 19;

            guiLoaded = true;
        }
    }

}