// This script defines the tab selection logic.
using UnityEditor.Animations;
using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;


[System.Serializable]
public class Panel
{
    public string code;

    public string label;

    public Panel()
    {

    }

    public bool Init(VisualElement tab_bar)
    {
        var code_bt = code + "_tabs";
        Button tab_button = (Button) tab_bar.Q(code + "_tabs");
        if (tab_button == null)
        {
            Debug.LogError("Not found: " + code_bt);
            return false;
        }

        tab_button.text = label;




        return true;
    }
}

public class TabbedMenuController
{
    public List<Panel> panels = new();

    /* Define member variables*/

   
    private const string tabClassName = "unity-button.tab";
    private const string currentlySelectedTabClassName = "currentlySelectedTab";
    private const string unselectedContentClassName = "unselectedContent";
    // Tab and tab content have the same prefix but different suffix
    // Define the suffix of the tab name
    private const string tabNameSuffix = "Tab";
    // Define the suffix of the tab content name
    private const string contentNameSuffix = "Content";

    private readonly VisualElement root;

    public TabbedMenuController(VisualElement root)
    {
        this.root = root;
    }

    public void Init(List<Panel> panels)
    {
        this.panels = panels;
        RegisterTabCallbacks();
    }

    public void RegisterTabCallbacks()
    {
        var tabs_bar = root.Q("tabs");

        foreach(var panel in panels)
        {
            panel.Init(tabs_bar);
        }


        UQueryBuilder<VisualElement> tabs = GetAllTabs();
        tabs.ForEach((VisualElement tab) => {
            tab.RegisterCallback<ClickEvent>(TabOnClick);
        });
    }

    /* Method for the tab on-click event: 

       - If it is not selected, find other tabs that are selected, unselect them 
       - Then select the tab that was clicked on
    */
    private void TabOnClick(ClickEvent evt)
    {
        Label clickedTab = evt.currentTarget as Label;
        if (!TabIsCurrentlySelected(clickedTab))
        {
            GetAllTabs().Where(
                (tab) => tab != clickedTab && TabIsCurrentlySelected(tab)
            ).ForEach(UnselectTab);
            SelectTab(clickedTab);
        }
    }
    //Method that returns a Boolean indicating whether a tab is currently selected
    private static bool TabIsCurrentlySelected(VisualElement tab)
    {
        return tab.ClassListContains(currentlySelectedTabClassName);
    }

    private UQueryBuilder<VisualElement> GetAllTabs()
    {
        return root.Query<VisualElement>(className: tabClassName);
    }

    /* Method for the selected tab: 
       -  Takes a tab as a parameter and adds the currentlySelectedTab class
       -  Then finds the tab content and removes the unselectedContent class */
    private void SelectTab(Label tab)
    {
        tab.AddToClassList(currentlySelectedTabClassName);
        Label content = (Label) FindContent(tab);
        content.RemoveFromClassList(unselectedContentClassName);
        Debug.Log(content.text);
    }

    /* Method for the unselected tab: 
       -  Takes a tab as a parameter and removes the currentlySelectedTab class
       -  Then finds the tab content and adds the unselectedContent class */
    private void UnselectTab(VisualElement tab)
    {
        tab.RemoveFromClassList(currentlySelectedTabClassName);
        VisualElement content = FindContent(tab);
        content.AddToClassList(unselectedContentClassName);
    }

    // Method to generate the associated tab content name by for the given tab name
    private static string GenerateContentName(VisualElement tab) =>
        tab.name.Replace(tabNameSuffix, contentNameSuffix);

    // Method that takes a tab as a parameter and returns the associated content element
    private VisualElement FindContent(VisualElement tab)
    {
        return root.Q(GenerateContentName(tab));
    }
}