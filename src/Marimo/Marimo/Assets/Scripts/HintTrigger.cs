﻿using UnityEngine;

/// <summary>
/// Triggers a hint in the player's thought bubble when it enters the trigger area
/// </summary>
public class HintTrigger : MonoBehaviour
{
    #region Public editor variables

    /// <summary>
    /// The tool icon to be displayed for this hint
    /// </summary>
    public Enums.ToolIcon ToolIcon;

    /// <summary>
    /// The interaction icon to be displayed for this hint
    /// </summary>
    public Enums.InteractionIcon InteractionIcon;

    /// <summary>
    /// The percentage of the maximum travelled speed the player must reduce by to invoke the trigger
    /// </summary>
    public int SpeedPercentageDecreaseToTrigger = 50;

    #endregion

    #region  Private variables

    private GameObject m_connectedPlayer;
    private Animator m_toolAnimator;
    private Animator m_interationAnimator;
    private Animator m_bubbleAnim;
    private Rigidbody2D m_playerRig;
    private Collider2D m_collider;
    private bool m_hasShownHint;
    private float m_maxSpeed;
    private float m_currentSpeed;

    #endregion

    /// <summary>
    /// Used for initialization
    /// </summary>
    void Start()
    {
        m_collider = GetComponent<Collider2D>();
        m_hasShownHint = false;
        m_maxSpeed = 0;
        m_currentSpeed = 0;
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        // If a thought bubble can't be shown, hide it
        if (m_connectedPlayer == null || !m_connectedPlayer.GetComponentInParent<RobotController>().CanShowThoughtBubble)
        {
            HideHint();
        }
        else
        {
            // only show the hint if the player expresses interest by slowing down
            if (IsInterestExpressed())
                ShowHint();
            else if (m_hasShownHint)
                HideHint();
        }
    }

    /// <summary>
    /// Checks if the player has slowed down in the trigger area by the percentage specified in <see cref="SpeedPercentageDecreaseToTrigger"/>
    /// </summary>
    /// <returns>True if the player is interested</returns>
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

    /// <summary>
    /// Shows the thought bubble with the hint
    /// </summary>
    private void ShowHint()
    {
        if (m_hasShownHint)
        {
            AnimatorStateInfo animStateInfo = m_bubbleAnim.GetCurrentAnimatorStateInfo(0);
            if ((m_toolAnimator != null) && animStateInfo.IsName(Globals.ANIMSTATE_HINT_IDLE_OPEN) && (animStateInfo.normalizedTime > 1))
            {
                string toolAnimState = "";
                switch (ToolIcon)
                {
                    case Enums.ToolIcon.UpDownArrows:
                        toolAnimState = Globals.ANIMSTATE_HINT_TOOL_UPDOWNARROWS;
                        break;
                    case Enums.ToolIcon.Wrench:
                        toolAnimState = Globals.ANIMSTATE_HINT_TOOL_WRENCH;
                        break;
                    default:
                        toolAnimState = Globals.ANIMSTATE_IDLE;
                        break;
                }
                m_toolAnimator.Play(toolAnimState);
            }
            if ((m_interationAnimator != null) && animStateInfo.IsName(Globals.ANIMSTATE_HINT_IDLE_OPEN) && (animStateInfo.normalizedTime > 1))
            {
                string interactionAnimState = "";
                switch (InteractionIcon)
                {
                    case Enums.InteractionIcon.Elevator:
                        interactionAnimState = Globals.ANIMSTATE_HINT_INTERACTION_ELEVATOR;
                        break;
                    case Enums.InteractionIcon.Switch:
                        interactionAnimState = Globals.ANIMSTATE_HINT_INTERACTION_SWITCH;
                        break;
                    default:
                        interactionAnimState = Globals.ANIMSTATE_IDLE;
                        break;
                }
                m_interationAnimator.Play(interactionAnimState);
            }
        }
        else if (m_bubbleAnim != null)
        {
            AnimatorStateInfo animStateInfo = m_bubbleAnim.GetCurrentAnimatorStateInfo(0);
            float currentAnimTime = 0;
            if (animStateInfo.IsName(Globals.ANIMSTATE_HINT_APPEAR))
                currentAnimTime = animStateInfo.normalizedTime < 0 ? 0 : animStateInfo.normalizedTime;
            
            // Play the thought bubble animation from the current frame
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

            // Play the thought bubble animation from the current frame
            m_bubbleAnim.Play(Globals.ANIMSTATE_HINT_APPEAR, 0, currentAnimTime);
            m_bubbleAnim.SetFloat(Globals.ANIM_PARAM_SPEED, -1.5f);

            if (m_toolAnimator != null)
                m_toolAnimator.Play(Globals.ANIMSTATE_IDLE);
            if (m_interationAnimator != null)
                m_interationAnimator.Play(Globals.ANIMSTATE_IDLE);

            m_hasShownHint = false;
        }
    }

    /// <summary>
    /// Occurs when a <see cref="Collider2D"/> stays in the trigger area
    /// </summary>
    /// <param name="col">The <see cref="Collider2D"/> which is in the trigger area</param>
    private void OnTriggerStay2D(Collider2D col)
    {
        if (m_connectedPlayer != null)
            return;

        if (col.tag == Globals.TAG_TREADS)
        {
            m_connectedPlayer = col.gameObject;
            RobotController robot = m_connectedPlayer.GetComponentInParent<RobotController>();
            m_bubbleAnim = robot.Animator_ThoughtBubble;
            m_interationAnimator = robot.Animator_InteractionIcon;
            m_toolAnimator = robot.Animator_ToolIcon;
            m_playerRig = robot.GetComponent<Rigidbody2D>();
        }
    }

    /// <summary>
    /// Occurs when a <see cref="Collider2D"/> exits the trigger area
    /// </summary>
    /// <param name="col">The <see cref="Collider2D"/> which left the trigger area</param>
    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.tag == Globals.TAG_TREADS)
        {
            HideHint();
            m_connectedPlayer = null;
            m_bubbleAnim = null;
            m_interationAnimator = null;
            m_toolAnimator = null;
            m_playerRig = null;
            m_currentSpeed = 0;
        }
    }
}
