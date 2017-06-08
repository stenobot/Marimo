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

	// The conveyor's animator
	private Animator m_animator;

	// A list of game objects that are on the conveyor
	private List<GameObject> m_objects;

	// Holds a reference to the constant force 2D component
	private ConstantForce2D m_constantForce;

	// Use this for initialization
	void Start() 
	{ 
		m_objects = new List<GameObject>();
		m_animator = GetComponent<Animator>();
		m_constantForce = null;
	}
	
	// Update is called once per frame
	void Update() 
	{
		// Set the speed of the conveyor animation
		SetSpeed();

		// Set conveyor movement animation
		MoveConveyor();

		// start or stop movement of all objects on conveyor
		if (IsMoving)
			StartObjectsMovement();
		else
			StopObjectsMovement();
	}

	/// <summary>
	/// Adds constant force to each of the colliding objects
	/// based on the speed and direction of the conveyor.
	/// </summary>
	private void StartObjectsMovement()
	{
		foreach (GameObject obj in m_objects) 
		{
			// check if constant force component already exists
			// so we only add it once
			if (obj != null && !obj.GetComponent<ConstantForce2D>()) 
			{
				// add the component
				m_constantForce = obj.AddComponent<ConstantForce2D>();
				// set the component's speed and direction
				m_constantForce.force = (IsReverse ? Vector2.left : Vector2.right) * Speed;
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
	private void MoveConveyor()
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
	private void SetSpeed()
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
}