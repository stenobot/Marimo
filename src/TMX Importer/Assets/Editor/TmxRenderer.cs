using Assets.Schema;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TmxRenderer : MonoBehaviour
{
    public static void RenderMap(Map map, Dictionary<int, string> tilePaths)
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
                    if (tileId > 0 && tilePaths.ContainsKey(tileId))
                    {
                        GameObject g = new GameObject(tileId.ToString());
                        g.transform.parent = layerParent.transform;
                        g.transform.position = new Vector2(col, map.Height - row);
                        SpriteRenderer r = g.AddComponent<SpriteRenderer>();
                        r.sortingOrder = layerIndex;
                        Sprite sprite = null;
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        sprite = AssetDatabase.LoadAssetAtPath<Sprite>(tilePaths[tileId]);

                        if (sprite != null)
                            r.sprite = sprite;
                        else
                            Debug.LogWarning(string.Format("Couldn't load sprite from AssetDatabase at path {0}", tilePaths[tileId]));
                    }
                }
            }
        }
    }
}
