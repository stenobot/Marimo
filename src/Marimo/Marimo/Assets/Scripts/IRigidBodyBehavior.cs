using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface for <see cref="RigidBodyBehavior"/>
/// </summary>
public interface IRigidBodyBehavior
{
    /// <summary>
    /// Contains the list of force modifiers which should be applied to the velocity of the <see cref="Rigidbody2D"/>
    /// </summary>
    List<ForceModifier> ForceModifiers { get; }

    /// <summary>
    /// Creates a new <see cref="ForceModifier"/> and adds it to the <see cref="ForceModifiers"/> list
    /// Only 1 <see cref="ForceModifier"/> can be applied per owner.
    /// If the owner already exists, the <see cref="ForceModifier.Force"/> and <see cref="ForceModifier.MaxSpeed"/> values will be updated
    /// </summary>
    /// <param name="owner">The object which is applying the force</param>
    /// <param name="force">The force to apply</param>
    /// <param name="maxSpeed">The maximum speed the force can reach</param>
    void AddConstantForce(GameObject owner, Vector2 force, Vector2 maxSpeed);

    /// <summary>
    /// Removes a constant force from <see cref="ForceModifiers"/>
    /// </summary>
    /// <param name="owner">The owner of the <see cref="ForceModifier"/></param>
    void RemoveConstantForce(GameObject owner);
}