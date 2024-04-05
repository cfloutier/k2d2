using K2D2.Controller;
using K2UI;
using K2UI.Tabs;
using KSP.UI.Binding;
using KTools;
using RTG;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;

namespace K2D2.UI;

/// <summary>
/// Controller for the K2D2Window UI.
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
            _rootElement.Show(value);
            // Alternatively, you can deactivate the window game object to close the window and stop it from updating,
            // which is useful if you perform expensive operations in the window update loop. However, this will also
            // mean you will have to re-register any event handlers on the window elements when re-enabled in OnEnable.
            // gameObject.SetActive(value);

            // Update the Flight AppBar button state
            GameObject.Find(K2D2_Plugin.ToolbarFlightButtonID)
                ?.GetComponent<UIValue_WriteBool_Toggle>()
                ?.SetValue(value);

            // Update the OAB AppBar button state
            // GameObject.Find(K2D2Plugin.ToolbarOabButtonID)
            //     ?.GetComponent<UIValue_WriteBool_Toggle>()
            //     ?.SetValue(value);
        }
    }

    TabbedPage tab_page;

    List<K2Page> all_panels = new();

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

        IsWindowOpen = false;

        // Get the close button from the window
        var closeButton = _rootElement.Q<Button>("close-button");
        // Add a click event handler to the close button
        closeButton.clicked += () => IsWindowOpen = false; 

        // list all pilot panel
        all_panels.Clear();
        foreach(var pilot in K2D2_Plugin.Instance.pilots_manager.pilots)
        {
            var panel = pilot.page;
            if (panel != null)
                all_panels.Add(panel);
        }

        all_panels.Add(new AboutUI());

        tab_page = _rootElement.Q<TabbedPage>();
        tab_page.Init(all_panels);
        // save the current_tab to settings
        tab_page.Bind("main_page", "node");
        
        var title_bar = _rootElement.Q("title-bar");

        var settings_button = title_bar.Q<ToggleButton>("settings-toggle");
        var staging_toggle = title_bar.Q<ToggleButton>("staging-toggle");

        settings_button.Bind(GlobalSetting.settings_visible);
        staging_toggle.RegisterCallback<ChangeEvent<bool>>( evt => StagingPilot.Instance.Enabled = evt.newValue );

        _rootElement.Query<IntegerField>().ForEach(field => field.DisableGameInputOnFocus());
        _rootElement.Query<FloatField>().ForEach(field => field.DisableGameInputOnFocus());
        _rootElement.Query<RepeatButton>().ForEach(field => field.DisableGameInputOnFocus());

        _rootElement.AddManipulator(new DragManipulator(false, "main_window_pos"));
    }

    void Update()
    {
        tab_page.Update();
    }
}
