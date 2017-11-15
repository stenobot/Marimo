using UnityEditor;
using UnityEngine;

/// <summary>
/// The TMX Importer window in Unity's menu @ "Window/TMX Importer"
/// Exposes settings to control aspects of the TMX map import process
/// </summary>
public class TmxImporter : EditorWindow
{
    // TextureImporterSettings
    private int m_pixelsPerUnit;
    private SpriteImportMode m_spriteMode;
    private FilterMode m_filterMode;
    private bool m_alphaIsTransparency;
    private TextureImporterAlphaSource m_alphaSource;
    private bool m_mipMapEnabled;
    private uint m_extrudeEdges;

    // TextureImporterPlatformSettings
    private TextureImporterCompression m_textureCompression;
    private TextureSize m_maxTextureSize;

    // Enum used to limit available values in the MaxTextureSize dropdown
    private enum TextureSize
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

    /// <summary>
    /// Invoked when the TMX Importer window is opened
    /// </summary>
    private void OnEnable()
    {
        // TextureImporterSettings
        m_pixelsPerUnit = EditorPrefs.GetInt("PixelsPerUnit");
        m_spriteMode = (SpriteImportMode)EditorPrefs.GetInt("SpriteMode");
        m_filterMode = (FilterMode)EditorPrefs.GetInt("FilterMode");
        m_alphaIsTransparency = EditorPrefs.GetBool("AlphaIsTransparency");
        m_alphaSource = (TextureImporterAlphaSource)EditorPrefs.GetInt("AlphaSource");
        m_mipMapEnabled = EditorPrefs.GetBool("MipMapEnabled");
        m_extrudeEdges = (uint)EditorPrefs.GetInt("ExtrudeEdges");

        // TextureImporterPlatformSettings
        m_textureCompression = (TextureImporterCompression)EditorPrefs.GetInt("TextureCompression");
        m_maxTextureSize = (TextureSize)EditorPrefs.GetInt("MaxTextureSize");
    }

    /// <summary>
    /// Invoked when the TMX Importer window is closed 
    /// </summary>
    private void OnDisable()
    {
        // TextureImporterSettings
        EditorPrefs.SetInt("PixelsPerUnit", m_pixelsPerUnit);
        EditorPrefs.SetInt("SpriteMode", (int)m_spriteMode);
        EditorPrefs.SetInt("FilterMode", (int)m_filterMode);
        EditorPrefs.SetBool("AlphaIsTransparency", m_alphaIsTransparency);
        EditorPrefs.SetInt("AlphaSource", (int)m_alphaSource);
        EditorPrefs.SetBool("MipMapEnabled", m_mipMapEnabled);
        EditorPrefs.SetInt("ExtrudeEdges", (int)m_extrudeEdges);

        // TextureImporterPlatformSettings
        EditorPrefs.SetInt("TextureCompression", (int)m_textureCompression);
        EditorPrefs.SetInt("MaxTextureSize", (int)m_maxTextureSize);
    }

    /// <summary>
    /// Sets up the menu entry and the window
    /// </summary>
    [MenuItem("Window/TMX Importer")]
    private static void OpenWindow()
    {
        TmxImporter window = GetWindow<TmxImporter>();
        window.titleContent = new GUIContent("TMX Importer");
    }

    /// <summary>
    /// Draws the UI for the TMX Importer window
    /// </summary>
    void OnGUI()
    {
        GUILayout.Label("Sprite Import Settings", EditorStyles.boldLabel);
        m_pixelsPerUnit = EditorGUILayout.IntField("Pixels Per Unit", m_pixelsPerUnit);
        m_spriteMode = (SpriteImportMode)EditorGUILayout.EnumPopup("Sprite Mode: ", m_spriteMode);
        m_filterMode = (FilterMode)EditorGUILayout.EnumPopup("Filter Mode: ", m_filterMode);
        m_alphaIsTransparency = EditorGUILayout.Toggle("Alpha is Transparency: ", m_alphaIsTransparency);
        m_alphaSource = (TextureImporterAlphaSource)EditorGUILayout.EnumPopup("Alpha Source: ", m_alphaSource);
        m_mipMapEnabled = EditorGUILayout.Toggle("Generate Mip Maps: ", m_mipMapEnabled);
        m_extrudeEdges = (uint)EditorGUILayout.IntSlider("Extrude Edges: ", (int)m_extrudeEdges, 0, 32);
        m_textureCompression = (TextureImporterCompression)EditorGUILayout.EnumPopup("Texture Compression: ", m_textureCompression);
        m_maxTextureSize = (TextureSize)EditorGUILayout.EnumPopup("Max Size: ", m_maxTextureSize);

        // Save current values
        PlayerPrefs.Save();
    }
}
