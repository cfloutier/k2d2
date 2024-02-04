using AmplifyImpostors;
using K2D2.KSPService;
using KSP.Sim;
using KTools.UI;
using UnityEngine;
using BepInEx.Logging;

namespace K2D2.Controller;

public class FindSecrets : BaseController
{
    KSPVessel current_vessel;

    public FindSecrets()
    {
        // logger.LogMessage("LandingController !");
        current_vessel = K2D2_Plugin.Instance.current_vessel;
        debug_mode_only = true;
        name = "Secrets";
    }

    public override void Update()
    {

    }

    /*

    Ok trouvé. pour la lune, tout est dans un GameObject nommé Celestial.Mun.Simulation/

    on peux tenter de se baser sur le nom du body en cours pour récupérer cet objet.

    ce go contients un fils nommé objects qui contiens la liste de GameObjects de la planète.

    ils contiennent tous un PQSObject

    */

    public ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.LandingController");

    List<GameObject> objects = new List<GameObject>();

    void find()
    {
        objects.Clear();

        string body_name = current_vessel.VesselComponent.mainBody.Name;
        last_body_name = body_name;
        logger.LogMessage($"{body_name} !");
        string main_go_name = $"Celestial.{body_name}.Simulation";
        logger.LogMessage($"search for '{main_go_name}'");
        //GameObject main_go = GameObject.Find(main_go_name);
        var celestian = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == main_go_name).ToList();
        if (celestian == null || celestian.Count() == 0)
        {
            logger.LogMessage($"error finding '{main_go_name}'");
            return;
        }

        // Drawing.
        GameObject main_go = celestian[0];
        main_go = main_go.transform.Find("objects").gameObject;

        foreach(Transform child in main_go.transform)
        {
            objects.Add(child.gameObject);
        }
    }

    string last_body_name = "";

    Vector2 scrollPos = Vector2.zero;

    public override void onGUI()
    {
        if (UI_Tools.BigButton("find"))
        {
            find();
        }

        if (last_body_name != current_vessel.VesselComponent.mainBody.Name)
        {
            objects.Clear();
        }

        if (objects != null && objects.Count() > 0)
        {
            scrollPos = UI_Tools.BeginScrollView(scrollPos, 200);

            foreach(GameObject obj in objects)
            {
                UI_Tools.Console(obj.name);
            }

            GUILayout.EndScrollView();
        }

        /*var body = current_vessel.VesselComponent.mainBody;
        var transforms = body.transform.children;
        foreach (var child in transforms)
        {
            var pos = child.Position;
            var direction = pos - current_vessel.VesselComponent.transform.Position;
            var Upcoords = current_vessel.VesselVehicle.Up.coordinateSystem;

            var Surface_Dir = Vector.Reframed(direction, Upcoords).vector;
            var North = Vector.Reframed(current_vessel.VesselVehicle.North, Upcoords).vector;
            var Up = current_vessel.VesselVehicle.Up.vector;

            // var Up = Vector.Reframed(current_vessel.VesselVehicle.Up, Upcoords).vector;
            var heading = (float)-Vector3d.SignedAngle(Surface_Dir.normalized, North, Up);
            GUILayout.BeginHorizontal();
            UI_Tools.Console($"head. {heading}°");
            UI_Tools.Console($"dist. {StrTool.DistanceToString(Surface_Dir.magnitude)}");

            GUILayout.EndHorizontal();

        }*/


    }


}
