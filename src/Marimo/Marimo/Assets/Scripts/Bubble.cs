using UnityEngine;

public class Bubble : MonoBehaviour
{
    public Vector3 MinScale;
    public Vector3 MaxScale;
    public Vector3 GrowthIncrease;
    public float GrowInterval = .1f;
    public float MinMoveInterval = .05f;
    public float MaxMoveInterval = .2f;
    private Rigidbody2D m_rig;

    // Use this for initialization
    void Start()
    {
        m_rig = GetComponent<Rigidbody2D>();
        Invoke("Move",0);
        InvokeRepeating("IncreaseScale", GrowInterval, GrowInterval);
    }

    /// <summary>
    /// Makes the bubbles move more randomly like bubbles
    /// </summary>
    private void Move()
    {
        m_rig.AddForce(Random.insideUnitCircle * 10.0f);
        Invoke("Move", Random.Range(MinMoveInterval, MaxMoveInterval));
    }

    private void IncreaseScale()
    {
        Vector3 newScale = transform.localScale + GrowthIncrease;
        if (newScale.magnitude > MaxScale.magnitude)
        {
            transform.localScale = MaxScale;
            // Pop the bubble so it doesn't hang around forever, clogging pipes
            Invoke("Destroy", 5f);
            CancelInvoke("IncreaseScale");
        }
        else
        {
            transform.localScale = newScale;
        }
    }

    private void Destroy()
    {
        Destroy(gameObject);
    }
}
