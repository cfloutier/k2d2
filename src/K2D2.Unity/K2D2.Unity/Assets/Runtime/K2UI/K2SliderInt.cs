using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using KTools;

namespace K2UI
{
    /// <summary>
    /// complete copy of the K2Slider, I've not figured out how to make it more generic
    /// </summary>
    public class K2SliderInt : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<K2SliderInt, UxmlTraits> { }

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

            private UxmlIntAttributeDescription m_Value = new()
            { name = "value", defaultValue = 0 };

            private UxmlIntAttributeDescription m_Min = new()
            { name = "min", defaultValue = 0 };

            private UxmlIntAttributeDescription m_Max = new()
            { name = "max", defaultValue = 100 };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                K2SliderInt k2_slider = (K2SliderInt)ve;
                SliderInt main_slider = k2_slider.main_slider;

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

        public int value
        {
            get { return main_slider.value; }
            set { 
                if (value == main_slider.value) return;
                main_slider.value = value; 
                listeners?.Invoke(value);
            }
        }

        public delegate void OnChanged(int value);

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

        public int Min
        {
            get { return main_slider.lowValue; }
            set { main_slider.lowValue = value; }
        }
        public int Max
        {
            get { return main_slider.highValue; }
            set { main_slider.highValue = value; }
        }

        public void InitValues(int value, int min, int max)
        {
            Min = min;
            Max = max;
            this.value = value;
        }

        protected SliderInt main_slider;

        Label label_element;

        VisualElement dragger;
        VisualElement tracker;

        VisualElement fill_bar;

        VisualElement min_max_bar;
        Label min_element;
        Label max_element;

        const string slider_uss = "k2-slider";

        const string k2slider_uss = "k2-slider-main";

        public K2SliderInt()
        {
            AddToClassList(k2slider_uss);
            main_slider = new SliderInt() { name = "main_slider" };
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
            main_slider.RegisterCallback<ChangeEvent<int>>((evt) => { SliderValueChanged(); });
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
            if (_labelOnTop)
            {
                if (label_element.parent == null) return;

                if (label_element.parent != this)
                {
                    label_element.parent.Remove(label_element);
                    Insert(0, label_element);
                }
            }
            else
            {
                if (label_element.parent == null) return;

                label_element.parent.Remove(label_element);
                main_slider.Insert(0, label_element);
            }

        }

        void setLabels()
        {
            setLabelPos();

            if (printValue)
                main_slider.label = Label + $" : {value}";
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
        }

        public void Bind(Setting<int> setting)
        {
            this.value = setting.V;
            setting.listeners += v => this.value = v;
            RegisterCallback<ChangeEvent<int>>(evt => setting.V = evt.newValue);
        }

        public K2SliderInt Bind(ClampSetting<int> setting)
        {
            this.Min = setting.min;
            this.Max = setting.max;
            
            this.value = setting.V;
            setting.listeners += v => this.value = v;
            RegisterCallback<ChangeEvent<int>>(evt => setting.V = evt.newValue);
            return this;
        }
    }

}