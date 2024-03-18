

using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace K2UI
{
    /// <summary>
    /// Extensions usied for VisualElement
    /// </summary>
    public static class ExtensionMethods
    {
        public static void Clean(this VisualElement el)
        {
            var count = el.childCount;
            for (int i=0; i< count; i++)
            {
                el.RemoveAt(0);
            }
        }

        /// <summary>
        /// Make a VisualElement draggable by adding a DragManipulator.
        /// </summary>
        /// <param name="element">The element to make draggable.</param>
        /// <param name="checkScreenBounds">Should the element be draggable only within the screen bounds?</param>
        /// <typeparam name="T">The type of the element which must be a subclass of VisualElement.</typeparam>
        /// <returns>The element which was made draggable.</returns>
        public static VisualElement MakeDraggable(this VisualElement element)
        {
            element.AddManipulator(new DragManipulator());
            return element;
        }
    }
}