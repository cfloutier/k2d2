using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace K2UI.Tabs
{
    /// <summary>
    /// TabButton have two states : active (showing current content) and lighted (pilot is on)
    /// </summary>
    public class TabButton : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<TabButton, UxmlTraits> { }

        // Add the two custom UXML attributes.
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription m_Label =
                new() { name = "label", defaultValue = "Tab Button" };
            UxmlBoolAttributeDescription m_Active =
                new() { name = "active", defaultValue = false };
            UxmlBoolAttributeDescription m_Lighted =
                new() { name = "lighted", defaultValue = false };         

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var ate = ve as TabButton;

                ate.label = m_Label.GetValueFromBag(bag, cc);
                ate.Active = m_Active.GetValueFromBag(bag, cc);
                ate.Lighted = m_Lighted.GetValueFromBag(bag, cc);
            }
        }

        // Must expose your element class to a { get; set; } property that has the same name 
        // as the name you set in your UXML attribute description with the camel case format
        public string _label;
        public string label
        {
            get { return _label; }
            set
            {
                _label = value;
                el_label.text = value;
            }
        }
        bool _active;
        public bool Active
        {
            get { return _active; }
            set
            {
                if (_active == value)
                    return;

                var evt = ChangeEvent<bool>.GetPooled(_active, value);
                evt.target = this;
                _active = value;
                EnableInClassList(activeUss, _active);
                SendEvent(evt);
            }
        }

        bool _lighted;
        public bool Lighted
        {
            get { return _lighted; }
            set
            {
                _lighted = value;
                el_light.EnableInClassList(lightedUss, _lighted);
            }
        }

        // In the spirit of the BEM standard, the TabButton has its own block class and two element classes. It also
        // has a class that represents the enabled state of the toggle.
        public static readonly string ussClassName = "k2-tab-button";
        public static readonly string activeUss = ussClassName+"--active";

        public static readonly string usslightName = "tab_light";

        public static readonly string lightedUss = usslightName+"--lighted";

        Label el_label;
        VisualElement el_light;
        
        // This constructor allows users to set the contents of the label.
        public TabButton()
        {
            el_light = new VisualElement();
            el_light.name = "tab_light";
            el_light.AddToClassList(usslightName);
            Add(el_light);

            el_label = new Label();
            Add(el_label);

            // Style the control overall.
            AddToClassList(ussClassName);
            this.AddManipulator(new Clickable(evt => {
                ToggleActive();
                }));
        }

        // All three callbacks call this method.
        void ToggleActive()
        {
            Active = !Active;
        }
    }
}