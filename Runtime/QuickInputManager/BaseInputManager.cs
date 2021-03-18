using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using QuickVR;

[System.Serializable]
public abstract class BaseInputManager : MonoBehaviour {

	#region PUBLIC PARAMETERS

	public bool _active = true;
    public bool _debug = false;

	public enum DefaultCode
    {
		None,
    }

	#endregion

	#region PROTECTED PARAMETERS

	protected static InputManager _inputManager
	{
		get
		{
			if (!m_InputManager)
            {
				m_InputManager = QuickSingletonManager.GetInstance<InputManager>();
			}
			return m_InputManager;
		}
	}
	protected static InputManager m_InputManager = null;

	//[SerializeField, HideInInspector]
	[SerializeField]
	protected List<AxisMapping> _axisMapping = new List<AxisMapping>();

	//[SerializeField, HideInInspector]
	[SerializeField]
	protected List<ButtonMapping> _buttonMapping = new List<ButtonMapping>();

	//Maps a specific device button to a virtual button
	protected List<KeyValuePair<string, string>> _buttonToVirtual = new List<KeyValuePair<string, string>>();

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

	protected virtual void Awake()
	{
		Reset();

		foreach (ButtonMapping bMapping in _buttonMapping)
        {
			if (bMapping._keyCode != NULL_MAPPING)
            {
				_buttonToVirtual.Add(new KeyValuePair<string, string>(bMapping._keyCode, bMapping._actionName));
            }

			if (bMapping._altKeyCode != NULL_MAPPING)
            {
				_buttonToVirtual.Add(new KeyValuePair<string, string>(bMapping._keyCode, bMapping._actionName));
			}
        }
	}

    public virtual void Reset() 
	{
        name = this.GetType().FullName;

        int numVirtualAxes = _inputManager.GetVirtualAxes().Count;
		while (_axisMapping.Count != numVirtualAxes)
		{
			AddAxisMapping();
		}

        int numVirtualButtons = _inputManager.GetVirtualButtons().Count;
		while (_buttonMapping.Count != numVirtualButtons)
		{
			AddButtonMapping();
		}

		List<string> virtualButtons = _inputManager.GetVirtualButtons();
		for (int i = 0; i < virtualButtons.Count; i++)
		{
			_buttonMapping[i]._actionName = virtualButtons[i];
		}
	}

    public virtual void ConfigureDefaultAxis(string virtualAxisName, string axisName)
    {
		AxisMapping aMapping = GetAxisMapping(virtualAxisName);
		if (aMapping != null)
        {
			if (aMapping._axisCode == NULL_MAPPING)
			{
				aMapping._axisCode = axisName;
			}
		}
	}

	public virtual void ConfigureDefaultAxis(string virtualAxisName, string kPositive, string kNegative)
	{
		AxisMapping aMapping = GetAxisMapping(virtualAxisName);
		if (aMapping == null) return;

		if (aMapping.GetPositiveButton()._keyCode == NULL_MAPPING) aMapping.GetPositiveButton()._keyCode = kPositive;
		if (aMapping.GetNegativeButton()._keyCode == NULL_MAPPING) aMapping.GetNegativeButton()._keyCode = kNegative;
	}

	public virtual void ConfigureDefaultButton(string virtualButtonName, string key, string altKey = NULL_MAPPING)
	{
		ButtonMapping bMapping = GetButtonMapping(virtualButtonName);
		if (bMapping == null) return;

		if (bMapping._keyCode == NULL_MAPPING) bMapping._keyCode = key;
		if (bMapping._altKeyCode == NULL_MAPPING) bMapping._altKeyCode = altKey;
	}

	#endregion

	#region MAPPING

	public virtual void RemoveLastAxisMapping() 
	{
		if (_axisMapping.Count > 0)
        {
			_axisMapping.RemoveAt(_axisMapping.Count - 1);
        }
	}

	public virtual void RemoveLastButtonMapping() 
	{
		if (_buttonMapping.Count > 0)
        {
			_buttonMapping.RemoveAt(_buttonMapping.Count - 1);
        }
	}

	public AxisMapping GetAxisMapping(int axisID) 
	{
		return (axisID >= _axisMapping.Count) ? null : _axisMapping[axisID];
	}

	public AxisMapping GetAxisMapping(string virtualAxis) {
        List<string> virtualAxes = _inputManager.GetVirtualAxes();
        for (int i = 0; i < virtualAxes.Count; i++)
        {
			if (virtualAxes[i] == virtualAxis)
			{
				return _axisMapping[i];
			}
        }

        return null;
	}

	public ButtonMapping GetButtonMapping(int buttonID) 
	{
        return (buttonID >= _buttonMapping.Count) ? null : _buttonMapping[buttonID];
	}

	public ButtonMapping GetButtonMapping(string virtualButton) 
	{
		List<string> virtualButtons = _inputManager.GetVirtualButtons();
		for (int i = 0; i < virtualButtons.Count; i++)
        {
			if (virtualButtons[i] == virtualButton)
            {
				return _buttonMapping[i];
            }
        }

		return null;
	}

	public virtual void AddAxisMapping() 
	{
		_axisMapping.Add(new AxisMapping());
	}

	public virtual void AddButtonMapping() 
	{
		_buttonMapping.Add(new ButtonMapping());
	}

	public virtual void ResetAllMapping() 
	{
		ResetAxesMapping();
		ResetButtonMapping();
	}

	public virtual void ResetAxesMapping() {
		_axisMapping.Clear();
		
		int numAxes = _inputManager.GetNumAxes();
		for (int i = 0; i < numAxes; i++)
		{
			AddAxisMapping();
		}
	}

	public virtual void ResetButtonMapping() {
		_buttonMapping.Clear();

		int numButtons = _inputManager.GetNumButtons();
		for (int i = 0; i < numButtons; i++)
		{
			AddButtonMapping();
		}
	}

	public virtual int GetNumAxesMapped() 
	{
        return _axisMapping.Count;
	}

	public virtual int GetNumButtonsMapped() 
	{
		return _buttonMapping.Count;
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
	
	public virtual bool IsActive() {
		return _active;
	}

	#endregion
	
	#region UPDATE

	public virtual void UpdateMappingState()
    {

  //      if (!IsActive()) return;

  //      //Update the state of the axes
		//foreach (AxisMapping aMapping in _axisMapping)
  //      {
		//	UpdateAxisState(aMapping);
  //      }

		////Update the state of the buttons
		//foreach (ButtonMapping bMapping in _buttonMapping)
  //      {
		//	UpdateButtonState(bMapping);
  //      }
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

	public virtual bool CheckButtonPressed(ButtonMapping bMapping) 
	{
		return (
				((bMapping._keyCode != NULL_MAPPING) && ImpGetButton(bMapping._keyCode)) ||
				((bMapping._altKeyCode != NULL_MAPPING) && ImpGetButton(bMapping._altKeyCode))
				);
	}
	
	#endregion
}