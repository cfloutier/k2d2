using UnityEngine;

namespace K2D2
{
    public class ToolTipsManager
    {

        public static void setToolTip(string tooltip)
        {
            if (Event.current.type == EventType.Repaint)
            {
                // tooltip = "toto";
                if (last_tool_tip != tooltip)
                {
                    //Debug.Log("changed");

                    if (!string.IsNullOrEmpty(tooltip))
                    {
                        show = true;
                        show_time = Time.time + delay;
                        draw_tool_tip = tooltip;
                    }
                    else
                    {
                        show = false;
                    }
                }

                last_tool_tip = tooltip;
            }
        }

        static float show_time;
        const float delay = 0.5f;
        static bool show = false;

        static Vector2 offset = new Vector2(20, 10);

        static string last_tool_tip;
        static string draw_tool_tip;
        public static void DrawToolTips()
        {
            if (!show)
                return;

            if (Time.time > show_time)
            {
                float minWidth, maxWidth;
                GUI.skin.button.CalcMinMaxWidth(new GUIContent(draw_tool_tip), out minWidth, out maxWidth);
                var tooltip_pos = new Rect(Input.mousePosition.x + offset.x, Screen.height - Input.mousePosition.y + offset.y, maxWidth, 10);
                UIWindow.check_window_pos(ref tooltip_pos);

                GUILayout.Window(3, tooltip_pos, WindowFunction, "", GUI.skin.button);
            }
        }

        static void WindowFunction(int windowID)
        {
            //Debug.Log(draw_tool_tip);
            GUILayout.Label(draw_tool_tip);
        }
    }

}