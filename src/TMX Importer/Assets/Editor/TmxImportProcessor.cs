using Assets.Schema;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;

public class TmxImportProcessor : AssetPostprocessor
{
    private static Dictionary<int, string> m_assetDbTilePaths;
    private static string m_imgDir;
    private static string m_imgFsDir;
    private static int m_pixelsPerUnit;
    //private static HashSet<TileSet> m_tileSets;

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
        m_imgFsDir = (Application.dataPath + "/Sprites/TmxImporter").Replace('/', '\\');
        if (!Directory.Exists(m_imgDir))
            Directory.CreateDirectory(m_imgDir);
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
        if (!assetPath.Contains(m_imgDir))
        {
            Debug.Log(assetPath + " didn't make the cut");
            return;
        }

        m_pixelsPerUnit = EditorPrefs.GetInt("PPU");

        TextureImporter importer = assetImporter as TextureImporter;
        if (importer != null)
        {
            TextureImporterSettings texSettings = new TextureImporterSettings();
            importer.ReadTextureSettings(texSettings);
            texSettings.spriteMode = (int)SpriteImportMode.Single;
            texSettings.filterMode = FilterMode.Point;
            texSettings.alphaIsTransparency = true;
            texSettings.alphaSource = TextureImporterAlphaSource.FromInput;
            texSettings.mipmapEnabled = false;
            texSettings.spritePixelsPerUnit = m_pixelsPerUnit;
            texSettings.spriteExtrude = 0;

            TextureImporterPlatformSettings texPlatSettings = new TextureImporterPlatformSettings();
            texPlatSettings.textureCompression = TextureImporterCompression.Uncompressed;
            texPlatSettings.maxTextureSize = 4096;

            importer.mipmapEnabled = false;
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

        foreach (string asset in assets)
        {
            assetCount++;

            if (!asset.EndsWith(".tmx", StringComparison.OrdinalIgnoreCase))
                continue;

            switch (changeType)
            {
                case AssetChangeType.Import:
                    Map map = ImportMap(asset);
                    ImportTiles(map, asset);
                    AssetDatabase.Refresh();
                    // TODO: The asset database doesn't appear to be done refreshing. 
                    // TOFIX: Set a "renderNeeded" bool and render when !EditorApplication.isCompiling (or some other sensible time)
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

    private static void ImportTiles(Map map, string asset)
    {
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

                    if (!file.Contains(m_imgFsDir) && !File.Exists(newFile))
                    {
                        // Copy external file to the project
                        File.Copy(file, newFile);

                        // Detemine path in asset database
                        string assetDbPath = newFile.Replace('\\', '/');
                        assetDbPath = assetDbPath.Substring(assetDbPath.IndexOf(m_imgDir));
                        if (!m_assetDbTilePaths.ContainsKey(tile.Id))
                            m_assetDbTilePaths.Add(tile.Id, assetDbPath);
                        AssetDatabase.ImportAsset(assetDbPath);
                        AssetDatabase.Refresh();
                        tilesImported++;
                    }
                }
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.StopAssetEditing();
        AssetDatabase.Refresh();
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

    private static void RenderMap(Map map)
    {
        if (map == null)
            return;

        GameObject mapParent = new GameObject("Map");
        for (int layerIndex = 0; layerIndex < map.Layers.Count; layerIndex++)
        {
            GameObject layerParent = new GameObject("Layer_" + layerIndex);
            layerParent.transform.parent = mapParent.transform;

            Layer layer = map.Layers[layerIndex];
            //Debug.Log("processing layer " + layer.Name + " d: " + layer.Data.MapData.Length + " w: " + layer.Width + " h: " + layer.Height);
            for (int col = 0; col < layer.Width; col++)
            {
                for (int row = 0; row < layer.Height; row++)
                {
                    int tileId = layer.Data.MapData[col, row];
                    if (tileId >= 0 && m_assetDbTilePaths.ContainsKey(tileId))
                    {
                        GameObject g = new GameObject(tileId.ToString());
                        g.transform.parent = layerParent.transform;
                        g.transform.position = new Vector2(col, row);
                        SpriteRenderer r = g.AddComponent<SpriteRenderer>();
                        r.sortingOrder = layerIndex;

                        UnityEngine.Object tex2d = AssetDatabase.LoadMainAssetAtPath(m_assetDbTilePaths[tileId]);
                        if (tex2d != null)
                        {
                            //Sprite s = Sprite.Create(tex2d, new Rect(), Vector2.zero);
                            Debug.Log("s IS NOT NULL for " + tileId + " at path " + m_assetDbTilePaths[tileId]);
                            //r.sprite = s;
                        }
                        else
                        {
                            Debug.Log(string.Format("Couldn't load sprite from AssetDatabase at path {0}", m_assetDbTilePaths[tileId]));
                        }
                    }
                }
            }
        }

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
            Map map = null;
            FileStream stream = null;
            XmlReader reader = null;
            m_assetDbTilePaths = new Dictionary<int, string>();

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
