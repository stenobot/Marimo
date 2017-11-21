using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// To be inherited by rigidbodies which need to react to force modifiers
/// </summary>
public abstract class RigidBodyBehavior : MonoBehaviour
{
    /// <summary>
    /// Contains the list of force modifiers which should be applied to the velocity of the <see cref="Rigidbody2D"/>
    /// </summary>
    public List<ForceModifier> ForceModifiers { get; private set; }

    private Rigidbody2D m_rig;
    private Vector2 m_totalForce;
    private Vector2 m_maxSpeed;

    /// <summary>
    /// Use this for initialization
    /// </summary>
    private void Start()
    {
        ForceModifiers = new List<ForceModifier>();
        m_rig = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// FixedUpdate should be used instead of Update when dealing with Rigidbody
    /// </summary>
    private void FixedUpdate()
    {
        if (m_rig != null && !m_rig.isKinematic)
            ApplyForce();
    }

    /// <summary>
    /// Creates a new <see cref="ForceModifier"/> and adds it to the <see cref="ForceModifiers"/> list
    /// Only 1 <see cref="ForceModifier"/> can be applied per owner.
    /// If the owner already exists, the <see cref="ForceModifier.Force"/> and <see cref="ForceModifier.MaxSpeed"/> values will be updated
    /// </summary>
    /// <param name="owner">The object which is applying the force</param>
    /// <param name="force">The force to apply</param>
    /// <param name="maxSpeed">The maximum speed the force can reach</param>
    public void AddConstantForce(GameObject owner, Vector2 force, Vector2 maxSpeed)
    {
        // Create the force modifier
        ForceModifier forceMod = new ForceModifier(owner, force, maxSpeed);
        // Assume the force modifier is unique until we check
        bool isUnique = true;
        foreach (ForceModifier m in ForceModifiers)
        {
            // Only 1 force modifier can exist per owner
            if (m.Owner == owner)
            {
                isUnique = false;
                // If the force modifier is not unique, update its MaxSpeed and Force parameters as they may have changed since they were set
                m.MaxSpeed = maxSpeed;
                m.Force = force;
                break;
            }
        }

        // Add a new force modifier if it is unique
        if (isUnique)
            ForceModifiers.Add(forceMod);
    }

    /// <summary>
    /// Removes a constant force from <see cref="ForceModifiers"/>
    /// </summary>
    /// <param name="owner">The owner of the <see cref="ForceModifier"/></param>
    public void RemoveConstantForce(GameObject owner)
    {
        ForceModifier modToRemove = null;
        foreach (ForceModifier m in ForceModifiers)
        {
            if (m.Owner == owner)
            {
                modToRemove = m;
                break;
            }
        }

        if (modToRemove != null)
            ForceModifiers.Remove(modToRemove);
    }

    /// <summary>
    /// Calculates the total force to apply to the <see cref="Rigidbody2D"/>, calculates the maximum speed and clamps the <see cref="Rigidbody2D"/>
    /// </summary>
    private void ApplyForce()
    {
        // Initialize m_totalForce to gravity * drag * gravity scale, as gravity also needs to be factored into the total force and speed calculations
        m_totalForce = Physics2D.gravity * m_rig.drag * m_rig.gravityScale;
        // Initialize max speed
        m_maxSpeed = Vector2.zero;

        // Add the force and max speeds for all active force modifiers
        foreach (ForceModifier mod in ForceModifiers)
        {
            m_totalForce += mod.Force;
            m_maxSpeed += new Vector2(Mathf.Abs(mod.MaxSpeed.x), Mathf.Abs(mod.MaxSpeed.y));
        }

        // Apply the total force to the rigidbody
        m_rig.AddForce(m_totalForce);

        // The minimum speed should use the inverse of the absolute value of m_maxSpeed's X axis to permit movement in both directions
        // Allow infinite downward speed for gravity (except for Muckle)
        Vector2 minSpeed = new Vector2(-Mathf.Abs(m_maxSpeed.x),
            (tag == Globals.TAG_MUCKLE ? -Mathf.Abs(m_maxSpeed.y) : Mathf.NegativeInfinity));
        // The maximum speed should use the absolute values of m_maxSpeed's axes
        Vector2 maxSpeed = new Vector2(Mathf.Abs(m_maxSpeed.x), Mathf.Abs(m_maxSpeed.y));
        // Clamp the RigidBody's velocity if m_maxSpeed is greater than zero. 
        // If m_maxSpeed is zero, no force modifiers are applied and we should just let gravity do its thing without interfering
        m_rig.velocity = m_maxSpeed != Vector2.zero ? MathHelper.Clamp(m_rig.velocity, minSpeed, maxSpeed) : m_rig.velocity;
    }
}

