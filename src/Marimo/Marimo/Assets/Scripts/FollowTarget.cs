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
    /// The lowest Y position the camera can reach before it stops following the target
    /// </summary>
    public float MinYPosition = 0f;

    /// <summary>
    /// LateUpdate is called after all Update functions have been called.
    /// It needs to be used for follow cam because they track objects which may have moved during the other Update functions.
    /// </summary>
    void LateUpdate () {
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
        Vector3 newPos = Vector2.SmoothDamp(transform.position, Target.position, ref currentSpeed, SmoothTime, 10f, 0.1f);
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
