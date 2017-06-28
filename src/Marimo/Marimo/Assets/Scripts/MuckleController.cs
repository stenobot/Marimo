using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Character controller for Muckle. Inherits <see cref="RigidBodyBehavior"/> and implements <see cref="IRigidBodyBehavior"/>
/// </summary>
public class MuckleController : RigidBodyBehavior
{
    #region Public editor varaibles

    /// <summary>
    /// The maximum movement speed for the character
    /// </summary>
    public float MaxSpeed = 5f;

    /// <summary>
    /// The force to apply for movement. Higher values mean faster acceleration and more power for climbing slopes.
    /// </summary>
    public float MoveForce = 5f;

    /// <summary>
    /// The maximum speed increase for the character's boost function
    /// </summary>
    public float BoostSpeed = 3f;

    /// <summary>
    /// The force to apply to the character's boost
    /// </summary>
    public float BoostForce = 1f;

    /// <summary>
    /// The duration of the boost function
    /// </summary>
    public float BoostDuration = 0.5f;

    #endregion

    #region Private component references

    private Animator m_anim;
    private AudioSource m_audio;
    private Rigidbody2D m_rigidBody;

    #endregion

    #region Private variables

    // Determines if the character can be user-controlled
    private bool m_canControl = false;
    // Tracks if the player's boost mode is active
    private bool m_boostMode = false;
    // Tracks the current boost speed to apply (to permit deceleration)
    private float m_boostSpeed;

    #endregion

    /// <summary>
    /// Use this for initialization
    /// </summary>
    protected override void Start()
    {
        base.Start();
        m_anim = GetComponentInChildren<Animator>();
        m_audio = GetComponentInChildren<AudioSource>();
        m_rigidBody = GetComponent<Rigidbody2D>();
        m_boostSpeed = BoostSpeed;
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    protected override void Update()
    {
        base.Update();

        if (!m_canControl)
            return;
        // Process input
        HandleInput();

        // If boost mode is enabled, reduce the speed applied each frame
        if (m_boostMode)
            DecreaseBoost();
    }

    /// <summary>
    /// Reduces the boost speed by an equal amount each frame
    /// </summary>
    private void DecreaseBoost()
    {
        m_boostSpeed -= (BoostSpeed / (BoostDuration * 60));
    }

    /// <summary>
    /// Determines if the user can control the character
    /// </summary>
    /// <param name="value">If true, the user can control the character</param>
    public void CanControl(bool value)
    {
        m_canControl = value;
        if (m_canControl)
        {
            m_audio.enabled = true;
            m_anim.enabled = true;
            Camera.main.GetComponent<FollowTarget>().Target = transform;
        }
        else
        {
            if (m_audio != null)
                m_audio.enabled = false;
            if (m_anim != null)
                m_anim.enabled = false;
        }
    }

    /// <summary>
    /// Handles user input
    /// </summary>
    private void HandleInput()
    {
        // Capture the X and Y axis input values (Input.GetAxis returns a float between -1 and 1)
        float xAxisInput = Input.GetAxis(Globals.INPUT_AXIS_HORIZONTAL);
        float yAxisInput = Input.GetAxis(Globals.INPUT_AXIS_VERTICAL);

        if (xAxisInput != 0 || yAxisInput != 0)
        {
            transform.localScale = new Vector2(
                (xAxisInput < 0) ? -1 :
                (xAxisInput > 0) ? 1 :
                transform.localScale.x, transform.localScale.y);

            // Boost mode only permitted if the X or Y axis has input
            if (Input.GetButtonDown(Globals.INPUT_BUTTON_FIRE0))
            {
                if (!m_boostMode)
                {
                    BoostMode();
                    Vector2 currentDirection = new Vector2(xAxisInput, yAxisInput).normalized;
                    m_rigidBody.AddForce(currentDirection * BoostForce, ForceMode2D.Impulse);
                }
            }
        }

        // Calculate the real max speed by adding the current boost speed to MaxSpeed
        Vector2 trueMaxSpeed = new Vector2(
            MaxSpeed + ((m_boostMode) ? m_boostSpeed : 0),
            MaxSpeed + ((m_boostMode) ? m_boostSpeed : 0));

        // Apply the force modifier
        AddConstantForce(gameObject, new Vector2(xAxisInput * MoveForce, yAxisInput * MoveForce), trueMaxSpeed);
    }

    /// <summary>
    /// Enables boost mode for <see cref="BoostDuration"/>
    /// </summary>
    public void BoostMode()
    {
        // Don't disable boost mode after the previous duration expires
        CancelInvoke("DisableBoost");
        m_boostMode = true;
        m_boostSpeed = BoostSpeed;
        // Disable boost mode after the boost duration expires
        Invoke("DisableBoost", BoostDuration);
    }

    /// <summary>
    /// Disables boost mode
    /// </summary>
    public void DisableBoost()
    {
        m_boostMode = false;
        m_boostSpeed = 0;
    }
}
