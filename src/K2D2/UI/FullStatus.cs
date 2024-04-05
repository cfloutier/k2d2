
using K2UI;
using UnityEngine.UIElements;

namespace K2D2.UI;

public class FullStatus
{
    public FullStatus(VisualElement group)
    {
        status = group.Q<StatusLine>("status_pilot");
        console = group.Q<K2UI.Console>("pilot_console");
        progressBar = group.Q<K2UI.K2ProgressBar>("progress");
        main_group = status.parent;
    }

    VisualElement main_group;
    public K2UI.Console console;
    public K2UI.StatusLine status;
    public K2UI.K2ProgressBar progressBar;  

    public void Reset()
    {
        main_group.Show(true);
        console.Show(false);
        console.text = "??????????????";

        status.Show(false);
        status.text = "";
        
        progressBar.Show(false);          
    }

    public void Console(string txt)
    {
        console.Add(txt);
    }

    public void Warning(string text)
    {
        Status(text, StatusLine.Level.Warning);
    }

     public void Error(string text)
    {
        Status(text, StatusLine.Level.Error);
    }

    public void Status(string text, StatusLine.Level level = StatusLine.Level.Normal)
    {
        status.Set(text, level);
        status.Show(true);
    }

    public void Progress(double ratio, string label = null)
    {
        progressBar.value = (float)(ratio * 100);
        progressBar.Show(true);
        if (!string.IsNullOrEmpty(label))
        {
            progressBar.Label = label;
        }
    }
}

