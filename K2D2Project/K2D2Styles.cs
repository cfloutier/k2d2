
using UnityEngine;
using SpaceWarp.API.UI;

using KTools.UI;
using KTools;

namespace K2D2.UI;

public class K2D2Styles
{
    private static bool guiLoaded = false;

    public static bool Init()
    {
        if (guiLoaded)
            return true;

        if (!KBaseStyle.Init())
            return false;

        // Load specific icon and style here
        k2d2_big_icon = AssetsLoader.loadIcon("k2d2_big_icon");

      //  guiLoaded = true;
        return true;
    }

    public static Texture2D k2d2_big_icon;
}
