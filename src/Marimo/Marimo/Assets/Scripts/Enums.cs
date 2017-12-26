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

    /// <summary>
    /// Icons for tools which appears in a <see cref="HintTrigger"/>
    /// </summary>
    public enum ToolIcon
    {
        None,
        Wrench,
        UpDownArrows
    }

    /// <summary>
    /// Icons for interactions which appears in a <see cref="HintTrigger"/>
    /// </summary>
    public enum InteractionIcon
    {
        None,
        Switch,
        Elevator
    }

    /// <summary>
    /// Types of interactions which can be taken on a <see cref="Conveyor"/> by a <see cref="InteractiveItemController"/>
    /// </summary>
	public enum ConveyorInteraction
	{
		Direction,
		Speed,
		Position,
		Moving,
		None
	}

    /// <summary>
    /// Contains the various types of playable character
    /// </summary>
    public enum PlayableCharacter
    {
        Robot,
        Muckle
    }

	/// <summary>
	/// Pipe tile sprites
	/// </summary>
	public enum PipeTileSprite
	{
		CornerBottomLeft,
		CornerBottomRight,
		CornerTopLeft,
		CornerTopRight,
		Horizontal,
		OpenBottom,
		OpenLeft,
		OpenRight,
		OpenTop,
		CapBottom,
		CapLeft,
		CapRight,
		CapTop,
		Vertical,
		OpenAll
	}

    public enum PipeType
    {
        None = -1,
        CornerBottomLeft = 0,
        CornerBottomRight = 1,
        CornerTopLeft = 2,
        CornerTopRight = 3,
        Horizontal = 4,
        OpenBottom = 5,
        OpenLeft = 6,
        OpenRight = 7,
        OpenTop = 8,
        VentBottom = 9,
        VentLeft = 10,
        VentRight = 11,
        VentTop = 12,
        Vertical = 13,
        Water = 14
    }
}