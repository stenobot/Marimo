using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashDispensor : MonoBehaviour {

	// Tracks whether the dispensor is on
	public bool IsOn;

	// How frequently the despensor dispenses
	public float Interval = 5f;

	// The dispensor's animator component
	private Animator m_animator;

	// Pool of pre-defined trash objects
	private GameObject[] m_trash;

	// Tracks the index of the most recently dispensed piece of trash
	private int m_trashIndex;

	// Local variable for the current interval
	private float m_currInterval;

	// Use this for initialization
	void Start() 
	{
		// set current interval
		m_currInterval = Interval;

		// get the dispensor's animator
		m_animator = GetComponent<Animator>();

		// create a pool of pre-defined tagged trash objects
		m_trash = GameObject.FindGameObjectsWithTag(Globals.TAG_TRASH);

		// initialize each piece of trash
		foreach (GameObject trash in m_trash) 
			InitializeTrash(trash);

		// initialize index
		m_trashIndex = 0;
	}
	
	// Update is called once per frame
	void Update() 
	{
		// animate the dispensor
		SetDispensorAnimation();

		// if dispensor is on, run it
		if (IsOn)
			RunDispensor();
	}

	/// <summary>
	/// Sets the dispensor animation based on Interval speed
	/// </summary>
	private void SetDispensorAnimation()
	{
		// set animation speed
		m_animator.SetFloat(Globals.ANIM_PARAM_SPEED, (Interval / 5));
	}

	/// <summary>
	/// Dispenses trash from a pre-defined pool of trash objects
	/// </summary>
	private void RunDispensor()
	{
		// decrement current interval
		m_currInterval -= Time.deltaTime;

		if (m_currInterval <= 0.0f) 
		{
			// either reset index or increment
			if (m_trashIndex == m_trash.Length - 1)
				m_trashIndex = 0;
			else
				m_trashIndex++;

			// if trash is already active, re-initialize
			if (m_trash[m_trashIndex].activeSelf)
				InitializeTrash(m_trash[m_trashIndex]);

			// activate trash
			m_trash[m_trashIndex].SetActive(true);

			// reset current interval
			m_currInterval = Interval;
		}
	}

	/// <summary>
	/// Initializes a piece of trash by deactivating it and
	/// setting it to the center position of the dispensor
	/// </summary>
	/// <param name="trash">Trash game object</param>
	private void InitializeTrash(GameObject trash)
	{
		trash.SetActive(false);
		trash.transform.position = new Vector2(transform.position.x + 1.5f, transform.position.y + 1.5f);
	}
}
