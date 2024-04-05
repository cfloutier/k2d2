

using K2UI;
using K2UI.Tabs;
using KTools;
using UnityEngine.UIElements;




namespace K2D2.UI;


public class AboutUI : K2Page
{
    public AboutUI()
    {
        code = "about";
    }

    void onSettingsChanged(bool visible)
    {
        enabled = visible;
    }


    public override bool onInit()
    {
        // enable only if settings is visible
        GlobalSetting.settings_visible.listen(onSettingsChanged);

        Label label = panel.Q<Label>("version");
        label.text = K2D2_Plugin.ModVer;

        label = panel.Q<Label>("thanks");
        label.text = @"Thanks to <b>KSP2 Modding Society</b> for the huge help and support. 

Especially <b>@Munix</b> for the UITK for KSP2 mod and It's great project template.
<b>@Schlorats</b> for the Flight Plan Mod and for the communicative enthusiasm.
<b>@DunaColonist</b> for it's HUD display that help me a lot for debugging dock pilot.
and <b>@Cheese</b> of course for creating the Modding society itself.";
    
        panel.Q<Button>("reset_all").listenClick( () => SettingsFile.Instance.Reset());
        panel.Q<ToggleButton>("debug").Bind(K2D2Settings.debug_mode);

        return true;
    }
}