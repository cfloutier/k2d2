using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace K2UI
{
    class BigToggleButton : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<BigToggleButton, UxmlTraits> { }

        // Add the two custom UXML attributes.
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription m_String =
                new() { name = "label", defaultValue = "Big Toggle Button !" };
            UxmlBoolAttributeDescription m_Bool =
                new() { name = "value", defaultValue = false };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var ate = ve as BigToggleButton;

                ate.label = m_String.GetValueFromBag(bag, cc);
                ate.active = m_Bool.GetValueFromBag(bag, cc);
            }
        }

        Label label_el;

        // Must expose your element class to a { get; set; } property that has the same name 
        // as the name you set in your UXML attribute description with the camel case format
        public string _label;
        public string label
        {
            get { return _label; }
            set
            {
                _label = value;
                label_el.text = value;
            }
        }
        bool _active;
        public bool active
        {
            get { return _active; }
            set
            {
                _active = value;
                EnableInClassList(checkedUssClassName, _active);
                foreach(var callbacks in onChangedEvent)
                {
                    callbacks(active);
                }
            }
        }

        // In the spirit of the BEM standard, the BigToggleButton has its own block class and two element classes. It also
        // has a class that represents the enabled state of the toggle.
        public static readonly string ussClassName = "big-toggle-button";
        public static readonly string checkedUssClassName = "checked";

        // This constructor allows users to set the contents of the label.
        public BigToggleButton()
        {
            label_el = new Label();
            Add(label_el);

            // Style the control overall.
            AddToClassList(ussClassName);
            this.AddManipulator(new Clickable(evt => {
                ToggleValue();
                }));
        }



        public delegate void OnChanged(bool active);
        List<OnChanged> onChangedEvent = new();

        // All three callbacks call this method.
        void ToggleValue()
        {
            active = !active;
        }

        // // Because ToggleValue() sets the value property, the BaseField class fires a ChangeEvent. This results in a
        // // call to SetValueWithoutNotify(). This example uses it to style the toggle based on whether it's currently
        // // enabled.
        // public override void SetValueWithoutNotify(bool newValue)
        // {
        //     base.SetValueWithoutNotify(newValue);

        //     //This line of code styles the input element to look enabled or disabled.

        // }
    }

}