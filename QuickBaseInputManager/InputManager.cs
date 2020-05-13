using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

using QuickVR;

/// <summary>
/// Offers a common and simple interface to manage any type of Input
/// </summary>
/// \author Ramon Oliva
[System.Serializable]
public class InputManager : MonoBehaviour
{

    #region PROTECTED PARAMETERS

    [SerializeField]
    protected List<string> _virtualAxes = new List<string>();

    [SerializeField]
    protected List<string> _virtualButtons = new List<string>();

    protected Dictionary<string, bool> _activeVirtualAxes = new Dictionary<string, bool>();

    protected Dictionary<string, bool> _activeVirtualButtons = new Dictionary<string, bool>();

    protected enum VirtualButtonState
    {
        UP,
        DOWN,
        PRESSED,
    }

    [SerializeField, HideInInspector]
    protected bool _initialized = false;

    #endregion

    #region CONSTANTS

    public const string DEFAULT_BUTTON_CONTINUE = "Continue";
    public const string DEFAULT_BUTTON_EXIT = "Exit";
    public const string DEFAULT_BUTTON_CANCEL = "Cancel";
    public const string DEFAULT_BUTTON_CALIBRATE = "Calibrate";

    public const string DEFAULT_AXIS_HORIZONTAL = "Horizontal";
    public const string DEFAULT_AXIS_VERTICAL = "Vertical";

    public const string DEFAULT_AXIS_LEFT_TRIGGER = "LeftTrigger";
    public const string DEFAULT_AXIS_RIGHT_TRIGGER = "RightTrigger";

    #endregion

    #region EVENTS

    public static Action OnPreUpdateInput;
    public static Action OnPostUpdateInput;

    #endregion

    #region CREATION AND DESTRUCTION

    protected virtual void Awake()
    {
        if (!_initialized) Reset();
    }

    protected virtual void Reset()
    {
        name = this.GetType().FullName;

        CreateDefaultAxes();
        CreateDefaultButtons();

        //Create the default InputManagerUnity implementation
        InputManagerUnity iManager = new GameObject().AddComponent<InputManagerUnity>();
        iManager.transform.parent = transform;
        iManager.transform.ResetTransformation();
        iManager.Reset();

        _initialized = true;
    }

    protected virtual void CreateDefaultAxes()
    {
        //Create the default axes needed for applications constructed with QuickVR
        CreateDefaultAxis(DEFAULT_AXIS_HORIZONTAL);
        CreateDefaultAxis(DEFAULT_AXIS_VERTICAL);

        CreateDefaultAxis(DEFAULT_AXIS_LEFT_TRIGGER);
        CreateDefaultAxis(DEFAULT_AXIS_RIGHT_TRIGGER);
    }

    protected virtual void CreateDefaultButtons()
    {
        //Create the default buttons needed for applications constructed with QuickVR
        CreateDefaultButton(DEFAULT_BUTTON_CONTINUE);
        CreateDefaultButton(DEFAULT_BUTTON_CANCEL);
        CreateDefaultButton(DEFAULT_BUTTON_EXIT);
        CreateDefaultButton(DEFAULT_BUTTON_CALIBRATE);
    }

    public virtual void CreateDefaultButton(string virtualButtonName)
    {
        if (_virtualButtons.Contains(virtualButtonName)) return;

        AddNewButton();
        _virtualButtons[_virtualButtons.Count - 1] = virtualButtonName;
    }

    public virtual void CreateDefaultAxis(string virtualAxisName)
    {
        if (_virtualAxes.Contains(virtualAxisName)) return;

        AddNewAxis();
        _virtualAxes[_virtualAxes.Count - 1] = virtualAxisName;
    }

    #endregion

    #region GET AND SET

    public BaseInputManager[] GetInputManagers()
    {
        return GetComponentsInChildren<BaseInputManager>();
    }

    public static T GetInputImplementation<T>() where T : BaseInputManager
    {
        BaseInputManager[] inputManagers = QuickSingletonManager.GetInstance<InputManager>().GetInputManagers();
        foreach (BaseInputManager bManager in inputManagers)
        {
            if (bManager is T) return (T)bManager;
        }
        return null;
    }

    public virtual void SetInputImplementationActive<T>(bool active) where T : BaseInputManager
    {
        BaseInputManager iManager = GetInputImplementation<T>();
        if (iManager) iManager._active = active;
    }

    public virtual bool IsInputImplementationActive<T>() where T : BaseInputManager
    {
        BaseInputManager iManager = GetInputImplementation<T>();
        return (iManager && (iManager.IsActive()));
    }

    public virtual bool IsActiveVirtualAxis(string aName)
    {
        if (IsVirtualAxis(aName))
        {
            return _activeVirtualAxes.ContainsKey(aName) ? _activeVirtualAxes[aName] : true;
        }

        return false;
    }

