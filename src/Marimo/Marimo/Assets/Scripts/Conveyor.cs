using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conveyor : MonoBehaviour {
	// The speed of the conveyor belt
	public float Speed = 3f;
	// Tracks whether the conveyor belt is moving
	public bool IsMoving { get; private set; }
	// Tracks the conveyor belt's direction
	public bool IsReverse { get; private set; }

	// Holds reference to rigid body
	private Rigidbody2D m_rig;
	// Holds reference to player's rigidbody
	private Rigidbody2D m_playerRig;

	// Use this for initialization
	void Start () 
	{
		// Set the rigidbody
		m_rig = GetComponent<Rigidbody2D>();
		// Set player rigidbody
		m_playerRig = GameObject.FindGameObjectWithTag(Globals.TAG_PLAYER).GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		Move(IsReverse);
	}

	/// <summary>
	/// Moves the elevator in the provided direction
	/// </summary>
	/// <param name="direction">The direction to move</param>
	public void Move(bool isReverse)
	{
		// Make the player a child of the conveyor
		GameObject.FindGameObjectWithTag(Globals.TAG_PLAYER).transform.parent = transform;

		if (!IsReverse)
			m_rig.velocity = Vector2.right * Speed;
		else
			m_rig.velocity = Vector2.left * Speed;
	}
}
