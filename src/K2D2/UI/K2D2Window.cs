using K2D2.Controller;
using K2UI;
using K2UI.Tabs;
using KSP.UI.Binding;

using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;

namespace K2D2.UI;

/// <summary>
/// Controller for the MyFirstWindow UI.
/// </summary>
public class K2D2Window : MonoBehaviour
{
    // The UIDocument component of the window game object
    private UIDocument _window;

    // The elements of the window that we need to access
    private VisualElement _rootElement;

    // The backing field for the IsWindowOpen property
    private bool _isWindowOpen;

    /// <summary>
    /// The state of the window. Setting this value will open or close the window.
    /// </summary>
    public bool IsWindowOpen
    {
        get => _isWindowOpen;
        set
        {
            _isWindowOpen = value;

            // Set the display style of the root element to show or hide the window
            _rootElement.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
            // Alternatively, you can deactivate the window game object to close the window and stop it from updating,
            // which is useful if you perform expensive operations in the window update loop. However, this will also
            // mean you will have to re-register any event handlers on the window elements when re-enabled in OnEnable.
            // gameObject.SetActive(value);

            // Update the Flight AppBar button state
            GameObject.Find(K2D2Plugin.ToolbarFlightButtonID)
                ?.GetComponent<UIValue_WriteBool_Toggle>()
                ?.SetValue(value);

            // Update the OAB AppBar button state
            // GameObject.Find(K2D2Plugin.ToolbarOabButtonID)
            //     ?.GetComponent<UIValue_WriteBool_Toggle>()
            //     ?.SetValue(value);
        }
    }

    TabbedPage tab_page;

    List<K2Panel> pilots_panels = new();

    /// <summary>
    /// Runs when the window is first created, and every time the window is re-enabled.
    /// </summary>
    private void OnEnable()
    {
        // Get the UIDocument component from the game object
        _window = GetComponent<UIDocument>();

        // Get the root element of the window.
        // Since we're cloning the UXML tree from a VisualTreeAsset, the actual root element is a TemplateContainer,
        // so we need to get the first child of the TemplateContainer to get our actual root VisualElement.
        _rootElement = _window.rootVisualElement[0];

        // Center the window by default
        _rootElement.CenterByDefault();

        IsWindowOpen = false;

        // Get the close button from the window
        var closeButton = _rootElement.Q<Button>("close-button");
        // Add a click event handler to the close button
        closeButton.clicked += () => IsWindowOpen = false; 

        // list all pilot panel
        pilots_panels.Clear();
        foreach(var pilot in K2D2Plugin.Instance.pilots_manager.pilots)
        {
            var panel = pilot.panel;
            if (panel != null)
                pilots_panels.Add(panel);
        }

        tab_page = _rootElement.Q<TabbedPage>();
        tab_page.Init(pilots_panels);
        // inti current tab from settings
        tab_page.Select(K2D2Settings.current_tab.V);
        // save the current_tab to settings
        tab_page.RegisterCallback<ChangeEvent<string>>(evt => K2D2Settings.current_tab.V = evt.newValue);


        var title_bar = _rootElement.Q("title-bar");

        var settings_button = title_bar.Q<ToggleButton>("settings-toggle");
        var staging_toggle = title_bar.Q<ToggleButton>("staging-toggle");

        GlobalSetting.settings_visible.Bind(settings_button);

        settings_button.RegisterCallback<ChangeEvent<bool>>( evt => StagingPilot.Instance.Enabled = evt.newValue );
    }

}