    public virtual void SetActiveVirtualAxis(string aName, bool active)
    {
        if (IsVirtualAxis(aName)) _activeVirtualAxes[aName] = active;
    }

    public virtual bool IsActiveVirtualButton(string bName)
    {
        if (IsVirtualButton(bName))
        {
            return _activeVirtualButtons.ContainsKey(bName) ? _activeVirtualButtons[bName] : true;
        }

        return false;
    }

    public virtual void SetActiveVirtualButton(string bName, bool active)
    {
        if (IsVirtualButton(bName)) _activeVirtualButtons[bName] = active;
    }

    public List<string> GetVirtualAxes()
    {
        return _virtualAxes;
    }

    public List<string> GetVirtualButtons()
    {
        return _virtualButtons;
    }

    public string GetVirtualAxis(int axisID)
    {
        if (axisID >= _virtualAxes.Count) return null;
        return _virtualAxes[axisID];
    }

    public string GetVirtualButton(int buttonID)
    {
        if (buttonID >= _virtualButtons.Count) return null;
        return _virtualButtons[buttonID];
    }

    [ButtonMethod]
    public void AddNewAxis()
    {
        _virtualAxes.Add("New Axis");

        BaseInputManager[] inputManagers = GetInputManagers();
        foreach (BaseInputManager manager in inputManagers)
        {
            manager.AddAxisMapping();
        }
    }

    [ButtonMethod]
    public void RemoveLastAxis()
    {
        if (_virtualAxes.Count == 0) return;
        _virtualAxes.RemoveAt(_virtualAxes.Count - 1);

        BaseInputManager[] inputManagers = GetInputManagers();
        foreach (BaseInputManager manager in inputManagers)
        {
            manager.RemoveLastAxisMapping();
        }
    }

    [ButtonMethod]
    public void AddNewButton()
    {
        _virtualButtons.Add("New Button");

        BaseInputManager[] inputManagers = GetInputManagers();
        foreach (BaseInputManager manager in inputManagers)
        {
            manager.AddButtonMapping();
        }
    }

    [ButtonMethod]
    public void RemoveLastButton()
    {
        if (_virtualButtons.Count == 0) return;
        _virtualButtons.RemoveAt(_virtualButtons.Count - 1);

        BaseInputManager[] inputManagers = GetInputManagers();
        foreach (BaseInputManager manager in inputManagers)
        {
            manager.RemoveLastButtonMapping();
        }
    }

    public int GetNumAxes()
    {
        return _virtualAxes.Count;
    }

    public int GetNumButtons()
    {
        return _virtualButtons.Count;
    }

    #endregion

    #region INPUT MANAGEMENT

    protected virtual bool IsVirtualAxis(string axis)
    {
        return _virtualAxes.Contains(axis);
    }

    protected virtual bool IsVirtualButton(string button)
    {
        return _virtualButtons.Contains(button);
    }

    public static float GetAxis(string axis)
    {
        float value = 0.0f;
        if (QuickSingletonManager.GetInstance<InputManager>().IsActiveVirtualAxis(axis))
        {
            BaseInputManager[] inputManagers = QuickSingletonManager.GetInstance<InputManager>().GetInputManagers();
            foreach (BaseInputManager iManager in inputManagers)
            {
                if (!iManager.IsActive()) continue;

                float v = iManager.GetAxis(axis);
                if (Mathf.Abs(v) > Mathf.Abs(value))
                {
                    value = v;
                }
            }
        }

        return value;
    }

    protected static bool IsButtonState(string button, VirtualButtonState state)
    {
        bool inState = false;
        if (QuickSingletonManager.GetInstance<InputManager>().IsActiveVirtualButton(button))
        {
            BaseInputManager[] inputManagers = QuickSingletonManager.GetInstance<InputManager>().GetInputManagers();
            for (int i = 0; !inState && (i < inputManagers.Length); i++)
            {
                BaseInputManager iManager = inputManagers[i];
                if (!iManager._active) continue;

                if (state == VirtualButtonState.UP) inState = iManager.GetButtonUp(button);
                else if (state == VirtualButtonState.DOWN) inState = iManager.GetButtonDown(button);
                else inState = iManager.GetButton(button);
            }
        }

        return inState;
    }

    public static bool GetButton(string button)
    {
        return IsButtonState(button, VirtualButtonState.PRESSED);
    }

    public static bool GetButtonDown(string button)
    {
        return IsButtonState(button, VirtualButtonState.DOWN);
    }

    public static bool GetButtonUp(string button)
    {
        return IsButtonState(button, VirtualButtonState.UP);
    }

    #endregion

    #region UPDATE

    public virtual void UpdateState()
    {
        if (OnPreUpdateInput != null) OnPreUpdateInput();

        BaseInputManager[] inputManagers = GetInputManagers();
        foreach (BaseInputManager iManager in inputManagers)
        {
            iManager.UpdateMappingState();
        }

        if (OnPostUpdateInput != null) OnPostUpdateInput();
    }

    #endregion

}
