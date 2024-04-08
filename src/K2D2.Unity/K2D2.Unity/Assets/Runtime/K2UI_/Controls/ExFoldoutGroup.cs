using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
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

            UxmlIntAttributeDescription m_OpenedIndex =
                new() { name = "opened-index", defaultValue = -1 };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var ate = ve as ExFoldoutGroup;

                ate.openedIndex = m_OpenedIndex.GetValueFromBag(bag, cc);
            }
        }

        public ExFoldoutGroup()
        {
            
            RegisterCallback<AttachToPanelEvent>(onAttached);
        }

        int _openedIndex = -1;
        int openedIndex
        {
            get { return _openedIndex;}
            set { _openedIndex = value; UpdateState();}
        }

        List<Foldout> list_foldout;

        private void onAttached(AttachToPanelEvent evt)
        {
            updateList();
        }

        private void onChanged(ChangeEvent<bool> evt)
        {
            VisualElement target = evt.target as VisualElement;
            
            if (target.GetType() != typeof(Foldout))
                return;

            if (evt.newValue)
            {
                openedIndex = list_foldout.IndexOf(evt.target as Foldout);
                // Debug.Log($"index {openedIndex}");
                UpdateState();
            }
            // Debug.Log($"evt {evt.target}");
        }

        public void updateList()
        {
            if (list_foldout != null)
            {
                foreach(var foldout in list_foldout)
                {
                    foldout.UnregisterCallback<ChangeEvent<bool>>(onChanged);     
                }
            }

            list_foldout = this.Query<Foldout>().ToList();
            // openedIndex = -1;
            UpdateState();

            foreach(var foldout in list_foldout)
            {
                // foldout.value = false;
                foldout.RegisterCallback<ChangeEvent<bool>>(onChanged);     
            }
        }


        void UpdateState()
        {
            if (list_foldout == null) return;
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