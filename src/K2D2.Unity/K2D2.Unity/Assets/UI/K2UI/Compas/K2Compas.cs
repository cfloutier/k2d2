using System.Diagnostics.Tracing;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.UIElements;

namespace K2UI.Compas
{
    class K2Compass : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<K2Compass, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            private UxmlFloatAttributeDescription m_Value = new()
            { name = "value", defaultValue = 0 };

            private UxmlFloatAttributeDescription m_AngleRange = new()
            { name = "angle-range", defaultValue = 90f };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                K2Compass k2_compas = (K2Compass)ve;

                k2_compas.Value = m_Value.GetValueFromBag(bag, cc);
                k2_compas.AngleRange = m_AngleRange.GetValueFromBag(bag, cc);

                k2_compas.UpdateContent();
            }
        }

        float _value = 0;
        public float Value
        {
            get { return _value; }
            set
            {
                if (_value == value) return;
                _value = value;
                UpdateContent();
            }
        }

        float _angleRange = 0;
        public float AngleRange
        {
            get { return _angleRange; }
            set
            {
                if (_angleRange == value) return;
                _angleRange = Mathf.Clamp(value, 0, 360);
                UpdateContent();
            }
        }

        VisualElement el_line;
        VisualElement el_texts;
        VisualElement el_shadow;
        

        public K2Compass()
        {
            AddToClassList("k2compas");
            el_line = new VisualElement() { name = "lines" };
            el_line.AddToClassList("lines");

            el_texts = new VisualElement() { name = "texts" };
            el_line.AddToClassList("texts");

            el_shadow = new VisualElement() { name = "shadow" };
            el_shadow.AddToClassList("shadow");

            Add(el_line);
            Add(el_texts);
            Add(el_shadow);

            el_line.generateVisualContent += Draw;

            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<MouseMoveEvent>(OnMouseMove);
            RegisterCallback<MouseUpEvent>(OnMouseUp);

            // Register a callback after custom style resolution.
            RegisterCallback<CustomStyleResolvedEvent>(evt => CustomStylesResolved(evt));
            RegisterCallback<GeometryChangedEvent>(onGeometryChanged);
        }

        void CustomStylesResolved(CustomStyleResolvedEvent evt)
        {
            K2Compass element = (K2Compass)evt.currentTarget;
            element.UpdateCustomStyles();
        }

        void UpdateContent()
        {
            setUpLabels();
            el_line.MarkDirtyRepaint();
        }

        class K2CompasCustomStyle
        {
            static CustomStyleProperty<float> s_CenterLineWidth = new CustomStyleProperty<float>("--center-line-width");
            static CustomStyleProperty<Color> s_CenterLineColor = new CustomStyleProperty<Color>("--center-line-color");

            static CustomStyleProperty<float> s_LineWidth = new CustomStyleProperty<float>("--line-width");
            static CustomStyleProperty<Color> s_LineColor_1 = new CustomStyleProperty<Color>("--line-color-1");
            static CustomStyleProperty<Color> s_LineColor_2 = new CustomStyleProperty<Color>("--line-color-2");
            static CustomStyleProperty<Color> s_LineColor_3 = new CustomStyleProperty<Color>("--line-color-3");

            static CustomStyleProperty<float> s_line_y1 = new CustomStyleProperty<float>("--line-y1");
            static CustomStyleProperty<float> s_line_y5 = new CustomStyleProperty<float>("--line-y5");
            static CustomStyleProperty<float> s_line_y10 = new CustomStyleProperty<float>("--line-y10");
            static CustomStyleProperty<float> s_line_y45 = new CustomStyleProperty<float>("--line-y45");

            public float centerLineWidth = 1;
            public Color centerLineColor = Color.white;

            public float lineWidth = 2;
            public Color lineColor_1 = Color.white;
            public Color lineColor_2 = Color.white;
            public Color lineColor_3 = Color.white;

            public float y_1 = 30;
            public float y_5 = 25;
            public float y_10 = 20;
            public float y_45 = 25;

            public void UpdateCustomStyles(ICustomStyle customStyle)
            {
                if (customStyle.TryGetValue(s_CenterLineColor, out var my_color))
                {
                    this.centerLineColor = my_color;
                }

                if (customStyle.TryGetValue(s_CenterLineWidth, out var my_float))
                {
                    this.centerLineWidth = my_float;
                }

                if (customStyle.TryGetValue(s_LineColor_1, out my_color))
                {
                    this.lineColor_1 = my_color;
                }

                if (customStyle.TryGetValue(s_LineColor_2, out my_color))
                {
                    this.lineColor_2 = my_color;
                }

                if (customStyle.TryGetValue(s_LineColor_3, out my_color))
                {
                    this.lineColor_3 = my_color;
                }   

                if (customStyle.TryGetValue(s_LineWidth, out my_float))
                {
                    this.lineWidth = my_float;
                }

                if (customStyle.TryGetValue(s_line_y1, out my_float))
                {
                    this.y_1 = my_float;
                }

                if (customStyle.TryGetValue(s_line_y5, out my_float))
                {
                    this.y_5 = my_float;
                }

                if (customStyle.TryGetValue(s_line_y10, out my_float))
                {
                    this.y_10 = my_float;
                }

                if (customStyle.TryGetValue(s_line_y45, out my_float))
                {
                    this.y_45 = my_float;
                }
            }
        }

        // The custome style accessor
        K2CompasCustomStyle cs = new();

        void UpdateCustomStyles()
        {
            cs.UpdateCustomStyles(customStyle);

            UpdateContent();
        }
        // After the custom colors are resolved, this method uses them to color the meshes and (if necessary) repaint
        // the control.


        private void onGeometryChanged(GeometryChangedEvent evt)
        {
            UpdateContent();
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (m_IsDragging)
            {
                m_IsDragging = false;
                MouseCaptureController.ReleaseMouse(this);
            }
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (m_IsDragging)
            {
                Vector2 delta = evt.mousePosition - start_mouse_pos;
                // Debug.Log("delta" + delta);
                float old_value = Value;
                Value = start_value - delta.x / pixel_per_deg;
                var my_event =  ChangeEvent<float>.GetPooled(old_value, Value);
                my_event.target = this;
                SendEvent(my_event);
            }
        }

        private bool m_IsDragging = false;
        private Vector2 start_mouse_pos;
        private float start_value;

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (!m_IsDragging)
            {
                MouseCaptureController.CaptureMouse(this);
                m_IsDragging = true;
                start_mouse_pos = evt.mousePosition;
                start_value = Value;
            }
        }

        LabelFactory labels_factory = new();

        // setup in uss

        string[] directions = { "N", "NE", "E", "SE", "S", "SW", "W", "NW", "N", "NE", "E", "SE", "S", "SW", "W", "NW", "N", "NE", "E" };
        // No degrees for those values
        int[] forbiden_values = { 40, 50, 130, 140, 220, 230, 310, 320 };

        // internals values
        float width, height;

        int min_deg, max_deg;

        float pixel_per_deg;

        void compute_values()
        {
            Rect rect = el_line.contentRect;
            width = rect.width;
            height = rect.height;
            pixel_per_deg = width / AngleRange;

            min_deg = (int)Mathf.Ceil(xpos_to_deg(0, Value - 10));
            max_deg = (int)Mathf.Floor(xpos_to_deg(width + 10, Value));
        }

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

        void setUpLabels()
        {
            compute_values();

            // restart factory 
            labels_factory.start();

            float deg = min_deg.FloorTen();
            while (deg <= max_deg)
            {
                float x_pos = deg_to_xpos(deg, Value);

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

            painter.lineCap = LineCap.Round;
            while (deg < max_deg)
            {
                float x_pos = deg_to_xpos(deg, Value);
                float h;

                float drawn_deg = fixDeg(deg);

                if (drawn_deg % 45 == 0)
                {
                    h = cs.y_45;
                    painter.lineWidth = cs.lineWidth;
                    painter.strokeColor = cs.lineColor_1;
                }
                else if (drawn_deg % 10 == 0)
                {
                    h = cs.y_10;
                    painter.lineWidth = cs.lineWidth * 0.8f;
                    painter.strokeColor = cs.lineColor_1;
                }
                else if (drawn_deg % 5 == 0)
                {
                    h = cs.y_5;
                    painter.lineWidth = cs.lineWidth * 0.6f;
                    painter.strokeColor = cs.lineColor_2;
                }
                else
                {
                    h = cs.y_1;
                    painter.lineWidth = cs.lineWidth * 0.4f;
                    painter.strokeColor = cs.lineColor_3;
                }

                painter.BeginPath();
                painter.MoveTo(new Vector2(x_pos, 0));
                painter.LineTo(new Vector2(x_pos, height - h));

                painter.Stroke();

                deg += 1;
            }

            painter.lineWidth = cs.centerLineWidth;
            painter.strokeColor = cs.centerLineColor;

            painter.BeginPath();
            painter.MoveTo(new Vector2(width / 2, 0));
            painter.LineTo(new Vector2(width / 2, height));
            painter.Stroke();
        }


    }
}