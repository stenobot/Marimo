using UnityEngine;

/// <summary>
/// Character controller script for the robot
/// </summary>
public class RobotController : MonoBehaviour
{
    // Player characteristics
    public float MoveForce = 16f;
    public float JumpForce = 800f;
    public float MaxSpeed = 14f;
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

    // Allows the user to select the ground layer(s), used to detect if player is grounded or in the air
    public LayerMask GroundLayerMask;

    // References to components
    private Rigidbody2D m_rigidBody;
    private AudioSource m_audio;
    private GameManager m_gameManager;

    // Misc
    private bool m_isGrounded = true;
    private bool m_isMoving = false;
    private bool m_isDead = false;

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
        InvokeRepeating("HaveIFallen", 1, 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_isDead)
        {
            ProcessInput();
        }
    }

    /// <summary>
    /// Checks if the robot has fallen off screen
    /// </summary>
    void HaveIFallen()
    {
        if (transform.position.y < Camera.main.GetComponent<FollowTarget>().MinYPosition)
        {
            m_isDead = true;
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
    /// Currently does way too much and should be divided up
    /// </summary>
    private void ProcessInput()
    {
        // Check if player is touching the ground layer
        m_isGrounded = Physics2D.OverlapCircle(transform.position, .4f, GroundLayerMask);

        // TODO: Add game controller support
        MoveTelescope();
        MoveRobot();
    }

    private void MoveRobot()
    {
        if (Input.GetKey(KeyCode.A))
        {
            if (!m_isMoving)
            {
                m_isMoving = true;
                Invoke("Bump", UnityEngine.Random.Range(MinBumpTime, MaxBumpTime));
                Animator_Treads.Play(Globals.ANIMSTATE_MOVE_RIGHT);
                m_audio.clip = Audio_Move;
                m_audio.Play();
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
                Animator_Treads.Play(Globals.ANIMSTATE_MOVE_RIGHT);
                m_audio.clip = Audio_Move;
                m_audio.Play();
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
            Animator_Treads.Play(Globals.ANIMSTATE_IDLE);
            CancelInvoke("Bump");
            if (!Input.GetKeyDown(KeyCode.W) && !Input.GetKeyDown(KeyCode.S))
            {
                m_audio.Stop();
            }
        }

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && m_isGrounded)
        {
            m_rigidBody.AddForce(Vector2.up * JumpForce);
            // Play the jump sound effect on the camera's audio source as its pitch won't be adjusted like this object's audio source
            Camera.main.GetComponent<AudioSource>().PlayOneShot(Audio_Jump);
        }

        if (m_isMoving)
        {
            // Set the tread animation speed to player speed
            Animator_Treads.SetFloat(Globals.ANIM_PARAM_SPEED, (m_rigidBody.velocity.x < 0 ? m_rigidBody.velocity.x * -1 : m_rigidBody.velocity.x) * TreadAnimSpeedMultiplier);
            m_audio.pitch = (m_rigidBody.velocity.x < 0 ? m_rigidBody.velocity.x * -1 : m_rigidBody.velocity.x) * TreadAudioPitchMultiplier;
        }
        else
        {
            m_audio.Stop();
        }

        // Clamp the velocity to the maximum speed
        m_rigidBody.velocity = MathHelper.Clamp(m_rigidBody.velocity, new Vector2(-MaxSpeed, Mathf.NegativeInfinity), new Vector2(MaxSpeed, Mathf.Infinity));
    }

    private void MoveTelescope()
    {
        // Only extend the robot's telescopic neck if he's grounded and stationary
        if (m_isGrounded && m_rigidBody.velocity.x == 0)
        {
            if (Input.GetKey(KeyCode.W))
            {
                    // Set the AnimSpeed parameter in the Animator
                    Animator_Body.SetFloat(Globals.ANIM_PARAM_SPEED, 1);
                    // Raise the telescope
                    Animator_Body.Play(Globals.ANIMSTATE_ROBOT_RAISE);
            }
            else if (Input.GetKey(KeyCode.S))
            {
                // Set the AnimSpeed parameter in the Animator to -1 to reverse the animation
                Animator_Body.SetFloat(Globals.ANIM_PARAM_SPEED, -1);
                // Lower the telescope
                Animator_Body.Play(Globals.ANIMSTATE_ROBOT_RAISE);
            }
            else if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S))
            {
                // Set the AnimSpeed parameter in the Animator to zero so it stops on the current frame
                Animator_Body.SetFloat(Globals.ANIM_PARAM_SPEED, 0);
            }
        }
    }
}
