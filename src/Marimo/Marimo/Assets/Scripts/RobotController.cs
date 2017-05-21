using System;
using UnityEngine;

/// <summary>
/// Character controller script for the robot
/// </summary>
public class RobotController : MonoBehaviour
{
    // The force to apply to the player each frame while moving
    public float MoveForce = 16f;
    // The force to apply to the player while jumping
    public float JumpForce = 800f;
    // The player's maximum speed
    public float MaxSpeed = 14f;
    // If the player makes impact at a greater velocity than the maximum it will be destroyed
    public float MaximumImpactVelocity = 30f;
    // The smash effect gameobject to enable when the player is smashed
    public GameObject SmashEffectObj;

    // This is multiplied by the velocity to determine the speed of the treads animation
    public float TreadAnimSpeedMultiplier = 1f;
    // This is multiplied by the velocity to determine the pitch of the sound effect for the moving treads
    public float TreadAudioPitchMultiplier = 1f;

    // Minimum and maximum length between bumps while moving
    public float MinBumpTime = 0.5f;
    public float MaxBumpTime = 3f;

    // Animators
    public Animator Animator_Treads;
    public Animator Animator_Body;

    // Sound effects
    public AudioClip Audio_Move;
    public AudioClip Audio_Jump;

    // The ground physics layer(s), used to detect if player is grounded or in the air
    public LayerMask GroundLayerMask;
    // The elevator physics layer(s) to detect if player is standing on an elevator
    public LayerMask ElevatorLayerMask;

    // References to components
    private Rigidbody2D m_rigidBody;
    private AudioSource m_audio;
    private GameManager m_gameManager;

    // Misc
    private bool m_isGrounded = true;
    private bool m_isMoving = false;
    private bool m_isDead = false;
    private bool m_isOnDownwardSlope = false;
    private bool m_isOnUpwardSlope = false;
    private bool m_isNeckExtended = false;
    private bool m_canMoveVertical = false;
    private bool m_canMoveHorizontal = false;
    private bool m_hasMovedForFrame = false;
    private Elevator m_elevator;

    // Use this for initialization
    void Start()
    {
        // Set the game manager reference
        m_gameManager = GameObject.FindGameObjectWithTag(Globals.TAG_GAMEMANAGER).GetComponent<GameManager>();
        // Set the rigidbody reference
        m_rigidBody = GetComponent<Rigidbody2D>();
        // Set the audio source reference
        m_audio = GetComponent<AudioSource>();
        // Call invoke once per second to test if robot fell off screen
        InvokeRepeating("HasFallen", 1, 1);
    }

    // Update is called once per frame
    void Update()
    {
        m_hasMovedForFrame = false;
        if (!m_isDead)
        {
            // Process input
            HandleInput();
        }
    }

    /// <summary>
    /// Checks if the robot has fallen off screen
    /// </summary>
    private void HasFallen()
    {
        if (transform.position.y < Camera.main.GetComponent<FollowTarget>().MinYPosition)
        {
            m_isDead = true;
            m_gameManager.GameOver();
            m_audio.Stop();
        }
    }

    /// <summary>
    /// Causes the Bump animation to be played. Exposed as a function so it can be called on random intervals.
    /// </summary>
    private void Bump()
    {
        // This animation may already be playing so set the play time back to zero
        Animator_Body.Play("bump", -1, 0f);
        // Invoke self at a random interval. This function will continue to be called until CancelInvoke() is called.
        Invoke("Bump", UnityEngine.Random.Range(MinBumpTime, MaxBumpTime));
    }

    /// <summary>
    /// Process controller input
    /// </summary>
    private void HandleInput()
    {
        // Capture the X and Y axis input values (Input.GetAxis returns a float between -1 and 1)
        float xAxisInput = Input.GetAxis(Globals.INPUT_AXIS_HORIZONTAL);
        float yAxisInput = Input.GetAxis(Globals.INPUT_AXIS_VERTICAL);
        // Check if the Jump button is pressed
        bool tryJump = Input.GetButtonDown(Globals.INPUT_BUTTON_JUMP);

        // Test if the player is grounded, on a slope or an elevator
        CheckIfGrounded();
        // Compare the X and Y axis input and determine which should take preference
        SetMovementAxes(xAxisInput, yAxisInput);
            // Move the player vertically (if permitted)
            MoveVertical(yAxisInput);
            // Move the player horizontally (if permitted)
            MoveHorizontal(xAxisInput);

        // Only allow jumping while grounded
        if (tryJump)
            Jump();
        else
            SetIdle();

        // Set animation state, speed, and audio pitch
        SetAnimationStates();
    }

