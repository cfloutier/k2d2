
using SpaceWarp.API.UI;
using UnityEngine;

namespace KTools.UI;

public class KBaseStyle
{
    public static bool Init()
    {
        return BuildStyles(Skins.ConsoleSkin);
    }

    public static GUISkin skin;
    private static bool guiLoaded = false;

    public static bool BuildStyles(GUISkin baseSkin)
    {
        if (guiLoaded)
            return true;

        skin = CopySkin(baseSkin);

        LoadFonts();
        LoadIcons();


        BuildLabels();
        BuildFrames();

        BuildSliders();
        BuildButtons();
        BuildTabs();
        BuildFoldout();
        BuildToggle();
        BuildProgressBar();

        BuildHeading();

        guiLoaded = true;
        return true;
    }

    public static GUIStyle error, warning, label, mid_text, console_text, phase_ok, phase_warning, phase_error, value_field;

    public static GUIStyle icons_label, title, slider_text;
    public static Font arial_font, arial_bold_font, caravan_font;


    static void LoadFonts()
    {
        arial_font = AssetsLoader.loadFont("ARIAL");
        arial_bold_font = AssetsLoader.loadFont("ARIALBD");
        caravan_font = AssetsLoader.loadFont("Caravan");

        skin.font = arial_font;
    }

    static void BuildLabels()
    {
        icons_label = new GUIStyle(GUI.skin.GetStyle("Label"));
        icons_label.border = new RectOffset(0, 0, 0, 0);
        icons_label.padding = new RectOffset(0, 0, 0, 0);
        icons_label.margin = new RectOffset(0, 0, 0, 0);
        icons_label.overflow = new RectOffset(0, 0, 0, 0);

        error = new GUIStyle(GUI.skin.GetStyle("Label"));
        warning = new GUIStyle(GUI.skin.GetStyle("Label"));
        error.normal.textColor = Color.red;
        warning.normal.textColor = Color.yellow;
        //labelColor = GUI.skin.GetStyle("Label").normal.textColor;

        phase_ok = new GUIStyle(GUI.skin.GetStyle("Label"));
        phase_ok.normal.textColor = ColorTools.parseColor("#00BC16");
        // phase_ok.fontSize = 20;

        phase_warning = new GUIStyle(GUI.skin.GetStyle("Label"));
        phase_warning.normal.textColor = ColorTools.parseColor("#BC9200");
        // phase_warning.fontSize = 20;

        phase_error = new GUIStyle(GUI.skin.GetStyle("Label"));
        phase_error.normal.textColor = ColorTools.parseColor("#B30F0F");
        // phase_error.fontSize = 20;

        console_text = new GUIStyle(GUI.skin.GetStyle("Label"));
        console_text.normal.textColor = ColorTools.parseColor("#B6B8FA");
        // console_text.fontSize = 15;
        console_text.padding = new RectOffset(0, 0, 0, 0);
        console_text.margin = new RectOffset(0, 0, 0, 0);

        slider_text = new GUIStyle(console_text);
        slider_text.normal.textColor = ColorTools.parseColor("#C0C1E2");

        mid_text = new GUIStyle(slider_text);

        slider_text.margin = new RectOffset(5, 0, 0, 0);
        slider_text.contentOffset = new Vector2(8, 5);

        label = new GUIStyle(GUI.skin.GetStyle("Label"));
        // label.fontSize = 17;
        label.margin = new RectOffset(0, 0, 0, 0);
        label.padding = new RectOffset(0, 0, 0, 0);

        title = new GUIStyle();
        title.normal.textColor = ColorTools.parseColor("#C0C1E2");
        title.font = arial_bold_font;
        title.fontSize = 20;
        title.margin = new RectOffset(5, 0, 2, 10);

        var textField = skin.textField;
        textField.padding = new RectOffset(5, 5, 0, 0);
        //textField.margin = new RectOffset(0, 0, 0, 0);
        textField.fixedHeight = 22;
        //textField.fontSize = 5;

    }

