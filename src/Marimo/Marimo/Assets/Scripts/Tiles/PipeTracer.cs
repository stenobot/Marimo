using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PipeTracer : MonoBehaviour
{
    public GameObject PipePrefab;
    public Sprite[] PipeFullSprites;

    private Tilemap m_map;
    private List<GameObject> m_pipes;

    // Use this for initialization
    void Start()
    {
        m_map = GetComponent<Tilemap>();
        m_pipes = new List<GameObject>();
        TracePipes();
        SetSprites();
    }

    private void TracePipes()
    {
        for (int x = m_map.cellBounds.xMin; x < m_map.cellBounds.xMax; x++)
        {
            for (int y = m_map.cellBounds.yMin; y < m_map.cellBounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                PipeTile pipe = m_map.GetTile(pos) as PipeTile;
                if (pipe != null)
                    m_pipes.Add(Instantiate(PipePrefab, m_map.CellToWorld(pos) + new Vector3(1, 1, 0), Quaternion.identity));
            }
        }
    }

    private void SetSprites()
    {
        foreach (GameObject g in m_pipes)
        {
            SpriteRenderer r = g.GetComponent<SpriteRenderer>();
            r.sprite = GetSprite(g);
        }
    }

    private Sprite GetSprite(GameObject g)
    {
        Sprite sprite = null;
        Vector2 pos = g.transform.position;

        bool pipeUp = m_map.GetTile(m_map.WorldToCell(pos + (Vector2.up * 2))) != null;
        bool pipeDown = m_map.GetTile(m_map.WorldToCell(pos + (Vector2.down * 2))) != null;
        bool pipeLeft = m_map.GetTile(m_map.WorldToCell(pos + (Vector2.left * 2))) != null;
        bool pipeRight = m_map.GetTile(m_map.WorldToCell(pos + (Vector2.right * 2))) != null;

        // corner bottom left
        sprite = (pipeUp && !pipeDown) && (!pipeLeft && pipeRight) ? PipeFullSprites[0] : sprite;
        // corner bottom right
        sprite = (pipeUp && !pipeDown) && (pipeLeft && !pipeRight) ? PipeFullSprites[1] : sprite;
        // corner top left
        sprite = (!pipeUp && pipeDown) && (!pipeLeft && pipeRight) ? PipeFullSprites[2] : sprite;
        // corner top right
        sprite = (!pipeUp && pipeDown) && (pipeLeft && !pipeRight) ? PipeFullSprites[3] : sprite;
        // horizontal sprite
        sprite = (!pipeUp && !pipeDown) && (pipeLeft || pipeRight) ? PipeFullSprites[4] : sprite;
        // vertical sprite
        sprite = (pipeUp && pipeDown) && (!pipeLeft && !pipeRight) ? PipeFullSprites[13] : sprite;
        // water sprite
        sprite = (pipeUp && pipeDown) && (pipeLeft || pipeRight) ? PipeFullSprites[14] : sprite;
        sprite = (pipeUp || pipeDown) && (pipeLeft && pipeRight) ? PipeFullSprites[14] : sprite;

        // Vents are special, they're already ok on the tile grid and should never drain
        // vent bottom sprite
        sprite = pipeUp && !pipeDown && !pipeLeft && !pipeRight ? null : sprite;
        // vent left sprite
        sprite = pipeRight && !pipeUp && !pipeDown && !pipeLeft ? null : sprite;
        // vent right sprite
        sprite = pipeLeft && !pipeUp && !pipeDown && !pipeRight ? null : sprite;
        // vent top sprite
        sprite = pipeDown && !pipeUp && !pipeLeft && !pipeRight ? null : sprite;
        return sprite;
    }
}