    /// <summary>
    /// Compares the X and Y axis input and sets the allowable movement direction
    /// </summary>
    /// <param name="xAxis">The X axis input value</param>
    /// <param name="yAxis">The Y axis input value</param>
    private void SetMovementAxes(float xAxisInput, float yAxisInput)
    {
        // Clamp the inputs to the valid input range (-1 to 1)
        xAxisInput = Mathf.Clamp(xAxisInput, -1, 1);
        yAxisInput = Mathf.Clamp(yAxisInput, -1, 1);
        // Get absolute values for comparison
        float xAxisAbs = Mathf.Abs(xAxisInput);
        float yAxisAbs = Mathf.Abs(yAxisInput);
        // Compare the absolute values of the input axes. If they are equal, the horizontal axis will take priority
        m_canMoveHorizontal = xAxisInput == 0 ? false : xAxisAbs >= yAxisAbs;
        m_canMoveVertical = yAxisInput == 0 ? false : yAxisAbs > xAxisAbs;
    }

    /// <summary>
    /// Makes the robot jump if it is grounded and neck is not extended
    /// </summary>
    private void Jump()
    {
        if ((m_isGrounded || (m_elevator != null && !m_elevator.IsMoving)) && !m_isNeckExtended)
        {
            // Add jump force to the player
            m_rigidBody.AddForce(Vector2.up * JumpForce);
            // Play the jump sound effect on the camera's audio source as its pitch won't be adjusted like this object's audio source
            Camera.main.GetComponent<AudioSource>().PlayOneShot(Audio_Jump);
            // Run Jump animation
            Animator_Body.Play(Globals.ANIMSTATE_ROBOT_JUMP, 0, 0);
        }
    }

    /// <summary>
    /// Sets the correct animation state and speed for the player based on its slope and movement, sets the movement audio pitch
    /// </summary>
    private void SetAnimationStates()
    {
        // If the player is moving, set correct animation states, speeds, and audio pitch
        if (m_isMoving)
        {
            // Get the absolute value for the player's horizontal speed
            float xSpeed = Mathf.Abs(m_rigidBody.velocity.x);
            // Get the correct animation state
            string animState = GetMovementAnimState();
            // Play treads animation
            Animator_Treads.Play(animState);
            // Clamp the X velocity to the maximum speed
            m_rigidBody.velocity = MathHelper.Clamp(m_rigidBody.velocity, new Vector2(-MaxSpeed, Mathf.NegativeInfinity), new Vector2(MaxSpeed, Mathf.Infinity));
            // Set the tread animation speed based on the horizontal speed
            Animator_Treads.SetFloat(Globals.ANIM_PARAM_SPEED, xSpeed * TreadAnimSpeedMultiplier);
            // Set the audio pitch based on the horizontal speed
            m_audio.pitch = xSpeed * TreadAudioPitchMultiplier;
        }
    }

    /// <summary>
    /// Sets the player to the idle state
    /// </summary>
    private void SetIdle()
    {
        if ((m_isGrounded || m_elevator != null) && m_rigidBody.velocity.x == 0)
        {
            m_isMoving = false;
            // Get the idle animation state for the treads
            string animState = GetIdleAnimState();
            // Play the tread animation
            Animator_Treads.Play(animState);
            // Stop the Bump animation
            CancelInvoke("Bump");
            // Stop the movement audio
            m_audio.Stop();
        }
    }

