
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

public class MainTabs
{
    public static bool TabButton(bool is_current, bool isActive, string txt)
    {
        GUIStyle style = isActive ? K2D2Styles.tab_active : K2D2Styles.tab_normal;
        return GUILayout.Toggle(is_current, txt, style);
    }
}

public class MainUI
{
    public ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.MainUI");

    bool init_done = false;
    BaseController current_page = null;
    public List<BaseController> pages = new List<BaseController>();
    public List<BaseController> filtered_pages = new List<BaseController>();

    public MainUI()
    {

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
                K2D2Styles.tab_normal.CalcMinMaxWidth(new GUIContent(page.Name, ""), out minWidth, out maxWidth);
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
            if (MainTabs.TabButton(is_current, page.isActive, page.Name))
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

    public void Update()
    {
        if (!init_done)
        {
            pages.Add(AutoExecuteManeuver.Instance);
            pages.Add(CircleController.Instance);

            pages.Add(LandingController.Instance);
            pages.Add(DroneController.Instance);

            pages.Add(AutoLiftController.Instance);
            pages.Add(AttitudeController.Instance);

            // waiting for mole
            // pages.Add(SimpleManeuverController.Instance);
            pages.Add(new OrbitInfos());
            pages.Add(new K2D2.InfosPages.SASInfos());
            pages.Add(new VesselInfos());

            Settings.current_interface_mode = GeneralTools.ClampInt(Settings.current_interface_mode, 0, pages.Count - 1);
            current_page = pages[Settings.current_interface_mode];
            current_page.ui_visible = true;

            init_done = true;
        }

        if (Settings.debug_mode)
        {
            filtered_pages = pages;
        }
        else
        {
            filtered_pages = new List<BaseController>();
            for (int index = 0; index < pages.Count; index++)
            {
                if (!pages[index].debug_mode)
                    filtered_pages.Add(pages[index]);

            }
        }
    }

    public void onGUI()
    {
        if (!init_done) return;

        // string [] pages_str = Settings.debug_mode ? interfaceModes_debug : interfaceModes;
        int result = DrawTabs(Settings.current_interface_mode);
        if (result != Settings.current_interface_mode)
        {
            current_page.ui_visible = false;
            Settings.current_interface_mode = result;
            current_page = filtered_pages[Settings.current_interface_mode];
            current_page.ui_visible = true;
        }

        pages[result].onGUI();
    }
}
