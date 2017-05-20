using UnityEngine;

/// <summary>
/// Controls the elevators in the game
/// </summary>
public class Elevator : MonoBehaviour {
    public Vector2 StartLocation;
    public Vector2 StopLocation;

	// Use this for initialization
	void Start () {
        // Ensure the elevator is at the start position
        transform.localPosition = StartLocation;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
