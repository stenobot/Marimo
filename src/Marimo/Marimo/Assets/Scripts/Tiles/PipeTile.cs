using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System;

public class PipeTile : Tile
{
    private const int SIDE_LEFT = 0;
    private const int SIDE_BOTTOM = 1;
    private const int SIDE_TOP = 2;
    private const int SIDE_RIGHT = 3;

    [Serializable]
    public struct DrainAnim
    {
        public Enums.PipeTileSprite PipeSpriteType;
        public Sprite[] AnimSprites;
    }

    // array of all pipe sprites
    public Sprite[] PipeSprites;
    // Holds drain animation sprites for each pipe type
    public DrainAnim[] DrainAnimations;
    
    // Holds the dictionary representation of DrainAnimations
    private Dictionary<Enums.PipeTileSprite, Sprite[]> m_drainSprites = new Dictionary<Enums.PipeTileSprite, Sprite[]>();
    private ITilemap m_tileMap;
    private Vector3Int m_tilePos;
    private TileAnimationData m_animData;
    private float m_animSpeed = 0;

    public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
    {
        if (m_drainSprites.Count == 0)
            // Populate dictionary for efficient lookups
            foreach (DrainAnim anim in DrainAnimations)
                m_drainSprites.Add(anim.PipeSpriteType, anim.AnimSprites);
        return base.StartUp(position, tilemap, go);
    }

