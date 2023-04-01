using System;
using System.Text.RegularExpressions;
using KSP.Sim.Maneuver;
using KSP.Game;

namespace K2D2
{
    public static class GeneralTools
    {
        public static GameInstance Game => GameManager.Instance == null ? null : GameManager.Instance.Game;

        /// <summary>
        /// Converts a string to a double, if the string contains a number. Else returns -1
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static double GetNumberString(string str)
        {
            string number = Regex.Replace(str, "[^0-9.]", "");

            return number.Length > 0 ? double.Parse(number) : -1;

        }

        static public string VectorToString(Vector3d vec)
        {
            return $"{vec.x:n2} {vec.y:n2} {vec.z:n2}";
        }

        static public string DurationToString(double secs)
        {
            if (secs < 0)
            {
                secs = -secs;
                TimeSpan t = TimeSpan.FromSeconds(secs);

                return string.Format("- {0:D2}:{1:D2}:{2:D2}:{3:D3}",
                    t.Hours,
                    t.Minutes,
                    t.Seconds,
                    t.Milliseconds);
                }
            else
            {
                TimeSpan t = TimeSpan.FromSeconds(secs);

                return string.Format("{0:D2}:{1:D2}:{2:D2}:{3:D3}",
                    t.Hours,
                    t.Minutes,
                    t.Seconds,
                    t.Milliseconds);
            }

            
        }

        public static Vector3d correctEuler(Vector3d euler)
        {
            Vector3d result = euler;
            if (result.x > 180)
            {
                result.x -= 360;
            }
            if (result.y > 180)
            {
                result.y -= 360;
            }
            if (result.z > 180)
            {
                result.z -= 360;
            }

            return result;
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
                meters = - meters;
            }
            if (meters > (Parsec / 10))
            {
                return $"{sign}{(meters / Parsec):n2} pc";
            }
            if (meters > (AstronomicalUnit / 10))
            {
                return $"{sign}{(meters / AstronomicalUnit):n2} AU";
            }
            if (meters > (997))
            {
                return $"{sign}{(meters / 1000):n2} km";
            }
            if (meters < 1)
            {
                return $"{sign}{meters * 100:0} cm";
            }

            return sign + meters.ToString("0") + " m";
        }

        public static double remainingStartTime(ManeuverNodeData node)
        {
            var dt = node.Time - Game.UniverseModel.UniversalTime;
            return dt;
        }

        public static double remainingEndTime(ManeuverNodeData node)
        {
            var dt = node.Time + node.BurnDuration - Game.UniverseModel.UniversalTime;
            return dt;
        }

        public static Guid createGuid()
        {
            return Guid.NewGuid();
        }

    }
}