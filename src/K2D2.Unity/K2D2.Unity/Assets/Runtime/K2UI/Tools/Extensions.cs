

using UnityEngine;
using UnityEngine.UIElements;
using System;
using KTools;
using System.Runtime.CompilerServices;

namespace K2UI
{
    /// <summary>
    /// Extensions usied for VisualElement
    /// </summary>
    public static class VisualElementExtension
    {
        public static void Clean(this VisualElement el)
        {
            var count = el.childCount;
            for (int i=0; i< count; i++)
            {
                el.RemoveAt(0);
            }
        }

      
        public static void Show(this VisualElement element, bool show)
        {
            element.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public static IntegerField Bind(this IntegerField element, Setting<int> setting) 
        {
            element.value = setting.V;
            setting.listeners += v => element.value = v;
            element.RegisterCallback<ChangeEvent<int>>(evt => setting.V = evt.newValue);
            element.isDelayed = true;
            return element;
        }

        public static FloatField Bind(this FloatField element, Setting<float> setting)
        {
            element.value = setting.V;
            setting.listeners += v => element.value = v;
            element.RegisterCallback<ChangeEvent<float>>(evt => setting.V = evt.newValue);
            element.isDelayed = true;
             return element;
        }

        public static Label AddLine(this Label label, string text)
        {
            label.text += "\n" + text;
            return label;
        }


        public delegate void onClicFct();
        public static Button listenClick(this Button button, onClicFct on_clic)
        {
            button.RegisterCallback<ClickEvent>(evt =>
            {
                on_clic();
            });

            return button;
        }
    }
}