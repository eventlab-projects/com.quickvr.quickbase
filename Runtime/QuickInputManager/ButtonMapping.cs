using UnityEngine;
using UnityEngine.SceneManagement;

using System.Collections;
using System.Collections.Generic;

public enum ButtonState {
	IDLE,
	TRIGGERED,
	PRESSED,
	RELEASED,
};

[System.Serializable]
public class ButtonMapping {

	#region PUBLIC PARAMETERS

	public bool _showInInspector = true;

	public string _actionName = "";
    public string _keyCode = BaseInputManager.NULL_MAPPING;
    public string _altKeyCode = BaseInputManager.NULL_MAPPING;

	public ButtonState _state = ButtonState.IDLE;

    #endregion

    #region GET AND SET

    public virtual bool IsIdle() {
		return (_state == ButtonState.IDLE);
	}

	public virtual bool IsTriggered() {
		return (_state == ButtonState.TRIGGERED);
	}

	public virtual bool IsPressed() {
		return (IsTriggered() || (_state == ButtonState.PRESSED));
	}

	public virtual bool IsReleased() {
		return (_state == ButtonState.RELEASED);
	}

	public virtual void SetState(ButtonState state) {
		_state = state;
	}

	#endregion

}