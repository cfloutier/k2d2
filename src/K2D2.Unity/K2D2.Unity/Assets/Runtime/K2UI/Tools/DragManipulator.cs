// using UitkForKsp2;
using UnityEngine;
using UnityEngine.UIElements;

namespace K2UI
{
    /// <summary>
    /// A manipulator to make UI Toolkit elements draggable within the screen bounds.
    /// </summary>
    public class DragManipulator : IManipulator
    {
        private VisualElement _target;
        private Vector3 _offset;
        // private PickingMode _mode;

        /// <summary>
        /// Indicates whether the element is currently being dragged.
        /// </summary>
        public bool IsDragging { get; private set; }

        /// <summary>
        /// Enables or disables the dragging functionality.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// The target element that will be made draggable.
        /// </summary>
        public VisualElement target
        {
            get => _target;
            set
            {
                _target = value;
                _target.RegisterCallback<PointerDownEvent>(OnPointerDown);
                _target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
                _target.RegisterCallback<PointerUpEvent>(OnPointerUp);
            }
        }


        /// <summary>
        /// recursive search in parent element for a type
        /// </summary>
        /// <typeparam name="TargetType">the type we search</typeparam>
        /// <param name="target">target element</param>
        /// <param name="nb_parents">nb parents that will be checked</param>
        /// <returns>true if the type is valid or if one of the parent type is valid</returns>
        private bool checkTarget<TargetType>(VisualElement target, int nb_parents = 5)
        {
            // Debug.Log("type is " + target);
            if (target is TargetType)
            {
                // Debug.Log("OK " + target);
                return true;
            }

            if (nb_parents == 0)
                return false;

            if (target.parent == null)
                return false;

            nb_parents--;

            return checkTarget<TargetType>(target.parent, nb_parents);
        }

        /// <summary>
        /// Handles the initiation of the dragging process.
        /// </summary>
        private void OnPointerDown(PointerDownEvent evt)
        {
            if (!(evt.target is VisualElement))
                return;
            VisualElement target = evt.target as VisualElement;

            // IntegerField field;

            if (!IsEnabled) return;
            if (checkTarget<IntegerField>(target)) return;
            if (checkTarget<FloatField>(target)) return;
            if (checkTarget<TextField>(target)) return;
            if (checkTarget<K2Toggle>(target)) return;
            if (checkTarget<K2Compass>(target)) return;

            // _mode = target.pickingMode;
            // target.pickingMode = PickingMode.Ignore;
            IsDragging = true;
            _offset = evt.localPosition;
            _target.CapturePointer(evt.pointerId);
        }

        /// <summary>
        /// Handles the movement of the draggable element.
        /// </summary>
        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!IsDragging || !IsEnabled)
            {
                return;
            }

            var delta = evt.localPosition - _offset;
            var newPosition = target.transform.position + delta;

            _target.transform.position = newPosition;
        }

        /// <summary>
        /// Handles the end of the dragging process.
        /// </summary>
        private void OnPointerUp(PointerUpEvent evt)
        {
            IsDragging = false;
            _target.ReleasePointer(evt.pointerId);
            // target.pickingMode = _mode;
        }
    }
}