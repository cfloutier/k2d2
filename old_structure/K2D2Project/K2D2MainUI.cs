
using UnityEngine;
using BepInEx.Logging;
using K2D2.Controller;
using K2D2.InfosPages;

using KTools;
using KTools.UI;

namespace K2D2;

public class K2D2MainUI
{
    public ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.MainUI");

    bool init_done = false;

    TabsUI tabs = new TabsUI();

    public K2D2MainUI()
    {

    }

    public void Update()
    {
        if (init_done)
            tabs.Update();
    }

    public void onGUI()
    {
        if (!init_done)
        {
            tabs.pages.Add(NodeExecute.Instance);
          //  tabs.pages.Add(CircleController.Instance);

            tabs.pages.Add(LandingController.Instance);
            tabs.pages.Add(DroneController.Instance);

            tabs.pages.Add(LiftController.Instance);
            tabs.pages.Add(AttitudeController.Instance);
            // tabs.pages.Add(WarpController.Instance);
            // tabs.pages.Add(TestObjects.Instance);

            tabs.pages.Add(DockingAssist.Instance);
            tabs.pages.Add(StagingController.Instance);

            // tabs.pages.Add(new TestDisablePage());
            // tabs.pages.Add(new TestFlightPlan());
            // tabs.pages.Add(new FindSecrets());

            // waiting for mole
            // pages.Add(SimpleManeuverController.Instance);
            tabs.pages.Add(new OrbitInfos());
            tabs.pages.Add(new K2D2.InfosPages.SASInfos());
            tabs.pages.Add(new VesselInfos());

            tabs.Init();

            init_done = true;
        }

        tabs.onGUI();
    }
}