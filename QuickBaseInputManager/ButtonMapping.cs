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
public class ButtonMapping : MonoBehaviour {

	#region PUBLIC PARAMETERS

	public bool _showInInspector = true;

    public string _keyCode = BaseInputManager.NULL_MAPPING;
    public string _altKeyCode = BaseInputManager.NULL_MAPPING;

	public ButtonState _state = ButtonState.IDLE;

    #endregion

    #region CREATION AND DESTRUCTION

    protected virtual void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    protected virtual void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    protected virtual void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetState(ButtonState.IDLE);
    }

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

	public virtual ButtonState GetState() {
		return _state;
	}

	public virtual void SetState(ButtonState state) {
		_state = state;
	}

	public virtual List<string> GetValidKeyCodes() {
		List<string> result = new List<string>();

		if (_keyCode != BaseInputManager.NULL_MAPPING) result.Add(_keyCode);
		if (_altKeyCode != BaseInputManager.NULL_MAPPING) result.Add(_altKeyCode);

		return result;
	}

	#endregion

}