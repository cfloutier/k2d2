

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

        TabsBar tabsbar_el;
        VisualElement content_el;

        public override VisualElement contentContainer 
        {
            get{
                 if (content_el != null) 
                    return content_el;
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
            tabsbar_el = new TabsBar() {name = "TabsBar"};
            var content = new VisualElement() {name = "Content"};

            Add(tabsbar_el);
            Add(content);
            // setup main content after adding to parent
            content_el = content;
            tabsbar_el.RegisterCallback<ChangeEvent<string>>(onTabChanged);
        }

        private void InitializeUI()
        {
            content_el.RegisterCallback<GeometryChangedEvent>(HandleContentChanged);
            
        }

        private void HandleContentChanged(GeometryChangedEvent evt)
        {
            BuildButtons();

        }


        bool hasChanged()
        {
            var buttons = tabsbar_el.Query<TabButton>().ToList();
            var pages = content_el.Query<TabPage>().ToList();

            if (buttons.Count != pages.Count)
                return true;

            for (int i = 0; i < buttons.Count ; i++)
            {
                if (buttons[i].name != pages[i].name)
                    return true;
            }

            return false;
        }

        void BuildButtons()
        {
            if (!hasChanged())
                return;

            tabsbar_el.Clear();
            var pages = content_el.Query<TabPage>().ToList();
            foreach (var page in pages)
            {
                var bt = new TabButton();
                page.setButton(bt);
                tabsbar_el.Add(bt);
            }
            tabsbar_el.updateList();
        }

        private void onTabChanged(ChangeEvent<string> evt)
        {
            // Debug.Log("changed "+evt.newValue);
            ShowContent(evt.newValue);   
        }

        void ShowContent(string code)
        {
            foreach (var page in content_el.Children())
            {
                page.Show(page.name == code);
            }

            if (panels == null)
                return;

            foreach (var panel in panels)
            {
                panel.isVisible = panel.code == code;    
            }
        }

        public void Enable(string code, bool enable)
        {
            foreach (var panel in panels)
            {
                if (panel.code == code)
                    panel.enabled = enable;
            }
        }

        List<K2Panel> panels;

        public void Init(List<K2Panel> panels)
        {
            BuildButtons();
            this.panels = panels;
            foreach(K2Panel panel in this.panels)
                panel.Init(tabsbar_el, content_el);
        }

        public void Select(string code)
        {
            tabsbar_el.setOpenedPage(code);
            ShowContent(code);         
        }

        public void Update()
        {
            foreach(K2Panel panel in this.panels)
                panel.onUpdateUI();
        }


    }
}