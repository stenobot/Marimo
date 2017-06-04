using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintTrigger : MonoBehaviour
{
    public Animator ToolAnimator;
    public Animator InterationAnimator;

    public int SpeedPercentageDecreaseToTrigger = 20;
    private GameObject m_player;
    private Rigidbody2D m_playerRig;
    private Collider2D m_collider;
    private Animator m_bubbleAnim;
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
        if (m_hasShownHint)
        {
            AnimatorStateInfo animStateInfo = m_bubbleAnim.GetCurrentAnimatorStateInfo(0);
            if ((ToolAnimator != null) && animStateInfo.IsName(Globals.ANIMSTATE_HINT_IDLE_OPEN) && (animStateInfo.normalizedTime > 1.1))
                    ToolAnimator.Play(Globals.ANIMSTATE_HINT_TOOL_WRENCH);
            if ((InterationAnimator != null) && animStateInfo.IsName(Globals.ANIMSTATE_HINT_IDLE_OPEN) && (animStateInfo.normalizedTime > 1.1))
                InterationAnimator.Play(Globals.ANIMSTATE_HINT_INTERACTION_SWITCH);
        }
        else if (m_bubbleAnim != null)
        {
            AnimatorStateInfo animStateInfo = m_bubbleAnim.GetCurrentAnimatorStateInfo(0);
            float currentAnimTime = 0;
            if (animStateInfo.IsName(Globals.ANIMSTATE_HINT_APPEAR))
                currentAnimTime = animStateInfo.normalizedTime < 0 ? 0 : animStateInfo.normalizedTime;

            m_bubbleAnim.Play(Globals.ANIMSTATE_HINT_APPEAR, 0, currentAnimTime);
            m_bubbleAnim.SetFloat(Globals.ANIM_PARAM_SPEED, 1f);
            m_hasShownHint = true;
        }
    }

    private void HideHint()
    {
        if (m_hasShownHint && m_bubbleAnim != null)
        {
            AnimatorStateInfo animStateInfo = m_bubbleAnim.GetCurrentAnimatorStateInfo(0);
            float currentAnimTime = 1;
            if (animStateInfo.IsName(Globals.ANIMSTATE_HINT_APPEAR))
                currentAnimTime = animStateInfo.normalizedTime > 1 ? 1 : animStateInfo.normalizedTime;

            m_bubbleAnim.Play(Globals.ANIMSTATE_HINT_APPEAR, 0, currentAnimTime);
            m_bubbleAnim.SetFloat(Globals.ANIM_PARAM_SPEED, -1f);

            if (ToolAnimator!=null)
                ToolAnimator.Play(Globals.ANIMSTATE_IDLE);
            if (InterationAnimator != null)
                InterationAnimator.Play(Globals.ANIMSTATE_IDLE);

            m_hasShownHint = false;
        }
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        if (m_player != null)
            return;

        if (col.tag == Globals.TAG_TREADS)
        {
            m_player = col.gameObject;
            m_bubbleAnim = m_player.GetComponentInParent<RobotController>().Animator_ThoughtBubble;
            m_playerRig = m_player.GetComponentInParent<Rigidbody2D>();
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.tag == Globals.TAG_TREADS)
        {
            HideHint();
            m_player = null;
            m_bubbleAnim = null;
            m_playerRig = null;
            m_currentSpeed = 0;
        }
    }
}
