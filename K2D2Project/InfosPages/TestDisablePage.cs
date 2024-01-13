

using K2D2.Controller;
using KTools.UI;
using UnityEngine;
namespace K2D2.InfosPages;
class TestDisablePage : BaseController
{
    public TestDisablePage()
    {
        debug_mode_only = true;
        name = "TestDisablePage";
    }

    public override void onGUI()
    {

        K2D2_Plugin plugin = K2D2_Plugin.Instance;
        var pilots_names = plugin.GetPilotsNames();
        foreach(var pilot_name in pilots_names)
        {
            bool is_enabled = plugin.isPilotEnabled(pilot_name);
            bool result = UI_Tools.Toggle(plugin.isPilotEnabled(pilot_name), pilot_name);
            if (result != is_enabled)
            {
                plugin.EnablePilot(pilot_name, result);
            }
        }

        GUILayout.BeginHorizontal();
        if (UI_Tools.Button("Disable all"))
            plugin.EnableAllPilots(false);

        if (UI_Tools.Button("Enable all"))
            plugin.EnableAllPilots(true);

        GUILayout.EndHorizontal();


       
       
    }
}
