using UnityEngine;


using KTools;
using KTools.UI;

namespace K2D2;

public class K2D2Settings
{
    public static bool debug_mode
    {
        get => KBaseSettings.sfile.GetBool("debug_mode", false);
        set { KBaseSettings.sfile.SetBool("debug_mode", value); }
    }

    public static bool auto_next
    {
        get => KBaseSettings.sfile.GetBool("auto_next", true);
        set { KBaseSettings.sfile.SetBool("auto_next", value); }
    }

    public static float ui_size
    {
        get
        {
            // avoid ui_size < 0.5f
            var value = KBaseSettings.sfile.GetFloat("ui_size", 1);
            if (value < 0.5f)
                value = 0.5f;

            return value;
        }
        set
        {
            if (value < 0.5f)
                value = 0.5f;

            KBaseSettings.sfile.SetFloat("ui_size", value);
        }
    }

    public static void onGUI()
    {
        GUILayout.BeginHorizontal();
 
        // VERSION
        UI_Tools.Console($"K2D2 v{K2D2_Plugin.ModVer}");
        GUILayout.FlexibleSpace();
        K2D2Settings.debug_mode = UI_Tools.miniToggle(K2D2Settings.debug_mode, "DBG MODE", "Debug mode open\nWIP features and verbose informations.");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        UI_Tools.Label("UI Size");
        // does not work with decimals
        //if (UI_Tools.SmallButton("0.75"))
        //    ui_size = 0.75f;
        if (UI_Tools.SmallButton("x1"))
            ui_size = 1f;
        //if (UI_Tools.SmallButton("1.5"))
        //    ui_size = 1.5f;
        if (UI_Tools.SmallButton("x2"))
            ui_size = 2f;
        if (UI_Tools.SmallButton("x4"))
            ui_size = 4f;
        GUILayout.EndHorizontal();

        if (UI_Tools.miniButton("Close Settings"))
        {
            K2D2_Plugin.Instance.settings_visible = false;
        }

        UI_Tools.Separator();
    }
}

