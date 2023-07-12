
namespace K2D2.Controller.Docks;


using KSP.Sim.impl;

public class DockTools
{
    public static List<PartComponent> ListDocks(VesselComponent vessel)
    {
        PartOwnerComponent owner = vessel.GetControlOwner().PartOwner;
        var docks = new List<PartComponent>();

        foreach(var part in owner.Parts)
        {
            if (part.Name.ToLower().Contains("dock"))
                docks.Add(part);
        }

        return docks;
    }
}