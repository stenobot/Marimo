﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Character controller for Muckle
/// </summary>
public class MuckleController : MonoBehaviour
{
    #region Public editor variables

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
    public float MaxBoostSpeed = 3f;

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
    private GameManager m_gameManager;
	private ChargeController m_chargeController;
	private GrowController m_growController;

    #endregion

    #region Private variables

    // Determines if the character can be user-controlled
    private bool m_canControl = false;
    // Tracks if the player's boost mode is active
    private bool m_boostMode = false;
    // Tracks the current maximum boost speed to apply (to permit deceleration)
    private float m_maxBoostSpeed;

    #endregion

    /// <summary>
    /// Use this for initialization
    /// </summary>
    private void Start()
    {
        m_gameManager = GameObject.FindGameObjectWithTag(Globals.TAG_GAMEMANAGER).GetComponent<GameManager>();
        m_anim = GetComponentInChildren<Animator>();
        m_audio = GetComponentInChildren<AudioSource>();
        m_rigidBody = GetComponent<Rigidbody2D>();
		m_chargeController = GetComponentInChildren<ChargeController>();
		m_growController = GetComponentInChildren<GrowController>();
        m_maxBoostSpeed = MaxBoostSpeed;
    }

    /// <summary>
    /// Update should be used for handling input, as FixedUpdate can have duplicate buttonup/down events or loss
    /// </summary>
    private void Update()
    {
        if (!m_canControl || m_gameManager.IsPaused)
            return;
        // Process input
        HandleInput();
    }
    /// <summary>
    /// FixedUpdate should be used instead of Update when dealing with Rigidbody
    /// </summary>
    private void FixedUpdate()
    {
        // If boost mode is enabled, reduce the speed applied each frame
        if (m_boostMode)
            DecreaseBoost();
    }

    /// <summary>
    /// Reduces the boost speed by an equal amount each frame
    /// </summary>
    private void DecreaseBoost()
    {
        m_maxBoostSpeed -= (MaxBoostSpeed / (BoostDuration * 60));
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
            FollowTarget followTgt = Camera.main.GetComponent<FollowTarget>();
            if (followTgt != null)
                followTgt.Target = transform;
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
        // Handle charge up and charge down
		if (Input.GetButton(Globals.INPUT_BUTTON_FIRE1)) 
		{
            m_chargeController.ChargeUp(m_anim, m_growController.CurrentGrowthLevel >= 1);
			return;
		} else if (m_chargeController.CurrentLevel > 0) 
		{
			m_chargeController.ChargeDown();
			return;
		}

		// always force charge down when button is released
		if (Input.GetButtonUp(Globals.INPUT_BUTTON_FIRE1)) 
		{
			if (m_chargeController.ForceChargeDown)
				m_chargeController.ForceChargeDown = false;
		}

		// handle grow and shrink
		if (Input.GetButton(Globals.INPUT_BUTTON_FIRE2)) 
		{
            m_growController.Grow(m_anim);
		} else
		{
			m_growController.Shrink(m_anim);
		}

        // Capture the X and Y axis input values (Input.GetAxis returns a float between -1 and 1)
        float xAxisInput = Input.GetAxis(Globals.INPUT_AXIS_HORIZONTAL);
        float yAxisInput = Input.GetAxis(Globals.INPUT_AXIS_VERTICAL);

        if (xAxisInput != 0 || yAxisInput != 0)
        {
            transform.localScale = new Vector2(
                (xAxisInput < 0) ? -1 :
                (xAxisInput > 0) ? 1 :
                transform.localScale.x, transform.localScale.y);

            Vector2 currentDirection = new Vector2(xAxisInput, yAxisInput).normalized;

            // TODO: Move all the rigidbody movement under FixedUpdate(). Input needs to stay on Update().
            // Boost mode only permitted if the X or Y axis has input
            if (Input.GetButtonDown(Globals.INPUT_BUTTON_FIRE0))
            {
                if (!m_boostMode)
                {
                    BoostMode();
                    m_rigidBody.AddForce(currentDirection * BoostForce, ForceMode2D.Impulse);
                }
            }
            else
            {
                m_rigidBody.AddForce(currentDirection * MoveForce, ForceMode2D.Force);
            }
        }

        // Calculate the real max speed by adding the current boost speed to MaxSpeed
        // TODO: Factor in the force applied by any intersecting AreaEffector2D
        Vector2 trueMaxSpeed = new Vector2(
            MaxSpeed + ((m_boostMode) ? m_maxBoostSpeed : 0),
            MaxSpeed + ((m_boostMode) ? m_maxBoostSpeed : 0));

        m_rigidBody.velocity = MathHelper.Clamp(m_rigidBody.velocity, -trueMaxSpeed, trueMaxSpeed);
    }

    /// <summary>
    /// Enables boost mode for <see cref="BoostDuration"/>
    /// </summary>
    public void BoostMode()
    {
        // Don't disable boost mode after the previous duration expires
        CancelInvoke("DisableBoost");
        m_boostMode = true;
        m_maxBoostSpeed = MaxBoostSpeed;
        // Disable boost mode after the boost duration expires
        Invoke("DisableBoost", BoostDuration);
    }

    /// <summary>
    /// Disables boost mode
    /// </summary>
    public void DisableBoost()
    {
        m_boostMode = false;
        m_maxBoostSpeed = 0;
    }
}