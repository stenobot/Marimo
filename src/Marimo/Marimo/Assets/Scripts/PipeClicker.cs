using UnityEngine;

public class PipeClicker : MonoBehaviour
{
    private PipeTracer m_tracer;

    private void Start()
    {
        m_tracer = GetComponent<PipeTracer>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 0f);
            if (hit.collider != null && hit.collider.isTrigger)
            {
                PipeNode n = hit.collider.GetComponent<PipeNode>();
                n.Drain(Vector2.right);
            }
        }

    }
}
