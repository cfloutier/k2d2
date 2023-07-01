using KTools;
using UnityEngine;
namespace KTools.UI;

public interface PageContent
{
    // Name drawn in the Tab button 
    public string Name
    {
        get;
    }

    // if is isRunning, UI is drawn lighted
    public bool isRunning
    {
        get;
    }

    // if isActive Tab is visible
    public bool isActive
    {
        get;
    }

    // usefull to knows is current page is visible (you can switch off not needed updates if not set)
    public bool UIVisible
    {
        get;
        set;
    }

    // Main Page UI called Here
    public void onGUI();
}

public class TabsUI
{
    public List<PageContent> pages = new List<PageContent>();

    private List<PageContent> filtered_pages = new List<PageContent>();

    PageContent current_page = null;

    // must be called after adding pages
    private bool TabButton(bool is_current, bool isActive, string txt)
    {
        GUIStyle style = isActive ? KBaseStyle.tab_active : KBaseStyle.tab_normal;
        return GUILayout.Toggle(is_current, txt, style, GUILayout.ExpandWidth(true));
    }

    List<float> tabs_Width = new List<float>();

    public int DrawTabs(int current, float max_width = 300)
    {
        current = GeneralTools.ClampInt(current, 0, filtered_pages.Count - 1);
        GUILayout.BeginHorizontal();

        int result = current;

        // compute sizes
        if (tabs_Width.Count != filtered_pages.Count)
        {
            tabs_Width.Clear();
            for (int index = 0; index < filtered_pages.Count; index++)
            {
                var page = filtered_pages[index];
                float minWidth, maxWidth;
                KBaseStyle.tab_normal.CalcMinMaxWidth(new GUIContent(page.Name, ""), out minWidth, out maxWidth);
                tabs_Width.Add(minWidth);
            }
        }
        float xPos = 0;

        for (int index = 0; index < filtered_pages.Count; index++)
        {
            var page = filtered_pages[index];

            float width = tabs_Width[index];

            if (xPos > max_width)
            {
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                xPos = 0;
            }
            xPos += width;

            bool is_current = current == index;
            if (TabButton(is_current, page.isRunning, page.Name))
            {
                if (!is_current)

                    result = index;
            }
        }

        if (xPos < max_width * 0.9f)
        {
            GUILayout.FlexibleSpace();
        }
        GUILayout.EndHorizontal();

        UI_Tools.Separator();
        return result;
    }


    public void Init()
    {
        current_page = pages[KBaseSettings.main_tab_index];
        current_page.UIVisible = true;
    }

    // must be called to rebuild the filtered_pages list 
    public void Update()
    {
        filtered_pages = new List<PageContent>();
        for (int index = 0; index < pages.Count; index++)
        {
            if (pages[index].isActive)
                filtered_pages.Add(pages[index]);
        }
    }

    public void onGUI()
    {
        int current_index = KBaseSettings.main_tab_index;

        if (filtered_pages.Count == 0)
        {
            UI_Tools.Error("NO active Tab tage !!!");
            return;
        }
        int result = current_index;
        if (filtered_pages.Count == 1)
        {
            result = 0;
        }
        else
        {
            result = DrawTabs(current_index, 300);
        }

        result = GeneralTools.ClampInt(result, 0, filtered_pages.Count - 1);
        var page = filtered_pages[result];

        if (page != current_page)
        {
            current_page.UIVisible = false;
            current_page = page;
            current_page.UIVisible = true;
        }

        KBaseSettings.main_tab_index = result;

        current_page.onGUI();
    }
}
