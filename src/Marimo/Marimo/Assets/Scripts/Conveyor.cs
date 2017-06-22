using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conveyor : MonoBehaviour, IInteractiveItem
{
	#region Public variables

	// Tracks the speed of the conveyor belt
	public float Speed {get; private set;}

	// Tracks the conveyor belt's direction
	public bool IsReverse;
	// Tracks whether the conveyor belt is moving
	public bool IsMoving;
	// Tracks how far the conveyor can move on horizontal axis
	public float PositionOffset = 0f;
	// The way that the conveyor can be interacted with
	public Enums.ConveyorInteraction Interaction = Enums.ConveyorInteraction.None;

	#endregion

	#region Private variables

	// constant values for belt speed
	private const float SPEED_HIGH = 25f;
	private const float SPEED_SLOW = 15f;

	// The conveyor's animator
	private Animator m_animator;

	// A list of game objects that are on the conveyor
	private List<GameObject> m_objects;

	// Holds a reference to the constant force 2D component
	private ConstantForce2D m_constantForce;

	// Conveyor position tracking
	private bool m_moveToOffset;
	private Vector2 m_startingPosition;
	private Vector2 m_offsetPosition;
	private float m_currOffset;

	// The previous state of IsReverse, to track if the direction changed
	private bool m_previousIsReverse;

	#endregion

	// Runs once when object is initialized
	void Start() 
	{ 
		m_objects = new List<GameObject>();
		m_animator = GetComponent<Animator>();
		m_startingPosition = transform.position;
		m_offsetPosition = new Vector2((transform.position.x + PositionOffset), transform.position.y);
		m_currOffset = 0;
		m_constantForce = null;
		m_moveToOffset = false;
		m_previousIsReverse = IsReverse;

		Speed = SPEED_SLOW;
	}
	
	// Called once per frame
	void Update()
	{
		// Set the speed of the conveyor animation
		SetBeltSpeed();

		// Set conveyor movement animation
		MoveBelt();

		// start or stop movement of all objects on conveyor
		if (IsMoving)
			StartObjectsMovement();
		else
			StopObjectsMovement();

		// check and move the conveyor's position
		MoveConveyorPosition();
	}

	/// <summary>
	/// Checks if the conveyor should move, and moves 
	/// the conveyor's X position based on the OffsetX
	/// </summary>
	private void MoveConveyorPosition()
	{
		// don't do anything if the offset is 0
		if (PositionOffset == 0)
			return;

		// if the conveyor is being instructed to move position
		if (m_moveToOffset) 
		{
			// increment current x offset if it's less than 1
			if (m_currOffset < 1)
				m_currOffset += 0.005f;

			// move toward the offset position
			transform.position = Vector2.Lerp(m_startingPosition, m_offsetPosition, m_currOffset);
		} else if (m_currOffset > 0) 
		{
			// move conveyor back toward starting position
			m_currOffset -= 0.005f;
			transform.position = Vector2.Lerp(m_startingPosition, m_offsetPosition, m_currOffset);
		}
	}

	/// <summary>
	/// Adds constant force to each of the colliding objects
	/// based on the speed and direction of the conveyor.
	/// </summary>
	private void StartObjectsMovement()
	{
		// check if the direction has changed since the last frame
		if (IsReverse != m_previousIsReverse) 
		{
			// stop all movement and return
			m_previousIsReverse = IsReverse;
			StopObjectsMovement();
			return;
		}

		// iterate through objects on conveyor
		foreach (GameObject obj in m_objects) 
		{
			if (obj != null) 
			{
				// only clamp for max speed if object is not the player
				if (!obj.CompareTag(Globals.TAG_PLAYER)) 
				{
					// clamp object velocity so it doesn't continue to accelerate
					Rigidbody2D rig = obj.GetComponent<Rigidbody2D>();
					rig.velocity =
						MathHelper.Clamp(
							rig.velocity,
							new Vector2(-(Speed / 8), Mathf.NegativeInfinity),
							new Vector2((Speed / 8), Mathf.Infinity)
						);
				}

				// check if constant force component already exists
				// so we only add it once
				if (obj.GetComponent<ConstantForce2D>() == null) 
				{
					// add the component
					m_constantForce = obj.AddComponent<ConstantForce2D>();

					// set the component's speed and direction
					m_constantForce.relativeForce = (IsReverse ? Vector2.left : Vector2.right) * Speed;
				}
			}
		}
	}

	/// <summary>
	/// Stops all objects on conveyor
	/// by destroying their constant force components
	/// </summary>
	private void StopObjectsMovement()
	{
		foreach (GameObject obj in m_objects) 
		{
			if (obj != null)
				Destroy(obj.GetComponent<ConstantForce2D>());
		}
	}

	/// <summary>
	/// Changes the conveyor animation depending on direction
	/// </summary>
	private void MoveBelt()
	{ 
		if (IsReverse)
			m_animator.Play(Globals.ANIMSTATE_CONVEYOR_LEFT);
		else
			m_animator.Play(Globals.ANIMSTATE_CONVEYOR_RIGHT);
	}

	/// <summary>
	/// Sets the speed of the conveyor animation using a speed multiplier.
	/// Zero means the conveyor is not moving.
	/// </summary>
	private void SetBeltSpeed()
	{
		m_animator.SetFloat(Globals.ANIM_PARAM_SPEED, (IsMoving ? (Speed / 5) : 0));
	}

	/// <summary>
	/// Raises event when collision first occurs on an object.
	/// </summary>
	/// <param name="col">The 2D object collided with</param>
	private void OnCollisionEnter2D(Collision2D col)
	{
		if (col.gameObject == null)
			return;

		// add object to list if it's not already in there
		if (!m_objects.Contains(col.gameObject)) 
			m_objects.Add(col.gameObject);
	}

	/// <summary>
	/// Raises event when collision of an object is exited.
	/// </summary>
	/// <param name="col">Col.</param>
	private void OnCollisionExit2D(Collision2D col) 
	{
		if (col.gameObject == null)
			return;

		// destroy object's constant force component
		if (col.gameObject.GetComponent<ConstantForce2D>())
			Destroy(col.gameObject.GetComponent<ConstantForce2D>());

		// remove object from the list
		if (m_objects.Contains(col.gameObject)) 
			m_objects.Remove(col.gameObject);			 
	}

	#region Interactive Item methods

	public void Trigger()
	{
		if (Interaction == Enums.ConveyorInteraction.Direction)
			IsReverse = true;
		else if (Interaction == Enums.ConveyorInteraction.Speed)
			Speed = SPEED_HIGH;
		else if (Interaction == Enums.ConveyorInteraction.Moving)
			IsMoving = true;
		else if (Interaction == Enums.ConveyorInteraction.Position)
			m_moveToOffset = true;
	}

	public void Release()
	{
		if (Interaction == Enums.ConveyorInteraction.Direction)
			IsReverse = false;
		else if (Interaction == Enums.ConveyorInteraction.Speed)
			Speed = SPEED_SLOW;
		else if (Interaction == Enums.ConveyorInteraction.Moving)
			IsMoving = false;
		else if (Interaction == Enums.ConveyorInteraction.Position)
			m_moveToOffset = false;
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

	#endregion
}