

using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace K2UI.Tabs
{
    /// <summary>
    /// TabbedPage is the main class to use tabs feature. 
    /// 
    /// It create a tabsBar that will accept tabbed buttons
    /// 
    /// It change the content in #Content element depending on the current Tab
    /// 
    /// Adds TabButton to the Element they will be finally added in the #TabBar
    /// </summary>
    public class TabbedPage : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<TabbedPage, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {    
                get
                {
                    // we can add only tabButton here
                    yield return new UxmlChildElementDescription(typeof(VisualElement));
                }
            }

            UxmlStringAttributeDescription m_SelectedTabName =
                new() { name = "selected-tab-Name", defaultValue = "" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var ate = ve as TabbedPage;

                ate.SelectedTabName = m_SelectedTabName.GetValueFromBag(bag, cc);
            }
        }

        string _selected_tab_name = "";
        public string SelectedTabName
        {
            get {return _selected_tab_name;}
            set { _selected_tab_name = value; }
        }

        TabsBar el_tabsbar;
        VisualElement el_content;

        public override VisualElement contentContainer 
        {
            get{
                 if (el_content != null) 
                    return el_content;
                 else 
                    return this;
            }      
        }

        public TabbedPage()
        {       
            Setup();
            InitializeUI();
        }

        void Setup()
        {
            el_tabsbar = new TabsBar() {name = "TabsBar"};
            var content = new VisualElement() {name = "Content"};

            Add(el_tabsbar);
            Add(content);
            // setup main content after adding to parent
            el_content = content;

            el_tabsbar.RegisterCallback<ChangeEvent<string>>(onTabChanged);
        }

        private void InitializeUI()
        {
            CheckForTabs();
            el_content.RegisterCallback<GeometryChangedEvent>(HandleContentChanged);
            el_content.RegisterCallback<AttachToPanelEvent>(HandleAttachedToPanel);
        }

        private void HandleAttachedToPanel(AttachToPanelEvent evt)
        {
            CheckForTabs();
        }
 
        private void HandleContentChanged(GeometryChangedEvent evt)
        {
            CheckForTabs();
        }

        List<TabButton> all_tabs = new();

        void CheckForTabs()
        {
            // move all tabs from content and put them in the tab bar 
            var new_button = el_content.Q<TabButton>();
            while (new_button != null)
            {
                el_tabsbar.Add(new_button);
                new_button = el_content.Q<TabButton>();
            }
        }

        private void onTabChanged(ChangeEvent<string> evt)
        {
            // Debug.Log("changed "+evt.newValue);
            ShowContent(evt.newValue);   
        }

        void ShowContent(string code)
        {
            foreach (var page in el_content.Children())
            {
                if (page.name == code)
                {
                    page.style.display = DisplayStyle.Flex;
                }
                else
                {
                    page.style.display = DisplayStyle.None;
                }
            }

            foreach (var panel in panels)
            {
                panel.isVisible = panel.code == code;    
            }
        }

        List<K2Panel> panels;

        public void Init(List<K2Panel> panels)
        {
            this.panels = panels;
            foreach(K2Panel panel in this.panels)
                panel.Init(el_tabsbar, el_content);
        }

        public void Select(string code)
        {
            el_tabsbar.setOpenedPage(code);
            ShowContent(code);         
        }

        public void Update()
        {
            foreach(K2Panel panel in this.panels)
                panel.onUpdateUI();
        }
    }
}