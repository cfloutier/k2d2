using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
// using KTools;
// using System;

namespace K2UI
{
    public class InlineEnum : VisualElement
    {

        public new class UxmlFactory : UxmlFactory<InlineEnum, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            private UxmlStringAttributeDescription m_Labels = new()
            { name = "labels", defaultValue = "A;B;C" };

            private UxmlIntAttributeDescription m_Value = new()
            { name = "value", defaultValue = 0 };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                InlineEnum inline_enum = (InlineEnum) ve;
                inline_enum.labels = m_Labels.GetValueFromBag(bag, cc);
                inline_enum.value = m_Value.GetValueFromBag(bag, cc);
            }
        }

        int _value;
        public int value
        {
            get { return _value; }
            set { 
                if (value < 0) value = 0;
                if (labels_list != null)
                {
                    if (value >= labels_list.Length)
                        value = labels_list.Length -1;
                }

                if (_value == value) return;
                int old_value = _value;
                _value = value;
                var my_event = ChangeEvent<int>.GetPooled(old_value, value);
                my_event.target = this;
                SendEvent(my_event);
                UpdateValue();
            }
        }

        string _labels;
        public string labels
        {
            get { return _labels; }
            set
            {
                if (_labels == value) return;
                _labels = value;
                UpdateContent();
            }
        }

        public InlineEnum()
        {
            AddToClassList("inline_enum");
        }

        string[] labels_list = null;
        public List<Button> buttons = new();

        void UpdateContent()
        {
            Clear();
            buttons.Clear();

            labels_list = labels.Split(';');
            
            for (int i = 0 ; i < labels_list.Length; i++)
            {
                var bt = new Button(); 
                buttons.Add(bt);
                bt.AddToClassList("toggle-button");
                bt.text = labels_list[i];
                bt.name = labels_list[i];
                Add(bt);

                int index =i;
                bt.RegisterCallback<ClickEvent>(evt => value=index);
            }
            UpdateValue();
        }

        void UpdateValue()
        {
            for (int i = 0 ; i < buttons.Count; i++)
            {
                var bt = buttons[i];
                if (i == value)
                {
                    bt.AddToClassList("checked");
                }
                else
                {
                    bt.RemoveFromClassList("checked");
                }
            }
        }

       
    }

}