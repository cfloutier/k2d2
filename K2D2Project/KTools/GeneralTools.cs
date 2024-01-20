﻿using System.Text.RegularExpressions;
using KSP.Sim.Maneuver;
using KSP.Game;

namespace KTools;

public static class GeneralTools
{
    public static GameInstance Game => GameManager.Instance == null ? null : GameManager.Instance.Game;

    public static double Current_UT => Game.UniverseModel.UniverseTime;

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

    public static int ClampInt(int value, int min, int max)
    {
        return (value < min) ? min : (value > max) ? max : value;
    }

    public static Vector3d correctEuler(Vector3d euler)
    {
        Vector3d result = euler;
        if (result.x > 180)
            result.x -= 360;
        if (result.y > 180)
            result.y -= 360;
        if (result.z > 180)
            result.z -= 360;

        return result;
    }

    public static double remainingStartTime(ManeuverNodeData node)
    {
        var dt = node.Time - GeneralTools.Game.UniverseModel.UniverseTime;
        return dt;
    }

    public static float diffAngle(float angle_1, float angle_2)
    {
        float diff = angle_1 - angle_2;
        if (diff > 180) diff -= 360;
        if (diff < -180) diff += 360;
        return diff;
    }


    public static Guid createGuid()
    {
        return Guid.NewGuid();
    }

}
