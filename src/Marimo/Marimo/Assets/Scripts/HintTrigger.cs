using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintTrigger : MonoBehaviour
{
    public int SpeedPercentageDecreaseToTrigger = 20;
    private GameObject m_player;
    private Collider2D m_collider;
    private Rigidbody2D m_playerRig;
    private Animator m_anim;
    private bool m_hasShownHint;
    private float m_maxSpeed;
    private float m_currentSpeed;

    // Use this for initialization
    void Start()
    {
        m_collider = GetComponent<Collider2D>();
        m_hasShownHint = false;
        m_maxSpeed = 0;
        m_currentSpeed = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_player != null)
        {
            if (IsInterestExpressed())
            {
                ShowHint();
            }
            else if (m_hasShownHint)
            {
                HideHint();
            }
        }
    }

    private bool IsInterestExpressed()
    {
        if (m_playerRig == null)
            return false;

        m_currentSpeed = m_playerRig.velocity.magnitude;
        m_maxSpeed = m_currentSpeed > m_maxSpeed ? m_currentSpeed : m_maxSpeed;
        if (m_currentSpeed < (m_maxSpeed - ((m_maxSpeed / 100) * SpeedPercentageDecreaseToTrigger)))
            return true;
        else
            return false;
    }

    private void ShowHint()
    {
        if (!m_hasShownHint && m_anim != null)
        {
            AnimatorStateInfo animStateInfo = m_anim.GetCurrentAnimatorStateInfo(0);
            float currentAnimTime = 0;
            if (animStateInfo.IsName(Globals.ANIMSTATE_HINT_APPEAR))
                currentAnimTime = animStateInfo.normalizedTime < 0 ? 0 : animStateInfo.normalizedTime;

            m_anim.Play(Globals.ANIMSTATE_HINT_APPEAR, 0, currentAnimTime);
            m_anim.SetFloat(Globals.ANIM_PARAM_SPEED, 1f);
            m_hasShownHint = true;
        }
    }

    private void HideHint()
    {
        if (m_hasShownHint && m_anim != null)
        {
            AnimatorStateInfo animStateInfo = m_anim.GetCurrentAnimatorStateInfo(0);
            float currentAnimTime = 1;
            if (animStateInfo.IsName(Globals.ANIMSTATE_HINT_APPEAR))
                currentAnimTime = animStateInfo.normalizedTime > 1 ? 1 : animStateInfo.normalizedTime;

            m_anim.Play(Globals.ANIMSTATE_HINT_APPEAR, 0, currentAnimTime);
            m_anim.SetFloat(Globals.ANIM_PARAM_SPEED, -1f);
            m_hasShownHint = false;
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
            m_playerRig = m_player.GetComponentInParent<Rigidbody2D>();
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.tag == Globals.TAG_PLAYER)
        {
            HideHint();
            m_player = null;
            m_anim = null;
            m_playerRig = null;
            m_currentSpeed = 0;
        }
    }
}
