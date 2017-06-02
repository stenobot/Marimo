using UnityEngine;

/// <summary>
/// Invokes the Trigger function of the gameobject specified
/// </summary>
public class InteractiveItemController : MonoBehaviour
{
    // The method name to invoke on the gameobject specified
    private const string METHOD_INVOKE = "Trigger";
    /// The method name to invoke when releasing contact
    private const string METHOD_RELEASE = "Release";

    #region Public editor variables

    /// <summary>
    /// The <see cref="GameObject"/> to invoke the 'Trigger' function on
    /// </summary>
    public GameObject ObjectToInvoke;

    /// <summary>
    /// The direction in which interaction is permitted
    /// Note: This script has no awareness of the direction which invokes it so this is for the caller to handle
    /// </summary>
    public Enums.Direction InteractionDirection = Enums.Direction.Any;

    /// <summary>
    /// The delay before invoking the 'Trigger' method on <see cref="ObjectToInvoke"/>
    /// </summary>
    public float Delay = 0f;

    /// <summary>
    /// Determines if this item can be invoked more than once
    /// </summary>
    public bool CanInvokeMoreThanOnce = true;

    /// <summary>
    /// The audio to play when triggering a <see cref="IInteractiveItem"/>
    /// </summary>
    public AudioClip TriggerAudio;

    /// <summary>
    /// The audio to play when releasing a <see cref="IInteractiveItem"/>
    /// </summary>
    public AudioClip ReleaseAudio;
    #endregion

    #region Private variables

    // Tracks if the object has already been invoked
    private bool m_hasInvoked = false;
    // The audio source
    private AudioSource m_audio;

    #endregion

    /// <summary>
    /// Used for initialization
    /// </summary>
    void Start()
    {
        m_audio = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Triggers the <see cref="InvokeMethod"/> of <see cref="ObjectToInvoke"/> with the delay specified by <see cref="Delay"/>
    /// </summary>
    public void Trigger()
    {
        if (ObjectToInvoke != null && !m_hasInvoked)
        {
            ObjectToInvoke.GetComponent<IInteractiveItem>().StartMethod(METHOD_INVOKE, Mathf.Abs(Delay));
            m_hasInvoked = CanInvokeMoreThanOnce;
            if(m_audio!=null && TriggerAudio != null)
                m_audio.PlayOneShot(TriggerAudio);
        }
    }


    /// <summary>
    /// Triggers the <see cref="ReleaseMethod"/> of <see cref="ObjectToInvoke"/> immediately
    /// </summary>
    public void Release()
    {
        if(ObjectToInvoke !=null)
        {
            ObjectToInvoke.GetComponent<IInteractiveItem>().StartMethod(METHOD_RELEASE, Mathf.Abs(Delay));
            m_hasInvoked = false;
            if (m_audio != null && ReleaseAudio != null)
                m_audio.PlayOneShot(ReleaseAudio);
        }
    }
}
