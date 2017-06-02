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

	LayerMask ConveyorLayerMask;

	// Use this for initialization
	void Start() 
	{ }
	
	// Update is called once per frame
	void Update() 
	{
		MoveConveyor();
		SetSpeed();
	}

	/// <summary>
	/// Adds additional force or reduced force to the player
	/// depending on the speed and direction of the conveyor
	/// </summary>
	private void MovePlayer(Rigidbody2D rig)
	{
		if (IsMoving) 
			rig.AddForce(Vector2.right * Speed * ((IsReverse) ? -1 : 1));
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
	/// Raises the CollisionStay2D event, moves collided object.
	/// </summary>
	/// <param name="col">The 2D object collided with</param>
	private void OnCollisionStay2D(Collision2D col)
	{
		Rigidbody2D rig = col.gameObject.GetComponent<Rigidbody2D>();
		if (rig != null)
			MovePlayer(rig);
	}
}