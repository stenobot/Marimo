using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controller for the snake tool. Inherits <see cref="ToolBase"/>.
/// </summary>
public class SnakeController : ToolBase
{
    // The prefab to use for each node which makes up the snake and affect its physics
    public GameObject PhysicsNodePrefab;
    // The prefab which will be traced over the pixel-snapped positions of the node prefabs to give a retro effect
    public GameObject PixelPrefab;
    // The offset for the nodes spawn positions
    public Vector2 Offset = Vector2.zero;
    // The snake's length in pixels
    public int PixelLength = 64;
    // The speed that will be applied to extend the snake tool
    public float ExtendSpeed = 30f;
    // The speed that will be applied to retract the snake tool
    public float RetractSpeed = 20f;
    // The color to apply to the even pixels
    public Color EvenPixelColor;
    // The color to apply to the odd pixels
    public Color OddPixelColor;
    // The layer mask for objects which can be grappled
    public LayerMask GrappleLayerMask;

    // Tracks if the snake has been fired
    private bool m_hasFired;
    // Holds references to all active nodes that make up the snake
    private List<GameObject> m_nodes;
    // Holds references to all active pixels that make up the snake
    private List<GameObject> m_pixels;
    // The positions of all active pixels
    private List<Vector2> m_pixelPositions;
    // Tracks the last node that was iterated on to permit attached hinge joints
    private GameObject m_lastNode;
    // Holds a reference to the player object's robot controller script
    private RobotController m_player;
    // Holds a reference to the player's rigidbody
    private Rigidbody2D m_playerRig;
    // Anchors the extended snake to the player
    private Vector2 m_anchorPos;
    // Tracks if the snake is retracting
    private bool m_isRetracting;
    // Tracks the connected grapple hinge
    private DistanceJoint2D m_grappleHinge;
    // Tracks the offset from the grapple object where the hit occurred
    private Vector2 m_grappleOffset;
    // Holds a reference to the game manager
    private GameManager m_gameMgr;

