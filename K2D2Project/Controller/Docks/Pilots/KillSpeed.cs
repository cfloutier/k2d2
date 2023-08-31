using K2D2.KSPService;
using KSP.Sim;
using KSP.Sim.Maneuver;
using KTools;
using KTools.UI;
using UnityEngine;
using static KSP.Api.UIDataPropertyStrings.View;

namespace K2D2.Controller.Docks.Pilots;

/// <summary>
/// rotation used for docking
/// </summary>
public class KillSpeed : ExecuteController
{
    TurnTo turnTo =  new TurnTo();


    public override void Start()
    {
        finished = false;
        turnTo.StartRetroSpeed();
    }

    public override void Update()
    {
        turnTo.Update();
    }

    public override void onGUI()
    {
        UI_Tools.Warning("Kill Target Speed");

        turnTo.onGUI();
    }
}
