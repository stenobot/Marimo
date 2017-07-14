using UnityEngine;

/// <summary>
/// Base class which should be inherited by all tools
/// </summary>
public abstract class ToolBase : MonoBehaviour, IToolBase
{

    #region Public editor variables

    /// <summary>
    /// Holds reference to the robot body transform
    /// </summary>
    public Transform RobotBodyTransform;

    /// <summary>
    /// The resting distance when tool is selected
    /// </summary>
    public float RestingExtendDistance = 0.5f;

    /// <summary>
    /// The maximum distance to extend
    /// </summary>
    public float MaxExtendDistance = 1.0f;

    /// <summary>
    /// The movement distance per frame for extend and retract animations
    /// </summary>
    public float MoveDistancePerFrame = 0.02f;

    #endregion

    #region Public script-only variables

    /// <summary>
    /// Will hold the start position
    /// </summary>
    public Vector2 StartPos { get; private set; }

    /// <summary>
    /// Tracks if the tool is retracting
    /// </summary>
    public bool IsRetracting { get; set; }

    /// <summary>
    /// Tracks if the tool is fully retracted
    /// </summary>
    public bool IsRetracted { get; private set; }

    /// <summary>
    /// Tracks if the tool is extending
    /// </summary>
    public bool IsExtending { get; set; }

    /// <summary>
    /// Tracks if the tool should retract to the resting position
    /// </summary>
    public bool IsRetractingToRestPosition { get; set; }

    /// <summary>
    /// Tracks if the tool is enabled
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Will hold the resting position X coordinate
    /// </summary>
    public float RestingPosX { get; private set; }

    /// <summary>
    /// Will hold the maximum position X coordinate
    /// </summary>
    public float MaxPosX { get; private set; }

    // True if the Fire1 button is pressed
    public bool IsFire1Pressed { get; private set; }

    // True if the Fire1 button was released this frame
    public bool IsFire1Released { get; private set; }

    #endregion

    #region Private variables

    // Holds a reference to the attached AudioSource
    private AudioSource m_audio;
    // Holds a reference to the game manager
    private GameManager m_gameManager;
    // Tracks if the tool has extended during this frame
    private bool m_hasExtendedThisFrame;
    // Tracks if the tool has retracted during this frame
    private bool m_hasRetractedThisFrame;

    #endregion

    #region IToolBase implementation

    /// <summary>
    /// Returns the connected gameObject
    /// </summary>
    public GameObject GameObject
    {
        get
        {
            return gameObject;
        }
    }

    /// <summary>
    /// Enables the tool and extends it to the resting position
    /// </summary>
    public virtual void Enable()
    {
        IsExtending = true;
        IsRetracting = false;
        IsRetractingToRestPosition = false;
        IsRetracted = false;
    }

