using System.Diagnostics.Tracing;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections.Generic;

// TODO : créer le composant et revoir la classe
// ajouter la prise en charge de nouveaux styles pour la taille des traits, couleur et hauteur
// ajouter les boutons

public class LabelFactory
{
    string uss_big_text = "big_text";
    string uss_small_text = "small_text";

    public List<Label> labels = new List<Label>();

    int nb_used = 0; 
   
    public void start()
    {
        foreach(var label in labels)
        {
            label.style.display = DisplayStyle.None;
        }
        nb_used = 0;
    }
    
    public Label labelPool(bool big, string text, Vector2 pos)
    {
        Label label = null;
        if (nb_used >= labels.Count)
        {
            // need a new Label
            label = new Label();
            labels.Add(label);
        }
        else
        {
            label = labels[nb_used];      
        }

        nb_used++;

        if (big)
        {
            label.RemoveFromClassList(uss_small_text);
            label.AddToClassList(uss_big_text);
        }
        else
        {
            label.RemoveFromClassList(uss_big_text);
            label.AddToClassList(uss_small_text);
        }

        label.style.display = DisplayStyle.Flex;
        label.style.left = pos.x;
        label.style.bottom = pos.y;
        label.text = text;
        label.name = text;

        return label;
    }

}

public static class VisualElementExtensions
{
    public static void Clean(this VisualElement el)
    {
        var count = el.childCount;
        for (int i=0; i< count; i++)
        {
            el.RemoveAt(0);
        }
    }
}

public static class ExtensionMethods
{
    public static int FloorTen (this int i)
    {
        return ((int)Mathf.Floor(i / 10f)) * 10;
    }
}


[RequireComponent(typeof(UIDocument))]
public class CompasTest : MonoBehaviour
{

    VisualElement el_frame;
    VisualElement el_line;
    VisualElement el_texts;

    public void OnEnable()
    {
        var doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;
        // root.AddToClassList("compas");

        el_frame = root.Q<VisualElement>("frame");
        el_line = root.Q<VisualElement>("lines");
        el_texts = root.Q<VisualElement>("texts");
        // cleanup test label in content
        el_texts.Clean();

        el_line.generateVisualContent += Draw;

        el_frame.RegisterCallback<MouseDownEvent>(OnMouseDown);
        el_frame.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        el_frame.RegisterCallback<MouseUpEvent>(OnMouseUp);
    }

    private void OnMouseUp(MouseUpEvent evt)
    {
       if (m_IsDragging)
       {
            m_IsDragging = false;
            MouseCaptureController.ReleaseMouse(el_frame);
       }
    }

    private void OnMouseMove(MouseMoveEvent evt)
    {
        if (m_IsDragging)
        {
            Vector2 delta = evt.mousePosition - start_mouse_pos;
            // Debug.Log("delta" + delta);
            value = start_value - delta.x/ pixel_per_deg;
        }
    }

    private bool m_IsDragging = false;
    private Vector2 start_mouse_pos;
    private float start_value;

    private void OnMouseDown(MouseDownEvent evt)
    {
        if (!m_IsDragging)
        {
            MouseCaptureController.CaptureMouse(el_frame);
            m_IsDragging = true;
            start_mouse_pos = evt.mousePosition;
            start_value = value;
        }
         
    }

    LabelFactory labels_factory = new();

    public float range_angle = 180;
    public float value = -90;

    public float lineWidth = 2;

    public float height_frame = 50;

    float h_1 = 0.2f;
    float h_5 = 0.4f;
    float h_10 = 0.5f;
    float h_45 = 0.5f;

    // setup in uss

    string[] directions = { "N", "NE", "E", "SE", "S", "SW", "W", "NW", "N", "NE", "E", "SE", "S", "SW", "W", "NW", "N", "NE", "E" };
    // No degrees for those values
    int[] forbiden_values = { 40, 50, 130, 140, 220, 230, 310, 320 };

    public void Update()
    {
        setUpLabels();
        el_frame.style.height = new StyleLength(height_frame);
        el_line.MarkDirtyRepaint();
    }

    void compute_values()
    {
        Rect rect = el_line.contentRect;

        range_angle = Mathf.Clamp(range_angle, 0, 360);

        width = rect.width;
        height = rect.height;
        pixel_per_deg = width / range_angle;

        min_deg = (int)Mathf.Ceil(xpos_to_deg(0, value-10));
        max_deg = (int)Mathf.Floor(xpos_to_deg(width+10, value));
    }

    // internals values
    float width, height;

    int min_deg, max_deg;

    float pixel_per_deg;

    public FontAsset font;

    float xpos_to_deg(float xpos, float angle)
    {
        return angle + (-width / 2 + xpos) / pixel_per_deg;
    }

    float deg_to_xpos(float deg, float angle)
    {
        return (deg - angle) * pixel_per_deg + width / 2;
    }

    float fixDeg(float deg)
    {
        while (deg > 360)
            deg -= 360;
        while (deg < 0)
            deg += 360;

        return deg;
    }

    static CustomStyleProperty<float> s_LineWidth = new CustomStyleProperty<float>("--line-width");
    static CustomStyleProperty<Color> s_LineColor = new CustomStyleProperty<Color>("--line-color");

    void setUpLabels()
    {
        compute_values();

        // restart factory 
        labels_factory.start();

        float deg = min_deg.FloorTen();
        while (deg <= max_deg)
        {
            float x_pos = deg_to_xpos(deg, value);
            
            float drawn_deg = fixDeg(deg);
            if (drawn_deg % 45 == 0)
            {      
                // need a label
                int index = (int)(drawn_deg / 45) + 8;
                if (index >= 0 && index < directions.Length)
                {
                    var pos = new Vector2(x_pos, 0);   
                    // if interactive must have a button 
                    var label = labels_factory.labelPool(true, directions[index], pos);
                    el_texts.Add(label);
                }
                else
                    Debug.LogError("index = " + index);
            }
            else if (drawn_deg % 10 == 0)
            {
                if (!forbiden_values.Contains((int)drawn_deg))
                {
                    var pos = new Vector2(x_pos, 0);    
                    var label = labels_factory.labelPool(false, drawn_deg.ToString(), pos);
                    el_texts.Add(label); 
                }
            }
            deg += 5;
        }
    }

    void Draw(MeshGenerationContext ctx)
    {
        float deg = min_deg;

        Painter2D painter = ctx.painter2D;
        
        painter.strokeColor = Color.white;
        painter.lineCap = LineCap.Round;

        painter.lineWidth = lineWidth/2;

        painter.BeginPath();
        painter.MoveTo(new Vector2(width/2, 0));
        painter.LineTo(new Vector2(width/2, height));
        painter.Stroke();
    
        while (deg < max_deg)
        {
            float x_pos = deg_to_xpos(deg, value);
            float h;

            float drawn_deg = fixDeg(deg);

            if (drawn_deg % 45 == 0)
            {
                h = h_45;
                painter.lineWidth = lineWidth;
            }
            else if (drawn_deg % 10 == 0)
            {
                h = h_10;
                painter.lineWidth = lineWidth*0.8f;
            }
            else if (drawn_deg % 5 == 0)
            {   
                h = h_5;
                painter.lineWidth = lineWidth*0.6f;
            }
            else
            {
                h = h_1;
                painter.lineWidth = lineWidth*0.4f;
            }     

            painter.BeginPath();
            painter.MoveTo(new Vector2(x_pos, 0));
            painter.LineTo(new Vector2(x_pos, h * height));

            painter.Stroke();

            deg += 1;

        }
    }
}