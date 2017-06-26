using UnityEngine;

public class Bomb : MonoBehaviour
{
    #region Public Variables

    /// <summary>
    /// The ground layer mask (for checking if on an incline)
    /// </summary>
    public LayerMask GroundLayerMask;

    /// <summary>
    /// Holds the sprite for when bomb is on a non-inclined surface
    /// </summary>
    public Sprite UprightSprite;

    /// <summary>
    /// Holds the sprite for when bomb is tipped diagonally right
    /// </summary>
    public Sprite DiagRightSprite;

    /// <summary>
    /// Holds the sprite for when bomb is tipped diagonally left
    /// </summary>
    public Sprite DiagLeftSprite;

    /// <summary>
    /// Holds a Prefab of the bomb object, which we'll use to respawn after explosion
    /// </summary>
    public GameObject BombPrefab;

    #endregion

    #region Private Variables

    // components
    private Rigidbody2D m_rigidBody;
    private Animator m_anim;
    private SpriteRenderer m_renderer;
    private CapsuleCollider2D m_blastCollider;

    // tracks whether bomb is tipped diagonally right 
    private bool m_isOnRightDiagSlope = false;

    // tracks whether bomb is tipped diagonally left
    private bool m_isOnLeftDiagSlope = false;

    // holds the bomb's starting position
    private Vector2 m_startingPosition;

    #endregion


    // Use this for initialization
    void Start()
    {
        // get components
        m_rigidBody = GetComponent<Rigidbody2D>();
        m_anim = GetComponent<Animator>();
        m_renderer = GetComponent<SpriteRenderer>();
        m_blastCollider = GetComponent<CapsuleCollider2D>();

        // keep track of bomb's starting position for respawn
        m_startingPosition = gameObject.transform.position;

        // reduce constraints on start
        m_rigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    // Update is called once per frame
    void Update()
    {
        CheckIfSloped();
	}

    /// <summary>
    /// Explode the bomb, changing the sprite and activating
    /// a collider representing the blast radius
    /// then instantiate a new bomb in original position
    /// </summary>
    private void Explode()
    {
        transform.position = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y - 0.3f);
        m_rigidBody.constraints = RigidbodyConstraints2D.FreezeAll;

        // respawn bomb prefab in original position 
        Instantiate(BombPrefab, m_startingPosition, Quaternion.identity);

        m_blastCollider.enabled = true;
        m_anim.enabled = true;

        Destroy(gameObject, 1.5f);
    }

    /// <summary>
    /// Uses raycasting to check whether the bomb is on a left diagonal or right diagonal slope
    /// and update's the bomb's sprite
    /// </summary>
    private void CheckIfSloped()
    {
        // Set raycast parameters for slope testing
        float rayLength = .4f;
        float rayOffset = .3f;
        Vector2 rayDirection = Vector2.down;
        Vector2 rayOrigin = transform.position;

        // Create 3 downward raycasts. One at the bomb center, one in front and one behind
        RaycastHit2D centerHit = Physics2D.Raycast(rayOrigin, rayDirection, rayLength, GroundLayerMask);
        RaycastHit2D frontHit = Physics2D.Raycast(new Vector2(rayOrigin.x + rayOffset, rayOrigin.y), rayDirection, rayLength, GroundLayerMask);
        RaycastHit2D rearHit = Physics2D.Raycast(new Vector2(rayOrigin.x - rayOffset, rayOrigin.y), rayDirection, rayLength, GroundLayerMask);

        // Ensure the center collider is a hit
        if (centerHit.collider != null)
        {
            if (frontHit.collider != null)
            {
                // Compare front and center hit points. 
                // NOTE: After upgrading to Unity 5.6.1 the Y values were coming back in scientific notation with insignificant differences, so rounding
                m_isOnRightDiagSlope = Mathf.Round(centerHit.point.y * 1000) / 1000 > Mathf.Round(frontHit.point.y * 1000) / 1000;
                m_isOnLeftDiagSlope = Mathf.Round(frontHit.point.y * 1000) / 1000 > Mathf.Round(centerHit.point.y * 1000) / 1000;
            }
            if (rearHit.collider != null)
            {
                // Compare rear and center hit points
                // NOTE: After upgrading to Unity 5.6.1 the Y values were coming back in scientific notation with insignificant differences, so rounding
                m_isOnRightDiagSlope = Mathf.Round(centerHit.point.y * 1000) / 1000 < Mathf.Round(rearHit.point.y * 1000) / 1000 ? true : m_isOnRightDiagSlope;
                m_isOnLeftDiagSlope = Mathf.Round(rearHit.point.y * 1000) / 1000 < Mathf.Round(centerHit.point.y * 1000) / 1000 ? true : m_isOnLeftDiagSlope;
            }
            if (frontHit.collider == null && rearHit.collider == null)
            {
                // Assume flat surface
                m_isOnLeftDiagSlope = false;
                m_isOnRightDiagSlope = false;
            }
        }
        else
        {
            // Assume flat surface
            m_isOnLeftDiagSlope = false;
            m_isOnRightDiagSlope = false;
        }

        // update bomb sprite
        if (m_isOnRightDiagSlope)
            m_renderer.sprite = DiagRightSprite;
        else if (m_isOnLeftDiagSlope)
            m_renderer.sprite = DiagLeftSprite;
        else
            m_renderer.sprite = UprightSprite;
    }

    /// <summary>
    /// Fires when bomb object collides with something
    /// </summary>
    /// <param name="col">The collision object</param>
    void OnCollisionEnter2D(Collision2D col)
    {
        // if bomb is falling too fast, explode
        // bombs should never explode when landing on a conveyor
        // TODO: check colliding object's velocity (for falling trash)
        if ((col.relativeVelocity.y > 7 || 
            col.gameObject.tag == Globals.TAG_TRASH) &&
            col.gameObject.tag != Globals.TAG_CONVEYOR)
        {
            Explode();
        } 
    }
}
