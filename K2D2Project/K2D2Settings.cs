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

    public static K2D2SettingsUI ui = null;

    public static void onGUI()
    {
        if (ui == null)
            ui = new K2D2SettingsUI();

        ui.onGUI();
    }

    
    public static void CloseUI()
    {
        if (UI_Tools.miniButton("Close Settings"))
        {
            K2D2_Plugin.Instance.settings_visible = false;
        }
    } 

}


public class K2D2SettingsUI
{
    public FoldOut accordion = new FoldOut();

    void mainUI()
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
            K2D2Settings.ui_size = 1f;
        //if (UI_Tools.SmallButton("1.5"))
        //    ui_size = 1.5f;
        if (UI_Tools.SmallButton("x2"))
            K2D2Settings.ui_size = 2f;
        if (UI_Tools.SmallButton("x4"))
            K2D2Settings.ui_size = 4f;
        GUILayout.EndHorizontal();
    }

    public void onGUI()
    {
        if (accordion.Count == 0)
        {
            accordion.addChapter("Main Settings", mainUI);
            accordion.singleChapter = true;
        }

        accordion.OnGUI();


        UI_Tools.Separator();
    }
}
