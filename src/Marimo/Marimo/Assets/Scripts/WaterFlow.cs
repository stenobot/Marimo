using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterFlow : MonoBehaviour
{
    public float Speed = 1.0f;
    public LayerMask PipeLayerMask;
    public GameObject VisualizationPrefab;
    public float MoveScale = 2.0f;
    private List<Flow> m_flows;

    private class Flow
    {
        public Vector2 Position { get; set; }
        public Vector2 LastDirection { get; set; }
        public GameObject Object { get; set; }

        public Flow(Vector2 pos, Vector2 dir, GameObject obj)
        {
            Position = pos;
            LastDirection = dir;
            Object = obj;
        }
    }


    // Use this for initialization
    void Start()
    {
        m_flows = new List<Flow>() { new Flow(transform.position, Vector2.down, Instantiate(VisualizationPrefab, transform.position, Quaternion.identity)) };
        InvokeRepeating("Advance", 0, Speed);
    }

    void Advance()
    {
        // Cache the list of flows to operate so m_flows can be modified during loop
        Flow[] cachedFlows = new Flow[m_flows.Count];
        m_flows.CopyTo(cachedFlows);

        foreach (Flow flow in cachedFlows)
        {
            // Save start position and direction
            Vector2 flowPos = flow.Position;
            Vector2 flowDir = flow.LastDirection;

            // If true while raycasts are evaluated a new Flow should be added to m_flows, as there's a fork in the path
            bool hasFlowed = false;

            // Just cast 3 rays, water can't defy gravity
            RaycastHit2D hitDown = Physics2D.Raycast(flowPos, Vector2.down, 1.0f * MoveScale, PipeLayerMask);
            RaycastHit2D hitLeft = Physics2D.Raycast(flowPos, Vector2.left, 1.0f * MoveScale, PipeLayerMask);
            RaycastHit2D hitRight = Physics2D.Raycast(flowPos, Vector2.right, 1.0f * MoveScale, PipeLayerMask);

            if (hitDown.collider != null && hitLeft.collider != null && hitRight.collider != null)
                continue;

            if (m_flows.Count > 1000)
                throw new UnityException("Waterflow.cs is probably trying to break the computer...");

            if (hitDown.collider == null)
            {
                
                Debug.DrawRay(flowPos, Vector2.down, Color.yellow, .1f);
                flow.Position += Vector2.down * MoveScale;
                flow.LastDirection = Vector2.down;
                hasFlowed = true;
                if (flow.Object != null)
                    flow.Object.transform.position = flow.Position;
                else
                    Instantiate(VisualizationPrefab, flow.Position, Quaternion.identity);
            }

            if ((hitLeft.collider == null) && (flowDir != Vector2.right))
            {
                Debug.DrawRay(flowPos, Vector2.left, Color.magenta, .1f);

                if (hasFlowed)
                {
                    GameObject g = Instantiate(VisualizationPrefab, flowPos + (Vector2.left * MoveScale), Quaternion.identity);
                    Flow f = new Flow(g.transform.position, Vector2.left, g);
                    m_flows.Add(f);
                }
                else
                {
                    flow.Position += Vector2.left * MoveScale;
                    flow.LastDirection = Vector2.left;
                    if (flow.Object != null)
                        flow.Object.transform.position = flow.Position;
                    else
                        Instantiate(VisualizationPrefab, flow.Position, Quaternion.identity);
                }
                hasFlowed = true;
            }

            if ((hitRight.collider == null) && (flowDir != Vector2.left))
            {
                Debug.DrawRay(flowPos, Vector2.right, Color.green, .1f);
                if (hasFlowed)
                {
                    GameObject g = Instantiate(VisualizationPrefab, flowPos + (Vector2.right * MoveScale), Quaternion.identity);
                    Flow f = new Flow(g.transform.position, Vector2.right, g);
                    m_flows.Add(f);
                }
                else
                {
                    flow.Position += Vector2.right * MoveScale;
                    flow.LastDirection = Vector2.right;
                    if (flow.Object != null)
                        flow.Object.transform.position = flow.Position;
                    else
                        Instantiate(VisualizationPrefab, flow.Position, Quaternion.identity);
                }
            }
        }

        Debug.Log("Flows: " + m_flows.Count);
    }
}
