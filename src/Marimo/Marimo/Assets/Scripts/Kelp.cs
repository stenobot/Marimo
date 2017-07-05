using UnityEngine;

public class Kelp : MonoBehaviour
{

    private float m_updateSpeedInterval;

    // Use this for initialization
    void Start()
    {
        m_updateSpeedInterval = 0.5f;
    }

	// Update is called once per frame
	void Update()
    {
        UpdateAnimationSpeed();
	}

    /// <summary>
    /// Update the animation speed of kelp animators
    /// on an interval
    /// </summary>
    private void UpdateAnimationSpeed()
    {
        if (m_updateSpeedInterval > 0)
        {
            m_updateSpeedInterval -= Time.deltaTime;
        }
        else
        {
            foreach (Animator anim in GetComponentsInChildren<Animator>())
            {
                anim.SetFloat(Globals.ANIM_PARAM_SPEED, Random.Range(0.2f, (anim.tag == Globals.TAG_LEAF ? 1.1f : 0.6f)));
            }

            m_updateSpeedInterval = 0.5f;
        }
    }
}
