
using UnityEngine;
using KSP.Sim;
using BepInEx.Logging;
using SpaceWarp.API.Assets;
using KSP.Sim.DeltaV;

using System.Collections.Generic;
using System;
using K2D2.Controller;
using K2D2.InfosPages;
using KSP.Game;
using KSP.Rendering.impl;

using K2D2.UI;

namespace K2D2;


public interface PageContent
{
    


    public string Name
    {
        get;
    }

    // if is isRunning, UI is drawn lighted
    public bool isRunning
    {
        get;
    }

    // if isActive Tab is visible
    public bool isActive
    {
        get;
    }


    public bool UIVisible
    {
        get;
        set;
    }

    
    public void onGUI();



}



public class ExTabPage
{
    public List<PageContent> pages = new List<PageContent>();

    private List<PageContent> filtered_pages = new List<PageContent>();

    PageContent current_page = null;

    // must be called after adding pages
   



    private bool TabButton(bool is_current, bool isActive, string txt)
    {
        GUIStyle style = isActive ? GenericStyle.tab_active : GenericStyle.tab_normal;
        return GUILayout.Toggle(is_current, txt, style);
    }


    List<float> tabs_Width = new List<float>();

    public int DrawTabs(int current, float max_width = 300)
    {
        current = GeneralTools.ClampInt(current, 0, filtered_pages.Count - 1);
        GUILayout.BeginHorizontal();

        int result = current;

        // compute sizes
        if (tabs_Width.Count != filtered_pages.Count)
        {
            tabs_Width.Clear();
            for (int index = 0; index < filtered_pages.Count; index++)
            {
                var page = filtered_pages[index];
                float minWidth, maxWidth;
                GenericStyle.tab_normal.CalcMinMaxWidth(new GUIContent(page.Name, ""), out minWidth, out maxWidth);
                tabs_Width.Add(minWidth);
            }
        }
        float xPos = 0;

        for (int index = 0; index < filtered_pages.Count; index++)
        {
            var page = filtered_pages[index];

            float width = tabs_Width[index];

            if (xPos > max_width)
            {
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                xPos = 0;
            }
            xPos += width;

            bool is_current = current == index;
            if (TabButton(is_current, page.isRunning, page.Name))
            {
                if (!is_current)

                    result = index;
            }
        }

        if (xPos < max_width * 0.7f)
        {
            GUILayout.FlexibleSpace();
        }
        GUILayout.EndHorizontal();

        UI_Tools.Separator();
        return result;
    }


    public void Init()
    {
        Settings.main_tab_index = GeneralTools.ClampInt(Settings.main_tab_index, 0, pages.Count - 1);
        current_page = pages[Settings.main_tab_index];
        current_page.UIVisible = true;
    }

    public void Update()
    {
        filtered_pages = new List<PageContent>();
        for (int index = 0; index < pages.Count; index++)
        {
            if (!pages[index].isActive)
                filtered_pages.Add(pages[index]);
        }
    }

    public void onGUI()
    {

        int current_index = Settings.main_tab_index;
        // string [] pages_str = Settings.debug_mode ? interfaceModes_debug : interfaceModes;
        int result = DrawTabs(current_index);
        if (result != current_index)
        {
            current_page.UIVisible = false;
            Settings.main_tab_index = result;
            current_page = filtered_pages[result];
            current_page.UIVisible = true;
        }

        pages[result].onGUI();
    }
}

public class MainUI
{
    public ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.MainUI");

    bool init_done = false;

    ExTabPage tabs = new ExTabPage();

    public MainUI()
    {

    }


    public void Update()
    {
        if (!init_done)
        {
            tabs.pages.Add(AutoExecuteManeuver.Instance);
            tabs.pages.Add(CircleController.Instance);

            tabs.pages.Add(LandingController.Instance);
            tabs.pages.Add(DroneController.Instance);

            tabs.pages.Add(AutoLiftController.Instance);
            tabs.pages.Add(AttitudeController.Instance);
            tabs.pages.Add(WarpController.Instance);
            tabs.pages.Add(new FindSecrets());
            // waiting for mole
            // pages.Add(SimpleManeuverController.Instance);
            tabs.pages.Add(new OrbitInfos());
            tabs.pages.Add(new K2D2.InfosPages.SASInfos());
            tabs.pages.Add(new VesselInfos());

            tabs.Init();

            init_done = true;
        }


        tabs.Update();
    }

    public void onGUI()
    {
        if (!init_done) return;

        tabs.onGUI();
    }
}
