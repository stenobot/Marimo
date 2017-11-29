using UnityEngine;

public class BubbleEmitter : MonoBehaviour {
    // A selection of bubble prefabs which will be selected at random for instantiation
    public GameObject[] BubblePrefabs;
    // Emit() will be called on Start() if true
    public bool PlayOnAwake = false;
    // If true, Emit() will be called at a random interval between MinLoopDelay and MaxLoopDelay indefinitely
    public bool Loop = false;
    // The minimum delay before the loop replays
    public float MinLoopDelay = .1f;
    // The maximum delay before the loop replays
    public float MaxLoopDelay = 1f;

    /// <summary>
    /// Use this for initialization
    /// </summary>
    private void Start () {
        if (PlayOnAwake)
            Emit();
	}

    /// <summary>
    /// Instantiates a random bubble prefab from <see cref="BubblePrefabs"/> at the transform location
    /// </summary>
    public void Emit()
    {
        // Randomize the spawn position a little so we don't get straight lines of bubbles
        Vector2 pos = (Vector2)transform.position + new Vector2(Random.Range(-.2f, 0.2f), Random.Range(-.2f, 0.2f));
        // Spawn a bubble. Parent is intentionally null.
        GameObject bubble = Instantiate<GameObject>(BubblePrefabs[Random.Range(0, BubblePrefabs.Length)], pos, Quaternion.identity, null);

        if(Loop)
            Invoke("Emit", Random.Range(MinLoopDelay, MaxLoopDelay));
    }
}
