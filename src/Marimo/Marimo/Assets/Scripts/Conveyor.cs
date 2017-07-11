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
    /// The belt speed <see cref="Speed"/> will be multiplied by this value to produce the maximum speed for connected objects
    /// </summary>
    public float SpeedMultiplier;
    
    /// <summary>
    /// Tracks the constant force the conveyor belt applies to other objects
    /// </summary>
    public float ConstantForce;

    /// <summary>
    /// The impulse force applied to newly connected objects to get them up to max speed quickly
    /// Means we can apply less constant force so the player can try to work against the conveyor's direction
    /// </summary>
    public float ImpulseForce;

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
    private const float SPEED_HIGH = 25f;
    private const float SPEED_SLOW = 15f;

    // The conveyor's animator
    private Animator m_animator;

    // A list of RigidBodyBehaviors that are currently on the conveyor
    private List<RigidBodyBehavior> m_rigBehaviors;

    // Conveyor position tracking
    private bool m_moveToOffset;
    private Vector2 m_startingPosition;
    private Vector2 m_offsetPosition;
    private float m_currOffset;

    // The previous state of IsReverse, to track if the direction changed
    private bool m_previousIsReverse;

    #endregion

    /// <summary>
    /// Runs once when object is initialized
    /// </summary>
    void Start()
    {
        m_rigBehaviors = new List<RigidBodyBehavior>();
        m_animator = GetComponent<Animator>();
        m_startingPosition = transform.position;
        m_offsetPosition = new Vector2((transform.position.x + PositionOffset), transform.position.y);
        m_currOffset = 0;
        m_moveToOffset = false;
        m_previousIsReverse = IsReverse;

        Speed = SPEED_SLOW;
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
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
        }
        else if (m_currOffset > 0)
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

        // Update the force and max speed applied to all connected objects
        foreach (RigidBodyBehavior rig in m_rigBehaviors)
            rig.AddConstantForce(gameObject,
                (IsReverse ? Vector2.left : Vector2.right) * ConstantForce,
                new Vector2(Speed * SpeedMultiplier * (IsReverse ? -1f : 1f), 0));
    }

    /// <summary>
    /// Stops all objects on conveyor
    /// by destroying their constant force components
    /// </summary>
    private void StopObjectsMovement()
    {
        // Zero out the constant force being applied to all connected objects
        foreach (RigidBodyBehavior rig in m_rigBehaviors)
            rig.AddConstantForce(gameObject, Vector2.zero, Vector2.zero);
    }

    /// <summary>
    /// Changes the conveyor animation depending on direction
    /// </summary>
    private void MoveBelt()
    {
        m_animator.Play(IsReverse ? Globals.ANIMSTATE_CONVEYOR_LEFT : Globals.ANIMSTATE_CONVEYOR_RIGHT);
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
    /// Raises event when collision first occurs with an object.
    /// </summary>
    /// <param name="col">The 2D collider which was collided with</param>
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject == null)
            return;

        RigidBodyBehavior rig = col.gameObject.GetComponent<RigidBodyBehavior>();

        if (rig != null)
        {
            Vector2 speed = new Vector2(Speed * SpeedMultiplier, 0);
            Vector2 force = (IsReverse ? Vector2.left : Vector2.right) * ConstantForce;
            // Apply constant force to the RigidBodyBehavior
            rig.AddConstantForce(gameObject, force, speed);
            // Add impulse force to the RigidBody to get it up to speed quickly
            rig.GetComponent<Rigidbody2D>().AddForce(new Vector2(IsReverse ? -ImpulseForce : ImpulseForce, 0), ForceMode2D.Impulse);

            // Start tracking the object
            m_rigBehaviors.Add(rig);
        }
    }

    /// <summary>
    /// Raises event when collision of an object is exited.
    /// </summary>
    /// <param name="col">The 2D collider which is exiting the collider</param>
    private void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject == null)
            return;

        RigidBodyBehavior rigBehavior = col.gameObject.GetComponent<RigidBodyBehavior>();

        if (rigBehavior != null)
        {
            // Remove the constant force applied by this conveyor
            rigBehavior.RemoveConstantForce(gameObject);
            // Stop tracking the object
            m_rigBehaviors.Remove(rigBehavior);
        }
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