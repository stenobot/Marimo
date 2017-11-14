using Assets.Schema;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

public class TmxImporter : EditorWindow
{
    public static int PixelsPerUnit { get; set; }
    public static Dictionary<int, string> AssetDbTilePaths;

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

        GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        PixelsPerUnit = EditorGUILayout.IntField("Pixels Per Unit", PixelsPerUnit);
        PlayerPrefs.Save();
        //groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
        //myBool = EditorGUILayout.Toggle("Toggle", myBool);
        //myFloat = EditorGUILayout.Slider("Slider", myFloat, -3, 3);
        //EditorGUILayout.EndToggleGroup();
    }

    /// <summary>
    /// Imports a Tiled map in TMX format
    /// </summary>
    /// <param name="tmxFile">The path to the TMX file</param>
    /// <returns>A boolean indicating success</returns>
    public static Map ImportTmxFile(string tmxFile)
    {
        Map map = null;
        FileStream stream = null;
        XmlReader reader = null;
        AssetDbTilePaths = new Dictionary<int, string>();

        try
        {
            XmlSerializer mapSerializer = new XmlSerializer(typeof(Map));
            stream = new FileStream(tmxFile, FileMode.Open);
            reader = XmlReader.Create(stream);
            map = (Map)mapSerializer.Deserialize(reader);
        }
        catch (UnityException ex)
        {
            Debug.LogException(ex);
        }
        finally
        {
            if (reader != null)
                reader.Close();
            if (stream != null)
                stream.Close();
        }

        return map;
    }
}
