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
        static TimeWarp time_warp =>  GameManager.Instance?.Game?.ViewController?.TimeWarp;

        static public int CurrentRateIndex
        {
            get
            {
                if (time_warp == null) return 0;
                return time_warp.CurrentRateIndex;
            }
        }
        static public float CurrentRate
        {
            get
            {
                if (time_warp == null) return 0;
                return time_warp.CurrentRate;
            }
        }


        public static float indexToRatio(int index)
        {
            var levels = time_warp.GetWarpRates();
            if (index < 0 || index >= levels.Length) return 0f;

            return levels[index].TimeScaleFactor;
        }

        public static int ratioToIndex(float ratio)
        {
            var levels = time_warp.GetWarpRates();
            for (int index = 0; index < levels.Length; index++ )
            {
                float factor = levels[index].TimeScaleFactor;
                if (ratio < factor)
                    return index;
            }

            return levels.Length -1;
        }

        public static void SetRateIndex(int rate_index, bool instant)
        {
            if (time_warp == null) return;
            if (rate_index != time_warp.CurrentRateIndex)
                time_warp.SetRateIndex(rate_index, instant);
        }
    }

}

