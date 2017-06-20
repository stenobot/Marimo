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

    /// <summary>
    /// Used to trigger on a staggered rhythm as a value decreases, 
    /// based on a starting value.
    /// </summary>
    /// <param name="currValue">The current decreasing value</param>
    /// <param name="startValue">The starting value</param>
    /// <returns></returns>
    public static bool StaggerTrigger(float currValue, float startValue)
    {
        float stopOne = ((startValue / 3) * 2);
        float stopTwo = startValue / 3;
        float stopThree = startValue / 10;

        if ((currValue < stopOne && currValue > (stopOne - 0.05f)) ||
            (currValue < stopTwo && currValue > (stopTwo - 0.05f)) ||
            (currValue < stopThree && currValue > (stopThree - 0.05f)))
            return true;
        else
            return false;
    }
	
    /// <summary>
    /// Snaps the vector's coordinates to the closest pixel
    /// </summary>
    /// <param name="pos">The <see cref="Vector2"/> to snap</param>
    /// <returns>A <see cref="Vector2"/> snapped to the closest pixel</returns>
    public static Vector2 SnapToPixels(Vector2 pos)
    {
        return new Vector2(Mathf.RoundToInt(pos.x * Globals.PIXELS_PER_UNIT), Mathf.Round(pos.y * Globals.PIXELS_PER_UNIT)) / Globals.PIXELS_PER_UNIT;
    }

    /// <summary>
    /// Returns the point at the given distance between two <see cref="Vector2"/> positions
    /// </summary>
    /// <param name="origin">The start position</param>
    /// <param name="target">The end position</param>
    /// <param name="distance">The distance between the positions</param>
    /// <returns>The point at the given distance between the origin and target</returns>
    public static Vector2 LerpByDistance(Vector2 origin, Vector2 target, float distance)
    {
        Vector2 point = distance * (target - origin).normalized + origin;
        return point;
    }
}