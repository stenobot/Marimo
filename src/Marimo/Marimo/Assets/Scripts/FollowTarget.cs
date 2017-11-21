using UnityEngine;

/// <summary>
/// This script is attached to a camera and permits it to smoothly follow a target transform
/// </summary>
public class FollowTarget : MonoBehaviour {
    /// <summary>
    /// The target Transform to follow with the camera
    /// </summary>
    public Transform Target;

    /// <summary>
    /// The approximate time it should take to reach the target. A smaller value will reach the target faster.
    /// </summary>
    public float SmoothTime = 0.1f;

    /// <summary>
    /// The maximum speed the camera can move
    /// </summary>
    public float MaxSpeed = 25f;

    /// <summary>
    /// The lowest Y position the camera can reach before it stops following the target
    /// </summary>
    public float MinYPosition = 0f;

    private void Start()
    {
        // Scale camera all fancy-like for pixel perfection in any resolution
        GetComponent<Camera>().orthographicSize = Screen.height / (2 * (Globals.PIXELS_PER_UNIT * Globals.PIXEL_SCALE));
    }

    /// <summary>
    /// LateUpdate should be used for camera functions like this as the objects can move during Update() and LateUpdate() fires last
    /// </summary>
    private void LateUpdate()
    {
        MoveCam();
    }

    /// <summary>
    /// Smoothly moves the camera toward the target's position
    /// </summary>
    void MoveCam()
    {
        // Dummy speed value to pass to SmoothDamp method below
        Vector2 currentSpeed = Vector2.zero;
        // Calculate the camera position for the next frame. A Vector3 is used so the camera's Z position won't be altered
        Vector3 newPos = Vector2.SmoothDamp(transform.position, Target.position, ref currentSpeed, SmoothTime, MaxSpeed, 0.1f);
        // Set the Z position back to its original position
        newPos.z = transform.position.z;
        // Don't set the camera position if the player has fallen off screen
        if (newPos.y > MinYPosition)
        {
            // Set the camera position
            transform.position = newPos;
        }
    }
}
