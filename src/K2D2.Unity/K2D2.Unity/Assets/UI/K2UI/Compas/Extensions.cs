using UnityEngine;
using UnityEngine.UIElements;
namespace K2UI.Compas
{
    public static class ExtensionMethods
    {
        public static int FloorTen (this int i)
        {
            return ((int)Mathf.Floor(i / 10f)) * 10;
        }

        public static void Clean(this VisualElement el)
        {
            var count = el.childCount;
            for (int i=0; i< count; i++)
            {
                el.RemoveAt(0);
            }
        }
    }
}