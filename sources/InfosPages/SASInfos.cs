

using UnityEngine;
using KSP.Sim;
using K2D2.Controller;

namespace K2D2.InfosPages
{
    class SASInfos : BaseController
    {
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
                GUILayout.Label("NO SAS");
                return;
            }

            GUILayout.Label($"sas.dampingMode {sas.dampingMode}");
            GUILayout.Label($"sas.ReferenceFrame {sas.ReferenceFrame}");
            GUILayout.Label($"sas.AutoTune {sas.AutoTune}");
            GUILayout.Label($"sas.lockedMode {sas.lockedMode}");
            GUILayout.Label($"sas.LockedRotation {StrTool.VectorToString(sas.LockedRotation.eulerAngles)}");

            GUILayout.Label($"sas.TargetOrientation {StrTool.VectorToString(sas.TargetOrientation)}");
            GUILayout.Label($"sas.PidLockedPitch {sas.PidLockedPitch}");
            GUILayout.Label($"sas.PidLockedRoll {sas.PidLockedRoll}");
            GUILayout.Label($"sas.PidLockedYaw {sas.PidLockedYaw}");
        }
    }
}