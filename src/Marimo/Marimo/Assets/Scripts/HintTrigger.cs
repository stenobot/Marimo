using UnityEngine;

public class HintTrigger : MonoBehaviour
{
    public Enums.ToolIcon ToolIcon;
    public Enums.InteractionIcon InteractionIcon;
    public bool CanShowHints = true;

    public int SpeedPercentageDecreaseToTrigger = 20;
    private GameObject m_connectedPlayer;
    private Animator m_toolAnimator;
    private Animator m_interationAnimator;
    private Animator m_bubbleAnim;
    private Rigidbody2D m_playerRig;
    private Collider2D m_collider;
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
        if (m_connectedPlayer == null || !m_connectedPlayer.GetComponentInParent<RobotController>().CanShowThoughtBubble)
        {
            HideHint();
        }
        else
        {
            if (IsInterestExpressed())
                ShowHint();
            else if (m_hasShownHint)
                HideHint();
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
            m_bubbleAnim.SetFloat(Globals.ANIM_PARAM_SPEED, -1.5f);

            if (m_toolAnimator != null)
                m_toolAnimator.Play(Globals.ANIMSTATE_IDLE);
            if (m_interationAnimator != null)
                m_interationAnimator.Play(Globals.ANIMSTATE_IDLE);

            m_hasShownHint = false;
        }
    }

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
