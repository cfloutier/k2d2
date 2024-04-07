using UnityEngine;

// tools used for the lift controller

using KTools;
using KSP.Game;
using KSP.Sim;
using KSP.Sim.impl;
using KSP.Sim.Maneuver;
using KSP.Sim.State;
using K2D2.KSPService;
using BepInEx.Logging;
using UnityEngine.UIElements;
using K2UI.Graph;
using K2UI;

namespace K2D2.Lift;


public class MultiGraphLine
{
    protected float x_range = 100;
    protected float y_range = 100;

    protected VisualElement el_graph;

    List<GraphLine> _lines = new();
    int index_line = 0;

    protected void reset()
    {
        index_line = 0;
    }
    

    protected GraphLine get_line()
    {
        GraphLine line;
        if (_lines.Count <= index_line)
        {
            line = new GraphLine();
            line.LineWidth = 1;
            // line.style.cro
            
            el_graph.Add(line);
            _lines.Add(line);
       
        }
        else
            line = _lines[index_line++];

        // y is inverted (y up)
        line.setRanges(0, x_range, y_range, 0);
        return line;
    }
}



public class LiftAscentPath : MultiGraphLine
{
    public LiftAscentPath(LiftSettings lift_settings)
    {
        this.lift_settings = lift_settings;
    }

    public ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.LiftAscentPath");

    public float compute_elevation(float altitude_km)
    {
        float elevation;
        if (altitude_km < lift_settings.start_altitude_km.V)
        {
            elevation = 90;
        }
        else if (altitude_km < lift_settings.mid_rotate_altitude_km)
        {
            var ratio = Mathf.InverseLerp(lift_settings.start_altitude_km.V, lift_settings.mid_rotate_altitude_km, altitude_km);
            elevation = Mathf.Lerp(90, 45, ratio);
        }
        else
        {
            var ratio = Mathf.InverseLerp(lift_settings.mid_rotate_altitude_km, lift_settings.end_rotate_altitude_km, (float)altitude_km);
            elevation = Mathf.Lerp(45, 1, ratio);
        }

        return elevation;
    }

    LiftSettings lift_settings;

    public CelestialBodyComponent lastbody = null;
    public double last_max_alt;

    Texture2D gradient_Texture;

    
    VisualElement gradient;

    public void InitUI(VisualElement root)
    {
        gradient_Texture = new Texture2D(2, 256);

        el_graph = root.Q<VisualElement>("graph");
        el_graph.Clear();

        gradient = new VisualElement();
        gradient.AddToClassList("gradient");
        gradient.style.backgroundImage = gradient_Texture;
        el_graph.Add(gradient);
    }

    public void updateProfile(float current_altitude_km)
    {
        float scale = (float)(lift_settings.destination_Ap_km.V / y_range);

        if (lastbody != K2D2_Plugin.Instance.current_vessel.currentBody() ||
            last_max_alt != lift_settings.destination_Ap_km.V)
        {
            UpdateAtmoTexture(K2D2_Plugin.Instance.current_vessel.currentBody(), lift_settings.destination_Ap_km.V);
        }

        reset();
        // update range
        y_range = lift_settings.destination_Ap_km.V;
        x_range = 1 * y_range * get_line().aspectRatio();
        reset();

        DrawLines(current_altitude_km);
        updatePath();
        // addSinus();
    }

    void addSinus()
    { 
        GraphLine line = get_line();
        line.LineColor = Color.blue;
        int nb_points = 100;
        Vector2 point = Vector2.zero;
        List<Vector2> points = new();
        for (int i = 0 ; i < nb_points; i++)
        {
            point.x = (i * x_range) / nb_points;
            point.y = y_range* Mathf.Sin(Mathf.PI*i*2/nb_points);
            points.Add(point);
        }

        line.setPoints(points);
    }

    private void updatePath()
    {
        GraphLine line = get_line();

        float alt = 0;
        float downrange = 0;
        line.LineColor = Color.yellow;
        List<Vector2> points = new();
        Vector2 prev_point = Vector2.zero;
        points.Add(prev_point);
        Vector2 point = Vector2.zero;

        float definition = lift_settings.destination_Ap_km.V / 100f;

        while ((alt < lift_settings.destination_Ap_km.V) && (downrange < x_range))
        {
            float desiredAngle = (float)(alt < lift_settings.start_altitude_km.V ? 90 : compute_elevation(alt));

            alt += definition * Mathf.Sin(desiredAngle * Mathf.Deg2Rad);
            downrange += definition * Mathf.Cos(desiredAngle * Mathf.Deg2Rad);

            point.x = downrange;
            point.y = alt;

            points.Add(point);
            prev_point = point;     
        }

        line.setPoints(points);
    }

    void setHLine(float ypos, Color color)
    {
        GraphLine line = get_line();
        line.LineColor = color;
        line.setSegment(new Vector2(0, ypos), new Vector2(x_range, ypos));
        line.Show(true);
    }

    void HideLine()
    {
        get_line().Show(false);
    }

    private void DrawLines(float current_altitude_km)
    {
        setHLine(lift_settings.start_altitude_km.V, Color.gray);
        setHLine(lift_settings.mid_rotate_altitude_km, Color.gray);
        setHLine(lift_settings.end_rotate_altitude_km, Color.gray);

        if (maxAtmosphereAltitude_km > 0)
            setHLine(maxAtmosphereAltitude_km, Color.white);
        else
            HideLine();

        if (current_altitude_km > 0 && current_altitude_km < lift_settings.destination_Ap_km.V)
            setHLine(current_altitude_km, Color.green);
        else
            HideLine();
    }

    float maxAtmosphereAltitude_km = -1;


    Color colorTable(string body_name)
    {
        switch (body_name.ToLower())
        {
            case "laythe": return ColorTools.parseColor("#1C76E9");
            case "jool": return ColorTools.parseColor("#9FF911");
            case "duna": return ColorTools.parseColor("#FF4F18");
            case "kerbin": return ColorTools.parseColor("#5EAAFE");
            case "eve": return ColorTools.parseColor("#A63CFF");
        }

        return Color.black;
    }

    public void UpdateAtmoTexture(CelestialBodyComponent mainBody, double maxAltitude)
    {
        lastbody = mainBody;
        if (mainBody == null) return;
        last_max_alt = maxAltitude;

        var color_start = colorTable(mainBody.Name); 
        var color_end = Color.black;
        
        //mainBody.oceanFogColorEnd;

        double scale = maxAltitude / gradient_Texture.height; //meters per pixel

        if (mainBody.hasAtmosphere)
            maxAtmosphereAltitude_km = (float)(mainBody.atmosphereDepth / 1000);
        else
            maxAtmosphereAltitude_km = -1;

        double pressureSeaLevel = mainBody.atmospherePressureSeaLevel;

        for (int y = 0; y < gradient_Texture.height; y++)
        {
            double alt = scale * y;

            float atmo_ratio = (float)(mainBody.GetPressure(alt * 1000) / pressureSeaLevel);
            float altitude_atm_ratio = mainBody.hasAtmosphere ? (float)(1.0 - alt / maxAtmosphereAltitude_km) : 0.0f;

            var c = Color.Lerp(color_end, color_start, altitude_atm_ratio / 2) + new Color(atmo_ratio, atmo_ratio, atmo_ratio, 1);

            for (int x = 0; x < gradient_Texture.width; x++)
            {
                gradient_Texture.SetPixel(x, y, c);
            }
        }

        gradient_Texture.Apply();
    }

}