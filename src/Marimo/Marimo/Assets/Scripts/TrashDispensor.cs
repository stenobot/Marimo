using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashDispensor : MonoBehaviour {

	// Tracks whether the dispensor is on
	public bool IsOn;

	// The speed of the dispensor
	public float Speed = 100f;

	// Holds reference for conveyor's animator
	public Animator Animator_Dispensor;

	private float m_interval;

	// Use this for initialization
	void Start() 
	{
		m_interval = Speed;
	}
	
	// Update is called once per frame
	void Update() 
	{
		if (IsOn)
			RunDispensor();
		else
			Animator_Dispensor.SetFloat(Globals.ANIM_PARAM_SPEED, 0);


	}

	private void RunDispensor()
	{
		Animator_Dispensor.SetFloat(Globals.ANIM_PARAM_SPEED, 1);

		m_interval -= Time.deltaTime;

		if (m_interval <= 0.0f) 
		{
			//TODO: create trash object at, set it's sprite to random
			m_interval = Speed;
		}
	}

	private void Dispense()
	{
		
	}
}