    /// <summary>
    /// Moves the player vertically (raises the robot's neck)
    /// </summary>
    /// <param name="yAxisInput">The controller input value of the Y axis</param>
    private void MoveVertical(float yAxisInput)
    {
        if (m_elevator != null && !m_elevator.IsMoving && !m_isGrounded && m_canMoveVertical && yAxisInput != 0)
        {
            Vector2 moveDir = yAxisInput > 0 ? Vector2.up : Vector2.down;
            m_elevator.Move(moveDir);
            return;
        }

        // Handle idle state and vertical movement
        if (m_isGrounded && m_rigidBody.velocity.x == 0 && !m_isMoving)
        {
            // Retrieve the current animation state
            AnimatorStateInfo animStateInfo = Animator_Body.GetCurrentAnimatorStateInfo(0);
            // Get the current time for the animation, where 1 is 100% complete and 0 is 0% complete
            // If the "raise" animation state is not playing the time should always be zero
            float currentAnimTime = animStateInfo.IsName(Globals.ANIMSTATE_ROBOT_RAISE) ? animStateInfo.normalizedTime : 0;
            // Check if the animation has completed in the given playback direction
            bool canAnimate = yAxisInput > 0 ? currentAnimTime < 1 : currentAnimTime > 0;
            // Indicate that the neck is extended to other functions
            m_isNeckExtended = currentAnimTime > 0;

            if (canAnimate && m_canMoveVertical)
            {
                // Set the AnimSpeed parameter in the Animator
                Animator_Body.SetFloat(Globals.ANIM_PARAM_SPEED, yAxisInput);
                // Raise or lower the telescope
                Animator_Body.Play(Globals.ANIMSTATE_ROBOT_RAISE);
                // The player has completed their movement action for this frame
                m_hasMovedForFrame = true;
            }
            else
            {
                // Stop the animation on the current frame
                Animator_Body.SetFloat(Globals.ANIM_PARAM_SPEED, 0);
            }
        }
    }

    /// <summary>
    /// Moves the player horizontally
    /// </summary>
    /// <param name="xAxisInput">The controller input value of the X axis</param>
    private void MoveHorizontal(float xAxisInput)
    {
        if (m_canMoveHorizontal && !m_isNeckExtended && (m_elevator == null || !m_elevator.IsMoving))
        {
            // If the player was not previously moving, start moving
            if (!m_isMoving)
            {
                m_isMoving = true;
                // Play the move audio
                m_audio.clip = Audio_Move;
                m_audio.Play();
                // Start bump animation
                Invoke("Bump", UnityEngine.Random.Range(MinBumpTime, MaxBumpTime));
            }

            // Set scale to flip the player if moving left
            int scale = xAxisInput < 0 ? -1 : 1;
            Animator_Treads.transform.localScale = new Vector2(scale, 1);
            Animator_Body.transform.localScale = new Vector2(scale, 1);

            // Add force to the player
            m_rigidBody.AddForce(Vector2.right * MoveForce * xAxisInput);
            // The player has completed their movement action for this frame
            m_hasMovedForFrame = true;
        }
        else if (m_isNeckExtended && !m_hasMovedForFrame && xAxisInput != 0)
        {
            // Retract the robot's neck
            m_canMoveVertical = true;
            MoveVertical(-1);
        }
    }

