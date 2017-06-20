using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spring : MonoBehaviour {
    public float VelocityMultiplier = 1f;
    public float MaxVelocity = 15f;
    private const string ANIMSTATE_SPRING = "spring";

    private Rigidbody2D m_connectedBody;
    private Animator m_anim;
    private AudioSource m_audio;
    private Vector2 m_springVelocity;

	// Use this for initialization
	private void Start () {
        m_anim = GetComponent<Animator>();
        m_audio = GetComponent<AudioSource>();
        m_springVelocity = Vector2.zero;
    }

    private void Launch()
    {
        m_connectedBody.velocity = Vector2.zero;
        m_anim.Play(ANIMSTATE_SPRING);
        m_audio.PlayOneShot(m_audio.clip);
        m_connectedBody.velocity = m_springVelocity;
        m_springVelocity = Vector2.zero;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if(col.gameObject.tag == Globals.TAG_PLAYER && col.relativeVelocity.y < 0)
        {
            m_springVelocity = new Vector2(col.relativeVelocity.x, col.relativeVelocity.y * -VelocityMultiplier);
            m_springVelocity.y = m_springVelocity.y > MaxVelocity ? MaxVelocity : m_springVelocity.y;
            m_connectedBody = col.gameObject.GetComponent<Rigidbody2D>();
            Launch();
        }
    }
}
