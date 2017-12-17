﻿/// <summary>
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
}