    /// <summary>
    /// Used for initialization. Using Awake() instead of Start() so it fires each time the object is re-enabled.
    /// </summary>
    public override void Awake()
    {
        base.Awake();
        m_hasFired = false;
        m_gameMgr = GameObject.FindGameObjectWithTag(Globals.TAG_GAMEMANAGER).GetComponent<GameManager>();
        m_nodes = new List<GameObject>();
        m_pixels = new List<GameObject>();
        m_pixelPositions = new List<Vector2>();
        m_lastNode = null;
        m_isRetracting = false;
        m_player = GameObject.FindGameObjectWithTag(Globals.TAG_PLAYER).GetComponent<RobotController>();
        m_playerRig = m_player.GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    public override void Update()
    {
        base.Update();

        if (!m_player.IsDead && !m_gameMgr.IsPaused && IsFire1Pressed && !m_isRetracting && transform.localPosition.x >= RestingPosX)
        {
            // Fire or continue firing
            ActivateTool();
        }
        else if (!m_gameMgr.IsPaused)
        {
            DeactivateTool();
        }
    }

    /// <summary>
    /// Retracts the snake and then destroys it once complete
    /// </summary>
    private void DeactivateTool()
    {
        m_isRetracting = true;
        GameObject lastNode = null;

        // Disconnect grapple
        if (m_grappleHinge != null)
        {
            m_grappleHinge.connectedBody = null;
            m_grappleHinge.enabled = false;
            m_grappleOffset = Vector2.zero;
        }
        m_grappleHinge = null;

        foreach (GameObject node in m_nodes)
        {
            if (node != null && lastNode != null)
            {
                node.transform.position = lastNode.transform.position;
                lastNode = node;
            }
        }
        if (m_nodes.Count > 0)
        {
            Destroy(m_nodes[m_nodes.Count - 1]);
            m_nodes.RemoveAt(m_nodes.Count - 1);
        }
        if (m_nodes.Count > 0)
            m_nodes[m_nodes.Count - 1].GetComponent<HingeJoint2D>().connectedBody = m_playerRig;
        else
        {
            m_hasFired = false;
            m_lastNode = null;
            CancelInvoke("TracePixels");
            ClearNodes();
            ClearPixels();
            m_isRetracting = false;
        }
    }

    /// <summary>
    /// Activates the snake tool if it hasn't already been fired
    /// </summary>
    private void ActivateTool()
    {
        if (m_hasFired)
            return;

        // Clear the node references
        ClearNodes();

        for (int i = 0; i < PixelLength; i++)
        {
            // Determine the position to instantiate the snake node at
            Vector2 pos = (Vector2)transform.position + (RobotBodyTransform.localScale.x > 0 ? Offset : new Vector2(-Offset.x, Offset.y));

            // TODO: Object pooling (or just store nodes in disabled state in snake prefab)
            // NOTE: If not doing this, move this loop to a coroutine
            // Instantiate the node
            GameObject node = Instantiate(PhysicsNodePrefab, pos, Quaternion.identity, m_lastNode == null ? transform : m_lastNode.transform);
            if (i == 0)
            {
                m_anchorPos = node.transform.localPosition;
                node.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            }
            // Add the node to the collection of active nodes
            m_nodes.Add(node);
            if (m_lastNode != null)
            {
                // Connect the node's hinge joint to the previous node
                node.GetComponent<HingeJoint2D>().connectedBody = m_lastNode.GetComponent<Rigidbody2D>();
            }
            else
            {
                // Connect the node's hinge joint to the player
                node.GetComponent<HingeJoint2D>().connectedBody = GameObject.FindGameObjectWithTag(Globals.TAG_PLAYER).GetComponent<Rigidbody2D>();
            }
            node.GetComponent<Rigidbody2D>().velocity = ((RobotBodyTransform.localScale.x > 0) ? Vector2.right : Vector2.left) * ExtendSpeed;
            node.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionY;

            m_lastNode = node;
        }

        // Engage physics in .3 seconds
        Invoke("ReleaseNodesYConstraints", .3f);

        // Trace pixels at snapped positions over the node positions
        InvokeRepeating("TracePixels", 0f, 1f / 60f);
        m_hasFired = true;
    }

    /// <summary>
    /// Unsets the Y constraint on the nodes' rigidbodies
    /// </summary>
    public void ReleaseNodesYConstraints()
    {
        foreach (GameObject node in m_nodes)
        {
            if (node != null)
                node.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
        }
    }

    /// <summary>
    /// Stops the snake nodes from stretching more than a pixel's distance from each other
    /// </summary>
    private void ConstrainSnakeNodes()
    {
        if (m_nodes.Count > 0 && m_nodes[0] != null)
            m_nodes[0].transform.localPosition = m_anchorPos;

        for (int i = 0; i < m_nodes.Count; i++)
        {
            if (i > 0 && m_nodes[i] != null && m_nodes[i - 1] != null)
            {
                // Calculate the distance between the 2 nodes
                float distance = Vector2.Distance(m_nodes[i].transform.position, m_nodes[i - 1].transform.position);
                // If the distance is greater than 1 pixel, constrain the nodes
                if (distance > Globals.PIXEL_SIZE)
                {
                    Vector2 origin = m_nodes[i - 1].transform.position;
                    Vector2 target = m_nodes[i].transform.position;
                    // Get the point which is exactly 1 pixel away from the origin
                    target = MathHelper.LerpByDistance(origin, target, Globals.PIXEL_SIZE);
                    // Move the node to the target position
                    m_nodes[i].transform.position = target;
                }
            }
        }
    }

    /// <summary>
    /// Traces pixels over each node in <see cref="m_nodes"/>
    /// </summary>
    private void TracePixels()
    {
        // Clear the active pixels
        ClearPixels();

        if (m_grappleHinge == null)
        {
            // Constrain the nodes before tracing so the snake doesn't stretch
            ConstrainSnakeNodes();

            foreach (GameObject g in m_nodes)
            {
                if (g != null)
                {
                    // Get the snapped position
                    Vector2 snappedPos = MathHelper.SnapToPixels(g.transform.position);

                    // Don't add pixels in duplicate positions
                    if (!m_pixelPositions.Contains(snappedPos))
                    {
                        // Instantiate the pixel prefab
                        GameObject pix = Instantiate(PixelPrefab, snappedPos, Quaternion.identity, transform);
                        // Set the color of the sprite renderer
                        pix.GetComponent<SpriteRenderer>().color = ((m_pixels.Count + 1) % 2) == 0 ? EvenPixelColor : OddPixelColor;
                        // Add the pixel and its position to the pixel collections
                        m_pixels.Add(pix);
                        m_pixelPositions.Add(snappedPos);
                    }
                }
            }

            GameObject lastNode = m_nodes[m_nodes.Count - 1];
            Vector2 origin = lastNode.transform.position;
            Vector2 direction = RobotBodyTransform.localScale.x > 0 ? Vector2.right : Vector2.left;
            float distance = Globals.PIXEL_SIZE;
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance, GrappleLayerMask);
            if (hit.collider != null)
            {
                m_grappleHinge = hit.collider.GetComponent<DistanceJoint2D>();
                m_grappleHinge.connectedBody = m_playerRig;
                m_grappleHinge.enabled = true;
                m_grappleOffset = (Vector2)m_grappleHinge.transform.position - hit.point;
                lastNode.GetComponent<HingeJoint2D>().connectedBody = hit.collider.GetComponent<Rigidbody2D>();
                HingeJoint2D newHinge = lastNode.AddComponent<HingeJoint2D>();
                newHinge.connectedBody = null;
            }
        }
        else
        {
            Vector2 startPos = (Vector2)transform.position + (RobotBodyTransform.localScale.x > 0 ? Offset : new Vector2(-Offset.x, Offset.y));
            Vector2 endPos = (Vector2)m_grappleHinge.transform.position - m_grappleOffset;

            m_grappleHinge.GetComponent<DistanceJoint2D>().enabled = RobotBodyTransform.localScale.x > 0 ?
                (startPos.x > endPos.x ? true : false) :
                (startPos.x < endPos.x ? true : false);

            Vector2 currentpos = startPos;
            while (currentpos != endPos)
            {
                // Get the snapped position
                Vector2 snappedPos = MathHelper.SnapToPixels(currentpos);

                // Don't add pixels in duplicate positions
                if (!m_pixelPositions.Contains(snappedPos))
                {
                    // Instantiate the pixel prefab
                    GameObject pix = Instantiate(PixelPrefab, snappedPos, Quaternion.identity);
                    // Set the color of the sprite renderer
                    pix.GetComponent<SpriteRenderer>().color = ((m_pixels.Count + 1) % 2) == 0 ? EvenPixelColor : OddPixelColor;
                    // Add the pixel and its position to the pixel collections
                    m_pixels.Add(pix);
                    m_pixelPositions.Add(snappedPos);
                }

                bool lastMove = Vector2.Distance(currentpos, endPos) < Globals.PIXEL_SIZE;
                if (lastMove)
                    currentpos = endPos;
                else
                    currentpos = MathHelper.LerpByDistance(currentpos, endPos, Globals.PIXEL_SIZE);
            }
        }
    }

    /// <summary>
    /// Destroys the pixel objects in <see cref="m_pixels"/> and clears <see cref="m_pixelPositions"/>
    /// </summary>
    private void ClearPixels()
    {
        // TODO: Pooling
        foreach (GameObject g in m_pixels)
            Destroy(g);

        m_pixels.Clear();
        m_pixelPositions.Clear();
    }

    /// <summary>
    /// Destroys all node objects in <see cref="m_nodes"/>
    /// </summary>
    private void ClearNodes()
    {
        foreach (GameObject g in m_nodes)
        {
            // TODO: Object pooling (or just store nodes in disabled state in snake prefab)
            // NOTE: If not doing this, move this loop to a coroutine
            Destroy(g);
        }

        m_nodes.Clear();
    }
}
