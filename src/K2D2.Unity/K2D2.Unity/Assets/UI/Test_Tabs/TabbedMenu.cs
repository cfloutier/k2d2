// This script attaches the tabbed menu logic to the game.
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;


//Inherits from class `MonoBehaviour`. This makes it attachable to a game object as a component.
public class TabbedMenu : MonoBehaviour
{
    private TabbedMenuController controller;


    // public List<Panel> panels;

    private void OnEnable()
    {
        UIDocument menu = GetComponent<UIDocument>();
        VisualElement root = menu.rootVisualElement;

        List<Panel> panels = new();
        panels.Add(new Panel()
        {
            code = "node",
            label = "The Node executor !"
        });

        panels.Add(new Panel()
        {
            code = "lift",
            label = "To the orbit !"
        });

        panels.Add(new Panel()
        {
            code = "lift",
            label = "To the orbit !"
        });

        controller = new(root);
        controller.Init(panels);
    }
}