using UnityEngine;

/// <summary>
/// Controls the current for various sections of the water that runs through the pipes
/// </summary>
public class WaterCurrent : MonoBehaviour {
    #region public editor variables

    /// <summary>
    /// The maximum speed of the current
    /// </summary>
    public Vector2 MaxSpeed = Vector2.zero;

    /// <summary>
    /// The force of the current
    /// </summary>
    public Vector2 Force = Vector2.zero;

    #endregion

    /// <summary>
    /// Occurs when an object enters the trigger area. 
    /// The physics configuration only allows Muckle to cause the collision
    /// </summary>
    /// <param name="col">The <see cref="Collider2D"/> of the object which entered the trigger area</param>
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == Globals.TAG_MUCKLE)
        {
            IRigidBodyBehavior rig = col.GetComponent<IRigidBodyBehavior>();
            rig.AddConstantForce(gameObject, Force, MaxSpeed);
        }
    }

    /// <summary>
    /// Occurs when a collider leaves the trigger area
    /// </summary>
    /// <param name="col">The <see cref="Collider2D"/> which left the trigger area</param>
    void OnTriggerExit2D(Collider2D col)
    {
        if (col.tag == Globals.TAG_MUCKLE)
        {
            IRigidBodyBehavior rig = col.GetComponent<IRigidBodyBehavior>();
            rig.RemoveConstantForce(gameObject);
        }
    }
}
