using UnityEngine;

/// <summary>
/// Data container for force modifiers which can be applied to <see cref="Rigidbody2D"/>
/// </summary>
public class ForceModifier
{
    /// <summary>
    /// The object which is applying the force
    /// </summary>
    public GameObject Owner { get; set; }

    /// <summary>
    /// The force to apply to the <see cref="Rigidbody2D"/>
    /// </summary>
    public Vector2 Force { get; set; }

    /// <summary>
    /// The maximum speed that the force can reach. If it shouldn't cause movement in a particular direction, set that direction to zero
    /// As will be added to other force modifiers, avoid using <see cref="Mathf.Infinity"/> and <see cref="Mathf.NegativeInfinity"/>
    /// </summary>
    public Vector2 MaxSpeed { get; set; }

    /// <summary>
    /// Instantiates a new <see cref="ForceModifier"/>
    /// </summary>
    /// <param name="object">The object which is applying the force</param>
    /// <param name="force">The force to apply to the <see cref="Rigidbody2D"/></param>
    /// <param name="maxSpeed">The maximum speed that the force can reach</param>
    public ForceModifier(GameObject owner, Vector2 force, Vector2 maxSpeed)
    {
        Owner = owner;
        Force = force;
        MaxSpeed = maxSpeed;
    }
}