/// <summary>
/// Global enums that are exposed to all classes
/// </summary>
public class Enums
{
    /// <summary>
    /// Direction states
    /// </summary>
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right,
        Any,
        None
    }

    public enum ToolIcon
    {
        None,
        Wrench,
        UpDownArrows
    }

    public enum InteractionIcon
    {
        None,
        Switch,
        Elevator
    }

	public enum ConveyorInteraction
	{
		Direction,
		Speed,
		Position,
		Moving,
		None
	}
}