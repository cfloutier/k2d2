using System.Collections;
using System.Collections.Generic;
using BepInEx.Logging;
using UnityEngine;
using System.Linq;
using System.Globalization;

namespace KTools.UI;

public class HeadingSlider
{
    public static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.HeadingSlider");

    public HeadingSlider(bool interactive, int pixel_per_deg = 4)
    {
        this.pixel_per_deg = pixel_per_deg;
        this.interactive = interactive;
    }

    Vector2 pos = Vector2.zero;

    public bool interactive;

    bool dragin = false;
    Vector2 startPos;
    Vector2 deltapos;

   // static float heading = 0;
    float startDragHeadin = 0;

    int pixel_per_deg = 4;
    int Height_1 = 5;
    int Height_5 = 10;

    float fixDeg(float deg)
    {
        while (deg > 360)
            deg -= 360;
        while (deg < 0)
            deg += 360;

        return deg;
    }

    string[] directions = { "N", "NE", "E", "SE", "S", "SW", "W", "NW", "N", "NE", "E", "SE", "S", "SW", "W", "NW", "N", "NE", "E" };
    int[] forbiden_values = { 40, 50, 130, 140 , 220 , 230, 310, 320 };

    float drawContent(float value, Rect rt)
    {
        //  Rect rc = new Rect(deltapos.x, deltapos.y, rt.width, rt.height);
        //if (Event.current.type == EventType.Repaint )
        GUI.BeginClip(rt);

        float xpos = 0;

        float min_deg = Mathf.Ceil(xpos_to_deg(0, value, rt.width));
        float max_deg = Mathf.Floor(xpos_to_deg(rt.width, value, rt.width));
        float deg = min_deg;
        GUI.color = Color.white;
        while (deg < max_deg)
        {
            xpos = deg_to_xpos(deg, value, rt.width);
            float h = Height_1;

            if (deg % 5 == 0)
                h = Height_5;

            Rect rc = new Rect(xpos, 0, 1, h);
            GUI.Label(rc, "", KBaseStyle.v_line);

            float drawn_deg = fixDeg(deg);

            if (drawn_deg % 45 == 0)
            {
                int index = (int)(drawn_deg / 45) + 8;
                if (index >= 0 && index < directions.Length)
                {
                    rc = new Rect(xpos - 20, h - 2, 40, 20);
                    if (interactive)
                    {
                        if (GUI.Button(rc, directions[index], KBaseStyle.text_heading_big))
                        {
                            value = drawn_deg;

                            logger.LogInfo($"clic to {KBaseStyle.text_heading_big}");
                        }
                    }
                    else
                    {
                        GUI.Label(rc, directions[index], KBaseStyle.text_heading_big);
                    }

                }
                else
                    Debug.LogError("index = " + index);
            }
            else if (drawn_deg % 10 == 0)
            {
                if ( !forbiden_values.Contains((int) drawn_deg))
                {
                    rc = new Rect(xpos - 15, h, 30, 20);
                    GUI.Label(rc, drawn_deg.ToString(), KBaseStyle.text_heading_mini);
                }
            }

            deg += 1;
        }

        GUI.Button(new Rect(0, 0, rt.width, rt.height), "", KBaseStyle.heading);
        GUI.Label(new Rect(rt.width / 2, 0, 1, rt.height), "", KBaseStyle.v_line);
        GUI.EndClip();
        
        return value;
    }

    float xpos_to_deg(float xpos, float heading, float width)
    {
        return heading + (-width / 2 + xpos) / pixel_per_deg;
    }

    float deg_to_xpos(float deg, float heading, float width)
    {
        return (deg - heading)* pixel_per_deg + width/2;
    }

    public void onMouseUp()
    {
        dragin = false;
    }

    float on_GUI(float value)
    {
       // GUILayout.Box("", GUILayout.Width(200), GUILayout.Height(10));
        if (dragin )
        {
            deltapos = Event.current.mousePosition - startPos;
            value = startDragHeadin - deltapos.x / pixel_per_deg;
        }

        value = fixDeg(value);

        Rect rt = GUILayoutUtility.GetRect( new GUIContent("", ""), KBaseStyle.heading, GUILayout.Height(30));
        if (interactive)
        {
            if (rt.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    startPos = Event.current.mousePosition;
                        startDragHeadin = value;

                    dragin = true;
                }
            }
        }

        return drawContent(value, rt);
    }

    public static Dictionary<string, HeadingSlider> sliders = new Dictionary<string, HeadingSlider>();

    public static float onGUI(string ui_code, float value, bool interactive, int pixel_per_deg = 3)
    {
        HeadingSlider slider;
        if (!sliders.ContainsKey(ui_code))
        {
            slider = new HeadingSlider(interactive, pixel_per_deg);
            sliders[ui_code] = slider;
        }
        else
            slider = sliders[ui_code];

        return slider.on_GUI(value);
    }

    public static void onStaticUpdate()
    {
        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            foreach (var slider in sliders.Values)
            {
                slider.onMouseUp();
            }
        }
    }
}
