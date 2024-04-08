using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace K2UI.Graph
{
    public class GraphLine : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<GraphLine, UxmlTraits> { }

        // Add the two custom UXML attributes.
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlFloatAttributeDescription m_MinX =
                new() { name = "min-x", defaultValue = 0 };

            UxmlFloatAttributeDescription m_MaxX =
                new() { name = "max-x", defaultValue = 5 };

            UxmlFloatAttributeDescription m_MinY =
                new() { name = "min-y", defaultValue = -1 };

            UxmlFloatAttributeDescription m_MaxY =
                new() { name = "max-y", defaultValue = 1 };

            UxmlColorAttributeDescription m_Color =
                new() { name = "line-color", defaultValue = Color.white };    

            UxmlFloatAttributeDescription m_LineWidth =
                new() { name = "line-width", defaultValue = 1 };   
              

            UxmlFloatAttributeDescription m_seed =
                new() { name = "test-seed", defaultValue = -1 };      
            
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var ate = ve as GraphLine;
                ate.MinX = m_MinX.GetValueFromBag(bag, cc);     
                ate.MaxX = m_MaxX.GetValueFromBag(bag, cc);
                ate.MinY = m_MinY.GetValueFromBag(bag, cc);
                ate.MaxY = m_MaxY.GetValueFromBag(bag, cc);
                ate.LineColor = m_Color.GetValueFromBag(bag, cc);
                ate.LineWidth = m_LineWidth.GetValueFromBag(bag, cc);
                
                ate.TestSeed = m_seed.GetValueFromBag(bag, cc);     
            }
        }

        // the orther is left, right, top, bottom
        public void setRanges(float min_x, float max_x, float min_y, float max_y)
        {
            MinX = min_x;
            MaxX = max_x;
            MinY = min_y;
            MaxY = max_y;          
        }

        public float _min_x = 0;
        public float MinX
        {
            get { return _min_x; }
            set { _min_x = value; MarkDirtyRepaint(); }
        }

        public float _max_x = 5;
        public float MaxX
        {
            get { return _max_x; }
            set { _max_x = value; MarkDirtyRepaint(); }
        }

        public float _min_y = -1;
        public float MinY
        {
            get { return _min_y; }
            set { _min_y = value; MarkDirtyRepaint(); }
        }

        public float _max_y = -1;
        public float MaxY
        {
            get { return _max_y; }
            set { _max_y = value; MarkDirtyRepaint(); }
        }

        public Color _color = Color.white;
        public Color LineColor  {
            get { return _color; }
            set { _color = value; MarkDirtyRepaint(); }
        }

        public float _line_width = 1;
        public float LineWidth  {
            get { return _line_width; }
            set { _line_width = value; MarkDirtyRepaint(); }
        }

        public float _test_seed = -1;
        public float TestSeed  {
            get { return _test_seed; }
            set { _test_seed = value; 


            if (_test_seed >= 0)
            {
                rebuildtestLine();   
                MarkDirtyRepaint(); }
            }   
        }

        void rebuildtestLine()
        {
            int nb_point = 300;
            float max_x = 100;
            float period = 0.1f;

            points = new List<Vector2>(); 

            float x = 0; 
            float dx = max_x / nb_point;

            while (x <= max_x)
            {
                float y = Mathf.PerlinNoise(period*x, _test_seed);
                points.Add(new Vector2(x, y));
                x += dx;
            }

        }
        public GraphLine()
        {
           AddToClassList("graph-line");
           generateVisualContent += Draw;
        }

        List<Vector2> points = new();


        public void setSegment(Vector2 point_1, Vector2 point_2)
        {
            points.Clear();
            points.Add(point_1);
            points.Add(point_1);
            points.Add(point_2);
            MarkDirtyRepaint();
        }

        public void setPoints(List<Vector2> points)
        {
            this.points = points;
            MarkDirtyRepaint();
        }

        
        public float aspectRatio()
        {
            if (height_rect == 0) return 1;
            return width_rect / height_rect;
        }

        public float width_rect, height_rect;

        void compute_values()
        {
            Rect rect = contentRect;
            width_rect = rect.width;
            height_rect = rect.height;
        }

        Vector2 value_to_pixel(Vector2 value)
        {
            float ratio_x = (value.x - MinX)  /( MaxX - MinX );
            float ratio_y = (value.y - MinY)  /( MaxY - MinY );

            var res = new Vector2(
                width_rect * ratio_x, height_rect * ratio_y);
            return res;
        }

        void Draw(MeshGenerationContext ctx)
        {    
            compute_values();

            Painter2D painter = ctx.painter2D;

            if (points.Count < 2)
                return;

            painter.lineCap = LineCap.Round;

            painter.lineWidth = LineWidth;
            painter.strokeColor = LineColor;
            painter.BeginPath();

            painter.MoveTo(value_to_pixel(points[0]));
            for (int i = 1; i < points.Count; i++)
            {            
                painter.LineTo(value_to_pixel(points[i]));
            }

            painter.Stroke();
        }
    }
}