    /// <summary>
    /// Retracts and then disables the tool
    /// </summary>
    public virtual void Disable()
    {
        // Retract the tool
        IsRetracting = true;
        IsRetractingToRestPosition = false;
        IsExtending = false;
        IsEnabled = false;
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Plays the audio clip populated in <see cref="m_audio"/> once. 
    /// No code references, called from the <see cref="Animator"/>
    /// </summary>
    public void PlayToolAudio()
    {
        // Play the audio clip if populated in the AudioSource
        if (m_audio != null && m_audio.clip != null)
            m_audio.PlayOneShot(m_audio.clip);
    }

    #endregion

    #region Virtual methods

    /// <summary>
    /// Initializes the tool each time it is enabled
    /// </summary>
    public virtual void Awake()
    {
        m_gameManager = GameObject.FindGameObjectWithTag(Globals.TAG_GAMEMANAGER).GetComponent<GameManager>();
        m_audio = GetComponent<AudioSource>();
        IsEnabled = false;
        IsRetracting = false;
        IsRetracted = true;
        IsExtending = false;
        IsRetractingToRestPosition = false;
        StartPos = transform.localPosition;
        RestingPosX = StartPos.x + RestingExtendDistance;
        MaxPosX = StartPos.x + MaxExtendDistance;
        IsFire1Pressed = false;
        IsFire1Released = false;
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    public virtual void Update()
    {
        if (m_gameManager.IsPaused)
            return;

        m_hasExtendedThisFrame = false;
        m_hasRetractedThisFrame = false;

        // Extend or retract the tool if permitted
        ExtendOrRetract();
        // Handle user input
        HandleInput();
    }

    /// <summary>
    /// Handles user input for standard tool controls
    /// </summary>
    protected virtual void HandleInput()
    {
        // Fire 1 = Left Ctrl, LMB, or 'A' button on Xbox 360 controller
        // Fire 1 released: The tool will retract to the resting position
        IsFire1Released = Input.GetButtonUp(Globals.INPUT_BUTTON_FIRE1);

        if (IsFire1Released || (!IsFire1Released && !Input.GetButton(Globals.INPUT_BUTTON_FIRE1)))
            IsFire1Pressed = false;
        else
            // Fire 1 pressed: Extend/activate the tool
            IsFire1Pressed = !IsFire1Pressed ? Input.GetButtonDown(Globals.INPUT_BUTTON_FIRE1) : IsFire1Pressed;

        // Fire1: Retract the tool if the button is released. 
        // Retracts to the resting position if not currently retracting fully.
        IsRetractingToRestPosition = IsFire1Released ?
                (IsRetracting ? false : true) :
                (IsFire1Pressed ? false : IsRetractingToRestPosition);
    }

    /// <summary>
    /// Extends the tool
    /// </summary>
    /// <param name="xDistance">The distance from the starting X position where extension should stop</param>
    protected virtual void Extend(float xDistance = 0)
    {
        // Ensures movement speed is constant
        if (m_hasExtendedThisFrame)
            return;

        m_hasExtendedThisFrame = true;
        float targetX = StartPos.x + xDistance;

        if (transform.localPosition.x >= (StartPos.x + xDistance))
        {
            // Stop extending
            IsExtending = false;
            // Snap to exact target distance
            transform.localPosition = new Vector2(targetX, transform.localPosition.y);
        }
        else
        {
            IsRetracted = false;
            // Calculate the new position
            Vector2 newPos = new Vector2(transform.localPosition.x + MoveDistancePerFrame, transform.localPosition.y);
            // Set the local position to the new position, clamped to its bounds
            transform.localPosition = MathHelper.Clamp(newPos, new Vector2(StartPos.x, Mathf.NegativeInfinity), new Vector2(targetX, Mathf.Infinity));
        }
    }

    /// <summary>
    /// Retracts the tool
    /// </summary>
    /// <param name="xDistance">The distance from the starting X position where retraction should stop</param>
    protected virtual void Retract(float xDistance = 0)
    {
        // Ensures movement speed is constant
        if (m_hasRetractedThisFrame)
            return;

        m_hasRetractedThisFrame = true;
        float targetX = StartPos.x + xDistance;

        if (transform.localPosition.x <= targetX)
        {
            // Stop retracting
            IsRetracting = false;
            IsRetractingToRestPosition = false;
            IsRetracted = xDistance == 0 ? true : false;
            IsEnabled = IsRetracted ? false : IsEnabled;

            // Disable the gameobject to alert the tools controller that it can proceed with cycling to the next tool
            if (IsRetracted)
            {
                // Snap the transform back to the exact start position
                transform.localPosition = StartPos;
                // Cancel any existing press of Fire 1 at this point. The user must press the button again when the next tool is activated.
                IsFire1Pressed = false;
                gameObject.SetActive(false);
            }
        }
        else
        {
            // Calculate the new position
            Vector2 newPos = new Vector2(transform.localPosition.x - MoveDistancePerFrame, transform.localPosition.y);
            // Set the local position to the new position, clamped to its bounds
            transform.localPosition = MathHelper.Clamp(newPos, new Vector2(StartPos.x, Mathf.NegativeInfinity), new Vector2(targetX, Mathf.Infinity));
            transform.localPosition = newPos;
        }
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Extends or retracts the tool based on the values of <see cref="IsExtending"/>, <see cref="IsRetracting"/> and <see cref="IsRetractingToRestPosition"/>
    /// </summary>
    private void ExtendOrRetract()
    {
        // Handle retracting state
        if (IsRetracting)
            Retract();
        // Handle retracting to rest position state
        else if (IsRetractingToRestPosition)
            Retract(RestingExtendDistance);
        // Handle extending state
        else if (IsExtending)
            Extend(RestingExtendDistance);
    }

    #endregion
}
