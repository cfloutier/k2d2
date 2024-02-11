

using JetBrains.Annotations;
using UnityEngine.UIElements;


// a tab button with a running state (light on/off)
public class RunTab
{
    public VisualElement root_tab;
    Label the_label;
    VisualElement running_icon;

    string running_state_class = "running_icon";

    string label
    {
        set{
            the_label.text = value;
        }
    }

    //This function retrieves a reference to the 
    //character name label inside the UI element.

    public void SetVisualElement(VisualElement root_tab, Panel panel)
    {
        this.root_tab = root_tab;
        the_label = root_tab.Q<Label>("button_label");
        running_icon = root_tab.Q<VisualElement>("is_running_light");

        Running = false;
        label = panel.button_label;
        root_tab.name = panel.code;
    }

    bool _running = false;
    public bool Running
    {
        get {return _running;}
        set 
        { 
            _running = value;

            if (_running)
            {
                running_icon.AddToClassList(running_state_class);
            }     
            else
            {
                running_icon.RemoveFromClassList(running_state_class);
            }
        }
    }
}