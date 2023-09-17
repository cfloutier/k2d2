


using KSP.Game.StartupFlow;
using KSP.Map;
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

    Color default_unselected_color = ColorTools.parseColor("#CB5B00");
    Color default_vessel_color = ColorTools.parseColor("#00B7FF");
    Color default_target_color = ColorTools.parseColor("#00FF34");

    public Color unselected_color
    {
        get => KBaseSettings.sfile.GetColor("docks.unselected_color", default_unselected_color);
        set
        {
            KBaseSettings.sfile.SetColor("docks.unselected_color", value);
        }
    }

    public Color vessel_color
    {
        get => KBaseSettings.sfile.GetColor("docks.vessel_color", default_vessel_color);
        set
        {
            KBaseSettings.sfile.SetColor("docks.vessel_color", value);
        }
    }

    public Color target_color
    {
        get => KBaseSettings.sfile.GetColor("docks.target_color", default_target_color);
        set
        {
            KBaseSettings.sfile.SetColor("docks.target_color", value);
        }
    }

    public float pos_grid
    {
        get => KBaseSettings.sfile.GetFloat("docks.pos_grid", 1);
        set
        {
            KBaseSettings.sfile.SetFloat("docks.pos_grid", value);
        }
    }

    public float length_line
    {
        get => KBaseSettings.sfile.GetFloat("docks.length_line", 1);
        set
        {
            KBaseSettings.sfile.SetFloat("docks.length_line", value);
        }
    }

    public float sfx_blur
    {
        get => KBaseSettings.sfile.GetFloat("docks.sfx_blur", 1f);
        set
        {
            KBaseSettings.sfile.SetFloat("docks.sfx_blur", value);
        }
    }

    public float thickness_circle
    {
        get => KBaseSettings.sfile.GetFloat("docks.thickness_circle", 2f);
        set
        {
            KBaseSettings.sfile.SetFloat("docks.thickness_circle", value);
        }
    }

    public float thickness_line
    {
        get => KBaseSettings.sfile.GetFloat("docks.thickness_line", 1f);
        set
        {
            KBaseSettings.sfile.SetFloat("docks.thickness_line", value);
        }
    }

    public float pilot_power
    {
        get => KBaseSettings.sfile.GetFloat("docks.pilot_power", 1f);
        set
        {
            KBaseSettings.sfile.SetFloat("docks.pilot_power", value);
        }
    }

    public FoldOut accordion = new FoldOut();

    public void StyleGUI()
    {
        if (accordion.Count == 0)
        {
            accordion.addChapter("Gizmos", gizmos_styleUI);
            // accordion.addChapter("Pilot", pilot);
            //accordion.addChapter("Touch Down", TouchDown_UI);
            accordion.singleChapter = true;
        }

        accordion.OnGui();


    }


    void gizmos_styleUI()
    { 
        // pos_gizmo_dock = UI_Tools.LabelSlider("Position", pos_gizmo_dock, -10, 10 );

        length_line = UI_Tools.LabelSlider("Length Line", length_line, 0, 10 );
        // radius = UI_Tools.LabelSlider("radius", radius, 0, 100 );

        pos_grid = UI_Tools.LabelSlider("grid forward pos", pos_grid, 0, 10);
        sfx_blur = UI_Tools.LabelSlider("Sfx Blur", sfx_blur, 0, 10 );

        thickness_circle = UI_Tools.LabelSlider("Thickness Circle", thickness_circle, 0, 10);
        thickness_line = UI_Tools.LabelSlider("Thickness line", thickness_line, 0, 10);

        //color_editor.draw_ui("Color");

        pilot_power = UI_Tools.FloatSliderTxt("Pilot Power", pilot_power, 0, 5);
    }

}
