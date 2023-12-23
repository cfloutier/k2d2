


using K2D2.KSPService;
using KTools;
using KTools.UI;
using BepInEx.Logging;

using KSP.Sim.impl;
using KSP.Sim;
using KSP.Sim.ResourceSystem;
namespace K2D2.Controller;

using UnityEngine;

public class StagingSettings
{
    public static bool auto_staging
    {
        get => KBaseSettings.sfile.GetBool("staging.auto_staging", true);
        set
        {
            KBaseSettings.sfile.SetBool("staging.auto_staging", value);
        }
    }

    public static float freeze_duration
    {
        get => KBaseSettings.sfile.GetFloat("staging.freeze_duration", 1f);
        set
        {
            // value = Mathf.Clamp(value, 0 , 1);
            KBaseSettings.sfile.SetFloat("staging.freeze_duration", value);
        }
    }
    
    public static void settings_UI()
    {
        UI_Tools.Console("Next Stage if at least one tank is empty");
        auto_staging = UI_Tools.Toggle(auto_staging, "Auto Staging");

        UI_Tools.Console("Freeze K2D2 Pilots during staging");
        GUILayout.BeginHorizontal();
       
        UI_Tools.Label("Freeze Duration (s): ");
        
        freeze_duration = UI_Fields.FloatField("freeze_duration", freeze_duration, 1);
        GUILayout.EndHorizontal();
    }
}

/// <summary>
/// do autoStage
/// </summary>
public class StagingController : BaseController
{
    public static K2D2_Plugin Instance { get; private set; }

    public ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.StagingController");

    public bool is_staging = false;

    double end_time = -1;
    double current_time = -1;
    double Full_Stage_Percentage = -1;
    double Min_Stage_Percentage = -1;

    void reset()
    {
        is_staging = false;
        end_time = -1;
        current_time = -1;
    }

    void start()
    {
        is_staging = true;
        // record time
        end_time = GeneralTools.Game.UniverseModel.UniverseTime + StagingSettings.freeze_duration;
        current_time = GeneralTools.Game.UniverseModel.UniverseTime;
    }

    public override void Update()
    {
        CheckStaging();
    }

    Dictionary<ResourceDefinitionID, ContainedResourceData> fuelCapacity = null;
    VesselComponent vessel_component = null;
        
    public void CalculateVesselStageFuel()
    {
        var current_vessel = KSPVessel.current;

        logger.LogMessage("CalculateVesselStageFuel");

        Full_Stage_Percentage = -1.0;
        Min_Stage_Percentage = -1.0;

        // not staging, check fuel
        vessel_component = current_vessel.VesselComponent;
        if (vessel_component == null)
            return;

        if (fuelCapacity == null)
            fuelCapacity = new Dictionary<ResourceDefinitionID, ContainedResourceData>();
        else
            fuelCapacity.Clear();

        if (vessel_component.SimulationObject.objVesselBehavior == null || vessel_component.SimulationObject.objVesselBehavior.parts == null)
        {
            return;
        }
        foreach (PartComponent part in vessel_component.SimulationObject.PartOwner.Parts)
        {
            if (part == null || !part.TryGetModule<PartComponentModule_Engine>(out var module) || !module.EngineIgnited)
            {
                continue;
            }
            List<ContainedResourceData> containedResourceData = module.GetContainedResourceData();
            if (containedResourceData == null)
            {
                continue;
            }
            for (int i = 0; i < containedResourceData.Count; i++)
            {
                if (!fuelCapacity.ContainsKey(containedResourceData[i].ResourceID))
                {
                    ContainedResourceData value = default(ContainedResourceData);
                    value.CapacityUnits = containedResourceData[i].CapacityUnits;
                    value.StoredUnits = (module.IsPropellantStarved ? 0.0 : Math.Abs(containedResourceData[i].StoredUnits));
                    fuelCapacity.Add(containedResourceData[i].ResourceID, value);
                }
                else
                {
                    ContainedResourceData value2 = fuelCapacity[containedResourceData[i].ResourceID];
                    value2.CapacityUnits += containedResourceData[i].CapacityUnits;
                    value2.StoredUnits += (module.IsPropellantStarved ? 0.0 : Math.Abs(containedResourceData[i].StoredUnits));
                    fuelCapacity[containedResourceData[i].ResourceID] = value2;
                }
            }
        }
        if (fuelCapacity.Count > 0)
        {
            Full_Stage_Percentage = 0.0;
            Min_Stage_Percentage = 1;
            double num = 0.0;
            double num2 = 0.0;

            foreach (KeyValuePair<ResourceDefinitionID, ContainedResourceData> item in fuelCapacity)
            {
                if (item.Value.CapacityUnits != 0)
                {
                    double container_ratio =  item.Value.StoredUnits / item.Value.CapacityUnits;
                    if (container_ratio < Min_Stage_Percentage)
                        Min_Stage_Percentage = container_ratio;
                }

                num += item.Value.CapacityUnits;
                num2 += item.Value.StoredUnits;
            }
            if (num > 0.0)
            {
                Full_Stage_Percentage = num2 / num;
            }
            Full_Stage_Percentage *= 100.0;
            Min_Stage_Percentage *= 100.0; 
            logger.LogMessage($"CalculateVesselStageFuel {Min_Stage_Percentage:n2}%");
        }
    }


    public bool CheckStaging()
    {
        // return true if staging in progress
        if (!StagingSettings.auto_staging)
            return false;

        CalculateVesselStageFuel();

        if (is_staging)
        {
           // check time
           current_time = GeneralTools.Game.UniverseModel.UniverseTime;
           if (current_time >= end_time)
           {
               // if over reset flag
               reset();
           }

           return is_staging;
        }

        if (vessel_component == null)
            return false;

        if (Min_Stage_Percentage == 0)
        {
            // start timer
            start();
            // Activate next stage 
            vessel_component.ActivateNextStage();
        }

        return is_staging;
    }

    public override void onGUI()
    {
        //if (!is_staging)
        //    return;

        // UI_Tools.Title("--- Staging ---");

        // var StageFuelPercentage
        UI_Tools.Console($"Total : {Full_Stage_Percentage:n2}%");
        UI_Tools.Console($"Min : {Min_Stage_Percentage:n2}%");
        if (Min_Stage_Percentage == 0)
        {
            UI_Tools.Warning($"Stage NOW !");
        }

        if (end_time > 0 )
        {   
            var remaining = end_time - current_time; 
            UI_Tools.Console($"Timer : {remaining:n2} s");
        }
    }








}