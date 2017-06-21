using UnityEngine;

/// <summary>
/// Controls behaviour of a spring which the player can jump on to reach higher points
/// </summary>
public class Spring : MonoBehaviour {
    /// <summary>
    /// The relative velocity's Y velocity will be multiplied by this number
    /// </summary>
    public float VelocityMultiplier = 1f;

    /// <summary>
    /// The maximum Y velocity that can be applied. Stops fatal spring jumps from happening.
    /// </summary>
    public float MaxVelocity = 15f;

    // The spring animation
    private const string ANIMSTATE_SPRING = "spring";

    // Tracks the player's rigidbody which is connected to the spring
    private Rigidbody2D m_connectedBody;
    // Will hold the velocity to apply to the player
    private Vector2 m_springVelocity;
    private Animator m_anim;
    private AudioSource m_audio;

    /// <summary>
    /// Use this for initialization
    /// </summary>
    private void Start () {
        m_anim = GetComponent<Animator>();
        m_audio = GetComponent<AudioSource>();
        m_springVelocity = Vector2.zero;
    }

    /// <summary>
    /// Applies <see cref="m_springVelocity"/> to the player object
    /// </summary>
    private void Launch()
    {
        // Zero out the player's speed
        m_connectedBody.velocity = Vector2.zero;
        // Play the spring animation
        m_anim.Play(ANIMSTATE_SPRING);
        // Play the audio
        m_audio.PlayOneShot(m_audio.clip);
        // Set the player velocity
        m_connectedBody.velocity = m_springVelocity;
        // Zero the spring velocity
        m_springVelocity = Vector2.zero;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        // Only launch the player if they're moving downward
        if(col.gameObject.tag == Globals.TAG_PLAYER && col.relativeVelocity.y < 0)
        {
            // Invert the Y velocity of the collision and multiply it by the multiplier
            m_springVelocity = new Vector2(col.relativeVelocity.x, col.relativeVelocity.y * -VelocityMultiplier);
            // Clamp the Y velocity to the maximum velocity
            m_springVelocity.y = m_springVelocity.y > MaxVelocity ? MaxVelocity : m_springVelocity.y;
            // Set the connected body
            m_connectedBody = col.gameObject.GetComponent<Rigidbody2D>();
            // Launch the body
            Launch();
        }
    }
}
