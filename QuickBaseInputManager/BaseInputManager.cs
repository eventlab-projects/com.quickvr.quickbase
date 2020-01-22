﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using QuickVR;

[System.Serializable]
public abstract class BaseInputManager : MonoBehaviour {

	#region PUBLIC PARAMETERS

	public bool _active = true;
    public bool _debug = false;
	
	#endregion

	#region PROTECTED PARAMETERS

	[SerializeField, HideInInspector] 
    protected Transform _axisMappingRoot = null;

	[SerializeField, HideInInspector] 
    protected Transform _buttonMappingRoot = null;

	#endregion

	#region CONSTANTS

	public const string NULL_MAPPING = "None";

    protected const string ROOT_AXIS_MAPPING_NAME = "_AxisMappingRoot_";
    protected const string ROOT_BUTTON_MAPPING_NAME = "_ButtonMappingRoot_";

    protected const string AXIS_MAPPING_NAME = "_AxisMapping_";
    protected const string BUTTON_MAPPING_NAME = "_ButtonMapping_";

	#endregion
	
	#region ABSTRACT FUNCTIONS TO IMPLEMENT
	
	protected abstract float ImpGetAxis(string axis);
	protected abstract bool ImpGetButton(string button);

	#endregion

	#region CREATION AND DESTRUCTION

    public virtual void Reset() {
        name = this.GetType().FullName;

        _axisMappingRoot = transform.CreateChild(ROOT_AXIS_MAPPING_NAME);
        _buttonMappingRoot = transform.CreateChild(ROOT_BUTTON_MAPPING_NAME);

        int numVirtualAxes = GetComponentInParent<InputManager>().GetVirtualAxes().Count;
        while (_axisMappingRoot.childCount != numVirtualAxes) AddAxisMapping();

        int numVirtualButtons = GetComponentInParent<InputManager>().GetVirtualButtons().Count;
        while (_buttonMappingRoot.childCount != numVirtualButtons) AddButtonMapping();
	}

    protected virtual void OnDestroy() {
        DestroyAxisMapping();
		DestroyButtonMapping();
	}

	protected virtual void DestroyAxisMapping() {
        _axisMappingRoot.DestroyChildsImmediate();
	}

	protected virtual void DestroyButtonMapping() {
        _buttonMappingRoot.DestroyChildsImmediate();
	}

	#endregion

	#region MAPPING

	public virtual void RemoveLastAxisMapping() {
        _axisMappingRoot.DestroyChildImmediate(_axisMappingRoot.childCount - 1);
	}

	public virtual void RemoveLastButtonMapping() {
        _buttonMappingRoot.DestroyChildImmediate(_buttonMappingRoot.childCount - 1);
	}

	public AxisMapping GetAxisMapping(int axisID) {
        return (axisID >= _axisMappingRoot.childCount)? null : _axisMappingRoot.GetChild(axisID).GetComponent<AxisMapping>();
	}

	public AxisMapping GetAxisMapping(string virtualAxis) {
        List<string> virtualAxes = QuickSingletonManager.GetInstance<InputManager>().GetVirtualAxes();
        for (int i = 0; i < virtualAxes.Count; i++)
        {
            if (virtualAxes[i] == virtualAxis) return _axisMappingRoot.GetChild(i).GetComponent<AxisMapping>();
        }

        return null;
	}

	public ButtonMapping GetButtonMapping(int buttonID) {
        return (buttonID >= _buttonMappingRoot.childCount) ? null : _buttonMappingRoot.GetChild(buttonID).GetComponent<ButtonMapping>();
	}

	public ButtonMapping GetButtonMapping(string virtualButton) {
        List<string> virtualButtons = QuickSingletonManager.GetInstance<InputManager>().GetVirtualButtons();
        for (int i = 0; i < virtualButtons.Count; i++)
        {
            if (virtualButtons[i] == virtualButton) return _buttonMappingRoot.GetChild(i).GetComponent<ButtonMapping>();
        }

        return null;
	}

	public virtual void AddAxisMapping() {
        string mapName = AXIS_MAPPING_NAME + _axisMappingRoot.childCount.ToString();
        AxisMapping aMapping = _axisMappingRoot.CreateChild(mapName).gameObject.AddComponent<AxisMapping>();
        aMapping.Init();
	}

	public virtual void AddButtonMapping() {
        string mapName = BUTTON_MAPPING_NAME + _buttonMappingRoot.childCount.ToString();
        _buttonMappingRoot.CreateChild(mapName).gameObject.AddComponent<ButtonMapping>();
	}

	public virtual void ResetAllMapping() {
		ResetAxesMapping();
		ResetButtonMapping();
	}

	public virtual void ResetAxesMapping() {
		DestroyAxisMapping();
        _axisMappingRoot = transform.CreateChild(ROOT_AXIS_MAPPING_NAME);

		int numAxes = QuickSingletonManager.GetInstance<InputManager>().GetNumAxes();
        for (int i = 0; i < numAxes; i++) AddAxisMapping();
	}

	public virtual void ResetButtonMapping() {
		DestroyButtonMapping();
		_buttonMappingRoot = transform.CreateChild(ROOT_BUTTON_MAPPING_NAME);

		int numButtons = QuickSingletonManager.GetInstance<InputManager>().GetNumButtons();
		for (int i = 0; i < numButtons; i++) AddButtonMapping();
	}

