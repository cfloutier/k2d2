using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

using KSP.Game;
using ManeuverNodeController;
using FlightPlan;
using SpaceWarp.API.Assets;
using System.Reflection;
using UnityEngine;

namespace K2D2;

public class K2D2OtherModsInterface
{
    public static K2D2OtherModsInterface instance = null;

    // private static readonly GameInstance Game = GameManager.Instance.Game;

    ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.OtherModsInterface");

    // Reflection access variables for launching MNC & K2-D2
    public static bool mncLoaded, fpLoaded  = false;
    private PluginInfo _mncInfo, _fpInfo;
    private Version _mncMinVersion, _fpMinVersion;
    private int _mncVerCheck, _fpVerCheck;

    Type FPType, MNCType;
    PropertyInfo FPPropertyInfo, MNCPropertyInfo;
    MethodInfo CircularizeMethodInfo, MNCLaunchMNCMethodInfo;
    object FPInstance, MNCInstance;

    public void CheckModsVersions()
    {
        Logger.LogInfo($"ManeuverNodeControllerMod.ModGuid = {ManeuverNodeControllerMod.ModGuid}");
        if (Chainloader.PluginInfos.TryGetValue(ManeuverNodeControllerMod.ModGuid, out _mncInfo))
        {
            mncLoaded = true;
            Logger.LogInfo("Maneuver Node Controller installed and available");
            Logger.LogInfo($"_mncInfo = {_mncInfo}");
            // mncVersion = _mncInfo.Metadata.Version;
            _mncMinVersion = new Version(0, 8, 3);
            _mncVerCheck = _mncInfo.Metadata.Version.CompareTo(_mncMinVersion);
            Logger.LogInfo($"_mncVerCheck = {_mncVerCheck}");

            // Reflections method to attempt the same thing more cleanly
            MNCType = Type.GetType($"ManeuverNodeController.ManeuverNodeControllerMod, {ManeuverNodeControllerMod.ModGuid}");
            MNCPropertyInfo = MNCType!.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            MNCInstance = MNCPropertyInfo.GetValue(null);
            MNCLaunchMNCMethodInfo = MNCPropertyInfo!.PropertyType.GetMethod("LaunchMNC");
        }
        // else _mncLoaded = false;
        Logger.LogInfo($"_mncLoaded = {mncLoaded}");

        Logger.LogInfo($"FlightPlanPlugin.ModGuid = {FlightPlanPlugin.ModGuid}");
        if (Chainloader.PluginInfos.TryGetValue(FlightPlanPlugin.ModGuid, out _fpInfo))
        {
            _fpInfo = Chainloader.PluginInfos[FlightPlanPlugin.ModGuid];

            fpLoaded = true;
            Logger.LogInfo("FlightPlan installed and available");
            Logger.LogInfo($"FlightPlan = {_fpInfo}");
            _fpMinVersion = new Version(0, 9, 1);
            _fpVerCheck = _fpInfo.Metadata.Version.CompareTo(_fpMinVersion);
            Logger.LogInfo($"_fpVerCheck = {_fpVerCheck}");

            FPType = Type.GetType($"FlightPlan.FlightPlanPlugin, {FlightPlanPlugin.ModGuid}");
            FPPropertyInfo = FPType!.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            
            CircularizeMethodInfo = FPPropertyInfo!.PropertyType.GetMethod("Circularize");
        }

        Logger.LogInfo($"fpLoaded = {fpLoaded}");

        instance = this;
    }

    public bool Circularize(double burnUT, double burnOffsetFactor = -0.5)
    {
        if (fpLoaded && _fpVerCheck >= 0)
        {
            FPInstance = FPPropertyInfo.GetValue(null);

            K2D2_Plugin.logger.LogMessage($"Circularize at UT {burnUT} s (+-{burnOffsetFactor})");
            return (bool) CircularizeMethodInfo!.Invoke(FPInstance, [burnUT, burnOffsetFactor]);
        }

        return false;
    }
}