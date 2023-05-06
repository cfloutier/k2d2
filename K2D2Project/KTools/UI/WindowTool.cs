using UnityEngine;

namespace KTools.UI;

public class WindowTool
{
    /// <summary>
    ///  checks if the window is in screen
    /// </summary>
    /// <param name="window_frame"></param>
    public static void check_window_pos(ref Rect window_frame)
    {
        if (window_frame.xMax > Screen.width)
        {
            var dx = Screen.width - window_frame.xMax;
            window_frame.x += dx;
        }
        if (window_frame.yMax > Screen.height)
        {
            var dy = Screen.height - window_frame.yMax;
            window_frame.y += dy;
        }
        if (window_frame.xMin < 0)
        {
            window_frame.x = 0;
        }
        if (window_frame.yMin < 0)
        {
            window_frame.y = 0;
        }
    }

    /// <summary>
    /// check the window pos and load settings if not set
    /// </summary>
    /// <param name="window_frame"></param>
    public static void check_main_window_pos(ref Rect window_frame)
    {
        if (window_frame == Rect.zero)
        {
            int x_pos = KBaseSettings.window_x_pos;
            int y_pos = KBaseSettings.window_y_pos;

            if (x_pos == -1)
            {
                x_pos = 100;
                y_pos = 50;
            }

            window_frame = new Rect(x_pos, y_pos, 500, 100);
        }

        check_window_pos(ref window_frame);
    }
}
