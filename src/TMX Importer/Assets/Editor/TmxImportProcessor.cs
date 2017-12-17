using Assets.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Linq;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

public class TmxImportProcessor : AssetPostprocessor
{
    // The path to the image import directory in the asset database
    // Must use '/' in path regardless of platform. Root is at the 'Assets' folder. 
    // Example: Assets/Sprites/TmxImporter
    private static string m_imgDir;
    // Filesystem equivalent of m_imgDir to permit moving files around with System.IO
    private static string m_imgFsDir;
    // Holds the paths to the imported tiles assets in the AssetDatabase
    private static Dictionary<int, string> m_tilePaths;


    private enum AssetChangeType
    {
        Import,
        Delete,
        Move
    }

    public TmxImportProcessor()
    {
        // Create a directory where imported images will be stored
        m_imgDir = "Assets/Sprites/TmxImporter";
        m_imgFsDir = string.Format("{0}\\Sprites\\TmxImporter", Application.dataPath.Replace('/', '\\'));
    }

    /// <summary>
    /// Invoked by Unity during post-processing for asset import
    /// </summary>
    /// <param name="importedAssets">The list of imported assets</param>
    /// <param name="deletedAssets">The list of deleted assets</param>
    /// <param name="movedAssets">The list of moved assets</param>
    /// <param name="movedFromAssetPaths">Contains old paths if movedAssets is populated</param>
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        ProcessAssets(importedAssets, AssetChangeType.Import);
        //ProcessAssets(deletedAssets, AssetChangeType.Delete);
        //ProcessAssets(movedAssets, AssetChangeType.Move, movedFromAssetPaths);
    }

    /// <summary>
    /// Invoked during pre-processing when textures (or sprites) are being imported
    /// </summary>
    public void OnPreprocessTexture()
    {
        // Only process assets in the image import directory
        if (!assetPath.Contains(m_imgDir))
            return;

        TextureImporter importer = assetImporter as TextureImporter;
        if (importer != null)
        {
            TextureImporterSettings texSettings = new TextureImporterSettings();
            importer.ReadTextureSettings(texSettings);

            texSettings.spriteMode = EditorPrefs.GetInt("SpriteMode");
            texSettings.filterMode = (FilterMode)EditorPrefs.GetInt("FilterMode");
            texSettings.alphaIsTransparency = EditorPrefs.GetBool("AlphaIsTransparency");
            texSettings.alphaSource = (TextureImporterAlphaSource)EditorPrefs.GetInt("AlphaSource");
            texSettings.mipmapEnabled = EditorPrefs.GetBool("MipMapEnabled");
            texSettings.spriteExtrude = (uint)EditorPrefs.GetInt("ExtrudeEdges");
            texSettings.spritePixelsPerUnit = EditorPrefs.GetInt("PixelsPerUnit");

            TextureImporterPlatformSettings texPlatSettings = new TextureImporterPlatformSettings();
            texPlatSettings.textureCompression = (TextureImporterCompression)EditorPrefs.GetInt("TextureCompression");
            texPlatSettings.maxTextureSize = EditorPrefs.GetInt("MaxTextureSize");

            importer.SetTextureSettings(texSettings);
            importer.SetPlatformTextureSettings(texPlatSettings);
        }
    }

    /// <summary>
    /// Helper method to perform appropriate actions on assets based on file and change type
    /// </summary>
    /// <param name="assets">The array of assets to process</param>
    /// <param name="changeType">The type of change which occurred</param>
    /// <param name="movedFromAssetPaths">If the changeType is <see cref="AssetChangeType.Move"/>, this contains the old paths /></param>
    private static void ProcessAssets(string[] assets, AssetChangeType changeType, string[] movedFromAssetPaths = null)
    {
        string[] tmxFiles = (from x in assets where x.EndsWith(".tmx") select x).ToArray<string>();
        m_tilePaths = new Dictionary<int, string>();

        foreach (string tmxFile in tmxFiles)
        {
            switch (changeType)
            {
                case AssetChangeType.Import:
                    Map map = ImportMap(tmxFile);
                    ImportTiles(map, tmxFile);
                    TmxRenderer.RenderMap(map, m_tilePaths);
                    break;
                    //case AssetChangeType.Delete:
                    //    DeleteMap(asset);
                    //    break;
                    //case AssetChangeType.Move:
                    //    MoveMap(asset, movedFromAssetPaths[assetCount - 1]);
                    //    break;
            }
        }
    }

    private static void ImportTiles(Map map, string asset)
    {
        m_tilePaths = new Dictionary<int, string>();
        // This is used to resolve image paths as they are relative to the map directory, not the project's dir in Environment.CurrentDirectory
        string mapsDir = Path.GetDirectoryName(Path.Combine(Environment.CurrentDirectory, asset));
        long tilesImported = 0;

        AssetDatabase.StartAssetEditing();
        foreach (TileSet tileSet in map.Tilesets)
        {
            TileSet importedTileSet = tileSet;
            foreach (Tile tile in importedTileSet.Tiles)
            {
                string file = Path.GetFullPath(Path.Combine(mapsDir, tile.Image.Source.Replace('/', '\\')));
                if (File.Exists(file))
                {
                    string newFile = Path.Combine(m_imgFsDir, Path.GetFileName(file));
                    if (!Directory.Exists(m_imgFsDir))
                        Directory.CreateDirectory(m_imgFsDir);

                    AssetDatabase.ImportAsset(m_imgDir, ImportAssetOptions.ForceUpdate);
                    if (!file.Contains(m_imgFsDir))
                    {
                        // Copy external file to the project, overwrite if it exists
                        // TODO: Make overwrite optional? It will keep images refreshed but slow down map import.
                        File.Copy(file, newFile, true);
                        // Detemine path in asset database
                        string assetDbPath = newFile.Replace('\\', '/');
                        assetDbPath = assetDbPath.Substring(assetDbPath.IndexOf(m_imgDir));
                        if (!m_tilePaths.ContainsKey(tile.Id + int.Parse(tileSet.FirstGid.ToString())))
                            m_tilePaths.Add(tile.Id + int.Parse(tileSet.FirstGid.ToString()), assetDbPath);
                        AssetDatabase.ImportAsset(assetDbPath, ImportAssetOptions.ForceUpdate);
                        tilesImported++;
                    }
                }
            }
        }
        AssetDatabase.StopAssetEditing();
        Debug.Log("Imported " + tilesImported + " tiles. m_tilePaths contains " + m_tilePaths.Count + " paths");

    }


    /// <summary>
    /// Triggered when a map file moves from one location to another
    /// </summary>
    /// <param name="map">The new map location</param>
    /// <param name="movedFromMap">The old map location</param>
    private static void MoveMap(string map, string movedFromMap)
    {

        //EditorUtility.DisplayDialog("WOOHOO!", "WE DID IT!", "Thanks bro!");
    }

    /// <summary>
    /// Triggered when a map file is deleted
    /// </summary>
    /// <param name="map">The path of the map that was deleted</param>
    private static void DeleteMap(string map)
    {
        //EditorUtility.DisplayDialog("WOOHOO!", "WE DID IT!", "Thanks bro!");
    }

    /// <summary>
    /// Triggered when a map file is reimported
    /// </summary>
    /// <param name="mapFile">The map file which was imported</param>
    private static Map ImportMap(string mapFile)
    {
        Debug.Log("importing map " + mapFile);
        Map map = null;
        FileStream stream = null;
        XmlReader reader = null;

        try
        {
            XmlSerializer mapSerializer = new XmlSerializer(typeof(Map));
            stream = new FileStream(mapFile, FileMode.Open);
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
