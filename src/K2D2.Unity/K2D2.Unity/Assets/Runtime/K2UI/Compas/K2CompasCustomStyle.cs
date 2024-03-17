
using UnityEngine;

using UnityEngine.UIElements;
using System.Linq;
using System.Collections.Generic;


namespace K2UI.Compas
{


    class K2CompasCustomStyle
    {
        static CustomStyleProperty<float> s_CenterLineWidth = new CustomStyleProperty<float>("--center-line-width");
        static CustomStyleProperty<Color> s_CenterLineColor = new CustomStyleProperty<Color>("--center-line-color");

        static CustomStyleProperty<float> s_LineWidth = new CustomStyleProperty<float>("--line-width");
        static CustomStyleProperty<Color> s_LineColor_1 = new CustomStyleProperty<Color>("--line-color-1");
        static CustomStyleProperty<Color> s_LineColor_2 = new CustomStyleProperty<Color>("--line-color-2");
        static CustomStyleProperty<Color> s_LineColor_3 = new CustomStyleProperty<Color>("--line-color-3");

        static CustomStyleProperty<float> s_line_y1 = new CustomStyleProperty<float>("--line-y1");
        static CustomStyleProperty<float> s_line_y5 = new CustomStyleProperty<float>("--line-y5");
        static CustomStyleProperty<float> s_line_y10 = new CustomStyleProperty<float>("--line-y10");
        static CustomStyleProperty<float> s_line_y45 = new CustomStyleProperty<float>("--line-y45");

        public float centerLineWidth = 1;
        public Color centerLineColor = Color.white;

        public float lineWidth = 2;
        public Color lineColor_1 = Color.white;
        public Color lineColor_2 = Color.white;
        public Color lineColor_3 = Color.white;

        public float y_1 = 30;
        public float y_5 = 25;
        public float y_10 = 20;
        public float y_45 = 25;

        public void UpdateCustomStyles(ICustomStyle customStyle)
        {
            if (customStyle.TryGetValue(s_CenterLineColor, out var my_color))
            {
                this.centerLineColor = my_color;
            }

            if (customStyle.TryGetValue(s_CenterLineWidth, out var my_float))
            {
                this.centerLineWidth = my_float;
            }

            if (customStyle.TryGetValue(s_LineColor_1, out my_color))
            {
                this.lineColor_1 = my_color;
            }

            if (customStyle.TryGetValue(s_LineColor_2, out my_color))
            {
                this.lineColor_2 = my_color;
            }

            if (customStyle.TryGetValue(s_LineColor_3, out my_color))
            {
                this.lineColor_3 = my_color;
            }

            if (customStyle.TryGetValue(s_LineWidth, out my_float))
            {
                this.lineWidth = my_float;
            }

            if (customStyle.TryGetValue(s_line_y1, out my_float))
            {
                this.y_1 = my_float;
            }

            if (customStyle.TryGetValue(s_line_y5, out my_float))
            {
                this.y_5 = my_float;
            }

            if (customStyle.TryGetValue(s_line_y10, out my_float))
            {
                this.y_10 = my_float;
            }

            if (customStyle.TryGetValue(s_line_y45, out my_float))
            {
                this.y_45 = my_float;
            }
        }
    }
}