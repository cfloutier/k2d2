using System;
using UnityEngine;

namespace KTools
{
    public class StrTool
    {
        static public string DurationToString(double secs)
        {
            string prefix = "";
            if (secs < 0)
            {
                secs = -secs;
                prefix = "- ";
            }

            // more than one day
            if (secs > 21600) // 3600 * 6 = 21 600
            {
                int days = (int)(secs / 21600);
                secs = secs - days * 21600;
                prefix += $"{days}d ";
            }
            else if (secs < 60)
            {
                return $"{secs:n1} s";
            }

            try
            {
                TimeSpan t = TimeSpan.FromSeconds(secs);
                var result = prefix + string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D1}",
                t.Hours,
                t.Minutes,
                t.Seconds,
                t.Milliseconds);
                return result;
            }
            catch (System.Exception)
            {
                return prefix + $"{secs:n2} s";
            }    
        }

        public const double AstronomicalUnit = 149597870700;
        // https://en.wikipedia.org/wiki/Parsec
        public static double Parsec { get; } = (648000 / Math.PI) * AstronomicalUnit;
        public static string DistanceToString(double meters)
        {
            var sign = "";
            if (meters < 0)
            {
                sign = "-";
                meters = -meters;
            }
            if (meters > (Parsec / 10))
            {
                return $"{sign}{meters / Parsec:n2} pc";
            }
            if (meters > (AstronomicalUnit / 10))
            {
                return $"{sign}{meters / AstronomicalUnit:n2} AU";
            }
            if (meters > (997))
            {
                return $"{sign}{meters / 1000:n2} km";
            }
            if (meters < 1)
            {
                return $"{sign}{meters * 100:0} cm";
            }

            return sign + meters.ToString("0") + " m";
        }

        static public string Vector3ToString(Vector3 vec)
        {
            return $"{vec.x:n2} {vec.y:n2} {vec.z:n2}";
        }

        static public string Vector2ToString(Vector2 vec)
        {
            return $"{vec.x:n2} {vec.y:n2}";
        }

    }
}