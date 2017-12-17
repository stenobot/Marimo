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

	// array of all pipe sprites
	public Sprite[] PipeSprites;

    [Serializable]
    public struct DrainAnim
    {
        public Enums.PipeTileSprite PipeSpriteType;
        public Sprite[] AnimSprites;
    }

    public DrainAnim[] DrainAnimations;

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
				if (!IsAdjacentTile(x,y)) 
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
					} else // right no
					{
						return Enums.PipeTileSprite.OpenRight; 
					}
				} else // top no
				{
					if (AdjacentPipeTiles[SIDE_RIGHT]) 
					{
						return Enums.PipeTileSprite.OpenTop; 
					} else // right no
					{
						return Enums.PipeTileSprite.CornerTopRight; 
					}
				}
			} else // bottom no
			{
				if (AdjacentPipeTiles[SIDE_TOP]) 
				{
					if (AdjacentPipeTiles[SIDE_RIGHT]) 
					{
						return Enums.PipeTileSprite.OpenBottom; 
					} else // right no
					{
						return Enums.PipeTileSprite.CornerBottomRight; 
					}
				} else // top no
				{
					if (AdjacentPipeTiles [SIDE_RIGHT]) 
					{ 
						return Enums.PipeTileSprite.Horizontal; 
					} 
					else // right no
					{
						return Enums.PipeTileSprite.CapRight; 
					}
				}
			}
		} else // left no
		{
			if (AdjacentPipeTiles[SIDE_BOTTOM]) 
			{ 
				if (AdjacentPipeTiles[SIDE_TOP]) 
				{ 
					if (AdjacentPipeTiles[SIDE_RIGHT]) 
					{ 
						return Enums.PipeTileSprite.OpenLeft; 
					} else // right no
					{
						return Enums.PipeTileSprite.Vertical; 
					}
				} else // top no
				{
					if (AdjacentPipeTiles[SIDE_RIGHT]) 
					{
						return Enums.PipeTileSprite.CornerTopLeft; 
					} else // right no
					{
						return Enums.PipeTileSprite.CapTop; 
					}
				}
			} else // bottom no
			{
				if (AdjacentPipeTiles[SIDE_TOP]) 
				{
					if (AdjacentPipeTiles[SIDE_RIGHT]) 
					{
						return Enums.PipeTileSprite.CornerBottomLeft; 
					} else // right no
					{
						return Enums.PipeTileSprite.CapBottom; 
					}
				} else // top no
				{
					if (AdjacentPipeTiles [SIDE_RIGHT]) 
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
		return tilemap.GetTile(position) == this;
	}

	#if UNITY_EDITOR

	[MenuItem("Assets/Create/Tiles/PipeTile")]
	public static void CreatePipeTile()
	{
		string path = EditorUtility.SaveFilePanelInProject ("Save Pipe Tile", "New Pipe Tile", "asset", "Save Pipe Tile", "Assets");

		if (path == "")
			return;

		AssetDatabase.CreateAsset (ScriptableObject.CreateInstance<PipeTile> (), path);
	}

	#endif
}
