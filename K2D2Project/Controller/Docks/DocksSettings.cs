


using KTools;
using KTools.UI;

using UnityEngine;


namespace K2D2.Controller.Docks;

public class ColorEditor
{
    public float h= 0;
    public float s = 1;
    public float v = 1;

    public void draw_ui(string label)
    {
        GUILayout.Label(label, KBaseStyle.console_text);
        h = UI_Tools.LabelSlider("Hue", h, 0, 1 );
        s = UI_Tools.LabelSlider("Sat", s, 0, 1 );
        v = UI_Tools.LabelSlider("Val", v, 0, 1 );

        UI_Tools.Console(ColorTools.formatColorHtml(color));
    }

    public Color color
    {
        get
        {
            return ColorTools.FromHSV(h, s ,v, 1);
        }
    }
}


public class DocksSettings
{
    public bool show_gizmos
    {
        get => KBaseSettings.sfile.GetBool("docks.show_gizmos", false);
        set
        {
            KBaseSettings.sfile.SetBool("docks.show_gizmos", value);
        }
    }

    Color default_dock_color = ColorTools.parseColor("#CB5B00");
    Color default_vessel_color = ColorTools.parseColor("#00B7FF");
    Color default_selected_color = ColorTools.parseColor("#00FF34");

    public Color dock_color
    {
        get => KBaseSettings.sfile.GetColor("drone.dock_color", default_dock_color);
        set
        {
            KBaseSettings.sfile.SetColor("drone.dock_color", value);
        }
    }

    public Color vessel_color
    {
        get => KBaseSettings.sfile.GetColor("drone.vessel_color", default_vessel_color);
        set
        {
            KBaseSettings.sfile.SetColor("drone.vessel_color", value);
        }
    }

    public Color selected_color
    {
        get => KBaseSettings.sfile.GetColor("drone.selected_color", default_selected_color);
        set
        {
            KBaseSettings.sfile.SetColor("drone.selected_color", value);
        }
    }

    public float pos_gizmo_dock = 1;
    // {
    //     get => KBaseSettings.sfile.GetFloat("drone.delta_pos", 1);
    //     set
    //     {
    //         KBaseSettings.sfile.SetFloat("drone.delta_pos", value);
    //     }
    // }

    public float length_line
    {
        get => KBaseSettings.sfile.GetFloat("drone.length_line", 30);
        set
        {
            KBaseSettings.sfile.SetFloat("drone.length_line", value);
        }
    }

    public float sfx_blur
    {
        get => KBaseSettings.sfile.GetFloat("drone.sfx_blur", 3.5f);
        set
        {
            KBaseSettings.sfile.SetFloat("drone.sfx_blur", value);
        }
    }

    public float thickness_circle
    {
        get => KBaseSettings.sfile.GetFloat("drone.thickness_circle", 2f);
        set
        {
            KBaseSettings.sfile.SetFloat("drone.thickness_circle", value);
        }
    }

    public float thickness_line
    {
        get => KBaseSettings.sfile.GetFloat("drone.thickness_line", 1f);
        set
        {
            KBaseSettings.sfile.SetFloat("drone.thickness_line", value);
        }
    }

    public void StyleGUI()
    {
        // pos_gizmo_dock = UI_Tools.LabelSlider("Position", pos_gizmo_dock, -10, 10 );

        length_line = UI_Tools.LabelSlider("Length Line", length_line, 0, 200 );
        // radius = UI_Tools.LabelSlider("radius", radius, 0, 100 );

        sfx_blur = UI_Tools.LabelSlider("Sfx Blur", sfx_blur, 0, 10 );

        thickness_circle = UI_Tools.LabelSlider("Thickness Circle", thickness_circle, 0, 10);
        thickness_line = UI_Tools.LabelSlider("Thickness line", thickness_line, 0, 10);

        //color_editor.draw_ui("Color");
    }

}
