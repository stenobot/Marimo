using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PipeTracer : MonoBehaviour
{
    public GameObject PipePrefab;
    public Sprite[] PipeFullSprites;

    private string[] m_pipeAnimationStates = {
        "pipe_corner_bottom_left_drain",
        "pipe_corner_bottom_right_drain",
        "pipe_corner_top_left_drain",
        "pipe_corner_top_right_drain",
        "pipe_horizontal_drain",
        "pipe_open_bottom_drain",
        "pipe_open_left_drain",
        "pipe_open_right_drain",
        "pipe_open_top_drain",
        "",
        "",
        "",
        "",
        "pipe_vertical_drain",
        "pipe_water_drain"
    };
    private Tilemap m_map;
    private List<GameObject> m_pipes;

    private enum PipeType
    {
        None = -1,
        CornerBottomLeft = 0,
        CornerBottomRight = 1,
        CornerTopLeft = 2,
        CornerTopRight = 3,
        Horizontal = 4,
        OpenBottom = 5,
        OpenLeft = 6,
        OpenRight = 7,
        OpenTop = 8,
        VentBottom = 9,
        VentLeft = 10,
        VentRight = 11,
        VentTop = 12,
        Vertical = 13,
        Water = 14
    }

    // Use this for initialization
    void Start()
    {
        m_map = GetComponent<Tilemap>();
        m_pipes = new List<GameObject>();
        TracePipes();
        SetupComponents();
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

    private void SetupComponents()
    {
        foreach (GameObject g in m_pipes)
        {
            PipeType type = GetPipeType(g);
            g.GetComponent<SpriteRenderer>().sprite = type == PipeType.None ? null : PipeFullSprites[(int)type];
            if (type == PipeType.None)
                continue;
            g.GetComponent<Animator>().Play(m_pipeAnimationStates[(int)type]);
            g.GetComponent<Animator>().SetFloat("AnimSpeed", 1.0f);
        }
    }

    private PipeType GetPipeType(GameObject g)
    {
        Vector2 pos = g.transform.position;

        bool pipeUp = m_map.GetTile(m_map.WorldToCell(pos + (Vector2.up * 2))) != null;
        bool pipeDown = m_map.GetTile(m_map.WorldToCell(pos + (Vector2.down * 2))) != null;
        bool pipeLeft = m_map.GetTile(m_map.WorldToCell(pos + (Vector2.left * 2))) != null;
        bool pipeRight = m_map.GetTile(m_map.WorldToCell(pos + (Vector2.right * 2))) != null;

        if ((pipeUp && !pipeDown) && (!pipeLeft && pipeRight))
            return PipeType.CornerBottomLeft;
        if ((pipeUp && !pipeDown) && (pipeLeft && !pipeRight))
            return PipeType.CornerBottomRight;
        if ((!pipeUp && pipeDown) && (!pipeLeft && pipeRight))
            return PipeType.CornerTopLeft;
        if ((!pipeUp && pipeDown) && (pipeLeft && !pipeRight))
            return PipeType.CornerTopRight;
        if ((!pipeUp && !pipeDown) && (pipeLeft || pipeRight))
            return PipeType.Horizontal;
        if ((pipeUp && pipeDown) && (!pipeLeft && !pipeRight))
            return PipeType.Vertical;
        if (((pipeUp && pipeDown) && (pipeLeft || pipeRight)) || ((pipeUp || pipeDown) && (pipeLeft && pipeRight)))
            return PipeType.Water;

        return PipeType.None;
    }
}