    public override void RefreshTile(Vector3Int position, ITilemap tilemap)
    {
        // loop through grid surrounding a tile
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3Int currPos = new Vector3Int(position.x + x, position.y + y, position.z);

                // if tile has an existing pipe tile, call RefreshTile 
                if (HasPipeTile(tilemap, currPos))
                    tilemap.RefreshTile(currPos);
            }
        }
    }

    /// <summary>
    /// Gets the tile data. This gets fired each time RefreshTile is called on a tilemap position.
    /// </summary>
    /// <param name="position">Position.</param>
    /// <param name="tilemap">Tilemap.</param>
    /// <param name="tileData">Tile data.</param>
    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        // four adjacent tiles, are they pipes?
        bool[] AdjacentPipeTiles = new bool[4];

        int index = 0;

        // loop through grid surrounding tile
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                // if tile is not one of four adjacent tiles, do nothing
                if (!IsAdjacentTile(x, y))
                    continue;

                // if adjacent tile is a pipe, set array index to true
                if (HasPipeTile(tilemap, new Vector3Int(position.x + x, position.y + y, position.z)))
                    AdjacentPipeTiles[index] = true;
                else
                    AdjacentPipeTiles[index] = false;

                index++;
            }
        }

        // change the current sprite
        tileData.sprite = PipeSprites[(int)AdjustedTileSprite(AdjacentPipeTiles)];
    }


    /// <summary>
    /// Gets the animation data for the tile
    /// </summary>
    /// <param name="location">The tile location</param>
    /// <param name="tileMap">The tilemap</param>
    /// <param name="tileAnimationData">The <see cref="TileAnimationData"/> object to place the output</param>
    /// <returns>Boolean indicating success</returns>
    public override bool GetTileAnimationData(Vector3Int location, ITilemap tileMap, ref TileAnimationData tileAnimationData)
    {
        TileData data = new TileData();
        GetTileData(location, tileMap, ref data);
        Enums.PipeTileSprite tileType = GetTileType(data);

        if (m_drainSprites[tileType].Length > 0)
        {
            tileAnimationData.animatedSprites = m_drainSprites[tileType];
            tileAnimationData.animationSpeed = m_animSpeed;
            tileAnimationData.animationStartTime = 0;
            m_tilePos = location;
            m_tileMap = tileMap;
            m_animData = tileAnimationData;
            return true;
        }
        return false;
    }


    public void Drain()
    {
        m_animSpeed = 16f;
        GetTileAnimationData(m_tilePos, m_tileMap, ref m_animData);
    }


    private Enums.PipeTileSprite GetTileType(TileData data)
    {
        Enums.PipeTileSprite result = Enums.PipeTileSprite.CapTop;

        switch (data.sprite.name.ToLower())
        {
            case "pipe_corner_bottom_left":
                return Enums.PipeTileSprite.CornerBottomLeft;
            case "pipe_corner_bottom_right":
                return Enums.PipeTileSprite.CornerBottomRight;
            case "pipe_corner_top_left":
                return Enums.PipeTileSprite.CornerTopLeft;
            case "pipe_corner_top_right":
                return Enums.PipeTileSprite.CornerTopRight;
            case "pipe_horizontal":
                return Enums.PipeTileSprite.Horizontal;
            case "pipe_open_bottom":
                return Enums.PipeTileSprite.OpenBottom;
            case "pipe_open_left":
                return Enums.PipeTileSprite.OpenLeft;
            case "pipe_open_right":
                return Enums.PipeTileSprite.OpenRight;
            case "pipe_open_top":
                return Enums.PipeTileSprite.OpenTop;
            case "pipe_vertical":
                return Enums.PipeTileSprite.Vertical;
        }

        return result;
    }

    /// <summary>
    /// Adjusts the tile sprite based on it's adjacent tile
    /// </summary>
    /// <returns>The correct tile sprite</returns>
    /// <param name="AdjacentPipeTiles">Array of all four adjacent tiles "is pipe" status</param>
    private Enums.PipeTileSprite AdjustedTileSprite(bool[] AdjacentPipeTiles)
    {
        if (AdjacentPipeTiles[SIDE_LEFT])
        {
            if (AdjacentPipeTiles[SIDE_BOTTOM])
            {
                if (AdjacentPipeTiles[SIDE_TOP])
                {
                    if (AdjacentPipeTiles[SIDE_RIGHT])
                    {
                        return Enums.PipeTileSprite.OpenAll;
                    }
                    else // right no
                    {
                        return Enums.PipeTileSprite.OpenRight;
                    }
                }
                else // top no
                {
                    if (AdjacentPipeTiles[SIDE_RIGHT])
                    {
                        return Enums.PipeTileSprite.OpenTop;
                    }
                    else // right no
                    {
                        return Enums.PipeTileSprite.CornerTopRight;
                    }
                }
            }
            else // bottom no
            {
                if (AdjacentPipeTiles[SIDE_TOP])
                {
                    if (AdjacentPipeTiles[SIDE_RIGHT])
                    {
                        return Enums.PipeTileSprite.OpenBottom;
                    }
                    else // right no
                    {
                        return Enums.PipeTileSprite.CornerBottomRight;
                    }
                }
                else // top no
                {
                    if (AdjacentPipeTiles[SIDE_RIGHT])
                    {
                        return Enums.PipeTileSprite.Horizontal;
                    }
                    else // right no
                    {
                        return Enums.PipeTileSprite.CapRight;
                    }
                }
            }
        }
        else // left no
        {
            if (AdjacentPipeTiles[SIDE_BOTTOM])
            {
                if (AdjacentPipeTiles[SIDE_TOP])
                {
                    if (AdjacentPipeTiles[SIDE_RIGHT])
                    {
                        return Enums.PipeTileSprite.OpenLeft;
                    }
                    else // right no
                    {
                        return Enums.PipeTileSprite.Vertical;
                    }
                }
                else // top no
                {
                    if (AdjacentPipeTiles[SIDE_RIGHT])
                    {
                        return Enums.PipeTileSprite.CornerTopLeft;
                    }
                    else // right no
                    {
                        return Enums.PipeTileSprite.CapTop;
                    }
                }
            }
            else // bottom no
            {
                if (AdjacentPipeTiles[SIDE_TOP])
                {
                    if (AdjacentPipeTiles[SIDE_RIGHT])
                    {
                        return Enums.PipeTileSprite.CornerBottomLeft;
                    }
                    else // right no
                    {
                        return Enums.PipeTileSprite.CapBottom;
                    }
                }
                else // top no
                {
                    if (AdjacentPipeTiles[SIDE_RIGHT])
                    {
                        return Enums.PipeTileSprite.CapLeft;
                    }
                }
            }
        }

        return Enums.PipeTileSprite.Horizontal;
    }


    /// <summary>
    /// Determines whether this instance is an adjacent tile (as opposed to a corner)
    /// </summary>
    /// <returns><c>true</c> if this instance is an adjacent tile; otherwise, <c>false</c>.</returns>
    /// <param name="x">The x coordinate from center</param>
    /// <param name="y">The y coordinate from center</param>
    private bool IsAdjacentTile(int x, int y)
    {
        return ((x == -1 && y == 0) || (x == 0 && y == -1) || (x == 0 && y == 1) || (x == 1 && y == 0));
    }

    /// <summary>
    /// Determines whether this instance has a pipe tile at the specified tilemap position.
    /// </summary>
    /// <returns><c>true</c> if this instance has a pipe tile the specified tilemap position; otherwise, <c>false</c>.</returns>
    /// <param name="tilemap">The current tilemap</param>
    /// <param name="position">the position to check</param>
    private bool HasPipeTile(ITilemap tilemap, Vector3Int position)
    {
        if (tilemap != null)
            return tilemap.GetTile(position) == this;
        else
            return false;
    }

#if UNITY_EDITOR

    [MenuItem("Assets/Create/Tiles/PipeTile")]
    public static void CreatePipeTile()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Pipe Tile", "New Pipe Tile", "asset", "Save Pipe Tile", "Assets");

        if (path == "")
            return;

        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<PipeTile>(), path);
    }
#endif
}
