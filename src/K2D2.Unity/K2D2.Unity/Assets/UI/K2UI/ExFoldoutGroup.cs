using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEditor.Search;
using System.Runtime.ExceptionServices;

namespace K2UI
{
    class ExFoldoutGroup : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<ExFoldoutGroup, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get
                {
                    yield return new UxmlChildElementDescription(typeof(VisualElement));
                }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
            }
        }

        public ExFoldoutGroup()
        {
            RegisterCallback<AttachToPanelEvent>(onAttached);
        }

        int openedIndex = -1;

        List<Foldout> list_foldout;

        private void onAttached(AttachToPanelEvent evt)
        {

            list_foldout = this.Query<Foldout>().ToList();
            openedIndex = -1;
            UpdateState();

            foreach(var foldout in list_foldout)
            {
                foldout.value = false;
                foldout.RegisterCallback<ChangeEvent<bool>>(onChanged);
            }
        }

        private void onChanged(ChangeEvent<bool> evt)
        {
            VisualElement target = evt.target as VisualElement;
            
            if (target.GetType() != typeof(Foldout))
                return;

            if (evt.newValue)
            {
                openedIndex = list_foldout.IndexOf(evt.target as Foldout);
                Debug.Log($"index {openedIndex}");
                UpdateState();
            }
            // Debug.Log($"evt {evt.target}");
        }

        void UpdateState()
        {
            var index = 0;
            foreach(var foldout in list_foldout)
            {
                if (index != openedIndex)
                    foldout.value = false;

                index++;        
            }

        }
    }
}