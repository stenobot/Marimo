using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class PipeTile : Tile 
{
	// array of all pipe sprites
	public Sprite[] PipeSprites;

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
		tileData.sprite = PipeSprites[AdjustedTileSpriteIndex(AdjacentPipeTiles)];
	}

	/// <summary>
	/// Adjusts the index of the tile sprite based on it's adjacent tile
	/// </summary>
	/// <returns>The correct tile sprite index</returns>
	/// <param name="AdjacentPipeTiles">Array of all four adjacent tiles "is pipe" status</param>
	private int AdjustedTileSpriteIndex(bool[] AdjacentPipeTiles)
	{
		if (AdjacentPipeTiles[0] == true) // left yes
		{ 	
			if (AdjacentPipeTiles[1] == true) // bottom yes
			{ 
				if (AdjacentPipeTiles[2] == true) // top yes
				{ 
					if (AdjacentPipeTiles[3] == true) // right yes
					{ 
						return 14; // just water
					} else // right no
					{
						return 7; // open right
					}
				} else // top no
				{
					if (AdjacentPipeTiles[3] == true) // right yes
					{
						return 8; //open top
					} else // right no
					{
						return 3; // corner top right
					}
				}
			} else // bottom no
			{
				if (AdjacentPipeTiles[2] == true) // top yes
				{
					if (AdjacentPipeTiles[3] == true) // right yes
					{
						return 5; // open bottom
					} else // right no
					{
						return 1; // corner bottom right
					}
				} else // top no
				{
					if (AdjacentPipeTiles [3] == true) // right yes
					{ 
						return 4; // horizontal
					} 
					else // right no
					{
						return 11; // right cap
					}
				}
			}
		} else // left no
		{
			if (AdjacentPipeTiles[1] == true) // bottom yes
			{ 
				if (AdjacentPipeTiles[2] == true) // top yes
				{ 
					if (AdjacentPipeTiles[3] == true) // right yes
					{ 
						return 6; // open left
					} else // right no
					{
						return 13; // vertical
					}
				} else // top no
				{
					if (AdjacentPipeTiles[3] == true) // right yes
					{
						return 2; // corner top left
					} else // right no
					{
						return 12; // top cap
					}
				}
			} else // bottom no
			{
				if (AdjacentPipeTiles[2] == true) // top yes
				{
					if (AdjacentPipeTiles[3] == true) // right yes
					{
						return 0; // corner bottom left
					} else // right no
					{
						return 9; // bottom cap
					}
				} else // top no
				{
					if (AdjacentPipeTiles [3] == true) // right yes
					{ 
						return 10; // left cap
					} 
					else // right no
					{
					}
				}
			}
		}

		return 4; // default horizontal
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
