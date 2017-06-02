using System;
using UnityEngine;

/// <summary>
/// Basic controller to enable or disable a particle effect, implements <see cref="IInteractiveItem"/> and inherits <see cref="MonoBehaviour"/>
/// </summary>
public class ParticleController : MonoBehaviour, IInteractiveItem
{
    // The particle system to control
    private ParticleSystem m_particles;

    /// <summary>
    /// Used for initialization
    /// </summary>
    void Start()
    {
        m_particles = GetComponent<ParticleSystem>();
    }

    /// <summary>
    /// Invokes a method after the delay specified
    /// </summary>
    /// <param name="methodName">The method name to invoke</param>
    /// <param name="delay">The delay before invoking</param>
    public void StartMethod(string methodName, float delay)
    {
        Invoke(methodName, delay);
    }

    /// <summary>
    /// Stops the particle system
    /// </summary>
    public void Release()
    {
        if (m_particles != null)
            m_particles.Stop();
    }

    /// <summary>
    /// Starts the particle system
    /// </summary>
    public void Trigger()
    {
        if (m_particles != null)
            m_particles.Play();
    }
}
