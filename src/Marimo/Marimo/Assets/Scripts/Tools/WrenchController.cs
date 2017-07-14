using UnityEngine;

/// <summary>
/// Controls the wrench tool
/// </summary>
public class WrenchController : ToolBase
{
    #region Public editor variables

    /// <summary>
    /// The number of turns of the wrench it takes to activate an object
    /// </summary>
    public int TurnsToActivate = 3;

    /// <summary>
    /// The layermask for the switches
    /// </summary>
    public LayerMask SwitchLayerMask;

    /// <summary>
    /// The offset for the raycast's start position which will be used to check for contact with interactive items
    /// </summary>
    public Vector2 RayCastStartPositionOffset = Vector2.zero;

    #endregion

    #region Private variables

    // Holds a reference to the wrench Animator
    private Animator m_anim;
    // Tracks if the wrench is connected to an interactive item
    private bool m_isConnected;
    // Will hold the InteractiveItem of the collider if it is not null
    private InteractiveItemController m_collisionItem = null;
    // Will hold the last InteractiveItem to be the collision item
    private InteractiveItemController m_lastCollisionItem = null;
    // Will hold the hit result from a raycast
    private RaycastHit2D m_hit;
    #endregion

    #region ToolBase override methods

    /// <summary>
    /// Initializes the private variables each time the gameobject is enabled
    /// </summary>
    public override void Awake()
    {
        base.Awake();
        m_anim = GetComponent<Animator>();
        m_isConnected = false;
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    public override void Update()
    {
        base.Update();
        // Handle user input
        HandleInput();
    }

    /// <summary>
    /// Disables the tool
    /// </summary>
    public override void Disable()
    {
        base.Disable();
        // Disconnect any connected item
        DisconnectItem();
        // Play the idle animation state
        m_anim.Play(Globals.ANIMSTATE_IDLE);
    }

    /// <summary>
    /// Handles input for the wrench tool
    /// </summary>
    protected override void HandleInput()
    {
        base.HandleInput();

        // Get the raycast result to see if it hit something on the switch layer
        m_hit = HitTestSwitchLayer();

        // Fire1: Retract the tool when the button is released
        if (IsFire1Released)
        {
            IsEnabled = m_isConnected ? true : IsEnabled;
            // Play the idle animation state
            if(m_anim.gameObject.activeSelf)
                m_anim.Play(Globals.ANIMSTATE_IDLE, 0, 0);
            // Disconnect any connected item
            DisconnectItem();
        }

        // If the fire1 button is pressed, activate the tool
        if (IsFire1Pressed && IsEnabled)
            ActivateTool();

        if (m_isConnected)
        {
            // Disconnect the attached item if the collision object has changed since last frame
            if (m_collisionItem != m_lastCollisionItem)
                DisconnectItem();
        }
        else
        {
            // This forces the player to release the fire button before extending the wrench to an interactive item again
            bool isMoving = IsRetracting || IsRetractingToRestPosition || IsExtending;
            if (!IsEnabled && !isMoving && (transform.localPosition.x >= RestingPosX))
                IsEnabled = true;
        }

        // Set the last collision item to the current collision item
        m_lastCollisionItem = m_collisionItem;
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Extends the tool. Turns the tool once is reaches its target.
    /// </summary>
    private void ActivateTool()
    {
        // Don't allow activating the tool while not enabled, is retracted or retracting
        if (!IsEnabled || IsRetracting || IsRetracted)
            return;

        // Establish the current direction
        Enums.Direction currentDir = RobotBodyTransform.localScale.x > 0 ? Enums.Direction.Right : Enums.Direction.Left;

        // Check if the wrench can connect with the collision item in the current direction
        bool canConnect = m_collisionItem == null ? false :
            (m_collisionItem.InteractionDirection == Enums.Direction.Any || m_collisionItem.InteractionDirection == currentDir);

        if (canConnect && (m_hit.distance <= 0))
        {
            // Connect to the item
            ConnectToItem();
        }
        else if (!canConnect || m_hit.distance > 0)
        {
            // Play the idle animation state
            m_anim.Play(Globals.ANIMSTATE_IDLE, 0, 0);
            // Make sure the tool hasn't fully extended and isn't connected
            if (!m_isConnected && transform.localPosition.x < MaxPosX)
                Extend(MaxPosX);
        }
    }

    /// <summary>
    /// Handles connecting the wrench to interactive items
    /// </summary>
    private void ConnectToItem()
    {
        // Get the current animation state info
        AnimatorStateInfo animStateInfo = m_anim.GetCurrentAnimatorStateInfo(0);
        bool isTurning = animStateInfo.IsName(Globals.ANIMSTATE_WRENCH_TURN);

        // Get the playback time
        float currentAnimTime = animStateInfo.normalizedTime;

        // Only trigger the gameobject if the maximum turns have been reached
        if (isTurning && (currentAnimTime > TurnsToActivate))
        {
            // Trigger the collision item
            m_collisionItem.Trigger();
            // Disable the tool so it can't restart the animation
            IsEnabled = false;
            // Play the idle state
            m_anim.Play(Globals.ANIMSTATE_IDLE, 0, 0);
        }
        else
        {
            // The wrench is connected to an interactive item
            m_isConnected = true;
            // Play the turn animation state
            m_anim.Play(Globals.ANIMSTATE_WRENCH_TURN);
        }
    }

    /// <summary>
    /// Disconnects the wrench from any connected item
    /// </summary>
    private void DisconnectItem()
    {
        if (!m_isConnected)
            return;

        InteractiveItemController[] itemControllers = new InteractiveItemController[] { m_collisionItem, m_lastCollisionItem };
        // Iterate through the current and previous collision items
        for (int i = 0; i < itemControllers.Length; i++)
        {
            if (itemControllers[i] != null)
            {
                // Release the InteractiveItemController if it is not null
                itemControllers[i].Release();
                itemControllers[i] = null;
            }
        }

        m_isConnected = false;
    }

    /// <summary>
    /// Casts a ray to check if there are <see cref="IInteractiveItem"/> objects in front of the wrench tool
    /// </summary>
    /// <returns>The resulting <see cref="RaycastHit2D"/> of the raycast</returns>
    private RaycastHit2D HitTestSwitchLayer()
    {
        // Cast a ray to see if a switch is within range
        Vector2 rayOrigin = RobotBodyTransform.localScale.x > 0 ?
            (Vector2)transform.position + (RayCastStartPositionOffset) :
            (Vector2)transform.position + new Vector2(RayCastStartPositionOffset.x * -1, RayCastStartPositionOffset.y);

        // Set the ray direction based on whether the body is flipped
        Vector2 rayDirection = RobotBodyTransform.localScale.x > 0 ? Vector2.right : Vector2.left;

        // Cast a ray to see if it hits a switch
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, MaxExtendDistance, SwitchLayerMask);

        // Set the collision item
        m_collisionItem = hit.collider == null ? null : hit.collider.gameObject.GetComponent<InteractiveItemController>();

        return hit;
    }

    #endregion
}
