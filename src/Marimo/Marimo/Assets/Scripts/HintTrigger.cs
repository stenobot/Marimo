using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintTrigger : MonoBehaviour
{

    private GameObject m_player;
    private Collider2D m_collider;
    private Animator m_anim;
    private bool m_hasShowedHint;
    private float m_animLength;

    // Use this for initialization
    void Start()
    {
        m_collider = GetComponent<Collider2D>();
        m_hasShowedHint = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_player != null && IsInterestExpressed() && !m_hasShowedHint)
        {
            ShowHint();
        }
    }

    private bool IsInterestExpressed()
    {
        return true;
    }

    private void ShowHint()
    {
        if (m_anim != null)
        {
            AnimatorStateInfo animStateInfo = m_anim.GetCurrentAnimatorStateInfo(0);
            float currentAnimTime = 0;
            if (animStateInfo.IsName(Globals.ANIMSTATE_HINT_APPEAR))
                currentAnimTime = animStateInfo.normalizedTime < 0 ? 0 : animStateInfo.normalizedTime;

            m_anim.Play(Globals.ANIMSTATE_HINT_APPEAR, 0, currentAnimTime);
            m_anim.SetFloat(Globals.ANIM_PARAM_SPEED, 1f);
            m_hasShowedHint = true;
        }
    }

    private void HideHint()
    {
        if (m_anim != null)
        {
            AnimatorStateInfo animStateInfo = m_anim.GetCurrentAnimatorStateInfo(0);
            float currentAnimTime = 1;
            if (animStateInfo.IsName(Globals.ANIMSTATE_HINT_APPEAR))
                currentAnimTime = animStateInfo.normalizedTime > 1 ? 1 : animStateInfo.normalizedTime;

            m_anim.Play(Globals.ANIMSTATE_HINT_APPEAR, 0, currentAnimTime);
            m_anim.SetFloat(Globals.ANIM_PARAM_SPEED, -1f);
            m_hasShowedHint = false;
        }
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        if (m_player != null)
            return;

        if (col.tag == Globals.TAG_PLAYER)
        {
            m_player = col.gameObject;
            m_anim = m_player.GetComponentInParent<RobotController>().Animator_ThoughtBubble;
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.tag == Globals.TAG_PLAYER)
        {
            HideHint();
            m_player = null;
            m_anim = null;
        }
    }
}
