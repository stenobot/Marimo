using UnityEngine;

/// <summary>
/// Base interface for a tool
/// </summary>
public interface IToolBase
{
    /// <summary>
    /// Returns the tool's <see cref="GameObject"/>
    /// </summary>
    GameObject GameObject { get; }

    /// <summary>
    /// Enables the tool
    /// </summary>
    void Enable();

    /// <summary>
    /// Disables the tool
    /// </summary>
    void Disable();
}

