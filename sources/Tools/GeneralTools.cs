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

        static public string DistanceToString(double distance_m)
        {
            if (distance_m < 1000)
            {
                return string.Format("{0:n2 }m", distance_m);
            }
            else
            {
                return string.Format("{0:n2} km", distance_m/1000);
            }

        }

        static public string DurationToString(double secs)
        {
            string result = "";

            if (secs < 0)
            {
                secs = -secs;
                result = "- ";
            }
            if (secs < 60) //secs
            {
                
                TimeSpan t = TimeSpan.FromSeconds(secs);
                result += string.Format("{0:D2}.{1:D3}",
                    t.Seconds,
                    t.Milliseconds);
            }
            else if (secs < 3600) // Hours:secs.ms
            {
                TimeSpan t = TimeSpan.FromSeconds(secs);
                result += string.Format("{0:D2}:{1:D2}.{2:D3}",
                    t.Minutes,
                    t.Seconds,
                    t.Milliseconds);
            }
            else if (secs < 21600 ) // = 3600*6 Kerbal days = 6 hours
            {

                TimeSpan t = TimeSpan.FromSeconds(secs);
                result += string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D3}",
                    t.Minutes,
                    t.Seconds,
                    t.Milliseconds);
            }
            else
            {
                int days = (int)secs / 21600;
                secs -= days * 21600;
                TimeSpan t = TimeSpan.FromSeconds(secs);
                result += string.Format("{0}d {1:D2}:{2:D2}:{3:D2}.{4:D3}",
                    days,
                    t.Hours,
                    t.Minutes,
                    t.Seconds,
                    t.Milliseconds);
            }

            return result;
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

    }
}