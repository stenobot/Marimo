using UnityEngine;

public class Bubble : MonoBehaviour
{
    public Vector3 MinScale;
    public Vector3 MaxScale;
    public Vector3 GrowthIncrease;
    public float GrowInterval = .1f;

    // Use this for initialization
    void Start()
    {
        InvokeRepeating("IncreaseScale", GrowInterval, GrowInterval);
    }

    private void IncreaseScale()
    {
        Vector3 newScale = transform.localScale + GrowthIncrease;
        if (newScale.magnitude > MaxScale.magnitude)
        {
            transform.localScale = MaxScale;
            // Pop the bubble so it doesn't hang around forever, clogging pipes
            Invoke("Destroy", 1f);
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
