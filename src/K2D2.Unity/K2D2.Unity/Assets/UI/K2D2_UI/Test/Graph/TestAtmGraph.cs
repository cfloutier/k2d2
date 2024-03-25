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
public class TestAtmGraph : MonoBehaviour
{
    public Color color_background = Color.black;
    public Color color_planet = Color.blue;
    public Color color_atm = Color.white;

    public float high_density_h = 50;
    public float low_density_h = 90;

    VisualElement el_graph;

    K2Slider high_slider, low_slider;

    List<GraphLine> lines = new();

    VisualElement gradient;

    Texture2D gradient_Texture;

    public void OnEnable()
    {
        gradient_Texture = new Texture2D(2,256);
        var doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;

        el_graph = root.Q<VisualElement>("Graph");
        el_graph.Clear();

        gradient = new VisualElement();
        gradient.AddToClassList("gradient");
        gradient.style.backgroundImage = gradient_Texture;
        el_graph.Add(gradient);

        high_slider = root.Q<K2Slider>("high");
        high_slider.InitValues(high_density_h, 0, maxAltitude);

        low_slider = root.Q<K2Slider>("low");
        low_slider.InitValues(low_density_h, 0, maxAltitude);

        high_slider.RegisterCallback<ChangeEvent<float>>(evt => high_density_h = evt.newValue);
        low_slider.RegisterCallback<ChangeEvent<float>>(evt => low_density_h = evt.newValue);
    }

    void Update()
    {
        UpdateAtmoTexture(gradient_Texture);
    }
    float maxAltitude = 100;
    public void UpdateAtmoTexture(Texture2D texture)
    {
        float scale = maxAltitude / texture.height; //meters per pixel

        for (int y = 0; y < texture.height; y++)
        {
            float alt = scale * y;

            float low_ratio = alt / low_density_h;  
            float high_ratio = alt / high_density_h;  

            var c = Color.Lerp(color_planet, color_background, low_ratio) ;
            var atm_alpha = 0.5f*Mathf.Clamp01(1-high_ratio);
            var atm_color = new Color(atm_alpha, atm_alpha, atm_alpha, 1);
            c += atm_color;//Color.Lerp(color_atm, c , high_ratio/2) ;

            for (int x = 0; x < texture.width; x++)
            {
                texture.SetPixel(x, y, c);
            }
        }

        texture.Apply();
    }
}