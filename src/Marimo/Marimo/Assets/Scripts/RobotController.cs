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
    // The prefab to instantiate when the player is smashed
    public GameObject SmashPrefab;

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
        // Capture the X and Y axis input values
        float xAxis = Input.GetAxis(Globals.INPUT_AXIS_HORIZONTAL);
        float yAxis = Input.GetAxis(Globals.INPUT_AXIS_VERTICAL);

        // Set the movement properties
        bool moveLeft = xAxis < 0 ? true : false;
        bool moveRight = xAxis > 0 ? true : false;
        bool jump = Input.GetButtonDown(Globals.INPUT_BUTTON_JUMP);

        // Test if the player is grounded or on a slope
        CheckIfGrounded();

        // Handle vertical input
        MoveVertical(yAxis);

        // Handle horizontal input
        if (moveLeft || moveRight)
            MoveHorizontal(xAxis);

        // Only allow jumping while grounded
        if (jump)
            Jump();
        else
            SetIdle();

        // Set animation state, speed, and audio pitch
        SetAnimationStates();
    }

    private void Jump()
    {
        if (m_isGrounded)
        {
            // Add jump force to the player
            m_rigidBody.AddForce(Vector2.up * JumpForce);
            // Play the jump sound effect on the camera's audio source as its pitch won't be adjusted like this object's audio source
            Camera.main.GetComponent<AudioSource>().PlayOneShot(Audio_Jump);
			// Run Jump animation
			Animator_Body.Play(Globals.ANIMSTATE_ROBOT_JUMP);
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
            // Get the correct animation state
            string animState = GetMovementAnimState();
            // Play treads animation
            Animator_Treads.Play(animState);
            // Clamp the X velocity to the maximum speed
            m_rigidBody.velocity = MathHelper.Clamp(m_rigidBody.velocity, new Vector2(-MaxSpeed, Mathf.NegativeInfinity), new Vector2(MaxSpeed, Mathf.Infinity));
            // Set the tread animation speed to player speed
            Animator_Treads.SetFloat(Globals.ANIM_PARAM_SPEED, (m_rigidBody.velocity.x < 0 ? m_rigidBody.velocity.x * -1 : m_rigidBody.velocity.x) * TreadAnimSpeedMultiplier);
            // Set the audio pitch based on the horizontal speed
            m_audio.pitch = (m_rigidBody.velocity.x < 0 ? m_rigidBody.velocity.x * -1 : m_rigidBody.velocity.x) * TreadAudioPitchMultiplier;
        }
    }

    /// <summary>
    /// Sets the player to the idle state
    /// </summary>
    private void SetIdle()
    {
        if (m_isGrounded && m_rigidBody.velocity.x == 0)
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
    /// <param name="moveUp">Indicated that the player is moving up. If false it is implied that the player is moving down</param>
    private void MoveVertical(float moveSpeed)
    {
        // Handle idle state and vertical movement
        if (m_isGrounded && m_rigidBody.velocity.x == 0)
        {
            // Get the current time for the animation, where 1 is 100% complete and 0 is 0% complete
            float currentAnimTime = Animator_Body.GetCurrentAnimatorStateInfo(0).normalizedTime;
            // Check if the animation has completed in the given playback direction
            bool canAnimate = moveSpeed > 0 ? currentAnimTime < 1 : currentAnimTime > 0;

            if (canAnimate)
            {
                // Set the AnimSpeed parameter in the Animator
                Animator_Body.SetFloat(Globals.ANIM_PARAM_SPEED, moveSpeed);
                // Raise or lower the telescope
                Animator_Body.Play(Globals.ANIMSTATE_ROBOT_RAISE);
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
    /// <param name="moveLeft">Indicates that the player is moving left. If false it is implied that the player is moving right</param>
    private void MoveHorizontal(float moveSpeed)
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
        int scale = moveSpeed < 0 ? -1 : 1;
        Animator_Treads.transform.localScale = new Vector2(scale, 1);
        Animator_Body.transform.localScale = new Vector2(scale, 1);

        // Add force to the player
        m_rigidBody.AddForce(Vector2.right * MoveForce * moveSpeed);
    }

    /// <summary>
    /// Tests if the player is touching the ground and if they are standing on a slope
    /// </summary>
    private void CheckIfGrounded()
    {
        // Check if the player is touching the ground layer
        m_isGrounded = Physics2D.OverlapCircle(transform.position, .4f, GroundLayerMask);

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
            // Instantiate the "smash" effect prefab
            GameObject effect = Instantiate(SmashPrefab, transform.position, Quaternion.identity);
            // Add force and randomized torque to each rigidbody child of the effect object
            foreach (Rigidbody2D rig in effect.GetComponentsInChildren<Rigidbody2D>())
            {
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
