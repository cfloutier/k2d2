using UnityEngine;

using K2D2.Tools;
using K2D2.UI;

namespace K2D2;

public class K2D2Settings
{
    public static bool debug_mode
    {
        get => Settings.sfile.GetBool("debug_mode", false);
        set { Settings.sfile.SetBool("debug_mode", value); }
    }


    public static bool auto_next
    {
        get => Settings.sfile.GetBool("auto_next", true);
        set { Settings.sfile.SetBool("auto_next", value); }
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


public class Settings
{
    public static SettingsFile sfile = null;
    public static string s_settings_path;

    public static void Init(string settings_path)
    {
        sfile = new SettingsFile(settings_path);
    }

    // each setting is defined by an accessor pointing on s_settings_file
    // the convertion to type is made here
    // this way we can have any kind of settings without hard work



    public static int window_x_pos
    {
        get => sfile.GetInt("window_x_pos", 70);
        set { sfile.SetInt("window_x_pos", value); }
    }

    public static int window_y_pos
    {
        get => sfile.GetInt("window_y_pos", 50);
        set { sfile.SetInt("window_y_pos", value); }
    }

    public static int main_tab_index
    {
        get { return sfile.GetInt("main_tab_index", 0); }
        set { sfile.SetInt("main_tab_index", value); }
    }

}





