﻿using System;
using UnityEngine;

/// <summary>
/// Controls the elevators in the game
/// </summary>
public class Elevator : MonoBehaviour
{
    // The start height of the elevator
    public float StartHeight;
    // The stop height of the elevator
    public float StopHeight;
    // The move speed
    public float Speed = 3f;
    // Tracks if the elevator is in transit
    public bool IsMoving { get; private set; }
    // Tracks if elevator is at the top position
    public bool IsAtTop { get; private set; }
    // Tracks if elevator is at the bottom position
    public bool IsAtBottom { get; private set; }

    // Holds reference to rigid body
    private Rigidbody2D m_rig;
    // Holds reference to player's rigidbody
    private Rigidbody2D m_playerRig;
    // The target height to move to
    private float m_targetHeight;
    // Tracks the movement direction
    private Vector2 m_moveDirection = Vector2.zero;
    // Will be true if the start position is lower than the stop position
    private bool m_startPositionIsBottom = false;

    // Use this for initialization
    void Start()
    {
        // Set the rigidbody
        m_rig = GetComponent<Rigidbody2D>();
        // Make the rigidbody kinematic so it ignores physics
        m_rig.isKinematic = true;
        // Set player rigidbody
        m_playerRig = GameObject.FindGameObjectWithTag(Globals.TAG_PLAYER).GetComponent<Rigidbody2D>();
        // Ensure the elevator is at the start height
        transform.localPosition = new Vector2(transform.localPosition.x, StartHeight);
        // Work out which position is on the bottom
        m_startPositionIsBottom = StartHeight > StopHeight ? false : true;
        // Check if player is at top or bottom position
        if (m_startPositionIsBottom)
        {
            IsAtBottom = true;
            IsAtTop = false;
        }
        else
        {
            IsAtBottom = false;
            IsAtTop = true;
        }

        // Set the target height
        m_targetHeight = StopHeight;
    }

    /// <summary>
    /// FixedUpdate should be used instead of Update when dealing with Rigidbody
    /// </summary>
    void FixedUpdate()
    {
        if (IsMoving)
        {
            if (ElevatorCanMove())
                Move(m_moveDirection);
            else
                Stop();
        }
    }

    /// <summary>
    /// Moves the elevator in the provided direction
    /// </summary>
    /// <param name="direction">The direction to move</param>
    public void Move(Vector2 direction)
    {
        if ((IsAtTop && direction == Vector2.up) || (IsAtBottom && direction == Vector2.down))
            return;

        if (!IsMoving)
        {
            // Make the player a child of the elevator
            GameObject.FindGameObjectWithTag(Globals.TAG_PLAYER).transform.parent = transform;
            // Make the player kinematic so physics doesn't affect it, also disabled RigidBodyBehavior
            m_playerRig.isKinematic = true;
            IsMoving = true;
            m_targetHeight = m_targetHeight == StartHeight ? StopHeight : StartHeight;
            m_moveDirection = direction;
            m_rig.velocity = direction * Speed;
            IsAtBottom = false;
            IsAtTop = false;
        }
    }

    /// <summary>
    /// Checks if the elevator can move
    /// </summary>
    /// <returns></returns>
    private bool ElevatorCanMove()
    {
        bool result = false;
        result = (m_moveDirection == Vector2.up && m_startPositionIsBottom) ? (transform.localPosition.y < StopHeight) : result;
        result = (m_moveDirection == Vector2.up && !m_startPositionIsBottom) ? (transform.localPosition.y < StartHeight) : result;
        result = (m_moveDirection == Vector2.down && m_startPositionIsBottom) ? (transform.localPosition.y > StartHeight) : result;
        result = (m_moveDirection == Vector2.down && !m_startPositionIsBottom) ? (transform.localPosition.y > StopHeight) : result;
        return result;
    }

    /// <summary>
    /// Stops the elevator
    /// </summary>
    private void Stop()
    {
        float finalHeight = m_targetHeight == StartHeight ? StopHeight : StartHeight;
        IsMoving = false;
        m_playerRig.velocity = Vector2.zero;
        m_playerRig.isKinematic = false;
        m_rig.velocity = Vector2.zero;

        // Snap the local position exactly to avoid any collision problems
        transform.localPosition = new Vector2(transform.localPosition.x, finalHeight);

        IsAtBottom = (finalHeight == StartHeight && m_startPositionIsBottom) ? true :
            (finalHeight == StopHeight && !m_startPositionIsBottom) ? true : false;
        IsAtTop = !IsAtBottom;

        GameObject player = GameObject.FindGameObjectWithTag(Globals.TAG_PLAYER);
        player.transform.parent = null;
        player.GetComponent<RobotController>().IsInTransit = false;

    }
}
