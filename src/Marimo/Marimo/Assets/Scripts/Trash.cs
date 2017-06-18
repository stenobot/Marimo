using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trash : MonoBehaviour
{
    /// <summary>
    /// Tracks if the trash is active
    /// </summary>
    public bool IsActive;

    private bool m_isActivated;

    public GameObject SmashEffectObj;

    private float m_trashPartsFadeOutTimer;
    private Rigidbody2D[] m_trashPartRigs;
    private SpriteRenderer[] m_trashPartRenderers;
    private bool m_isSmashing;
    private float m_alpha;
    private float m_maxAlpha;

    private Rigidbody2D m_rigidBody;
    private Collider2D m_collider;
    private SpriteRenderer m_renderer;
    private Animator m_animator;


    // Use this for initialization
    void Start()
    {
        

       // gameObject.SetActive(false);
        m_rigidBody = GetComponent<Rigidbody2D>();
        m_collider = GetComponent<Collider2D>();
        m_renderer = GetComponent<SpriteRenderer>();
        m_animator = GetComponent<Animator>();

        m_isActivated = false;
        m_trashPartsFadeOutTimer = 1f;
        m_alpha = 1f;
        m_maxAlpha = 1f;
        m_isSmashing = false;

        m_trashPartRigs = SmashEffectObj.GetComponentsInChildren<Rigidbody2D>();
        m_trashPartRenderers = SmashEffectObj.GetComponentsInChildren<SpriteRenderer>();

        // start each object deactivated
        // trash must be activated from an outside source
        Deactivate();
    }
	
	// Update is called once per frame
	void Update()
    {
        // check if trash has been activated so we only 
        // activate once each time needed
        if (IsActive && !m_isActivated)
            Activate();

        // check that object has started smashing
        if (m_isSmashing)
            SmashTrashParts();
	}

    void OnCollisionEnter2D(Collision2D col)
    {
        if (IsActive && 
            Mathf.Abs(col.relativeVelocity.y) > 10 && 
            col.gameObject.tag != Globals.TAG_CONVEYOR)
        {
            // track that we are now smashing
            m_isSmashing = true;

            // deactivate trash object
            Deactivate();
       
            // activate smash effect object
            SmashEffectObj.SetActive(true);

            // add force to trash parts' rigid bodies
            foreach (Rigidbody2D rig in m_trashPartRigs)
            {
                rig.velocity = Vector2.zero;
                rig.AddForce(new Vector2(Random.Range(-8, 8), Mathf.Abs(col.relativeVelocity.y / 2)), ForceMode2D.Impulse);

                //Debug.Log("impulse force added to: " + rig);
            }
        }
    }

    private void Activate()
    {
        m_rigidBody.velocity = Vector2.zero;

        Debug.Log("activate: " + gameObject + 
            " position: " + gameObject.transform.position + 
            " velocity: " + gameObject.GetComponent<Rigidbody2D>().velocity);

        gameObject.SetActive(true);

        // enable renderer and collider, and remove all constraints
        m_renderer.enabled = true;
        m_collider.enabled = true;
        m_animator.enabled = true;
        
        m_rigidBody.constraints = RigidbodyConstraints2D.None;

        // only activate once as needed
        m_isActivated = true;
    }

    private void Deactivate()
    {
        // disable object's sprite renderer and collider components
        m_renderer.enabled = false;
        m_collider.enabled = false;
        m_animator.enabled = false;
        // freeze game object so it doesn't fall when collider is disabled
        m_rigidBody.constraints = RigidbodyConstraints2D.FreezeAll;

        IsActive = false;
        m_isActivated = false;
    }

    private void SmashTrashParts()
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
            ResetTrash();

            IsActive = false;
            SmashEffectObj.SetActive(false);
            gameObject.SetActive(false);

        }
    }

    private void ResetTrash()
    {
        foreach (Rigidbody2D rig in m_trashPartRigs)
        {
            rig.position = Vector2.zero;

            Debug.Log("rigidbody position after fadeout: " + rig.position);
        }

        Debug.Log("game object oposition after fadeout: " + gameObject.transform.position);
        Debug.Log("smash object opsition after fadout" + SmashEffectObj.transform.position);

        m_alpha = 1f;

        foreach (SpriteRenderer renderer in m_trashPartRenderers)
        {
            Color color = renderer.material.color;
            color.a = m_alpha;
            renderer.material.SetColor("_Color", color);
        }

        m_isSmashing = false;
    }
}
