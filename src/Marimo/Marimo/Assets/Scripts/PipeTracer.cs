using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PipeTracer : MonoBehaviour
{
    public GameObject PipePrefab;
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
        "pipe_vent_bottom_drain",
        "pipe_vent_left_drain",
        "pipe_vent_right_drain",
        "pipe_vent_top_drain",
        "pipe_vertical_drain",
        "pipe_water_drain"
    };

    private static Vector3 s_tileOffset = new Vector3(1, 1, 0);
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

    public PipeNode GetPipeNodeAtPosition(Vector2 pos)
    {
        Vector3Int cellPos = m_map.WorldToCell(pos);
        Vector2 nodePos = m_map.CellToWorld(cellPos) + s_tileOffset;

        return m_pipeNodes.ContainsKey(nodePos) ? m_pipeNodes[nodePos] : null;
    }


    /// <summary>
    /// Finds tiles of type <see cref="PipeTile"/> on the attached <see cref="Tilemap"/>
    /// Instantiates the <see cref="PipePrefab"/> at their world position
    /// </summary>
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
                    GameObject g = Instantiate(PipePrefab, m_map.CellToWorld(pos) + s_tileOffset, Quaternion.identity, transform);
                    PipeNode node = g.GetComponent<PipeNode>();
                    m_pipes.Add(g);
                    m_pipeNodes.Add(g.transform.position, node);
                }
            }
        }
    }

    /// <summary>
    /// Sets up components on the pipe gameobjects after they have all been created (so node connections can be enumerated)
    /// </summary>
    private void SetupComponents()
    {
        foreach (GameObject g in m_pipes)
        {
            // Get the pipe type
            Enums.PipeType pipeType = GetPipeType(g);

            // Setup node
            PipeNode node = g.GetComponent<PipeNode>();
            node.PipeType = pipeType;
            node.NodeTop = GetPipeNodeAtPosition((Vector2)g.transform.position + (Vector2.up * 2));
            node.NodeBottom = GetPipeNodeAtPosition((Vector2)g.transform.position + (Vector2.down * 2));
            node.NodeLeft = GetPipeNodeAtPosition((Vector2)g.transform.position + (Vector2.left * 2));
            node.NodeRight = GetPipeNodeAtPosition((Vector2)g.transform.position + (Vector2.right * 2));

            if (pipeType != Enums.PipeType.None)
            {
                // Set the animation state
                g.GetComponent<Animator>().Play(s_pipeAnimationStates[(int)pipeType], 0, 0);

                GameObject tempObj = Instantiate(ColliderPrefabs[(int)pipeType]);
                // Filthy non-composite colliders. I am a terrible human :)
                foreach (EdgeCollider2D edge in tempObj.GetComponents<EdgeCollider2D>())
                {
                    EdgeCollider2D newEdge = g.AddComponent<EdgeCollider2D>();
                    newEdge.points = edge.points;
                }
                Destroy(tempObj);
            }
        }
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
        if (pipeLeft && pipeRight && !pipeUp && pipeDown)
            return Enums.PipeType.OpenTop;
        if (pipeLeft && pipeRight && pipeUp && !pipeDown)
            return Enums.PipeType.OpenBottom;
        if ((pipeUp && pipeDown && pipeLeft && !pipeRight))
            return Enums.PipeType.OpenRight;
        if ((pipeUp && pipeDown && !pipeLeft && pipeRight))
            return Enums.PipeType.OpenLeft;
        if (pipeUp && !pipeDown && !pipeLeft && !pipeRight)
            return Enums.PipeType.VentBottom;
        if (!pipeUp && pipeDown && !pipeLeft && !pipeRight)
            return Enums.PipeType.VentTop;
        if (!pipeUp && !pipeDown && !pipeLeft && pipeRight)
            return Enums.PipeType.VentLeft;
        if (!pipeUp && !pipeDown && pipeLeft && !pipeRight)
            return Enums.PipeType.VentRight;
        if ((!pipeUp && !pipeDown) && (pipeLeft && pipeRight))
            return Enums.PipeType.Horizontal;
        if ((pipeUp && pipeDown) && (!pipeLeft && !pipeRight))
            return Enums.PipeType.Vertical;
        if (((pipeUp && pipeDown) && (pipeLeft || pipeRight)) || ((pipeUp || pipeDown) && (pipeLeft && pipeRight)))
            return Enums.PipeType.Water;

        return Enums.PipeType.None;
    }
}