    public static GUIStyle separator, window, box, field;
    static void BuildFrames()
    {
        // Define the GUIStyle for the window
        window = new GUIStyle(skin.window);

        window.border = new RectOffset(25, 25, 35, 25);
        window.margin = new RectOffset(0, 0, 0, 0);
        window.padding = new RectOffset(10, 10, 44, 10);
        window.overflow = new RectOffset(0, 0, 0, 0);

        window.font = caravan_font;
        window.fontSize = 25;

        window.contentOffset = new Vector2(31, -40);

        // Set the background color of the window
        window.normal.background = AssetsLoader.loadIcon("window");
        window.normal.textColor = Color.white;
        setAllFromNormal(window);
        window.alignment = TextAnchor.UpperLeft;
        window.stretchWidth = true;
        window.contentOffset = new Vector2(31, -40);

        skin.window = window;

        // Define the GUIStyle for the box
        box = new GUIStyle(window);
        box.normal.background = AssetsLoader.loadIcon("Box");
        setAllFromNormal(box);
        box.border = new RectOffset(10, 10, 10, 10);
        box.margin = new RectOffset(0, 0, 0, 0);
        box.padding = new RectOffset(10, 10, 10, 10);
        box.overflow = new RectOffset(0, 0, 0, 0);
        skin.box = box;
        skin.scrollView = box;
        // Define the GUIStyle for the editor field

        field = new GUIStyle(GUI.skin.textField);
        field.normal.background = box.normal.background;
        setAllFromNormal(field);
        field.border = new RectOffset(10, 10, 10, 10);
        field.fontSize = 14;
        field.fixedHeight = 25;
        field.contentOffset = new Vector2(2,1);
        GUI.skin.textField = field;

        // define the V scrollbar
        GUIStyle verticalScrollbar = new GUIStyle(GUI.skin.verticalScrollbar);

        verticalScrollbar.normal.background = AssetsLoader.loadIcon("VerticalScroll");
        setAllFromNormal(verticalScrollbar);
        verticalScrollbar.border = new RectOffset(5, 5, 5, 5);
        verticalScrollbar.fixedWidth = 10;

        skin.verticalScrollbar = verticalScrollbar;

        GUIStyle verticalScrollbarThumb = new GUIStyle(GUI.skin.verticalScrollbarThumb);

        verticalScrollbarThumb.normal.background = AssetsLoader.loadIcon("VerticalScroll_thumb");
        setAllFromNormal(verticalScrollbarThumb);
        verticalScrollbarThumb.border = new RectOffset(5, 5, 5, 5);
        verticalScrollbarThumb.fixedWidth = 10;

        skin.verticalScrollbarThumb = verticalScrollbarThumb;

        // separator
        separator = new GUIStyle(GUI.skin.box);
        separator.normal.background = AssetsLoader.loadIcon("line");
        separator.border = new RectOffset(2, 2, 0, 0);
        separator.margin = new RectOffset(10, 10, 5, 5);
        separator.fixedHeight = 3;
        setAllFromNormal(separator);
    }

    public static GUIStyle slider_line, slider_node, v_line;

    static void BuildSliders()
    {
        slider_line = new GUIStyle(GUI.skin.horizontalSlider);
        slider_line.normal.background = AssetsLoader.loadIcon("Slider");
        setAllFromNormal(slider_line);
        slider_line.border = new RectOffset(5, 5, 0, 0);

        slider_line.border = new RectOffset(12, 14, 0, 0);
        slider_line.fixedWidth = 0;
        slider_line.fixedHeight = 21;
        slider_line.margin = new RectOffset(0, 0, 2, 5);

        slider_node = new GUIStyle(GUI.skin.horizontalSliderThumb);
        slider_node.normal.background = AssetsLoader.loadIcon("SliderNode");
        setAllFromNormal(slider_node);
        slider_node.border = new RectOffset(0, 0, 0, 0);
        slider_node.fixedWidth = 21;
        slider_node.fixedHeight = 21;

        v_line = new GUIStyle(GUI.skin.box);
        v_line.normal.background = AssetsLoader.loadIcon("V_Line");
        setAllFromNormal(v_line);
        v_line.border = new RectOffset(0, 0, 0, 0);

    }

    public static GUIStyle heading, text_heading_mini, text_heading_big;

    static void BuildHeading()
    {
        heading = new GUIStyle(box);
        heading.normal.background = AssetsLoader.loadIcon("HeadingFrame");
        setAllFromNormal(heading);
        heading.margin = new RectOffset(0, 0, 15, 20);

        text_heading_mini = new GUIStyle(console_text);
        text_heading_mini.alignment = TextAnchor.UpperCenter;
        text_heading_mini.normal.textColor = Color.white;
        text_heading_mini.fontSize = 10;
        setAllFromNormal(text_heading_mini);

        text_heading_big = new GUIStyle(small_button);
        text_heading_big.padding = new RectOffset();
        text_heading_big.normal.background = AssetsLoader.loadIcon("Empty");
        text_heading_big.fontSize = 16;
        text_heading_big.font = arial_bold_font;
    }


    // icons
    public static Texture2D gear, icon, mnc_icon, cross;

    static void LoadIcons()
    {
        // icons
        gear = AssetsLoader.loadIcon("gear");
        icon = AssetsLoader.loadIcon("icon");
        // mnc_icon = AssetsLoader.loadIcon("mnc_new_icon_50");
        cross = AssetsLoader.loadIcon("Cross");
    }

    public static GUIStyle progress_bar_empty, progress_bar_full;

