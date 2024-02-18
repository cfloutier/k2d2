// This script attaches the tabbed menu logic to the game.
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;



public class Node: Panel
{
    public Node()
    {
        code = "node";
        button_label = "Node ex";
        title = "The Node executor !";

        
    }
}

public class Lift : Panel
{
    public Lift()
    {
        code = "lift";
        button_label = "Lift !";
        title = "To the orbit !";
    }
}

public class Drone : Panel
{
    public Drone()
    {
        code = "drone";
        button_label = "Drone";
        title = "let's ride";
    }

    public override void OnInit()
    {
        isRunning = true;
    }
}

public class Land : Panel
{
    public Land()
    {
        code = "land";
        button_label = "Land...";
        title = "to the ground";
    }
}


//Inherits from class `MonoBehaviour`. This makes it attachable to a game object as a component.
public class TabbedMenu : MonoBehaviour
{
    private TabbedMenuController controller;

    public VisualTreeAsset TabAsset;

    public int start_selected = 1;


    // public List<Panel> panels;

    private void OnEnable()
    {
        UIDocument menu = GetComponent<UIDocument>();
        VisualElement root = menu.rootVisualElement;

        List<Panel> panels = new()
        {
            new Node(),
            new Lift(),
            new Drone(),
            new Land()
        };

        controller = new(root);
        controller.Init(TabAsset, panels);
        controller.select(start_selected);
    }
}