	public virtual int GetNumAxesMapped() {
        return _axisMappingRoot.childCount;
	}

	public virtual int GetNumButtonsMapped() {
        return _buttonMappingRoot.childCount;
	}

	#endregion

	#region GET AND SET

    public virtual string[] GetAxisCodes()
    {
        string[] codes = { BaseInputManager.NULL_MAPPING };
        return codes;
    }

    public virtual string[] GetButtonCodes()
    {
        string[] codes = { BaseInputManager.NULL_MAPPING };
        return codes;
    }

    protected virtual string[] GetCodes<T>()
    {
        string[] names = System.Enum.GetNames(typeof(T));
        List<string> tmp = new List<string>();
        for (int i = 0; i < names.Length; i++)
        {
            tmp.Add(names[i]);
        }

        return GetCodes(tmp);
    }

    protected virtual string[] GetCodes(List<string> tmp)
    {
        //Force the option "None" to be the first one. 
        tmp.Remove(BaseInputManager.NULL_MAPPING);
        List<string> codes = new List<string>();
        codes.Add(BaseInputManager.NULL_MAPPING);
        codes.AddRange(tmp);

        return codes.ToArray();
    }

	/// <summary>
	/// Returns the value of the virtual axis identified by axisName
	/// </summary>
	public virtual float GetAxis(string virtualAxis) {
		if (!IsActive()) return 0.0f;

		AxisMapping mapping = GetAxisMapping(virtualAxis);
		float value = 0.0f;
		if (mapping != null) {
			value = mapping._value;
			if (value == 0) {
				if (mapping.GetPositiveButton().IsPressed()) value = 1.0f;
				else if (mapping.GetNegativeButton().IsPressed()) value = -1.0f;
			}
		}

		return value;
	}
	
	/// <summary>
	/// Returns true while the user is pressing the button
	/// </summary>
	public virtual bool GetButton(string virtualButton) {
		if (!IsActive()) return false;

		ButtonMapping mapping = GetButtonMapping(virtualButton);
		return (mapping == null)? false : mapping.IsPressed();
	}
	
	/// <summary>
	/// Returns true during the frame the user starts pressing down the key
	/// </summary>
	public virtual bool GetButtonDown(string virtualButton) {
		if (!IsActive()) return false;

		ButtonMapping mapping = GetButtonMapping(virtualButton);
		return (mapping == null)? false : mapping.IsTriggered();
	}
	
	/// <summary>
	/// Returns true during the frame the user releases the key
	/// </summary>
	public virtual bool GetButtonUp(string virtualButton) {
		if (!IsActive()) return false;

		ButtonMapping mapping = GetButtonMapping(virtualButton);
		return (mapping == null)? false : mapping.IsReleased();
	}

	public virtual bool IsActive() {
		return _active;
	}

	#endregion
	
	#region UPDATE

	protected virtual void OnDrawGizmos()
    {
        if (_axisMappingRoot && _buttonMappingRoot)
        {
            _axisMappingRoot.hideFlags = _buttonMappingRoot.hideFlags = _debug ? HideFlags.None : HideFlags.HideInHierarchy;
        }
    }

    public virtual void UpdateMappingState()
    {
        if (!IsActive()) return;

        //Update the state of the buttons
        for (int i = 0; i < _buttonMappingRoot.childCount; i++) UpdateButtonState(i);

        //Update the state of the buttons of the axis
        for (int i = 0; i < _axisMappingRoot.childCount; i++) UpdateAxisState(i);
    }

    protected virtual void UpdateAxisState(int axisID)
    {
        UpdateAxisState(GetAxisMapping(axisID));
    }

    protected virtual void UpdateAxisState(AxisMapping mapping)
    {
        if (mapping._axisCode != BaseInputManager.NULL_MAPPING)
        {
            mapping._value = ImpGetAxis(mapping._axisCode);
        }
        UpdateButtonState(mapping.GetPositiveButton());
        UpdateButtonState(mapping.GetNegativeButton());
    }

    protected virtual void UpdateButtonState(int buttonID)
    {
        UpdateButtonState(GetButtonMapping(buttonID));
    }

	protected virtual void UpdateButtonState(ButtonMapping mapping) {
		if (mapping.IsIdle() && CheckButtonPressed(mapping)) {
			//IDLE => TRIGGERED
			mapping.SetState(ButtonState.TRIGGERED);
		}
		else if (mapping.IsTriggered() && CheckButtonPressed(mapping)) {
			//TRIGGERED => PRESSED
			mapping.SetState(ButtonState.PRESSED);
		}
		else if (mapping.IsPressed() && !CheckButtonPressed(mapping)) {
			//PRESSED => RELEASED
			mapping.SetState(ButtonState.RELEASED);
		}
		else if (mapping.IsReleased()) {
			//RELEASED => IDLE
			mapping.SetState(ButtonState.IDLE);
		}
	}

	protected virtual bool CheckButtonPressed(ButtonMapping mapping) {
		List<string> keyCodes = mapping.GetValidKeyCodes();
		bool pressed = false;
		foreach (string k in keyCodes) pressed = pressed || ImpGetButton(k);

		return pressed;
	}
	
	#endregion
}