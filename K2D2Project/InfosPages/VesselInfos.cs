

using UnityEngine;
using KSP.Sim;
using K2D2.Controller;

using K2D2.UI;

namespace K2D2.InfosPages;

class VesselInfos : BaseController
{
    public VesselInfos()
    {
        debug_mode = true;
        Name = "Vessel Infos";
    }



    public override void onGUI()
    {
        if (K2D2_Plugin.Instance.settings_visible)
        {
            Settings.onGUI();
            return;
        }

        var vehicle = K2D2_Plugin.Instance.current_vessel.VesselVehicle;
        var vessel_component = K2D2_Plugin.Instance.current_vessel.VesselComponent;

        if (vehicle == null)
        {
            UI_Tools.Console("No vessel !");
            return;
        }

        UI_Tools.Console($"mainThrottle {vehicle.mainThrottle}");
        UI_Tools.Console($"pitch {vehicle.pitch:n3} yaw {vehicle.yaw:n3} roll {vehicle.roll:n3}");

        // UI_Tools.Console($"AltitudeFromTerrain {vehicle.AltitudeFromTerrain:n2} m");
        UI_Tools.Console($"Corrected alt : {K2D2_Plugin.Instance.current_vessel.GetApproxAltitude()} m");
        UI_Tools.Console($"Landed : {vessel_component.Landed}");
        UI_Tools.Console($"Lat {vehicle.Latitude:n2} Lon {vehicle.Longitude:n2}");
        UI_Tools.Console($"IsInAtmosphere {vehicle.IsInAtmosphere}");
        UI_Tools.Console($"Body {K2D2_Plugin.Instance.current_vessel.currentBody().Name}");

        //var body = K2D2_Plugin.Instance.current_vessel.currentBody();
        //var coord = body.coordinateSystem;
        //var body_location = Rotation.Reframed(vehicle.Rotation, coord);

        //  UI_Tools.Console($"coordinate_system {vehicle.Rotation.coordinateSystem}");
        //UI_Tools.Console($"body_location {StrTool.VectorToString(body_location.localRotation.eulerAngles)}");
    }
}
