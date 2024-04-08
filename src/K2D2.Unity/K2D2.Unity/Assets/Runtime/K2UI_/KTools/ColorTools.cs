using System.Globalization;
using UnityEngine;

namespace KTools
{
    /// <summary>
    /// A set of simple tools for colors that is missing in the main unity API
    /// </summary>
    public class ColorTools
    {
        /// <summary>
        /// Convert a HSV color from rgb Color values.
        /// </summary>
        public static void ToHSV(Color col, out float h, out float s, out float v)
        {
            float min, max, delta;

            float r = col.r;
            float g = col.g;
            float b = col.b;

            min = Mathf.Min(r, Mathf.Min(g, b));
            max = Mathf.Max(r, Mathf.Max(g, b));
            v = max;                // v

            delta = max - min;


            if (delta != 0)
            {
                s = delta / max;        // s
            }
            else
            {
                // r = g = b = 0		// s = 0, v is undefined
                s = 0;
                h = 0;
                return;
            }

            if (r == max)
            {
                h = (g - b) / delta;        // between yellow & magenta
            }
            else if (g == max)
            {
                h = 2 + (b - r) / delta;    // between cyan & yellow
            }
            else
            {
                h = 4 + (r - g) / delta;    // between magenta & cyan
            }

            h *= 60f / 360;             // 0-1

            if (h < 0)
                h += 1;
        }


        /// <summary>
        /// Convert a color from HSV values. any value is vetween 0 and one
        /// </summary>
        public static Color FromHSV(float hue, float saturation, float value, float alpha)
        {
            hue *= 360;
            //Debug.Log("hue : " + hue);

            int hi = ((int)(Mathf.Floor(hue / 60f))) % 6;
            //Debug.Log("hi : " + hi);
            float f = hue / 60f - Mathf.Floor(hue / 60f);
            //Debug.Log("f : " + f);

            float v = value;
            float p = value * (1 - saturation);
            float q = value * (1 - f * saturation);
            float t = value * (1 - (1 - f) * saturation);

            if (hi == 0)
                return new Color(v, t, p, alpha);
            else if (hi == 1)
                return new Color(q, v, p, alpha);
            else if (hi == 2)
                return new Color(p, v, t, alpha);
            else if (hi == 3)
                return new Color(p, q, v, alpha);
            else if (hi == 4)
                return new Color(t, p, v, alpha);
            else
                return new Color(v, p, q, alpha);
        }

        /// <summary>
        /// Parse a color as a string 
        /// </summary>
        /// <param name="color">the string representing the color
        /// Under can be any of the predefined color by Unity 
        /// Or a html rgb color ex FF0000 is red
        /// </param>
        /// <returns>the parsed Color</returns>
        static public Color parseColor(string color)
        {
            if (color == "") return Color.white;
            color = color.ToLower();
            switch (color)
            {
                case "black": return Color.black;
                case "blue": return Color.blue;
                case "clear": return Color.clear;
                case "cyan": return Color.cyan;
                case "gray": return Color.gray;
                case "green": return Color.green;
                case "grey": return Color.grey;
                case "magenta": return Color.magenta;
                case "red": return Color.red;
                case "white": return Color.white;
                case "yellow": return Color.yellow;
                case "orange": return new Color(1f, 0.76f, 0f);
                default:
                    {
                        if (color.StartsWith("#"))
                            color = color.Substring(1);

                        while (color.Length < 6)
                            color += "0";

                        int r, g, b;

                        System.Int32.TryParse(color.Substring(0, 2), System.Globalization.NumberStyles.AllowHexSpecifier,
                            CultureInfo.InvariantCulture.NumberFormat, out r);
                        System.Int32.TryParse(color.Substring(2, 2), System.Globalization.NumberStyles.AllowHexSpecifier,
                            CultureInfo.InvariantCulture.NumberFormat, out g);
                        System.Int32.TryParse(color.Substring(4, 2), System.Globalization.NumberStyles.AllowHexSpecifier,
                            CultureInfo.InvariantCulture.NumberFormat, out b);

                        return new Color(
                            ((float)r) / 255,
                            ((float)g) / 255,
                            ((float)b) / 255);
                    }
            }
        }

        static public string formatColorHtml(Color col)
        {
            int r = (int)(col.r * 255);
            int g = (int)(col.g * 255);
            int b = (int)(col.b * 255);
            return string.Format("{0:X2}{1:X2}{2:X2}", r, g, b);
        }

        // just a list of really differnts colors  that can be used for unitary test
        // any color is far enough from the next one to be fully visible 
        static public Color[] getRandomColorArray(int Nb, float saturation = 1)
        {

            float delta = 1f / Nb;
            Color[] colors = new Color[Nb];

            float h = UnityEngine.Random.Range(0f, 1);

            for (int i = 0; i < Nb; i++)
            {
                colors[i] = FromHSV(h, saturation, 1, 1);
                h += delta;
            }

            return colors;
        }

        // just a list of really differnts colors  that can be used for unitary test
        // any color is far enough from the next one to be fully visible 
        static public Color[] getRainbowColorArray(int Nb)
        {
            float delta = 1f / Nb;
            Color[] colors = new Color[Nb];

            float h = 0;

            for (int i = 0; i < Nb; i++)
            {
                colors[i] = FromHSV(h, 1, 1, 1);
                h += delta;
            }

            return colors;
        }


        static public Color randomColor()
        {
            float h = UnityEngine.Random.Range(0f, 1);
            //  float v = UnityEngine.Random.Range(0.5f, 1);
            return FromHSV(h, 1, 1, 1);

        }

        static public Color changeColorHSV(Color source, float deltaH, float deltaS, float deltaV)
        {
            float h, s, v;
            ToHSV(source, out h, out s, out v);

            h += deltaH;
            s += deltaS;
            v += deltaV;

            return ColorTools.FromHSV(h, s, v, source.a);
        }
    }
}