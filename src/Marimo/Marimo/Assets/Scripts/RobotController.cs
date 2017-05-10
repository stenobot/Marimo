using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RobotController : MonoBehaviour
{
    // The UI Text object with the Game Over text
    public GameObject GameOverTextObject;

    // Player characteristics
    public float MoveForce = 16f;
    public float JumpForce = 800f;
    public float MaxSpeed = 14f;

    // Minimum and maximum length between bumps while moving
    public float MinBumpTime = 0.5f;
    public float MaxBumpTime = 3f;

    // Animators
    public Animator Animator_Treads;
    public Animator Animator_Body;

    // Animation clips
    public AnimationClip Anim_Treads_Idle;
    public AnimationClip Anim_Treads_MoveRight;
    //public AnimationClip Anim_Treads_MoveRightSlope;
    public AnimationClip Anim_Body_Raise;
    public AnimationClip Anim_Body_Bump;

    // Allows the user to select the ground layer(s), used to detect if player is grounded or in the air
    public LayerMask GroundLayerMask;

    // References to components
    private Rigidbody2D m_rigidBody;

    // Misc
    private bool m_isGrounded = true;
    private bool m_isMoving = false;


    // Use this for initialization
    void Start()
    {
        // Set the rigidbody reference
        m_rigidBody = GetComponent<Rigidbody2D>();
        // Call invoke once per second to test if robot fell off screen
        InvokeRepeating("HaveIFallen", 1, 1);
    }

    // Update is called once per frame
    void Update()
    {
        ProcessInput();
    }

    /// <summary>
    /// Checks if the robot has fallen off screen
    /// </summary>
    void HaveIFallen()
    {
        if(transform.position.y < Camera.main.GetComponent<FollowTarget>().MinYPosition)
        {
            GameOverTextObject.GetComponent<Text>().enabled = true;
            Invoke("Restart", 2.0f);
        }
    }

    /// <summary>
    /// Reloads the current scene
    /// </summary>
    private void Restart()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Causes the Bump animation to be played. Exposed as a function so it can be called on random intervals.
    /// </summary>
    private void Bump()
    {
        Animator_Body.Play("bump");
        // This animation may already be playing so set the play time back to zero
        Animator_Body.SetTime(0);
        // Invoke self at a random interval. This function will continue to be called until CancelInvoke() is called.
        Invoke("Bump", UnityEngine.Random.Range(MinBumpTime, MaxBumpTime));
    }

    /// <summary>
    /// Process controller input
    /// Currently does way too much and should be divided up
    /// </summary>
    private void ProcessInput()
    {
        // Check if player is touching the ground layer
        m_isGrounded = Physics2D.OverlapCircle(transform.position, .4f, GroundLayerMask);

        // TODO: Add controller support

        // Only extend the robot's telescopic neck if he's grounded and stationary
        if (m_isGrounded && m_rigidBody.velocity.x == 0)
        {
            if (Input.GetKey(KeyCode.W))
            {
                // Set the AnimSpeed parameter in the Animator
                Animator_Body.SetFloat("AnimSpeed", 1);
                Animator_Body.Play(Anim_Body_Raise.name);
            }
            else if (Input.GetKey(KeyCode.S))
            {
                // Set the AnimSpeed parameter in the Animator
                Animator_Body.SetFloat("AnimSpeed", -1);
                Animator_Body.Play(Anim_Body_Raise.name);
            }
            else if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S))
            {
                // Set the AnimSpeed parameter in the Animator
                Animator_Body.SetFloat("AnimSpeed", 0);
            }
        }


        if (Input.GetKey(KeyCode.A))
        {
            if (!m_isMoving)
            {
                m_isMoving = true;
                Invoke("Bump", UnityEngine.Random.Range(MinBumpTime, MaxBumpTime));
                Animator_Treads.Play(Anim_Treads_MoveRight.name);
            }
            // Move the player
            m_rigidBody.AddForce(Vector2.left * MoveForce);
            // Set scale
            Animator_Treads.transform.localScale = new Vector2(-1, 1);
            Animator_Body.transform.localScale = new Vector2(-1, 1);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            if (!m_isMoving)
            {
                m_isMoving = true;
                Invoke("Bump", UnityEngine.Random.Range(MinBumpTime, MaxBumpTime));
                Animator_Treads.Play(Anim_Treads_MoveRight.name);
            }
            // Move the player
            m_rigidBody.AddForce(Vector2.right * MoveForce);
            // Set scale
            Animator_Treads.transform.localScale = new Vector2(1, 1);
            Animator_Body.transform.localScale = new Vector2(1, 1);
        }
        else if (m_rigidBody.velocity == Vector2.zero)
        {
            m_isMoving = false;
            Animator_Treads.Play(Anim_Treads_Idle.name);
            CancelInvoke("Bump");
        }

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && m_isGrounded)
        {
            m_rigidBody.AddForce(Vector2.up * JumpForce);
        }

        // Set the tread animation speed to player speed
        Animator_Treads.speed = (m_rigidBody.velocity.x < 0 ? m_rigidBody.velocity.x * -1 : m_rigidBody.velocity.x) / 2;
        // Clamp the velocity to the maximum speed
        m_rigidBody.velocity = MathHelper.Clamp(m_rigidBody.velocity, new Vector2(-MaxSpeed, -Mathf.Infinity), new Vector2(MaxSpeed, Mathf.Infinity));
    }
}
