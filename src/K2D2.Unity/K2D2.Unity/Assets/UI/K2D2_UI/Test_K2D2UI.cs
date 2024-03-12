// This script attaches the tabbed menu logic to the game.
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

using K2UI;
using K2UI.Tabs;
using KTools;

public class Node: K2Panel
{
    public Node()
    {
        code = "node";
    }
}

public class Lift : K2Panel
{
    public Lift()
    {
        code = "lift";
    }
}

public class Drone : K2Panel
{
    public Drone()
    {
        code = "drone";
    }

    public override bool onInit()
    {
        isRunning = true;
        return true;
    }
}

public class Land : K2Panel
{
    public Land()
    {
        code = "land";
    }
}

//Inherits from class `MonoBehaviour`. This makes it attachable to a game object as a component.
public class Test_K2D2UI : MonoBehaviour
{
    private TabbedPage pages_controler;

    public string start_selected = "controls";

    // public List<Panel> panels;
    private void OnEnable()
    {
       
        
    }

    private void Start()
    {
        UIDocument menu = GetComponent<UIDocument>();
        VisualElement root = menu.rootVisualElement;
        root.MakeDraggable();

        SettingsFile.Init("settings.json");

        List<K2Panel> panels = new()
        {
            new TestAllControls(),
            new TestSettings(),
            new Node(),
            new Lift(),
            new Drone(),
            new Land()
        };

        pages_controler = root.Q<TabbedPage>();
        pages_controler.Init(panels);
        pages_controler.Select(start_selected);
    }
}