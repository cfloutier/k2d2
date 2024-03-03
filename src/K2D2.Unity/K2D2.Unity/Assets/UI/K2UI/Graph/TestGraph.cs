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

    float max_x = 100;
    int nb_point = 500;

    List<Vector2> buildPerlin()
    {
        var points = new List<Vector2>(); 

        float x = 0; 
        float dx = max_x / nb_point;

        while (x <= max_x)
        {
            float y = Mathf.PerlinNoise(period*x,  speed*Time.time);
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

    Line my_line;
    K2Slider speed_slider, period_slider;

    public void OnEnable()
    {
        var doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;
        my_line = root.Q<Line>();

        speed_slider = root.Q<K2Slider>("speed");
        speed_slider.Value = speed;
        speed_slider.Min = 0;
        speed_slider.Max = 2;
        
        period_slider = root.Q<K2Slider>("period");
        period_slider.Value = period;
        period_slider.Min = 0;
        period_slider.Max = 0.3f;

        speed_slider.RegisterCallback<ChangeEvent<float>>(evt => speed = evt.newValue);
        period_slider.RegisterCallback<ChangeEvent<float>>(evt => period = evt.newValue);
    }

    void Update()
    {
        var points = buildPerlin();
        my_line.setPoints(points);
    }
}