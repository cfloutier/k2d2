using KSP.Sim.Definitions;
using KSP.Sim.impl;
using MoonSharp.Interpreter.Tree.Statements;
using System.Collections;

namespace K2D2.Controller.Docks;

public class DockTools
{
    public class NamedComponent
    {
        public string name;
        public PartComponent component;

        public NamedComponent(PartComponent part, string name)
        {
            this.component = part;
            this.name = name;
        }
    }

    public class ListPart 
    {
        public List<NamedComponent> Parts = new List<NamedComponent>();
        public void Add(PartComponent part)
        {
            L.Log($"part add + {part.Name}");

            string name = "";
            var category = part.PartData.category;
            
            if (category == PartCategories.Coupling)
            {
                name = "Dock #" + num_dock++ + "-" + part.PartData.sizeCategory;
            }
            else if (category == PartCategories.Pods)
            {
                name = "Pod #" + num_pod++;
            }
            else if (category == PartCategories.Control)
            {
                name = "Control #" + num_pod++;
            }
            else
            {
                name = category.ToString();
            }

            Parts.Add(new NamedComponent(part, name));
        }

        int num_pod = 1;
        int num_dock = 1;

        public void Clear()
        {
            Parts.Clear();
            num_pod = num_dock = 1;
        }

        public int Count
        {
            get { return Parts.Count; }
        }

        public int IndexOf(PartComponent part)
        {
            for (int i = 0; i < Count; i++)
            {
                if (Parts[i].component == part)
                    return i;
            }

            return -1;
        }
    }


    public static ListPart FindParts(VesselComponent vessel, bool control, bool docks)
    {
        var parts = new ListPart();
        if (vessel == null)
            return parts;

        //L.Log("FindParts ");

        PartOwnerComponent owner = vessel.GetControlOwner().PartOwner;

        foreach (var part in owner.Parts)
        {
            PartData data = part.PartData;
            if (data == null) continue;

            //parts.Add(part);

            if (control)
            {
                if (data.category == PartCategories.Control ||
                    data.category == PartCategories.Pods)
                    parts.Add(part);
            }
            if (docks && data.category == PartCategories.Coupling)
            {
                parts.Add(part);
            }
        }

        //L.Log($"Found {parts.Count}");

        return parts;
    }

}