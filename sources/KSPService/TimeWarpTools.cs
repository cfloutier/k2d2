using System;
using KSP.Sim;
using KSP.Sim.impl;
using KSP.Sim.Maneuver;
using KSP.Sim.State;
using KSP.Game;

using System.Reflection;

namespace K2D2
{
    public class TimeWarpTools
    {
        public static TimeWarp time_warp()
        {
            return GameManager.Instance?.Game?.ViewController?.TimeWarp;
        }

        public static float indexToRatio(int index)
        {
            var levels = time_warp().GetWarpRates();
            if (index < 0 || index >= levels.Length) return 0f;

            return levels[index].TimeScaleFactor;
        }

        public static int ratioToIndex(float ratio)
        {
            var levels = time_warp().GetWarpRates();
            for (int index = 0; index < levels.Length; index++ )
            {
                float factor = levels[index].TimeScaleFactor;
                if (ratio < factor)
                    return index;
            }

            return levels.Length -1;
        }
    }

}

