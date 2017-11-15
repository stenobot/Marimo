using UnityEditor;
using UnityEngine;

/// <summary>
/// The TMX Importer window in Unity's menu @ "Window/TMX Importer"
/// Exposes settings to control aspects of the TMX map import process
/// </summary>
public class TmxImporter : EditorWindow
{
    // TextureImporterSettings
    public static int PixelsPerUnit { get; private set; }
    public static SpriteImportMode SpriteMode { get; private set; }
    public static FilterMode FilterMode { get; private set; }
    public static bool AlphaIsTransparency { get; private set; }
    public static TextureImporterAlphaSource AlphaSource { get; private set; }
    public static bool MipMapEnabled { get; private set; }
    public static uint ExtrudeEdges { get; private set; }

    // TextureImporterPlatformSettings
    public static TextureImporterCompression TextureCompression { get; private set; }
    public static TextureSize MaxTextureSize { get; private set; }

    // Enum needed to limit available values in the MaxTextureSize dropdown
    public enum TextureSize
    {
        // Underscores are because we can't start a name with a number in C#
        _32 = 32,
        _64 = 64,
        _128 = 128,
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048,
        _4096 = 4096,
        _8192 = 8192
    }

    private bool m_collapseImportSettings = false;

    private void OnEnable()
    {
        // TextureImporterSettings
        PixelsPerUnit = EditorPrefs.GetInt("PixelsPerUnit");
        SpriteMode = (SpriteImportMode)EditorPrefs.GetInt("SpriteMode");
        FilterMode = (FilterMode)EditorPrefs.GetInt("FilterMode");
        AlphaIsTransparency = EditorPrefs.GetBool("AlphaIsTransparency");
        AlphaSource = (TextureImporterAlphaSource)EditorPrefs.GetInt("AlphaSource");
        MipMapEnabled = EditorPrefs.GetBool("MipMapEnabled");
        ExtrudeEdges = (uint)EditorPrefs.GetInt("ExtrudeEdges");

        // TextureImporterPlatformSettings
        TextureCompression = (TextureImporterCompression)EditorPrefs.GetInt("TextureCompression");
        MaxTextureSize = (TextureSize)EditorPrefs.GetInt("MaxTextureSize");
    }

    private void OnDisable()
    {
        // TextureImporterSettings
        EditorPrefs.SetInt("PixelsPerUnit", PixelsPerUnit);
        EditorPrefs.SetInt("SpriteMode", (int)SpriteMode);
        EditorPrefs.SetInt("FilterMode", (int)FilterMode);
        EditorPrefs.SetBool("AlphaIsTransparency", AlphaIsTransparency);
        EditorPrefs.SetInt("AlphaSource", (int)AlphaSource);
        EditorPrefs.SetBool("MipMapEnabled", MipMapEnabled);
        EditorPrefs.SetInt("ExtrudeEdges", (int)ExtrudeEdges);

        // TextureImporterPlatformSettings
        EditorPrefs.SetInt("TextureCompression", (int)TextureCompression);
        EditorPrefs.SetInt("MaxTextureSize", (int)MaxTextureSize);
    }

    [MenuItem("Window/TMX Importer")]
    private static void OpenWindow()
    {
        TmxImporter window = GetWindow<TmxImporter>();
        window.titleContent = new GUIContent("TMX Importer");
    }

    void OnGUI()
    {
        // Build out the window UI
        GUILayout.Label("Sprite Import Settings", EditorStyles.boldLabel);
        PixelsPerUnit = EditorGUILayout.IntField("Pixels Per Unit", PixelsPerUnit);
        SpriteMode = (SpriteImportMode)EditorGUILayout.EnumPopup("Sprite Mode: ", SpriteMode);
        FilterMode = (FilterMode)EditorGUILayout.EnumPopup("Filter Mode: ", FilterMode);
        AlphaIsTransparency = EditorGUILayout.Toggle("Alpha is Transparency: ", AlphaIsTransparency);
        AlphaSource = (TextureImporterAlphaSource)EditorGUILayout.EnumPopup("Alpha Source: ", AlphaSource);
        MipMapEnabled = EditorGUILayout.Toggle("Generate Mip Maps: ", MipMapEnabled);
        ExtrudeEdges = (uint)EditorGUILayout.IntSlider("Extrude Edges: ", (int)ExtrudeEdges, 0, 32);
        TextureCompression = (TextureImporterCompression)EditorGUILayout.EnumPopup("Texture Compression: ", TextureCompression);
        MaxTextureSize = (TextureSize)EditorGUILayout.EnumPopup("Max Size: ", MaxTextureSize);

        // Save current values
        PlayerPrefs.Save();
    }
}
