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
    private static string m_imgDir;
    private static string m_imgFsDir;
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

    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        ProcessAssets(importedAssets, AssetChangeType.Import);
        ProcessAssets(deletedAssets, AssetChangeType.Delete);
        ProcessAssets(movedAssets, AssetChangeType.Move, movedFromAssetPaths);
    }

    // texture asset preprocessor
    public void OnPreprocessTexture()
    {
        // Only process assets in the import directory
        if (!assetPath.Contains(m_imgDir))
            return;

        TextureImporter importer = assetImporter as TextureImporter;
        if (importer != null)
        {
            TextureImporterSettings texSettings = new TextureImporterSettings();
            importer.ReadTextureSettings(texSettings);
            texSettings.spriteMode = (int)TmxImporter.SpriteMode;
            texSettings.filterMode = TmxImporter.FilterMode;
            texSettings.alphaIsTransparency = TmxImporter.AlphaIsTransparency;
            texSettings.alphaSource = TmxImporter.AlphaSource;
            texSettings.mipmapEnabled = TmxImporter.MipMapEnabled;
            texSettings.spriteExtrude = TmxImporter.ExtrudeEdges;
            texSettings.spritePixelsPerUnit = TmxImporter.PixelsPerUnit;
            
            TextureImporterPlatformSettings texPlatSettings = new TextureImporterPlatformSettings();
            texPlatSettings.textureCompression = TmxImporter.TextureCompression;
            texPlatSettings.maxTextureSize = (int)TmxImporter.MaxTextureSize;

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
        int assetCount = 0;

        string[] tmxFiles = (from x in assets where x.EndsWith(".tmx") select x).ToArray<string>();

        foreach (string tmxFile in tmxFiles)
        {
            m_tilePaths = new Dictionary<int, string>();

            assetCount++;

            switch (changeType)
            {
                case AssetChangeType.Import:
                    Map map = ImportMap(tmxFile);
                    ImportTiles(map, tmxFile);
                    RenderMap(map);
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

    private static void RenderMap(Map map)
    {
        if (map == null)
            return;

        GameObject mapParent = new GameObject("Map");
        for (int layerIndex = 0; layerIndex < map.Layers.Count; layerIndex++)
        {
            Layer layer = map.Layers[layerIndex];
            GameObject layerParent = new GameObject("Layer_" + layerIndex);
            layerParent.transform.parent = mapParent.transform;

            for (int col = 0; col < layer.Width; col++)
            {
                for (int row = 0; row < layer.Height; row++)
                {
                    int tileId = layer.Data.MapData[col, row];
                    if (tileId > 0 && m_tilePaths.ContainsKey(tileId))
                    {
                        GameObject g = new GameObject(tileId.ToString());
                        g.transform.parent = layerParent.transform;
                        g.transform.position = new Vector2(col, map.Height-row);
                        SpriteRenderer r = g.AddComponent<SpriteRenderer>();
                        r.sortingOrder = layerIndex;
                        Sprite sprite = null;
                        int retries = 0;
                        while (sprite == null && retries < 100)
                        {
                            sprite = AssetDatabase.LoadAssetAtPath<Sprite>(m_tilePaths[tileId]);
                            retries++;
                        }

                        if (sprite != null)
                            r.sprite = sprite;
                        else
                            Debug.Log(string.Format("Couldn't load sprite from AssetDatabase at path {0}", m_tilePaths[tileId]));
                    }
                }
            }
        }
    }

    private static void ImportTiles(Map map, string asset)
    {
        m_tilePaths = new Dictionary<int, string>();
        // This is used to resolve image paths as they are relative to the map directory, not the project's dir in Environment.CurrentDirectory
        string mapsDir = Path.GetDirectoryName(Path.Combine(Environment.CurrentDirectory, asset));

        long tilesImported = 0;

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
        Debug.Log("Imported " + tilesImported + " tiles. m_tilePaths contains " + m_tilePaths.Count + " paths");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
    }

    //private static Tile GetTileFromIndex(int index)
    //{
    //    Tile tile = null;
    //    foreach (TileSet tileSet in m_tileSets)
    //    {
    //        tile = (from x in tileSet.Tiles where x.Id == index select x).Single<Tile>();
    //        if (tile != null)
    //            break;
    //    }
    //    return tile;
    //}

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
