using UnityEngine;

/// <summary>
/// Contains common math helper functions
/// </summary>
static class MathHelper
{
    /// <summary>
    /// Clamps a <see cref="Vector2"/> so its X and Y coordinates are within a set of minimum and maximum values
    /// </summary>
    /// <param name="pos">The position to clamp</param>
    /// <param name="minPos">The minimum allowable X and Y coordinates</param>
    /// <param name="maxPos">The maximum allowable X and Y coordinates</param>
    /// <returns>A clamped <see cref="Vector2"/></returns>
    public static Vector2 Clamp(Vector2 pos, Vector2 minPos, Vector2 maxPos)
    {
        Vector2 newPos = pos;
        newPos.x = newPos.x < minPos.x ? minPos.x : newPos.x;
        newPos.x = newPos.x > maxPos.x ? maxPos.x : newPos.x;
        newPos.y = newPos.y < minPos.y ? minPos.y : newPos.y;
        newPos.y = newPos.y > maxPos.y ? maxPos.y : newPos.y;
        return newPos;
    }
}