using System.Diagnostics.Tracing;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
using K2UI.Graph;
using K2UI;
using System.Linq;
using System.Collections.Generic;

[RequireComponent(typeof(UIDocument))]
public class TestGraph : MonoBehaviour
{
    public float speed = 0;
    public float period = 50;
    public float dt = 0.1f;

    float max_x = 100;
    int nb_point = 500;

    public Color color_lines = Color.white;

    public int nb_lines = 10;

    float t = 0;

    List<Vector2> buildPerlin(float h)
    {
        var points = new List<Vector2>(); 

        float x = 0; 
        float dx = max_x / nb_point;

        t = t + speed*Time.deltaTime;

        while (x <= max_x)
        {
            float y = Mathf.PerlinNoise(period*x, h + t);
            points.Add(new Vector2(x, y));
            x += dx;
        }
        return points;
    }

    List<Vector2> buildSinus()
    {
        var points = new List<Vector2>(); 

        float x = 0; 
        float dx = max_x / nb_point;

        while (x <= max_x)
        {
            float y = Mathf.Sin(period * (x+speed*Time.time) * Mathf.Deg2Rad);
            points.Add(new Vector2(x, y));
            x += dx;
        }

        return points;
    }

    VisualElement el_graph;
    K2Slider speed_slider, period_slider, dt_slider;

    List<GraphLine> lines = new();

    public void OnEnable()
    {
        var doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;
        el_graph = root.Q<VisualElement>("Graph");

        for (int i = 0 ; i < nb_lines ; i++)
        {
            var line =  new GraphLine();
            var color = color_lines;
            color.a = Mathf.Lerp(1, 0.05f, ((float)i)/nb_lines );
            line.LineColor = color;
            line.LineWidth = 5;
            el_graph.Add(line);
            lines.Add(line);
            line.setRanges(0, 100, 0, 1);
        }

        speed_slider = root.Q<K2Slider>("speed");
        speed_slider.InitValues(speed, 0, 0.2f);
       
        period_slider = root.Q<K2Slider>("period");
        period_slider.InitValues(period, 0, 0.2f);

        dt_slider = root.Q<K2Slider>("dt");
        dt_slider.InitValues(dt, 0, 0.2f);

        speed_slider.RegisterCallback<ChangeEvent<float>>(evt => speed = evt.newValue);
        period_slider.RegisterCallback<ChangeEvent<float>>(evt => period = evt.newValue);
        dt_slider.RegisterCallback<ChangeEvent<float>>(evt => dt = evt.newValue);
    }

    void Update()
    {
        for (int i = 0 ; i < lines.Count; i++)
        {
            var points = buildPerlin(i*dt);
            lines[i].setPoints(points);
        }      
    }
}