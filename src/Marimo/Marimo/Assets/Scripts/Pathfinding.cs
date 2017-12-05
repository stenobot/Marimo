using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Pathfinding : MonoBehaviour
{
    // Holds all tilemaps which should cause collision for the player
    public List<Tilemap> CollisionTilemaps;

    private void Start()
    {
        // Find a path from position A to position B
        FindPath(transform.position, transform.position * 2);
    }

    /// <summary>
    /// Finds a path from one point on the map to another, avoiding collisions
    /// </summary>
    /// <param name="startPosition">The starting position</param>
    /// <param name="targetPosition">The target position</param>
    /// <returns>A list of <see cref="Vector2"/> containing each of the path's points on the map grid</returns>
    public IEnumerable<Vector2> FindPath(Vector2 startPosition, Vector2 targetPosition)
    {
        // This will hold the eventual path
        IEnumerable<Vector2> path = new List<Vector2>();

        foreach (Tilemap map in CollisionTilemaps)
        {
            for (int x = 0; x < map.size.x; x++)
                for (int y = 0; y < map.size.y; y++)
                    for (int z = 0; z < map.size.z; z++)
                        if (map.GetColliderType(new Vector3Int(x, y, z)) == Tile.ColliderType.Sprite)
                            Debug.Log("Found collider on tile: " + map.GetTile(new Vector3Int(x, y, z)).name);
        }
        return path;
    }
}
