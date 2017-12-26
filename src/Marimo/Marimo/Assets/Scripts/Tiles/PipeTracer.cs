﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PipeTracer : MonoBehaviour
{
    public GameObject PipePrefab;
    public Sprite[] PipeFullSprites;
    public GameObject[] ColliderPrefabs;

    private static string[] s_pipeAnimationStates = {
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
    private Dictionary<Vector2, PipeNode> m_pipeNodes;

    // Use this for initialization
    void Start()
    {
        m_map = GetComponent<Tilemap>();
        m_pipes = new List<GameObject>();
        m_pipeNodes = new Dictionary<Vector2, PipeNode>();

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
                {
                    GameObject g = Instantiate(PipePrefab, m_map.CellToWorld(pos) + new Vector3(1, 1, 0), Quaternion.identity, transform);
                    PipeNode node = g.AddComponent<PipeNode>();
                    m_pipes.Add(g);
                    m_pipeNodes.Add(g.transform.position, node);
                }
            }
        }
    }

    private void SetupComponents()
    {
        foreach (GameObject g in m_pipes)
        {
            // Get the pipe type
            Enums.PipeType pipeType = GetPipeType(g);

            // Set sprite (null if bad pipe type)
            g.GetComponent<SpriteRenderer>().sprite = pipeType == Enums.PipeType.None ? null : PipeFullSprites[(int)pipeType];

            // Bail if pipe type is none
            if (pipeType == Enums.PipeType.None)
                continue;

            // Setup node
            PipeNode node = g.GetComponent<PipeNode>();
            node.PipeType = pipeType;
            node.NodeTop = GetNodeAt((Vector2)g.transform.position + (Vector2.up * 2));
            node.NodeBottom = GetNodeAt((Vector2)g.transform.position + (Vector2.down * 2));
            node.NodeLeft = GetNodeAt((Vector2)g.transform.position + (Vector2.left * 2));
            node.NodeRight = GetNodeAt((Vector2)g.transform.position + (Vector2.right * 2));

            // Set animation state
            g.GetComponent<Animator>().Play(s_pipeAnimationStates[(int)pipeType]);
        }
    }

    private PipeNode GetNodeAt(Vector2 vector2)
    {
        if (m_pipeNodes.ContainsKey(vector2))
            return m_pipeNodes[vector2];

        return null;
    }

    private Enums.PipeType GetPipeType(GameObject g)
    {
        Vector2 pos = g.transform.position;

        bool pipeUp = m_map.GetTile(m_map.WorldToCell(pos + (Vector2.up * 2))) != null;
        bool pipeDown = m_map.GetTile(m_map.WorldToCell(pos + (Vector2.down * 2))) != null;
        bool pipeLeft = m_map.GetTile(m_map.WorldToCell(pos + (Vector2.left * 2))) != null;
        bool pipeRight = m_map.GetTile(m_map.WorldToCell(pos + (Vector2.right * 2))) != null;

        if ((pipeUp && !pipeDown) && (!pipeLeft && pipeRight))
            return Enums.PipeType.CornerBottomLeft;
        if ((pipeUp && !pipeDown) && (pipeLeft && !pipeRight))
            return Enums.PipeType.CornerBottomRight;
        if ((!pipeUp && pipeDown) && (!pipeLeft && pipeRight))
            return Enums.PipeType.CornerTopLeft;
        if ((!pipeUp && pipeDown) && (pipeLeft && !pipeRight))
            return Enums.PipeType.CornerTopRight;
        if ((!pipeUp && !pipeDown) && (pipeLeft && pipeRight))
            return Enums.PipeType.Horizontal;
        if ((pipeUp && pipeDown) && (!pipeLeft && !pipeRight))
            return Enums.PipeType.Vertical;
        if (((pipeUp && pipeDown) && (pipeLeft || pipeRight)) || ((pipeUp || pipeDown) && (pipeLeft && pipeRight)))
            return Enums.PipeType.Water;

        return Enums.PipeType.None;
    }
}