using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

// CF : this direct dependency cause the dll to be needed during build
// it is not really needed, we can easyly hardcode the mode names
// during the introduction of K2D2 UI it cause me many trouble in naming
using K2D2;
using KSP.Game;
using ManeuverNodeController;
using NodeManager;
using SpaceWarp.API.Assets;
using System.Reflection;
using UnityEngine;

namespace K2D2;

public class K2D2OtherModsInterface
{
    public static K2D2OtherModsInterface instance = null;

    private static readonly GameInstance Game = GameManager.Instance.Game;

    ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.OtherModsInterface");

    // Reflection access variables for launching MNC & K2-D2
    public static bool mncLoaded, fpLoaded  = false;
    private PluginInfo _mncInfo, _fpInfo;
    private Version _mncMinVersion, _fpMinVersion;
    private int _mncVerCheck, _fpVerCheck;

    Type FPType, MNCType;
    PropertyInfo FPPropertyInfo, MNCPropertyInfo;
    MethodInfo FPCirculirizeMethodInfo, MNCLaunchMNCMethodInfo;
    object FPInstance, MNCInstance;
    Texture2D mncButtonTex, fpButtonTex;
    GUIContent MNCButtonTexCon, FPButtonTexCon;

    private bool _launchMNC;

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

            // Get _mncInfo buton Icon
            // mncButtonTex = AssetManager.GetAsset<Texture2D>($"{FlightPlanPlugin.Instance.SpaceWarpMetadata.ModID}/images/mnc_icon_white_50.png");
            mncButtonTex = AssetManager.GetAsset<Texture2D>($"{FlightPlanPlugin.Instance.Info.Metadata.GUID}/images/mnc_icon_white_50.png");
            // mncButtonTex = AssetManager.GetAsset<Texture2D>($"{FlightPlanPlugin.Instance.SpaceWarpMetadata.ModID}/images/mnc_icon_white_50.png");
            MNCButtonTexCon = new GUIContent(mncButtonTex, "Launch Maneuver Node Controller");

            // Reflections method to attempt the same thing more cleanly
            MNCType = Type.GetType($"ManeuverNodeController.ManeuverNodeControllerMod, {ManeuverNodeControllerMod.ModGuid}");
            MNCPropertyInfo = MNCType!.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            MNCInstance = MNCPropertyInfo.GetValue(null);
            MNCLaunchMNCMethodInfo = MNCPropertyInfo!.PropertyType.GetMethod("LaunchMNC");
        }
        // else _mncLoaded = false;
        Logger.LogInfo($"_mncLoaded = {mncLoaded}");

        Logger.LogInfo($"K2D2_Plugin.ModGuid = {K2D2_Plugin.ModGuid}");
        if (Chainloader.PluginInfos.TryGetValue(K2D2_Plugin.ModGuid, out _fpInfo))
        {
            _fpInfo = Chainloader.PluginInfos[K2D2_Plugin.ModGuid];

            fpLoaded = true;
            Logger.LogInfo("K2-D2 installed and available");
            Logger.LogInfo($"K2D2 = {_fpInfo}");
            _fpMinVersion = new Version(0, 8, 1);
            _fpVerCheck = _fpInfo.Metadata.Version.CompareTo(_fpMinVersion);
            Logger.LogInfo($"_k2d2VerCheck = {_fpVerCheck}");
            string _toolTip;
            if (_fpVerCheck >= 0) _toolTip = "Have K2-D2 Execute this node";
            else _toolTip = "Launch K2-D2";

            // Get K2-D2 buton Icon
            // k2d2ButtonTex = AssetManager.GetAsset<Texture2D>($"{FlightPlanPlugin.Instance.SpaceWarpMetadata.ModID}/images/k2d2_icon.png");
            fpButtonTex = AssetManager.GetAsset<Texture2D>($"{FlightPlanPlugin.Instance.Info.Metadata.GUID}/images/k2d2_icon.png");
            FPButtonTexCon = new GUIContent(fpButtonTex, _toolTip);

            FPType = Type.GetType($"K2D2.K2D2_Plugin, {K2D2_Plugin.ModGuid}");
            FPPropertyInfo = FPType!.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            FPInstance = FPPropertyInfo.GetValue(null);
            K2D2ToggleMethodInfo = FPPropertyInfo!.PropertyType.GetMethod("ToggleAppBarButton");
            K2D2FlyNodeMethodInfo = FPPropertyInfo!.PropertyType.GetMethod("FlyNode");
            K2D2GetStatusMethodInfo = FPPropertyInfo!.PropertyType.GetMethod("GetStatus");
        }
        // else _k2d2Loaded = false;
        Logger.LogInfo($"_k2d2Loaded = {fpLoaded}");

        instance = this;
    }

    public void CallMNC()
    {
        if (mncLoaded && _mncVerCheck >= 0)
        {
            MNCLaunchMNCMethodInfo!.Invoke(MNCPropertyInfo.GetValue(null), null);
        }
    }

    public void CallK2D2()
    {
        if (fpLoaded)
        {
            // Reflections method to attempt the same thing more cleanly
            if (_fpVerCheck < 0)
            {
                K2D2ToggleMethodInfo!.Invoke(FPPropertyInfo.GetValue(null), new object[] { true });
            }
            else
            {
                K2D2FlyNodeMethodInfo!.Invoke(FPPropertyInfo.GetValue(null), null);
                checkK2D2status = true;

                FPStatus.K2D2Status(FpUiController.ManeuverDescription, FlightPlanPlugin.Instance._currentNode.BurnDuration);
            }
        }
    }

    public void GetK2D2Status()
    {
        if (fpLoaded)
        {
            if (_fpVerCheck >= 0)
            {
                k2d2Status = (string)K2D2GetStatusMethodInfo!.Invoke(FPInstance, null);

                if (k2d2Status == "Done")
                {
                    if (FlightPlanPlugin.Instance._currentNode.Time < Game.UniverseModel.UniverseTime)
                    {
                        // NodeManagerPlugin.Instance.DeleteNodes(0);
                        // NodeManagerPlugin.Instance.DeleteNode(0);
                        NodeManagerPlugin.Instance.DeletePastNodes();
                    }
                    checkK2D2status = false;
                }
            }
        }
    }

    //public void OnGUI(ManeuverNodeData currentNode)
    //{
    //    GUILayout.BeginHorizontal();

    //    if (FPStyles.SquareButton("Make\nNode"))
    //        FlightPlanUI.Instance.MakeNode();

    //    if (mncLoaded && _mncVerCheck >= 0)
    //    {
    //        GUILayout.FlexibleSpace();
    //        if (FPStyles.SquareButton(FPStyles.MNCIcon))
    //            CallMNC();
    //    }

    //    if (k2d2Loaded && currentNode != null)
    //    {
    //        GUILayout.FlexibleSpace();
    //        if (FPStyles.SquareButton(FPStyles.K2D2BigIcon))
    //            CallK2D2();
    //    }
    //    GUILayout.EndHorizontal();

    //    if (checkK2D2status)
    //    {
    //        GetK2D2Status();
    //        GUILayout.BeginHorizontal();
    //        KTools.UI.UI_Tools.Label($"K2D2: {k2d2Status}");
    //        GUILayout.EndHorizontal();
    //    }
    //}
}