    /// <summary>
    /// Tests if the player is touching the ground and if they are standing on a slope
    /// </summary>
    private void CheckIfGrounded()
    {
        // Check if the player is touching the ground layer
        m_isGrounded = Physics2D.OverlapCircle(transform.position, .05f, GroundLayerMask);
        Collider2D elevatorCol = Physics2D.OverlapCircle(transform.position, .05f, ElevatorLayerMask);
        m_elevator = (elevatorCol != null) ? elevatorCol.GetComponent<Elevator>() : null;

        // Set raycast parameters for slope testing
        float rayLength = .4f;
        float rayOffset = Animator_Body.transform.localScale.x > 0 ? .3f : -.3f;
        Vector2 rayDirection = Vector2.down;
        Vector2 rayOrigin = transform.position;

        // Create 3 downward raycasts. One at the player center, one in front and one behind
        RaycastHit2D centerHit = Physics2D.Raycast(rayOrigin, rayDirection, rayLength, GroundLayerMask);
        RaycastHit2D frontHit = Physics2D.Raycast(new Vector2(rayOrigin.x + rayOffset, rayOrigin.y), Vector2.down, rayLength, GroundLayerMask);
        RaycastHit2D rearHit = Physics2D.Raycast(new Vector2(rayOrigin.x - rayOffset, rayOrigin.y), rayDirection, rayLength, GroundLayerMask);

        // Ensure the center collider is a hit
        if (centerHit.collider != null)
        {
            if (frontHit.collider != null)
            {
                // Compare front and center hit points
                m_isOnDownwardSlope = centerHit.point.y > frontHit.point.y;
                m_isOnUpwardSlope = frontHit.point.y > centerHit.point.y;
            }
            if (rearHit.collider != null)
            {
                // Compare rear and center hit points
                m_isOnDownwardSlope = centerHit.point.y < rearHit.point.y ? true : m_isOnDownwardSlope;
                m_isOnUpwardSlope = rearHit.point.y < centerHit.point.y ? true : m_isOnUpwardSlope;
            }
            if (frontHit.collider == null && rearHit.collider == null)
            {
                // Assume flat surface
                m_isOnUpwardSlope = false;
                m_isOnDownwardSlope = false;
            }
        }
        else
        {
            // Assume flat surface
            m_isOnUpwardSlope = false;
            m_isOnDownwardSlope = false;
        }
    }

    /// <summary>
    /// Gets the correct idle animation state for the robot treads based on the current slope
    /// </summary>
    /// <returns>The animation state name</returns>
    private string GetIdleAnimState()
    {
        // Default to idling right on a flat surface
        string animState = Globals.ANIMSTATE_IDLE;
        // Set upward slope
        animState = m_isOnUpwardSlope ? Globals.ANIMSTATE_IDLE_UP : animState;
        // Set downward slope
        animState = m_isOnDownwardSlope ? Globals.ANIMSTATE_IDLE_DOWN : animState;
        // Return the state
        return animState;
    }

    /// <summary>
    /// Gets the correct movement animation state for the robot treads based on the current slope
    /// </summary>
    /// <returns>The animation state name</returns>
    private string GetMovementAnimState()
    {
        // Default to moving right on a flat surface
        string animState = Globals.ANIMSTATE_MOVE_RIGHT;
        // Set upward slope
        animState = m_isOnUpwardSlope ? Globals.ANIMSTATE_MOVE_RIGHT_UP : animState;
        // Set downward slope
        animState = m_isOnDownwardSlope ? Globals.ANIMSTATE_MOVE_RIGHT_DOWN : animState;
        // Return the state
        return animState;
    }

    /// <summary>
    /// Triggered when entering a new collision
    /// </summary>
    /// <param name="col">The <see cref="Collision2D"/> of the object the player collided with</param>
    void OnCollisionEnter2D(Collision2D col)
    {
        // Only trigger if the player isn't already dead and the impact velocity exceeds the maximum impact velocity
        if (!m_isDead && col.relativeVelocity.y > MaximumImpactVelocity)
        {
            // Stop the player's rigidbody from moving any further
            m_rigidBody.isKinematic = true;
            m_rigidBody.velocity = Vector2.zero;
            // Stop the move audio
            m_audio.Stop();
            // Disable sprite renderers in all children
            foreach (SpriteRenderer r in GetComponentsInChildren<SpriteRenderer>())
                r.enabled = false;
            // Enable the "smash" effect object
            SmashEffectObj.SetActive(true);
            // Add force and randomized torque to each rigidbody child of the effect object
            foreach (Rigidbody2D rig in SmashEffectObj.GetComponentsInChildren<Rigidbody2D>())
            {
                // Reset the current speed so it doesn't dampen the force applied
                rig.velocity = Vector2.zero;
                // Reverse the impact force and apply it to the rigidbody
                rig.AddForce(col.relativeVelocity * -1, ForceMode2D.Impulse);
                // Add randomized torque to cause some rotation
                rig.AddTorque(UnityEngine.Random.Range(-30, 30));
            }
            // Kill the player
            m_isDead = true;
            // Invoke game over screen after 1 second
            m_gameManager.Invoke("GameOver", 1.0f);
        }
    }
}