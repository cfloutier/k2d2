using BepInEx.Logging;
using K2D2.Controller;
using KSP.Sim.DeltaV;
using KTools.UI;
using UnityEngine;
namespace K2D2.KSPService;

/// Simple class used to compute Burned DV
public class BurndV : BaseController
{
    public ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.SettingsFile");

    KSPVessel current_vessel;

    public BurndV()
    {
        current_vessel = K2D2_Plugin.Instance.current_vessel;
    }

    public float burned_dV = 0;

    public void reset()
    {
        burned_dV = 0;
    }


    public Vector3 actual_thrust;
    public float actual_dv;

    public Vector3 full_thrust;
    public float full_dv;

    public override void FixedUpdate()
    {
        burned_dV += actual_dv * Time.fixedDeltaTime;
    }

    public override void LateUpdate()
    {
        Compute_Thrust();
    }

    // it is the way engine burning is computed in KSP
    public bool Engine_Running(DeltaVEngineInfo engine_info)
    {
        return (engine_info.Engine.EngineIgnited && engine_info.Engine.RequestedMassFlow > 0f);
    }

    float compute_full_thrust(DeltaVEngineInfo engineInfo)
    {
        var partref = engineInfo.PartInfo.PartRef;
        float staticPressureAtm = partref.StaticPressureAtm;
        if (staticPressureAtm > 0f)
        {
            return engineInfo.Engine.MaxThrustOutputAtm(runningActive: false, useThrustLimiter: true, staticPressureAtm, partref.AtmosphericTemperature, partref.AtmDensity);
        }
        else if (engineInfo.RequiresAir)
        {
            return 0f;
        }
        else
        {
            return engineInfo.Engine.MaxThrustOutputVac();
        }
    }

    public void Compute_Thrust()
    {
        if (current_vessel.VesselComponent == null) return;
        VesselDeltaVComponent delta_v = current_vessel.VesselComponent.VesselDeltaV;
        if (delta_v == null) return;

        var totalMass = current_vessel.VesselComponent.totalMass;

        actual_thrust = Vector3.zero;
        full_thrust = Vector3.zero;
        List<DeltaVEngineInfo> engineInfos = delta_v.EngineInfo;
        for (int i = 0; i < engineInfos.Count; i++)
        {
            DeltaVEngineInfo engineInfo = engineInfos[i];

            Vector3 vector = ((engineInfo.Engine != null) ? engineInfo.Engine.ThrustDirRelativePartWorldSpace : (1f * Vector3.back));

            actual_thrust += vector * engineInfo.Engine.FinalThrustValue;
            full_thrust += vector * compute_full_thrust(engineInfo);
        }

        actual_dv = (float)(actual_thrust.magnitude / totalMass);
        full_dv = (float)(full_thrust.magnitude / totalMass);
    }

    public override void onGUI()
    {
        if (current_vessel.VesselComponent == null) return;
        VesselDeltaVComponent delta_v = current_vessel.VesselComponent.VesselDeltaV;
        if (delta_v == null)
        {
            UI_Tools.Error("NO VesselDeltaVComponent");
            return;
        }
        //List<DeltaVEngineInfo> engineInfos = delta_v.EngineInfo;

        var vehicle = current_vessel.VesselVehicle;
        if (vehicle == null) return;
        var mainThrottle = vehicle.mainThrottle;

        //UI_Tools.Console($"nb_engines {engineInfos.Count}  ");


        UI_Tools.Console($"mainThrottle {mainThrottle}");
        // UI_Tools.Console($"actual_thrust  {Tools.printVector(actual_thrust)}  ");
        UI_Tools.Console($"actual_dv  {actual_dv:n5}  ");


        // UI_Tools.Console($"full_thrust  {Tools.printVector(full_thrust)}  ");
        UI_Tools.Console($"full_dv  {full_dv:n5}  ");
        UI_Tools.Console($"burned_dV <b> {burned_dV:n5} </b>");
        if (GUILayout.Button("Reset"))
            burned_dV = 0;
    }
}
