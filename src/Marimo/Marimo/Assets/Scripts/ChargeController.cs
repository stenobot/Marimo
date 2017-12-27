using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChargeController : MonoBehaviour 
{
	/// <summary>
	/// The animator containing charge up/down and hold animations.
	/// </summary>
	public Animator Animator_Charge;

	/// <summary>
	/// The charge bar UI.
	/// </summary>
	public Slider ChargeBar;

	/// <summary>
	/// The charge hold bar UI, only active when ChargeBar is full.
	/// </summary>
	public Slider ChargeHoldBar;

	/// <summary>
	/// The current charge up level.
	/// </summary>
	public float CurrentLevel;

	/// <summary>
	/// The max level of charge up, where 1 is max and enables full charge.
	/// </summary>
	public float MaxLevel;

	/// <summary>
	/// The max level of holding full charge, where 1 is max.
	/// </summary>
	public float MaxHoldLevel;

	/// <summary>
	/// Speed multiplier for how quickly charge up occurs, and how fast the charge hold animation runs.
	/// </summary>
	public float SpeedMultiplier;

	/// <summary>
	/// Whether charging is in progress.
	/// </summary>
	public bool IsCharging;

	/// <summary>
	/// Whether to force charge down regardless of button press.
	/// </summary>
	public bool ForceChargeDown;

	// Private member variables
	private bool m_forceChargeDown;
	private float m_animSpeedInterval;
	private float m_currentHoldLevel;
	private SpriteRenderer m_spriteRenderer;
	private CircleCollider2D m_chargeTrigger;

	// initialization
	private void Start() 
	{
		ForceChargeDown = false;
		m_animSpeedInterval = 0.5f;
		m_currentHoldLevel = MaxHoldLevel;
		CurrentLevel = 0;
		ChargeBar.value = 0;
		m_spriteRenderer = GetComponent<SpriteRenderer>();

		// initialize the trigger area collider
		m_chargeTrigger = GetComponent<CircleCollider2D>();
		m_chargeTrigger.radius = 1;
		m_chargeTrigger.enabled = false;

		// set the charge progress bar level
		ChargeBar.maxValue = MaxLevel;

		// Set widths of charge progress bars
		ChargeBar.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 380, (MaxLevel * 200));
		ChargeHoldBar.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 380, (MaxHoldLevel * 200));

		// Set both progress bars to inactive
		ChargeBar.gameObject.SetActive(false);
		ChargeHoldBar.gameObject.SetActive(false);
	}

	/// <summary>
    /// Charge up Muckle's ability.
    /// </summary>
    /// <param name="anim">Muckle's primary animator</param>
    /// <param name="isFullGrown">If set to <c>true</c> Muckle is full grown.</param>
    public void ChargeUp(Animator anim, bool isFullGrown)
	{		
		if (!IsCharging) 
		{
			// we're charging, so enable sprites, trigger area, and progress bar
			m_spriteRenderer.enabled = true;
			m_chargeTrigger.enabled = true;
			ChargeBar.gameObject.SetActive(true);
			IsCharging = true;
		}

		// mechanism for forcing charge down regardless of button press state
		if (!ForceChargeDown && MaxLevel != 1 && CurrentLevel >= MaxLevel) 
			ForceChargeDown = true;
		
		if (ForceChargeDown) 
		{
			ChargeDown();
			return;
		}

		// increment current charge level based on speed multiplier
		// snap to 1 if above threshold
		if (CurrentLevel > 0.97f) 
			CurrentLevel = 1;
		else 
			CurrentLevel += (Time.deltaTime * SpeedMultiplier);

		// mechanism for fully charged
		if (CurrentLevel == 1) 
		{
            // play muckle's full charge animation,
            // which one depends on whether he's full grown or not
            if (isFullGrown)
                anim.Play (Globals.ANIMSTATE_MUCKLE_GROW_FULL_CHARGE);
            else
			    anim.Play(Globals.ANIMSTATE_MUCKLE_FULL_CHARGE);
            
			// animation for holding charge
			UpdateHoldAnimSpeed();
			Animator_Charge.Play(Globals.ANIMSTATE_MUCKLE_CHARGE_HOLD);

			// set the charge hold progress bar to active
			ChargeHoldBar.gameObject.SetActive(true);

			// decrement hold level and update progress bar
			m_currentHoldLevel -= (Time.deltaTime / 4);
			ChargeHoldBar.value = m_currentHoldLevel;

			// mechanism for forcing charge down when hold juice runs out
			if (!ForceChargeDown && m_currentHoldLevel < 0.05) 
			{
				m_currentHoldLevel = 0;
				ForceChargeDown = true;
				ChargeHoldBar.gameObject.SetActive(false);
			}
		}
		else 
		{
			// update charging animation
			Animator_Charge.SetFloat(Globals.ANIM_PARAM_NORM_TIME, CurrentLevel);
		}

		// update the size of the charge hit area
		m_chargeTrigger.radius = CurrentLevel * 6;

		// update the charge progress bar
		ChargeBar.value = CurrentLevel;

		//Debug.Log ("charging up: " + CurrentLevel);
	}

	/// <summary>
	/// Charges down Muckle's ability.
	/// </summary>
	public void ChargeDown()
	{
		if (IsCharging && CurrentLevel < 0.05f) 
		{
			// snap level to zero and reset everything
			CurrentLevel = 0;
			m_spriteRenderer.enabled = false;
			m_chargeTrigger.radius = 1;
			m_chargeTrigger.enabled = false;
			ChargeBar.gameObject.SetActive(false);
			m_currentHoldLevel = MaxHoldLevel;
			IsCharging = false;
		} else 
		{
			// decrement current charge level  
			CurrentLevel -= Time.deltaTime;

			// make sure charge hold progress bar is deactivated
			ChargeHoldBar.gameObject.SetActive(false);
		}

		if (IsCharging)  
		{
			// still charging so play charge animation and set level
			Animator_Charge.Play(Globals.ANIMSTATE_MUCKLE_CHARGE);
			Animator_Charge.SetFloat(Globals.ANIM_PARAM_NORM_TIME, CurrentLevel);
		}

		// update the size of the charge hit area
		m_chargeTrigger.radius = CurrentLevel * 6;

		// update charge progress bar
		ChargeBar.value = CurrentLevel;

		//Debug.Log ("charging down: " + CurrentLevel);
	}


	/// <summary>
	/// Updates the animation speed for charge hold based on speed multiplier.
	/// </summary>
	private void UpdateHoldAnimSpeed()
	{
		if (m_animSpeedInterval > 0)
		{
			m_animSpeedInterval -= Time.deltaTime;
		}
		else
		{
			Animator_Charge.SetFloat(Globals.ANIM_PARAM_SPEED, (Random.Range(0.5f, 1.3f) * SpeedMultiplier));
			m_animSpeedInterval = 0.3f;
		}
	}
}