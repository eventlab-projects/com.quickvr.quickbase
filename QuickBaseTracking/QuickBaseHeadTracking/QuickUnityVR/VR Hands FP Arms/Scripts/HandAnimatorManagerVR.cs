using UnityEngine;
using System.Collections;
using System;

public class HandAnimatorManagerVR : MonoBehaviour
{
	public StateModel[] stateModels;
	Animator handAnimator;

	public int currentState = 0;
	int lastState = -1;

	public bool action = false;
	public bool hold = false;

	//grip axis 11 or 12
	public string holdKey = "Axis 12";

	public int numberOfAnimations = 8;

	// Use this for initialization
	void Start ()
	{
		string[] joys = UnityEngine.Input.GetJoystickNames ();
		foreach (var item in joys) {
			Debug.Log (item);
		}
		handAnimator = GetComponent<Animator> ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (InputManager.GetButtonDown("Change")) {
			currentState = (currentState + 1) % (numberOfAnimations + 1);
		}

        //if (Input.GetAxis (holdKey) > 0) {
        //	hold = true;
        //} else
        //	hold = false;

        action = InputManager.GetButton("Action");

        if (lastState != currentState) {
			lastState = currentState;
			handAnimator.SetInteger ("State", currentState);
			TurnOnState(currentState);
		}

		handAnimator.SetBool ("Action", action);
		handAnimator.SetBool ("Hold", hold);

	}

	void TurnOnState (int stateNumber)
	{
		foreach (var item in stateModels) {
			if (item.stateNumber == stateNumber && !item.go.activeSelf)
				item.go.SetActive (true);
			else if (item.go.activeSelf)
				item.go.SetActive (false);
		}
	}


}

