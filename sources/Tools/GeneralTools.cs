﻿using System;
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

                return string.Format("- {0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                    t.Hours,
                    t.Minutes,
                    t.Seconds,
                    t.Milliseconds);
                }
            else
            {
                TimeSpan t = TimeSpan.FromSeconds(secs);

                return string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
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