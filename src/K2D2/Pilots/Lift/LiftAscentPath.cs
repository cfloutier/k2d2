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

namespace K2D2.Controller;

public class LiftAscentPath
{
    public LiftAscentPath(LiftSettings lift_settings)
    {
        this.lift_settings = lift_settings;
    }

    public ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.LiftAscentPath");

    public float compute_elevation(float altitude_km)
    {
        float elevation = 0;
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

    private Texture2D  _pathTexture = new Texture2D(1,1);
    public CelestialBodyComponent lastbody = null;
    public double last_max_alt;

    Texture2D gradient_Texture;

    VisualElement el_graph;
    VisualElement gradient;

    

    float x_range = 100;
    float y_range = 1;

    public void InitUI(VisualElement root)
    {
        gradient_Texture = new Texture2D(2,256);
        
        el_graph = root.Q<VisualElement>("Graph");
        el_graph.Clear();

        gradient = new VisualElement();
        gradient.AddToClassList("gradient");
        gradient.style.backgroundImage = gradient_Texture;
        el_graph.Add(gradient);

        int nb_lines = 4;
        // Color color_lines = Color.gray;

        for (int i = 0 ; i < nb_lines ; i++)
        {
            
        }
    }


    List<GraphLine> _lines = new();
    int index_line = 0;
    GraphLine get_line()
    {
        if (_lines.Count <= index_line )
        {
            var line =  new GraphLine();
            // var color = color_lines;
            // color.a = Mathf.Lerp(1, 0.5f, ((float)i)/nb_lines );

            // line.LineColor = color;
            line.LineWidth = 1;
            el_graph.Add(line);
            _lines.Add(line);
            line.setRanges(0, x_range, 0, y_range);         
        }

        return _lines[index_line++];
    }

    public void updateProfile(float current_altitude_km)
    {
        float scale = (float)(lift_settings.destination_Ap_km.V / y_range);

        if (lastbody != K2D2Plugin.Instance.current_vessel.currentBody() || 
            last_max_alt != lift_settings.destination_Ap_km.V)
        {
            UpdateAtmoTexture( K2D2Plugin.Instance.current_vessel.currentBody(), lift_settings.destination_Ap_km.V );
        }

        index_line = 0;
        DrawLines(current_altitude_km);
        updatePath();
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
        
        while ((alt < lift_settings.destination_Ap_km.V) && (downrange < x_range))
        {
            float desiredAngle = (float)(alt < lift_settings.start_altitude_km.V ? 90 : compute_elevation(alt));

            alt       += y_range * Mathf.Sin(desiredAngle * Mathf.Deg2Rad);
            downrange += y_range * Mathf.Cos(desiredAngle * Mathf.Deg2Rad);

            point.x = downrange / x_range;
            point.y = alt / y_range;

            if ((prev_point - point).sqrMagnitude >= 1.0)
            {       
                points.Add(point);
                prev_point = point;
            }
        }

        line.setPoints(points);
    }

    void setHLine(float ypos)
    {
        GraphLine line = get_line();
        line.setSegment(new Vector2(0, ypos) , new Vector2(x_range, ypos));
        line.Show(true);
    }
    void HideLine()
    {
        get_line().Show(false); 
    }

    private void DrawLines(float current_altitude_km)
    {

        setHLine(lift_settings.start_altitude_km.V / y_range);
        setHLine(lift_settings.mid_rotate_altitude_km / y_range);
        setHLine(lift_settings.mid_rotate_altitude_km / y_range);
        
        if (maxAtmosphereAltitude_km > 0)
            setHLine( maxAtmosphereAltitude_km / y_range);
        else
            HideLine();


        if (current_altitude_km > 0 && current_altitude_km < lift_settings.destination_Ap_km.V)
            setHLine( current_altitude_km / y_range);
        else
            HideLine();
    }

    float maxAtmosphereAltitude_km = -1;

    public void UpdateAtmoTexture(CelestialBodyComponent mainBody, double maxAltitude)
    {
        lastbody = mainBody;
        last_max_alt = maxAltitude;

        double scale = maxAltitude / gradient_Texture.height; //meters per pixel

        if (mainBody.hasAtmosphere)
            maxAtmosphereAltitude_km = (float) (mainBody.atmosphereDepth / 1000);
        else
            maxAtmosphereAltitude_km = -1;

        double pressureSeaLevel = mainBody.atmospherePressureSeaLevel;

        for (int y = 0; y < gradient_Texture.height; y++)
        {
            double alt = scale * y;

            float atmo_ratio = (float)(mainBody.GetPressure(alt*1000) / pressureSeaLevel);
            float altitude_atm_ratio = mainBody.hasAtmosphere ?  (float)(1.0 - alt / maxAtmosphereAltitude_km) : 0.0f;

            var c = Color.Lerp(Color.black, Color.cyan, altitude_atm_ratio/2) + new Color(atmo_ratio, atmo_ratio, atmo_ratio, 1);

            for (int x = 0; x < gradient_Texture.width; x++)
            {
                gradient_Texture.SetPixel(x, y, c);

                // if (mainBody.hasAtmosphere && (int)(maxAtmosphereAltitude_km / scale) == y)
                //     texture.SetPixel(x, y, XKCDColors.LightGreyBlue);
            }
        }

        gradient_Texture.Apply();
    }

}