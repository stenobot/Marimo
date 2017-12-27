using UnityEngine;

public class PipeNode : MonoBehaviour
{
    // Node properties
    public PipeNode NodeLeft;
    public PipeNode NodeRight;
    public PipeNode NodeTop;
    public PipeNode NodeBottom;
    public Enums.PipeType PipeType { get; set; }

    private Vector2 m_lastDir;
    private Animator m_anim;

    private void Start()
    {
        m_anim = GetComponent<Animator>();
    }

    public void Drain(Vector2 startDirection)
    {
        m_anim.SetFloat("AnimSpeed", 1.0f);

        if (NodeRight && startDirection != Vector2.left)
        {
            NodeRight.Drain(Vector2.right);
        }
        if (NodeLeft && startDirection != Vector2.right)
        {
            NodeLeft.Drain(Vector2.left);
        }
    }

    public void DrainBottom()
    {
        if (NodeBottom)
            NodeBottom.Drain(Vector2.down);
    }
}
