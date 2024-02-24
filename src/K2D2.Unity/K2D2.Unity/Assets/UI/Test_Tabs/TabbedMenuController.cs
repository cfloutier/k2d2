// This script defines the tab selection logic.
using UnityEditor.Animations;
using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;


[System.Serializable]
public class Panel
{
    public string code;
    public string title;

    public string button_label;

    public VisualElement panel;
    public LightedTab tab;

    public Panel()
    {

    }

    public bool Init(LightedTab tab, VisualElement panels)
    {
        this.tab = tab;
        
        panel = panels.Q(code);
        (panel.Children().First() as Label).text = title;

        
        return onInit();
    }

    public virtual bool onInit()
    {
        return false;
    }

    bool _is_running = false;

    public bool isRunning
    {
        get
        {
            return _is_running;
        }
        set
        {
            _is_running = value;
            tab.Running = value;
        }
    }
}

public class TabbedMenuController
{
    public List<Panel> panels = new();

    VisualTreeAsset tab_asset;

    VisualElement tabs_bar;
    VisualElement content_root;

    //private const string tabClassName = "unity-button.tab";
    private const string currentlySelectedTabClassName = "current";
    private const string unselectedContentClassName = "unselectedContent";

    private readonly VisualElement root;

    public TabbedMenuController(VisualElement root)
    {
        this.root = root;
    }

    public void Init(VisualTreeAsset tab_asset, List<Panel> panels)
    {
        this.panels = panels;
        this.tab_asset = tab_asset;
        BuildTabBar();   
    }

    public void select(int index)
    {
        if (index == 0)
            index = 0;
        if (index >= panels.Count)
            index = panels.Count - 1;   

        var panel = panels[index];
        selectTab(panel.tab.root_tab);
    }

    public void BuildTabBar()
    {
        tabs_bar = root.Q("tabs");
        content_root = root.Q("content");

        tabs_bar.Clear();

        foreach (var panel in panels)
        {
            var tab_element = tab_asset.Instantiate();
            var tabbutton = tab_element.Q<VisualElement>("template_tab_button");
            var tab = new LightedTab();
            tab.SetVisualElement(tabbutton, panel);
            tabs_bar.Add(tabbutton);
            panel.Init(tab, content_root);
        }

        foreach(var tab in tabs_bar.Children())
        {
            tab.RegisterCallback<ClickEvent>(TabOnClick);
        }
    }

    void selectTab(VisualElement clickedTab)
    {
        if (!TabIsCurrentlySelected(clickedTab))
        {
            foreach (var tab in tabs_bar.Children())
            {
                if (tab != clickedTab)
                    UnselectTab(tab);
            }

            SelectTab(clickedTab);
        }
    }

    /* Method for the tab on-click event: 

       - If it is not selected, find other tabs that are selected, unselect them 
       - Then select the tab that was clicked on
    */
    private void TabOnClick(ClickEvent evt)
    {
        VisualElement clickedTab = evt.currentTarget as VisualElement;
        selectTab (clickedTab);
    }


    //Method that returns a Boolean indicating whether a tab is currently selected
    private static bool TabIsCurrentlySelected(VisualElement tab)
    {
        return tab.ClassListContains(currentlySelectedTabClassName);
    }

    /* Method for the selected tab: 
       -  Takes a tab as a parameter and adds the currentlySelectedTab class
       -  Then finds the tab content and removes the unselectedContent class */
    private void SelectTab(VisualElement tab)
    {
        tab.AddToClassList(currentlySelectedTabClassName);

        var content = FindContent(tab);
        content.RemoveFromClassList(unselectedContentClassName);
        //Debug.Log(content.text);
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
    //private static string GenerateContentName(VisualElement tab) =>
    //    tab.name.Replace(tabNameSuffix, contentNameSuffix);

    //// Method that takes a tab as a parameter and returns the associated content element
    private VisualElement FindContent(VisualElement tab)
    {
        return content_root.Q(tab.name);
    }
}