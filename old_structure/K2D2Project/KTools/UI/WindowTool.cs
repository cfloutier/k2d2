using K2D2;
using UnityEngine;

namespace KTools.UI
{
    public class WindowTool
    {

        static float last_ui_size = -1;
        /// <summary>
        ///  checks if the window is in screen
        /// </summary>
        /// <param name="window_frame"></param>
        public static void check_window_pos(ref Rect window_frame)
        {
            float ui_size = K2D2Settings.ui_size;

            if (last_ui_size == -1)
                last_ui_size = ui_size;

            if (last_ui_size != ui_size)
            {
                float last_x_pos = window_frame.x * last_ui_size;
                float new_x_pos = window_frame.x * ui_size;
                float delta_x = last_x_pos - new_x_pos;
                window_frame.x = window_frame.x + delta_x;

                float last_y_pos = window_frame.y * last_ui_size;
                float new_y_pos = window_frame.y * ui_size;
                float delta_y = last_y_pos - new_y_pos;
                window_frame.y = window_frame.y + delta_y;

                last_ui_size = ui_size;
            }



            Rect scaled = new Rect(window_frame.x * ui_size,
                                    window_frame.y * ui_size, 
                                    window_frame.width * ui_size, 
                                    window_frame.height * ui_size);


            
            if (scaled.xMax > Screen.width)
            {
                var dx = Screen.width - scaled.xMax;
                scaled.x += dx;
            }
            if (scaled.yMax > Screen.height)
            {
                var dy = Screen.height - scaled.yMax;
                scaled.y += dy;
            }
            if (scaled.xMin < 0)
            {
                scaled.x = 0;
            }
            if (scaled.yMin < 0)
            {
                scaled.y = 0;
            }

            window_frame.x = scaled.x/ ui_size;
            window_frame.y = scaled.y / ui_size;
            window_frame.width = scaled.width / ui_size;
            window_frame.height = scaled.height / ui_size;
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
}