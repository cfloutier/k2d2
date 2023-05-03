
using BepInEx.Logging;
using K2D2.KSPService;

using K2D2.UI;
namespace K2D2.Controller;

/// a simple test page to add the simple circle maneuvers node made by @mole
class CircleController : BaseController
{
    public ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.CircleController");

    ManeuverCreator maneuver_creator = new ManeuverCreator();

    public static CircleController Instance { get; set; }

    public CircleController()
    {
        Instance = this;
        debug_mode_only = false;
        name = "Circle";
    }

    public override void Update()
    {
        maneuver_creator.Update();
    }

    public override void onGUI()
    {
        if (UI_Tools.SmallButton("Circularize At Ap"))
        {
            maneuver_creator.CircularizeOrbitApoapsis();
        }

        if (UI_Tools.SmallButton("Circularize At Pe"))
        {
            maneuver_creator.CircularizeOrbitPeriapsis();
        }

        if (AutoExecuteManeuver.Instance.canStart())
        {
            if (UI_Tools.SmallButton("Execute"))
            {
                AutoExecuteManeuver.Instance.Start();
            }
        }
    }
}
