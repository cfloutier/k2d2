using K2D2.KSPService;
using KTools;

using BepInEx.Logging;
using KSP.Sim.impl;
using KSP.Sim.ResourceSystem;

namespace K2D2.Controller;

public class StagingSettings
{

    public static Setting<bool> auto_staging = new("staging.auto_staging", false);
   

    public static Setting<float> freeze_duration = new ("staging.freeze_duration", 1f);

    
    // public static void settings_UI()
    // {
    //     UI_Tools.Console("Next Stage if at least one tank is empty");
    //     auto_staging = UI_Tools.Toggle(auto_staging, "Auto Staging");

    //     UI_Tools.Console("Freeze K2D2 Pilots during staging");
    //     GUILayout.BeginHorizontal();
       
    //     UI_Tools.Label("Freeze Duration (s): ");
        
    //     freeze_duration = UI_Fields.FloatField("freeze_duration", freeze_duration, 1);
    //     GUILayout.EndHorizontal();
    // }
}

/// <summary>
/// do autoStage
/// </summary>
public class StagingController : BaseController
{
    public static StagingController Instance { get; private set; }

    public StagingController()  
    {
        Instance = this;
        debug_mode_only = false;
        name = "Staging";
        K2D2PilotsMgr.Instance.RegisterPilot("Staging", this);
    } 
    
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
        end_time = GeneralTools.Game.UniverseModel.UniverseTime + StagingSettings.freeze_duration.Value;
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

        // logger.LogMessage("CalculateVesselStageFuel");

        Full_Stage_Percentage = -1.0;
        Min_Stage_Percentage = 1.0;

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

        // var all_connection = vessel_component.SimulationObject.PartOwner.VirtualConnections;
        // List<VirtualConnection> fuel_lines = new List<VirtualConnection>();
        // foreach(var connection in all_connection)
        // {
        //     if (connection.relationshipType == PartRelationshipType.FuelLine)
        //         fuel_lines.Add(connection);
        // }

        // var flow_graph = vessel_component.SimulationObject.PartOwner.FlowGraph;


        // List all parts
        foreach (PartComponent part in vessel_component.SimulationObject.PartOwner.Parts)
        {

            // stop on any ignited engine
            if (part == null || !part.TryGetModule<PartComponentModule_Engine>(out var module) || !module.EngineIgnited)
            {
                continue;
            }

            // list all containers
            List<ContainedResourceData> containedResourceData = module.GetContainedResourceData();
            if (containedResourceData == null)
            {
                continue;
            }
            for (int i = 0; i < containedResourceData.Count; i++)
            {
                double StoredUnits = module.IsPropellantStarved ? 0.0 : Math.Abs(containedResourceData[i].StoredUnits);
                double CapacityUnits = containedResourceData[i].CapacityUnits;

                if (!fuelCapacity.ContainsKey(containedResourceData[i].ResourceID))
                {
                    ContainedResourceData value = default(ContainedResourceData);
                    value.CapacityUnits = CapacityUnits;
                    value.StoredUnits = StoredUnits;
                    fuelCapacity.Add(containedResourceData[i].ResourceID, value);
                }
                else
                {
                    ContainedResourceData value2 = fuelCapacity[containedResourceData[i].ResourceID];
                    value2.CapacityUnits += CapacityUnits;
                    value2.StoredUnits += StoredUnits;
                    fuelCapacity[containedResourceData[i].ResourceID] = value2;
                }

                double container_percent = StoredUnits / CapacityUnits;
                Min_Stage_Percentage = Math.Min(Min_Stage_Percentage, container_percent);
            }
        }
        if (fuelCapacity.Count > 0)
        {
            Full_Stage_Percentage = 0.0;
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
           
        }
    }


    public bool CheckStaging()
    {
        // always compute 
        CalculateVesselStageFuel();

        // return true if staging in progress
        if (!StagingSettings.auto_staging.Value)
        {
            is_staging = false;
            return false;
        }
           
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

    // public override void onGUI()
    // {  
    //     if (K2D2_Plugin.Instance.settings_visible)
    //     {
    //         K2D2Settings.onGUI();
    //         StagingSettings.settings_UI();
    //         return;
    //     }

    //     UI_Tools.Title("Staging");

    //     if (StagingSettings.auto_staging)
    //         UI_Tools.Label($"Auto Staging is On. (timer: {StagingSettings.freeze_duration}s) ");

    //     if (Full_Stage_Percentage < 0)
    //     {
    //         UI_Tools.Console($"No Active Stage.");
    //     }
    //     else
    //     {
    //         // var StageFuelPercentage
    //         UI_Tools.Console($"Total Fuel remaning : {Full_Stage_Percentage:n2}%");
    //         UI_Tools.Console($"Next Empty Tank : {Min_Stage_Percentage:n2}%");
    //         if (Min_Stage_Percentage == 0)
    //         {
    //             UI_Tools.Warning($"Stage NOW !");
    //         }
    //     }
    // }

    // public void stagingUI()
    // {
    //     if (!is_staging)
    //         return;

    //     UI_Tools.Title("Staging in progress");

    //     if (end_time > 0 )
    //     {   
    //         var remaining = end_time - current_time; 
    //         UI_Tools.Console($"Timer : {remaining:n2} s");
    //     }

    //     return;
    // }

    public override void onReset()
    {
        is_staging = false;
    }

}