using UnityEditor;
using UnityEngine;

public class TmxImporter : EditorWindow
{
    public static int PixelsPerUnit { get; set; }

    //bool groupEnabled = false;
    //bool myBool = true;
    //float myFloat = 1;

    private void OnEnable()
    {
        PixelsPerUnit = EditorPrefs.GetInt("PPU");
    }

    private void OnDisable()
    {
        EditorPrefs.SetInt("PPU", PixelsPerUnit);
    }

    [MenuItem("Window/TMX Importer")]
    private static void OpenWindow()
    {
        TmxImporter window = GetWindow<TmxImporter>();
        window.titleContent = new GUIContent("TMX Importer");
    }

    void OnGUI()
    {

        GUILayout.Label("Sprite Import Settings", EditorStyles.boldLabel);
        PixelsPerUnit = EditorGUILayout.IntField("Pixels Per Unit", PixelsPerUnit);
        PlayerPrefs.Save();
    }
}
