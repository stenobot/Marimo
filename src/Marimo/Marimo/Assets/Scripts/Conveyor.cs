using System;
using System.Collections.Generic;
using UnityEngine;

public class Conveyor : MonoBehaviour, IInteractiveItem
{
    #region Public variables

    /// <summary>
    /// Tracks the speed of the conveyor belt
    /// </summary>
    public float Speed { get; private set; }

    /// <summary>
    /// The <see cref="Speed"/> property will be multiplied by this value to set the Animator speed
    /// </summary>
    public float AnimSyncRatio = 1.36f;

    /// <summary>
    /// Tracks the conveyor belt's direction
    /// </summary>
    public bool IsReverse;

    /// <summary>
    /// Tracks whether the conveyor belt is moving
    /// </summary>
    public bool IsMoving;

    /// <summary>
    /// Tracks how far the conveyor can move on horizontal axis
    /// </summary>
    public float PositionOffset = 0f;

    /// <summary>
    /// The way that the conveyor can be interacted with
    /// </summary>
	public Enums.ConveyorInteraction Interaction = Enums.ConveyorInteraction.None;

    #endregion

    #region Private variables

    // constant values for belt speed
    private const float SPEED_HIGH = 2.8f;
    private const float SPEED_SLOW = 2.2f;

    // The conveyor's animator
    private Animator m_animator;
    // The surface effector
    private SurfaceEffector2D m_effector;

    // Conveyor position tracking
    private bool m_moveToOffset;
    private Vector2 m_startingPosition;
    private Vector2 m_offsetPosition;
    private float m_currOffset;

    #endregion

    /// <summary>
    /// Runs once when object is initialized
    /// </summary>
    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_effector = GetComponent<SurfaceEffector2D>();
        m_startingPosition = transform.position;
        m_offsetPosition = new Vector2((transform.position.x + PositionOffset), transform.position.y);
        m_currOffset = 0;
        m_moveToOffset = false;
        // TODO: We probably shouldn't hardcode the initial speed
        Speed = SPEED_SLOW;
        SetBeltSpeed();
    }

       
    /// <summary>
    /// Checks if the conveyor should move, and moves 
    /// the conveyor's X position based on the OffsetX
    /// </summary>
    private void MoveConveyorPosition()
    {
        // don't do anything if the offset is 0
        if (PositionOffset == 0)
        {
            CancelInvoke("MoveConveyorPosition");
            return;
        }

        // if the conveyor is being instructed to move position
        if (m_moveToOffset)
        {
            // increment current x offset if it's less than 1
            if (m_currOffset < 1)
                m_currOffset += 0.005f;

            // move toward the offset position
            transform.position = Vector2.Lerp(m_startingPosition, m_offsetPosition, m_currOffset);
        }
        else if (m_currOffset > 0)
        {
            // move conveyor back toward starting position
            m_currOffset -= 0.005f;
            transform.position = Vector2.Lerp(m_startingPosition, m_offsetPosition, m_currOffset);
        }
    }

    
    /// <summary>
    /// Sets the speed of the conveyor
    /// Zero means the conveyor is not moving.
    /// </summary>
    private void SetBeltSpeed()
    {
        m_animator.Play(IsReverse ? Globals.ANIMSTATE_CONVEYOR_LEFT : Globals.ANIMSTATE_CONVEYOR_RIGHT);
        m_animator.SetFloat(Globals.ANIM_PARAM_SPEED, (IsMoving ? (Speed) : 0) * AnimSyncRatio);
        m_effector.speed = IsMoving ? (IsReverse ? -Speed : Speed) : 0;
    }
    

    #region Interactive Item methods

    public void Trigger()
    {
        switch (Interaction)
        {
            case Enums.ConveyorInteraction.Direction:
                IsReverse = true;
                break;
            case Enums.ConveyorInteraction.Moving:
                IsMoving = true;
                break;
            case Enums.ConveyorInteraction.Position:
                m_moveToOffset = true;
                InvokeRepeating("MoveConveyorPosition", 0, 0.01f);
                break;
            case Enums.ConveyorInteraction.Speed:
                Speed = SPEED_HIGH;
                break;
            default:
                return;
        }

        SetBeltSpeed();
    }

    public void Release()
    {
        switch (Interaction)
        {
            case Enums.ConveyorInteraction.Direction:
                IsReverse = false;
                break;
            case Enums.ConveyorInteraction.Moving:
                IsMoving = false;
                break;
            case Enums.ConveyorInteraction.Position:
                m_moveToOffset = false;
                break;
            case Enums.ConveyorInteraction.Speed:
                Speed = SPEED_SLOW;
                break;
            default:
                return;
        }

        SetBeltSpeed();
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