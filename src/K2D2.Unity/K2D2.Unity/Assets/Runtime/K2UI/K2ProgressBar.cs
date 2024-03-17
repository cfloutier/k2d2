using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace K2UI
{
    public class K2ProgressBar : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<K2ProgressBar, UxmlTraits> { }

        // Add the two custom UXML attributes.
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription m_Label =
                new() { name = "label", defaultValue = "K2-Progress-Bar" };
            UxmlFloatAttributeDescription m_Value =
                new() { name = "value", defaultValue = 0 };

            UxmlFloatAttributeDescription m_Min =
                new() { name = "min", defaultValue = 0 };

            UxmlFloatAttributeDescription m_Max =
                new() { name = "max", defaultValue = 1 };

            UxmlBoolAttributeDescription m_LabelValue =
                new() { name = "set-label-to-value", defaultValue = false };

            UxmlStringAttributeDescription m_Postfix =
                new() { name = "postfix", defaultValue = "%" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var ate = ve as K2ProgressBar;

                ate.Label = m_Label.GetValueFromBag(bag, cc);
                ate.Value = m_Value.GetValueFromBag(bag, cc);
                ate.Min = m_Min.GetValueFromBag(bag, cc);
                ate.Max = m_Max.GetValueFromBag(bag, cc);

                ate.LabelValue = m_LabelValue.GetValueFromBag(bag, cc);
                ate.Postfix = m_Postfix.GetValueFromBag(bag, cc);
            }
        }

        // Must expose your element class to a { get; set; } property that has the same name 
        // as the name you set in your UXML attribute description with the camel case format
        public string _label;
        public string Label
        {
            get { return _label; }
            set
            { _label = value; updateRender(); }

        }

        float _value;
        public float Value
        {
            get { return _value; }
            set { _value = value; updateRender(); }
        }

        float _min;
        public float Min
        {
            get { return _min; }
            set { _min = value; updateRender(); }
        }

        float _max;
        public float Max
        {
            get { return _max; }
            set { _max = value; updateRender(); }
        }


        bool _label_value;
        public bool LabelValue
        {
            get { return _label_value; }
            set { _label_value = value; updateRender(); }
        }       

        string _postfix;
        public string Postfix
        {
            get { return _postfix; }
            set { _postfix = value; updateRender(); }
        }

        void updateRender()
        {
            if (el_progress != null)
            {
                var len = new StyleLength(Length.Percent(Mathf.InverseLerp(Min, Max, Value) * 100));
                el_progress.style.width = len;
            }
            if (el_label != null)
            {
                if (LabelValue)
                    el_label.text = Label + $"{Value:n2}{Postfix}";
                else
                   el_label.text = Label;
            }

        }

        // In the spirit of the BEM standard, the BigToggleButton has its own block class and two element classes. It also
        // has a class that represents the enabled state of the toggle.
        public static readonly string ussClassName = "k2-progress-bar";

        Label el_label;
        VisualElement el_frame;
        VisualElement el_progress;

        // This constructor allows users to set the contents of the label.
        public K2ProgressBar()
        {
            el_frame = new VisualElement() { name = "frame" };
            Add(el_frame);


            el_progress = new VisualElement() { name = "progress" };
            el_label = new Label();
            el_label.name = "label";


            el_frame.Add(el_progress);
            el_frame.Add(el_label);

            // Style the control overall.
            AddToClassList(ussClassName);
        }
    }

}