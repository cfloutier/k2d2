using UnityEngine;

using K2D2.Tools;
using K2D2.UI;

namespace K2D2;

public class K2D2Settings
{
    public static bool debug_mode
    {
        get => GeneralSettings.sfile.GetBool("debug_mode", false);
        set { GeneralSettings.sfile.SetBool("debug_mode", value); }
    }

    public static bool auto_next
    {
        get => GeneralSettings.sfile.GetBool("auto_next", true);
        set { GeneralSettings.sfile.SetBool("auto_next", value); }
    }


    public static void onGUI()
    {
        GUILayout.BeginHorizontal();
        if (UI_Tools.miniButton("Close Settings"))
        {
            K2D2_Plugin.Instance.settings_visible = false;
        }

        GUILayout.FlexibleSpace();
        // VERSION
        UI_Tools.Console($"v{K2D2_Plugin.ModVer}");
        GUILayout.FlexibleSpace();
        K2D2Settings.debug_mode = UI_Tools.miniToggle(K2D2Settings.debug_mode, "DBG MODE", "Debug mode open\nWIP features and verbose informations.");
        GUILayout.EndHorizontal();
        UI_Tools.Separator();
    }

}

