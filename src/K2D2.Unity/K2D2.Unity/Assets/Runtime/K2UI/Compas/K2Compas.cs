using System.Diagnostics.Tracing;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using KTools;
using K2UI.Compas;

namespace K2UI
{
    public class K2Compass : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<K2Compass, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private UxmlFloatAttributeDescription m_Value = new()
            { name = "value", defaultValue = 0 };

            private UxmlFloatAttributeDescription m_AngleRange = new()
            { name = "angle-range", defaultValue = 90f };

            private UxmlBoolAttributeDescription m_Interactive = new()
            { name = "interactive", defaultValue = true };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                K2Compass k2_compas = (K2Compass)ve;

                k2_compas.Value = m_Value.GetValueFromBag(bag, cc);
                k2_compas.AngleRange = m_AngleRange.GetValueFromBag(bag, cc);
                k2_compas.Interactive = m_Interactive.GetValueFromBag(bag, cc);

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

        // internal set value,
        // can be called to get the event in return
        public void forceValue(float new_value, bool send_event = true)
        {
            _value = new_value;
            UpdateContent();
            if (send_event)
            {
                float old_value = Value;
                var my_event = ChangeEvent<float>.GetPooled(old_value, Value);
                my_event.target = this;
                SendEvent(my_event);
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

        bool _interactive = true;
        public bool Interactive
        {
            get { return _interactive; }
            set
            {
                if (_interactive == value) return;
                _interactive = value;
                if (!_interactive)
                    m_IsDragging = false;

                UpdateContent();
            }
        }

        VisualElement el_line;
        VisualElement el_texts;
        VisualElement el_shadow;

        public K2Compass()
        {
            // Debug.Log("name:" + this.name);
            AddToClassList("k2compas");
            el_line = new VisualElement() { name = "lines" };
            el_line.AddToClassList("lines");

            el_texts = new VisualElement() { name = "texts" };
            el_texts.AddToClassList("texts");

            el_shadow = new VisualElement() { name = "shadow" };
            el_shadow.AddToClassList("shadow");
            el_shadow.pickingMode = PickingMode.Ignore;

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

        private bool m_IsDragging = false;
        private Vector2 start_mouse_pos;
        private float start_value;

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (!Interactive) return;

            if (m_IsDragging)
            {
                m_IsDragging = false;
                MouseCaptureController.ReleaseMouse(this);
            }
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (!Interactive) return;

            if (m_IsDragging)
            {
                Vector2 delta = evt.mousePosition - start_mouse_pos;
                // Debug.Log("delta" + delta);
                forceValue(fixDeg(start_value - delta.x / pixel_per_deg));
            }
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (!Interactive) return;

            if (!m_IsDragging)
            {
                MouseCaptureController.CaptureMouse(this);
                m_IsDragging = true;
                start_mouse_pos = evt.mousePosition;
                start_value = Value;
            }
        }

        LabelsFactory labels_factory = new LabelsFactory();
        ButtonsFactory buttons_factory = new ButtonsFactory();

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
            while (deg >= 360)
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
            buttons_factory.start();

            int deg = min_deg.FloorTen();
            while (deg <= max_deg)
            {
                float x_pos = deg_to_xpos(deg, Value);

                int drawn_deg = (int)fixDeg(deg);
                if (drawn_deg % 45 == 0)
                {
                    // need a label
                    int index = (int)(drawn_deg / 45) + 8;
                    if (index >= 0 && index < directions.Length)
                    {
                        var pos = new Vector2(x_pos, 0);
                        // if interactive must have a button 

                        VisualElement el;
                        if (Interactive)
                        {
                            el = buttons_factory.buttonPool(drawn_deg, this, directions[index], pos);
                        }
                        else
                        {
                            el = labels_factory.labelPool(true, directions[index], pos);
                        }

                        el_texts.Add(el);

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
            int deg = min_deg;

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