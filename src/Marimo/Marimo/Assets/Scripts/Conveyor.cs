using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conveyor : MonoBehaviour 
{
	// The speed of the conveyor belt
	public float Speed = 15f;
	// Tracks the conveyor belt's direction
	public bool IsReverse;
	// Tracks whether the conveyor belt is moving
	public bool IsMoving;

	// Holds reference for conveyor's animator
	public Animator Animator_Conveyor;

	// Tracks whether constant force should be applied to an object
	private bool m_objectCanMove;
	// Holds a reference to the game object that is on the conveyor
	private GameObject m_object;
	// Holds a reference to the constant force 2D component
	private ConstantForce2D m_constantForce;

	LayerMask ConveyorLayerMask;

	// Use this for initialization
	void Start() 
	{ 
		m_object = null;
		m_constantForce = null;
		m_objectCanMove = false;
	}
	
	// Update is called once per frame
	void Update() 
	{
		
		// Set the speed of the conveyor animation
		SetSpeed();

		// Set conveyor movement animation
		MoveConveyor();

		// Start or stop object movement using constant force
		if (m_objectCanMove && IsMoving)
			StartObjectMovement();
		else if (!m_objectCanMove || !IsMoving)
			StopObjectMovement();
	}

	/// <summary>
	/// Adds constant force to the colliding object
	/// based on the speed and direction of the conveyor.
	/// </summary>
	private void StartObjectMovement()
	{

			// check if constant force component already exists,
			// so we only add it once
			if (!m_object.GetComponent<ConstantForce2D>()) 
			{
				// add component
				m_constantForce = m_object.AddComponent(typeof(ConstantForce2D)) as ConstantForce2D;
				// set speed and direction
				m_constantForce.force = (IsReverse ? Vector2.left : Vector2.right) * Speed;
			}

	}

	/// <summary>
	/// Stops the object movement on conveyor
	/// by destroying constant force component
	/// </summary>
	private void StopObjectMovement()
	{
		Destroy (m_object.GetComponent<ConstantForce2D>());
	}

	/// <summary>
	/// Changes the conveyor animation depending on direction
	/// </summary>
	private void MoveConveyor()
	{ 
		if (IsReverse)
			Animator_Conveyor.Play(Globals.ANIMSTATE_CONVEYOR_LEFT);
		else
			Animator_Conveyor.Play(Globals.ANIMSTATE_CONVEYOR_RIGHT);
	}

	/// <summary>
	/// Sets the speed of the conveyor animation using a speed multiplier.
	/// Zero means the conveyor is not moving.
	/// </summary>
	private void SetSpeed()
	{
		Animator_Conveyor.SetFloat(Globals.ANIM_PARAM_SPEED, (IsMoving ? (Speed / 5) : 0));
	}

	/// <summary>
	/// Raises event when collision first occurs to 
	/// begin movement of collided object.
	/// </summary>
	/// <param name="col">The 2D object collided with</param>
	private void OnCollisionEnter2D(Collision2D col)
	{
		m_object = col.gameObject;

		if (m_object != null)
			m_objectCanMove = true;
	}

	/// <summary>
	/// Raises event when collision is exited to destroy 
	/// the game object's constant force component
	/// </summary>
	/// <param name="col">Col.</param>
	private void OnCollisionExit2D(Collision2D col) 
	{
		m_objectCanMove = false;
	}
}