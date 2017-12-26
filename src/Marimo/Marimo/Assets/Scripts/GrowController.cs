using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrowController : MonoBehaviour 
{
	/// <summary>
	/// Muckle's current level of growth, where 0 is normal size and 1 is largest
	/// </summary>
	private float m_currentGrowthLevel;

	/// <summary>
	/// Inflate Muckle.
	/// </summary>
	/// <param name="anim">Muckle's primary Animator</param>
	public void Grow(Animator anim)
	{
		if (m_currentGrowthLevel < 0.05f)
		{
			anim.Play(Globals.ANIMSTATE_MUCKLE_GROW);
		}  

		if (m_currentGrowthLevel > 0.95f) 
		{
			m_currentGrowthLevel = 1;
		} else 
		{
			m_currentGrowthLevel += (Time.deltaTime * 3);
		}

		anim.SetFloat(Globals.ANIM_PARAM_NORM_TIME, m_currentGrowthLevel);
		// Debug.Log(m_currentGrowthLevel);
	}

	/// <summary>
	/// Deflate Muckle.
	/// </summary>
	/// <param name="anim">Muckle's primary Animator</param>
	public void Shrink(Animator anim)
	{
		if (m_currentGrowthLevel <= 0)
		{
			anim.Play(Globals.ANIMSTATE_IDLE);
			return;
		} else if (m_currentGrowthLevel > 0 && m_currentGrowthLevel < 0.05f) 
		{
			m_currentGrowthLevel = 0;
		} else 
		{
			m_currentGrowthLevel -= (Time.deltaTime * 3);
		}

		anim.SetFloat(Globals.ANIM_PARAM_NORM_TIME, m_currentGrowthLevel);
		// Debug.Log(m_currentGrowthLevel);
	}
}