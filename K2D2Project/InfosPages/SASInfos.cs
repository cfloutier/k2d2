

using UnityEngine;
using KSP.Sim;
using K2D2.Controller;
using K2D2.UI;
namespace K2D2.InfosPages;
class SASInfos : BaseController
{
    public SASInfos()
    {
        debug_mode = true;
        Name = "SAS Infos";
    }


    public override void onGUI()
    {
        if (K2D2_Plugin.Instance.settings_visible)
        {
            Settings.onGUI();
            return;
        }

        var current_vessel = K2D2_Plugin.Instance.current_vessel.VesselComponent;

        var sas = current_vessel.Autopilot.SAS;
        if (sas == null)
        {
            UI_Tools.Console("NO SAS");
            return;
        }

        UI_Tools.Console($"sas.dampingMode {sas.dampingMode}");
        UI_Tools.Console($"sas.ReferenceFrame {sas.ReferenceFrame}");
        UI_Tools.Console($"sas.AutoTune {sas.AutoTune}");
        UI_Tools.Console($"sas.lockedMode {sas.lockedMode}");
        UI_Tools.Console($"sas.LockedRotation {StrTool.VectorToString(sas.LockedRotation.eulerAngles)}");

        UI_Tools.Console($"sas.TargetOrientation {StrTool.VectorToString(sas.TargetOrientation)}");
        UI_Tools.Console($"sas.PidLockedPitch {sas.PidLockedPitch}");
        UI_Tools.Console($"sas.PidLockedRoll {sas.PidLockedRoll}");
        UI_Tools.Console($"sas.PidLockedYaw {sas.PidLockedYaw}");
    }
}
