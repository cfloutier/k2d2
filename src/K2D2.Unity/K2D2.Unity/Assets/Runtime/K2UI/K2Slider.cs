using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Data;
using KTools;
using System.Net.Sockets;

namespace K2UI
{
    public class K2Slider : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<K2Slider, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            private UxmlStringAttributeDescription m_Label = new()
            { name = "label", defaultValue = "" };

            private UxmlBoolAttributeDescription m_labelOnTop = new()
            { name = "label-on-top", defaultValue = false };

            private UxmlBoolAttributeDescription m_printValue = new()
            { name = "print-value", defaultValue = false };

            private UxmlStringAttributeDescription m_MinMaxLabel = new()
            { name = "min-max-label", defaultValue = "" };

            private UxmlFloatAttributeDescription m_Value = new()
            { name = "value", defaultValue = 0f };

            private UxmlFloatAttributeDescription m_Min = new()
            {
                name = "min",
                defaultValue = 0f
            };

            private UxmlFloatAttributeDescription m_Max = new()
            {
                name = "max",
                defaultValue = 1f
            };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                K2Slider k2_slider = (K2Slider)ve;
                Slider main_slider = k2_slider.main_slider;

                k2_slider.value = m_Value.GetValueFromBag(bag, cc);
                k2_slider.printValue = m_printValue.GetValueFromBag(bag, cc);
                k2_slider.labelOnTop = m_labelOnTop.GetValueFromBag(bag, cc);
                k2_slider.minMaxLabel = m_MinMaxLabel.GetValueFromBag(bag, cc);

                k2_slider.Label = m_Label.GetValueFromBag(bag, cc);

                k2_slider.Min = m_Min.GetValueFromBag(bag, cc);
                k2_slider.Max = m_Max.GetValueFromBag(bag, cc);

                main_slider.direction = SliderDirection.Horizontal;//m_Direction.GetValueFromBag(bag, cc);
                main_slider.pageSize = 0;//m_PageSize.GetValueFromBag(bag, cc);
                main_slider.showInputField = false;//m_ShowInputField.GetValueFromBag(bag, cc);
                main_slider.inverted = false;//m_Inverted.GetValueFromBag(bag, cc);

                k2_slider.SliderValueChanged();
                k2_slider.setLabels();
            }
        }

        public float value
        {
            get { return main_slider.value; }
            set { 
                if (value == main_slider.value) return;
                main_slider.value = value; 
                listeners?.Invoke(value);
            }
        }

        public delegate void OnChanged(float value);

        public event OnChanged listeners;

        string _label;
        public string Label
        {
            get { return _label; }
            set
            {
                _label = value;
            }
        }

        bool _printValue = false;
        bool printValue
        {
            get { return _printValue; }
            set
            {
                _printValue = value;
                setLabels();
            }
        }

        bool _labelOnTop = true;
        bool labelOnTop
        {
            get { return _labelOnTop; }
            set
            {
                _labelOnTop = value;
                setLabels();
            }
        }

        string _min_max_label = "";
        public string minMaxLabel
        {
            get { return _min_max_label; }
            set
            {
                _min_max_label = value;
                setLabels();
            }
        }

        public float Min
        {
            get { return main_slider.lowValue; }
            set { main_slider.lowValue = value; }
        }
        public float Max
        {
            get { return main_slider.highValue; }
            set { main_slider.highValue = value; }
        }



        public void InitValues(float value, float min, float max)
        {
            Min = min;
            Max = max;
            this.value = value;
        }

        protected Slider main_slider;

        Label label_element;

        VisualElement dragger;
        VisualElement tracker;

        VisualElement fill_bar;

        VisualElement min_max_bar;
        Label min_element;
        Label max_element;

        const string slider_uss = "k2-slider";

        const string k2slider_uss = "k2-slider-main";

        public K2Slider()
        {
            AddToClassList(k2slider_uss);
            main_slider = new Slider() { name = "main_slider" };
            main_slider.AddToClassList(slider_uss);
            Add(main_slider);
            dragger = main_slider.Q<VisualElement>("unity-dragger");
            tracker = main_slider.Q<VisualElement>("unity-tracker");
            var container = main_slider.Q<VisualElement>("unity-drag-container");
            fill_bar = new VisualElement() { name = "fill_bar" };
            label_element = main_slider.labelElement;

            min_max_bar = new VisualElement() { name = "min_max_bar" };
            min_element = new Label() { name = "min_label" };
            max_element = new Label() { name = "max_label" };

            Add(min_max_bar);

            min_max_bar.Add(min_element);
            min_max_bar.Add(max_element);

            tracker.Add(fill_bar);
            main_slider.RegisterCallback<ChangeEvent<float>>((evt) => { SliderValueChanged(); });
            main_slider.RegisterCallback<GeometryChangedEvent>((evt) => SliderValueChanged());
        }

        void SliderValueChanged()
        {
            Vector2 pos = dragger.parent.LocalToWorld(dragger.transform.position);
            fill_bar.transform.position = fill_bar.parent.WorldToLocal(pos);

            setLabels();
        }

        void setLabelPos()
        {
            if (labelOnTop)
            {
                if (label_element.parent == null) 
                {
                    // Debug.Log("no parent el");
                    return;
                }
                
                if (label_element.parent != this)
                {          
                    // Debug.Log("moving to top");
                    Insert(0, label_element);
                }
            }
            else
            {
                if (label_element.parent == null) 
                {
                    // Debug.Log("no parent el");
                    return;
                }

                if (label_element.parent != main_slider)
                {
                    // Debug.Log("moving to line");
                    main_slider.Insert(0, label_element);
                }
            }
        }

        void setLabels()
        {
            

            if (printValue)
            {
                main_slider.label = Label + " : " + value.ToStringInvariant("N2"); ;
                // Thread.CurrentThread.CurrentCulture = previous_culture;
            }
            else
                main_slider.label = Label;

            if (string.IsNullOrEmpty(minMaxLabel))
            {
                min_max_bar.style.display = DisplayStyle.None;
            }
            else
            {
                min_max_bar.style.display = DisplayStyle.Flex;

                if (minMaxLabel == "x")
                {
                    // magic code to take from min max values
                    min_element.text = Min.ToStringInvariant();
                    max_element.text = Max.ToStringInvariant();
                }
                else
                {
                    var labels = minMaxLabel.Split("-");
                    if (labels.Length >= 1)
                        min_element.text = labels[0];

                    if (labels.Length >= 2)
                        max_element.text = labels[1];
                    else
                        max_element.text = "";
                }
            }

            setLabelPos();
        }

        // 2 ways binding
        public void Bind(Setting<float> setting)
        {
            this.value = setting.V;
            setting.listeners += v => this.value = v;
            RegisterCallback<ChangeEvent<float>>(evt => setting.V = evt.newValue);
        }

        public K2Slider Bind(ClampSetting<float> setting)
        {
            this.Min = setting.min;
            this.Max = setting.max;
            this.value = setting.V;
            setting.listeners += v => this.value = v;
            RegisterCallback<ChangeEvent<float>>(evt => setting.V = evt.newValue);
            return this;
        }
    }

}