    static void BuildProgressBar()
    {
        // progress bar
        progress_bar_empty = new GUIStyle(GUI.skin.box);
        progress_bar_empty.normal.background = AssetsLoader.loadIcon("progress_empty");
        progress_bar_empty.border = new RectOffset(2, 2, 2, 2);
        progress_bar_empty.margin = new RectOffset(5, 5, 5, 5);
        progress_bar_empty.fixedHeight = 20;
        setAllFromNormal(progress_bar_empty);

        progress_bar_full = new GUIStyle(progress_bar_empty);
        progress_bar_full.normal.background = AssetsLoader.loadIcon("progress_full");
        setAllFromNormal(progress_bar_empty);
    }


    public static GUIStyle bigicon_button, icon_button, small_button, big_button, button;

    static void BuildButtons()
    {
        // button std
        button = new GUIStyle(GUI.skin.GetStyle("Button"));
        button.normal.background = AssetsLoader.loadIcon("BigButton_Normal");
        button.normal.textColor = ColorTools.parseColor("#FFFFFF");
        setAllFromNormal(button);

        button.hover.background = AssetsLoader.loadIcon("BigButton_hover");
        button.active.background = AssetsLoader.loadIcon("BigButton_hover");
        // button.active.background = AssetsLoader.loadIcon("BigButton_on");
        // button.onNormal = button.active;
        // setFromOn(button);

        button.border = new RectOffset(5, 5, 5, 5);
        button.padding = new RectOffset(4, 4, 4, 4);
        button.overflow = new RectOffset(0, 0, 0, 0);
        // button.fontSize = 20;
        button.alignment = TextAnchor.MiddleCenter;
        skin.button = button;

        // Small Button
        small_button = new GUIStyle(GUI.skin.GetStyle("Button"));
        small_button.normal.background = AssetsLoader.loadIcon("Small_Button");
        setAllFromNormal(small_button);
        small_button.hover.background = AssetsLoader.loadIcon("Small_Button_hover");
        small_button.active.background = AssetsLoader.loadIcon("Small_Button_active");
        small_button.onNormal = small_button.active;
        setFromOn(small_button);

        small_button.border = new RectOffset(5, 5, 5, 5);
        small_button.padding = new RectOffset(6, 6, 2, 2);
        small_button.overflow = new RectOffset(0, 0, 0, 0);
        small_button.alignment = TextAnchor.MiddleCenter;

        big_button = new GUIStyle(GUI.skin.GetStyle("Button"));
        big_button.normal.background = AssetsLoader.loadIcon("BigButton_Normal");
        big_button.normal.textColor = ColorTools.parseColor("#FFFFFF");
        setAllFromNormal(big_button);

        big_button.hover.background = AssetsLoader.loadIcon("BigButton_Hover");
        big_button.active.background = AssetsLoader.loadIcon("BigButton_Active");
        big_button.onNormal = big_button.active;
        setFromOn(big_button);

        big_button.border = new RectOffset(5, 5, 5, 5);
        big_button.padding = new RectOffset(8, 8, 10, 10);
        big_button.overflow = new RectOffset(0, 0, 0, 0);
        // big_button.fontSize = 20;
        big_button.alignment = TextAnchor.MiddleCenter;

        // Small Button
        icon_button = new GUIStyle(small_button);
        icon_button.padding = new RectOffset(4, 4, 4, 4);

        bigicon_button = new GUIStyle(icon_button);
        bigicon_button.fixedWidth = 50;
        bigicon_button.fixedHeight = 50;
        bigicon_button.fontStyle = FontStyle.Bold;


        value_field = new GUIStyle(label);
        value_field.alignment = TextAnchor.MiddleCenter;
        value_field.normal = button.normal;

    }

    public static GUIStyle tab_normal, tab_active;
    static void BuildTabs()
    {
        tab_normal = new GUIStyle(button);
        
        tab_normal.border = new RectOffset(5, 5, 5, 5);
        tab_normal.padding = new RectOffset(10, 10, 5, 5);
        tab_normal.overflow = new RectOffset(0, 0, 0, 0);
        // big_button.fontSize = 20;
        tab_normal.alignment = TextAnchor.MiddleCenter;
        tab_normal.stretchWidth = true;

        tab_normal.normal.background = AssetsLoader.loadIcon("Tab_Normal");
        setAllFromNormal(tab_normal);

        tab_normal.hover.background = AssetsLoader.loadIcon("Tab_Hover");
        tab_normal.active.background = AssetsLoader.loadIcon("Tab_Active");
        tab_normal.onNormal = tab_normal.active;
        setFromOn(tab_normal);

        tab_active = new GUIStyle(tab_normal);
        tab_active.normal.background = AssetsLoader.loadIcon("Tab_On_normal");
        tab_active.normal.textColor = Color.black;
        setAllFromNormal(tab_active);

        tab_active.hover.background = AssetsLoader.loadIcon("Tab_On_hover");
        tab_active.active.background = AssetsLoader.loadIcon("Tab_On_Active");
        tab_active.onNormal = tab_active.active;
        setFromOn(tab_active);
    }


