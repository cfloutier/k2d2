

using UnityEngine;
using UnityEngine.UIElements;
using System;
using KTools;

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
    }
}