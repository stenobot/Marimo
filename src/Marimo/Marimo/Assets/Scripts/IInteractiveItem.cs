public interface IInteractiveItem
{
    /// <summary>
    /// The trigger method
    /// </summary>
    void Trigger();

    /// <summary>
    /// The release method
    /// </summary>
    void Release();

    /// <summary>
    /// Starts the method name provided after the delay specified
    /// </summary>
    /// <param name="methodName">The name of the method to invoke</param>
    /// <param name="delay">The delay before invoking the method</param>
    void StartMethod(string methodName, float delay);
}
