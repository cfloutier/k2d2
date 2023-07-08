using UnityEngine;
using SpaceWarp.API.Assets;
using K2D2;

namespace KTools.UI;

public class AssetsLoader
{
    // BEPEXVersion
    public static Texture2D loadIcon(string path)
    {
        var imageTexture = AssetManager.GetAsset<Texture2D>($"{K2D2_Plugin.mod_id}/images/{path}.png");

        //   Check if the texture is null
        if (imageTexture == null)
        {
            // Print an error message to the Console
            Debug.LogError("Failed to load image texture from path: " + path);

            // Print the full path of the resource
            Debug.Log("Full resource path: " + Application.dataPath + "/" + path);

            // Print the type of resource that was expected
            Debug.Log("Expected resource type: Texture2D");
        }

        return imageTexture;
    }

    public static Font loadFont(string path)
    {
        var font = AssetManager.GetAsset<Font>($"{K2D2_Plugin.mod_id}/k2d2/KtoolBundle/{path}.ttf");

         //   Check if the font is null
        if (font == null)
        {
            // Print an error message to the Console
            Debug.LogError("Failed to load font from path: " + path);

            // Print the full path of the resource
            Debug.Log("Full resource path: " + Application.dataPath + "/" + path);
        }

        return font;
    }


    public static GUISkin loadSkin(string path)
    {
        var skin = AssetManager.GetAsset<GUISkin>($"{K2D2_Plugin.mod_id}/k2d2/KtoolBundle/{path}.guiskin");

         //   Check if the font is null
        if (skin == null)
        {
            // Print an error message to the Console
            Debug.LogError("Failed to load from path: " + path);

            // Print the full path of the resource
            Debug.Log("Full resource path: " + Application.dataPath + "/" + path);
        }

        return skin;
    }
}
