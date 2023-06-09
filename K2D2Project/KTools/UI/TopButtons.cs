
using UnityEngine;

namespace KTools.UI;

public class TopButtons
{
    static Rect position = Rect.zero;
    const int space = 25;

    /// <summary>
    /// Must be called before any Button call
    /// </summary>
    /// <param name="widthWindow"></param>
    static public void Init(float widthWindow)
    {
        position = new Rect(widthWindow - 5, 4, 23, 23);
    }

    static public bool Button(string txt)
    {
        position.x -= space;
        return GUI.Button(position, txt, KBaseStyle.small_button);
    }
    static public bool Button(Texture2D icon)
    {
        position.x -= space;
        return GUI.Button(position, icon, KBaseStyle.icon_button);
    }

    static public bool Toggle(bool value, string txt)
    {
        position.x -= space;
        return GUI.Toggle(position, value, txt, KBaseStyle.small_button);
    }

    static public bool Toggle(bool value, Texture2D icon)
    {
        position.x -= space;
        return GUI.Toggle(position, value, icon, KBaseStyle.icon_button);
    }
}