    public static GUIStyle foldout_close, foldout_open;

    static void BuildFoldout()
    {

        foldout_close = new GUIStyle(small_button);
        foldout_close.fixedHeight = 30;
        foldout_close.padding = new RectOffset(23, 2, 2, 2);
        foldout_close.border = new RectOffset(23, 7, 27, 3);

        foldout_close.normal.background = AssetsLoader.loadIcon("Chapter_Off_Normal");
        foldout_close.normal.textColor = ColorTools.parseColor("#D4D4D4");
        foldout_close.alignment = TextAnchor.MiddleLeft;
        setAllFromNormal(foldout_close);
        foldout_close.hover.background = AssetsLoader.loadIcon("Chapter_Off_Hover");
        foldout_close.active.background = AssetsLoader.loadIcon("Chapter_Off_Active");

        foldout_open = new GUIStyle(foldout_close);
        foldout_open.normal.background = AssetsLoader.loadIcon("Chapter_On_Normal");
        foldout_open.normal.textColor = ColorTools.parseColor("#8BFF95");
        setAllFromNormal(foldout_open);

        foldout_open.hover.background = AssetsLoader.loadIcon("Chapter_On_Hover");
        foldout_open.active.background = AssetsLoader.loadIcon("Chapter_On_Active");
    }

    public static GUIStyle toggle;
    static void BuildToggle()
    {
        // Toggle Button
        toggle = new GUIStyle(GUI.skin.GetStyle("Button"));
        toggle.normal.background = AssetsLoader.loadIcon("Toggle_Off");
        toggle.normal.textColor = ColorTools.parseColor("#C0C1E2");


        setAllFromNormal(toggle);
        toggle.onNormal.background = AssetsLoader.loadIcon("Toggle_On");
        toggle.onNormal.textColor = ColorTools.parseColor("#C0E2DC");
        setFromOn(toggle);
        toggle.fixedHeight = 32;
        toggle.stretchWidth = false;

        toggle.border = new RectOffset(45, 5, 5, 5);
        toggle.padding = new RectOffset(34, 16, 0, 0);
        toggle.overflow = new RectOffset(0, 0, 0, 2);
    }


    /// <summary>
    /// copy all styles from normal state to others
    /// </summary>
    /// <param name="style"></param>
    private static void setAllFromNormal(GUIStyle style)
    {
        style.hover = style.normal;
        style.active = style.normal;
        style.focused = style.normal;
        style.onNormal = style.normal;
        style.onHover = style.normal;
        style.onActive = style.normal;
        style.onFocused = style.normal;
    }

    /// <summary>
    /// copy all styles from onNormal state to on others
    /// </summary>
    /// <param name="style"></param>
    private static void setFromOn(GUIStyle style)
    {
        style.onHover = style.onNormal;
        style.onActive = style.onNormal;
        style.onFocused = style.onNormal;
    }

    /// <summary>
    /// do a full copy of a skin
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    private static GUISkin CopySkin(GUISkin source)
    {
        var copy = new GUISkin();
        copy.font = source.font;
        copy.box = new GUIStyle(source.box);
        copy.label = new GUIStyle(source.label);
        copy.textField = new GUIStyle(source.textField);
        copy.textArea = new GUIStyle(source.textArea);
        copy.button = new GUIStyle(source.button);
        copy.toggle = new GUIStyle(source.toggle);
        copy.window = new GUIStyle(source.window);

        copy.horizontalSlider = new GUIStyle(source.horizontalSlider);
        copy.horizontalSliderThumb = new GUIStyle(source.horizontalSliderThumb);
        copy.verticalSlider = new GUIStyle(source.verticalSlider);
        copy.verticalSliderThumb = new GUIStyle(source.verticalSliderThumb);

        copy.horizontalScrollbar = new GUIStyle(source.horizontalScrollbar);
        copy.horizontalScrollbarThumb = new GUIStyle(source.horizontalScrollbarThumb);
        copy.horizontalScrollbarLeftButton = new GUIStyle(source.horizontalScrollbarLeftButton);
        copy.horizontalScrollbarRightButton = new GUIStyle(source.horizontalScrollbarRightButton);

        copy.verticalScrollbar = new GUIStyle(source.verticalScrollbar);
        copy.verticalScrollbarThumb = new GUIStyle(source.verticalScrollbarThumb);
        copy.verticalScrollbarUpButton = new GUIStyle(source.verticalScrollbarUpButton);
        copy.verticalScrollbarDownButton = new GUIStyle(source.verticalScrollbarDownButton);

        copy.scrollView = new GUIStyle(source.scrollView);

        return copy;

    }

}