using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;
 

namespace K2UI
{
    public class Console : Label
    {
        public static new readonly string ussClassName = "console";

        public new class UxmlFactory : UxmlFactory<Console, UxmlTraits> { }

        public new class UxmlTraits : TextElement.UxmlTraits { }

        public Console() : base()
        {
            AddToClassList(ussClassName);
        }

        public void Set(string txt)
        {
            this.text = txt;
            this.Show(true);
        }

        public void Add(string line)
        {
            if (string.IsNullOrEmpty(text))
                text = line;
            else      
                text += "\n"+line;
            this.Show(true);
        }
    }

    public class StatusLine : Label
    {
        public new class UxmlFactory : UxmlFactory<StatusLine, UxmlTraits> { }

        public enum Level
        {
            Normal,
            Warning,
            Error
        }

        const string uss_name = "k2-status-line";

        string getUss(Level level)
        {
            return uss_name+ "--" + Enum.GetName( typeof(Level), level).ToLower();
        }

        Level _level = Level.Normal;
        public Level level
        {
            get { return _level; }
            set
            {
                var current_uss = getUss(_level);
                RemoveFromClassList(current_uss);

                _level = value;
                current_uss = getUss(_level);
                AddToClassList(current_uss);
            }
        }

        public void Set(string text, Level level)
        {
            this.text = text; 
            this.level = level;
            this.Show(true);
        }

        public new class UxmlTraits : TextElement.UxmlTraits
        {
            private UxmlEnumAttributeDescription<Level> m_level = new()
            {
                name = "level"
            };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                StatusLine textElement = (StatusLine)ve;
                textElement.level = m_level.GetValueFromBag(bag, cc);
            }
        }

        public StatusLine() : base()
        {
            AddToClassList(uss_name);
        }
    }


}