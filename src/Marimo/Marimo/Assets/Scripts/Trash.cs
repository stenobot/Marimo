using UnityEngine;

public class Trash : MonoBehaviour
{
    /// <summary>
    /// Tracks if the trash is active or deactivated
    /// </summary>
    public bool IsActive;

    public bool IsDynamic;

    /// <summary>
    /// The object containing the trash smash effect
    /// </summary>
    public GameObject SmashEffectObj;

    // components for the smashing trash parts
    private Rigidbody2D[] m_trashPartRigs;
    private SpriteRenderer[] m_trashPartRenderers;
    private Collider2D[] m_trashPartColliders;
    
    // components for the main trash object
    private Rigidbody2D m_rigidBody;
    private Collider2D m_collider;
    private SpriteRenderer m_renderer;
    private Animator m_animator;

    // misc
    private bool m_isTrashActivated;
    private float m_trashPartsFadeOutTimer;
    private bool m_isSmashed;
    private float m_alpha;
    private float m_maxAlpha;
    private float m_maxFallSpeed;
    private float m_maxImpactVelocity;

    // object initialization
    void Start()
    {
        SmashEffectObj.SetActive(true);

        
        m_collider = GetComponent<Collider2D>();
        m_renderer = GetComponent<SpriteRenderer>();

        if (IsDynamic)
        {
            m_rigidBody = GetComponent<Rigidbody2D>();
            m_animator = GetComponent<Animator>();
        }
        

        m_trashPartRigs = SmashEffectObj.GetComponentsInChildren<Rigidbody2D>();
        m_trashPartRenderers = SmashEffectObj.GetComponentsInChildren<SpriteRenderer>();
        m_trashPartColliders = SmashEffectObj.GetComponentsInChildren<Collider2D>();

        m_isTrashActivated = true;
        m_trashPartsFadeOutTimer = 1f;
        m_alpha = 1f;
        m_maxAlpha = 1f;
        m_isSmashed = false;
        m_maxFallSpeed = 16f;
        m_maxImpactVelocity = 6f;

        // if trash object is dynamic, start off deactivated 
        if (IsDynamic)
            ResetTrash();

        // start each smash effect object deactivated
        ResetSmashedTrash();
    }
	
	// object update once per frame
	void Update()
    {
        if (IsActive)
        {
            // check if trash has not been activated, 
            // so we only activate once each time needed
            if (!m_isTrashActivated)
                ActivateTrash();
            
            if (IsDynamic)
                CheckFallSpeed();
        }

        // check if object smash has started, and begin the fade out
        if (m_isSmashed)
            FadeOutSmashedTrash();
    }

    /// <summary>
    /// Fires when trash object is collided with.
    /// Allows trash to land safely on a conveyor, but 
    /// break on collision with anything else
    /// </summary>
    /// <param name="col">The collision object</param>
    void OnCollisionEnter2D(Collision2D col)
    {
        if (IsActive && 
            Mathf.Abs(col.relativeVelocity.y) > m_maxImpactVelocity && 
            col.gameObject.tag != Globals.TAG_CONVEYOR)
        {
            // track that we are now smashing
            m_isSmashed = true;

            // deactivate trash object
            ResetTrash();

            // enable colliders
           foreach (Collider2D collider in m_trashPartColliders)
                collider.enabled = true;

            foreach (Rigidbody2D rig in m_trashPartRigs)
            {
                // reset rigid bodies and set position
                rig.constraints = RigidbodyConstraints2D.FreezeRotation;
                rig.position = gameObject.transform.position;
                rig.velocity = Vector2.zero;

                // add force to rigid bodies
                rig.AddForce(new Vector2(Random.Range(-8, 8), Mathf.Abs(col.relativeVelocity.y / 2)), ForceMode2D.Impulse);
            }

            // enable renderers after position has changed
            foreach (SpriteRenderer renderer in m_trashPartRenderers)
                renderer.enabled = true;
        }
    }

    /// <summary>
    /// Restricts the object falling speed to the max speed
    /// </summary>
    private void CheckFallSpeed()
    {
        m_rigidBody.velocity =
            MathHelper.Clamp(
                m_rigidBody.velocity,
                new Vector2(Mathf.NegativeInfinity, -m_maxFallSpeed),
                new Vector2(Mathf.Infinity, m_maxFallSpeed)
            );
    }

    /// <summary>
    /// Activate trash object and start it's animation
    /// </summary>
    private void ActivateTrash()
    {
        if (m_rigidBody != null)
            m_rigidBody.velocity = Vector2.zero;

        // enable renderer and collider, and remove all constraints
        m_renderer.enabled = true;
        m_collider.enabled = true;
        m_animator.enabled = true;

        if (m_rigidBody != null)
            m_rigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (m_animator != null)
            m_animator.Play("dispensing_trash", -1, 0f);

        // set to true so we don't activate again this session
        m_isTrashActivated = true;
    }

    /// <summary>
    /// Disable a trash object
    /// </summary>
    private void ResetTrash()
    {
        // disable object's sprite renderer and collider components
        m_renderer.enabled = false;
        m_collider.enabled = false;

        if (IsDynamic)
        {
            
            m_animator.enabled = false;

            // freeze game object so it doesn't fall when collider is disabled
            m_rigidBody.constraints = RigidbodyConstraints2D.FreezeAll;
        }            

        IsActive = false;
        m_isTrashActivated = false;
    }

    /// <summary>
    /// Begin the fade out of the smashed trash effect.
    /// When it is finished, reset the smashed trash effect.
    /// </summary>
    private void FadeOutSmashedTrash()
    {
        // update backup timer that will start fadeout if rigidbody check fails
        if (m_trashPartsFadeOutTimer > 0)
            m_trashPartsFadeOutTimer -= Time.deltaTime;

        // don't do anything unless all the trash parts have stopped moving
        // or the timer has expired
        foreach (Rigidbody2D rig in m_trashPartRigs)
        {
            if (rig.velocity != Vector2.zero && m_trashPartsFadeOutTimer > 0)
                return;
        }

        // stagger updating the color's alpha channel for each renderer
        if (MathHelper.StaggerTrigger(m_alpha, m_maxAlpha))
        {
            foreach (SpriteRenderer renderer in m_trashPartRenderers)
            {
                // update color's alpha channel
                Color color = renderer.material.color;
                color.a = m_alpha;
                renderer.material.SetColor("_Color", color);
            }
        }

        if (m_alpha >= 0)
        {
            // decrement alpha
            m_alpha -= 0.01f;
        }
        else
        {
            ResetSmashedTrash();
        }
    }

    /// <summary>
    /// Reset the smashed trash effect.
    /// </summary>
    private void ResetSmashedTrash()
    {
        m_alpha = 1f;

        foreach (SpriteRenderer renderer in m_trashPartRenderers)
        {
            Color color = renderer.material.color;
            color.a = m_alpha;
            renderer.material.SetColor("_Color", color);

            renderer.enabled = false;
        }

        foreach (Collider2D collider in m_trashPartColliders)
            collider.enabled = false;

        foreach (Rigidbody2D rig in m_trashPartRigs)
            rig.constraints = RigidbodyConstraints2D.FreezeAll;
        
        m_isSmashed = false;
    }
}
