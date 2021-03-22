using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

namespace QuickVR
{

    public class InputMap
    {
        public enum Type
        {
            Axis, 
            Button,
        }

        public BaseInputManager _inputManager = null;
        public string _inputCode = "";
        public Type _type = Type.Axis;
        public float _scale = 1;

        public InputMap(BaseInputManager iManager, string inputCode, Type t, float scale = 1)
        {
            _inputManager = iManager;
            _inputCode = inputCode;
            _type = t;
            _scale = scale;
        }
    }

    [System.Serializable]
    public class InputManager : MonoBehaviour
    {

        #region PROTECTED PARAMETERS

        protected static InputActionMap _actionMapDefault = null;

        [SerializeField]
        protected List<string> _virtualAxes = new List<string>();
        protected Dictionary<string, float> _virtualAxesState = new Dictionary<string, float>();
        protected Dictionary<string, int> _axisToID = new Dictionary<string, int>();

        [SerializeField]
        protected List<string> _virtualButtons = new List<string>();
        protected Dictionary<string, VirtualButtonState> _virtualButtonsState = new Dictionary<string, VirtualButtonState>();
        protected Dictionary<string, int> _buttonToID = new Dictionary<string, int>();

        protected Dictionary<string, bool> _activeVirtualAxes = new Dictionary<string, bool>();
        
        protected Dictionary<string, bool> _activeVirtualButtons = new Dictionary<string, bool>();

        protected List<List<InputMap>> _validAxesMapping = new List<List<InputMap>>();
        protected List<List<InputMap>> _validButtonsMapping = new List<List<InputMap>>();

        protected enum VirtualButtonState
        {
            Idle,
            Triggered, 
            Pressed,
            Released,
        }

        protected List<BaseInputManager> _inputManagers = new List<BaseInputManager>();

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

        protected static InputActionAsset _inputActionsDefault
        {
            get
            {
                if (m_InputActionsDefault == null)
                {
                    m_InputActionsDefault = Resources.Load<InputActionAsset>("QuickDefaultInputActions");
                    m_InputActionsDefault.Enable();
                }
                return m_InputActionsDefault;
            }
        }
        protected static InputActionAsset m_InputActionsDefault = null;

        protected XRController _xrControllerLeft
        {
            get
            {
                if (m_XRControllerLeft == null)
                {
                    m_XRControllerLeft = XRController.leftHand;
                }
                return m_XRControllerLeft;
            }
        }
        protected XRController m_XRControllerLeft = null;

        protected XRController _xrControllerRight
        {
            get
            {
                if (m_XRControllerRight == null)
                {
                    m_XRControllerRight = XRController.rightHand;
                }
                return m_XRControllerRight;
            }
        }
        protected XRController m_XRControllerRight = null;

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

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            Reset();

            if (Application.isPlaying)
            {
                for (int i = 0; i < _virtualAxes.Count; i++)
                {
                    string vAxis = _virtualAxes[i];
                    _virtualAxesState[vAxis] = 0;
                    _axisToID[vAxis] = i;
                }

                for (int i = 0; i < _virtualButtons.Count; i++)
                {
                    string vButton = _virtualButtons[i];
                    _virtualButtonsState[vButton] = VirtualButtonState.Idle;
                    _buttonToID[vButton] = i;
                }
            }
        }

        protected virtual void Start()
        {
            _validAxesMapping.Clear();
            for (int i = 0; i < _virtualAxes.Count; i++)
            {
                List<InputMap> maps = new List<InputMap>();
                foreach (BaseInputManager iManager in GetInputManagers())
                {
                    AxisMapping axisMapping = iManager.GetAxisMapping(i);
                    if (axisMapping._axisCode != BaseInputManager.NULL_MAPPING)
                    {
                        maps.Add(new InputMap(iManager, axisMapping._axisCode, InputMap.Type.Axis));
                    }
                    if (axisMapping._positiveButton._keyCode != BaseInputManager.NULL_MAPPING)
                    {
                        maps.Add(new InputMap(iManager, axisMapping._positiveButton._keyCode, InputMap.Type.Button, 1));
                    }
                    if (axisMapping._positiveButton._altKeyCode != BaseInputManager.NULL_MAPPING)
                    {
                        maps.Add(new InputMap(iManager, axisMapping._positiveButton._altKeyCode, InputMap.Type.Button, 1));
                    }
                    if (axisMapping._negativeButton._keyCode != BaseInputManager.NULL_MAPPING)
                    {
                        maps.Add(new InputMap(iManager, axisMapping._negativeButton._keyCode, InputMap.Type.Button, -1));
                    }
                    if (axisMapping._negativeButton._altKeyCode != BaseInputManager.NULL_MAPPING)
                    {
                        maps.Add(new InputMap(iManager, axisMapping._negativeButton._altKeyCode, InputMap.Type.Button, -1));
                    }
                }

                _validAxesMapping.Add(maps);
            }

            _validButtonsMapping.Clear();
            for (int i = 0; i < _virtualButtons.Count; i++)
            {
                List<InputMap> maps = new List<InputMap>();
                foreach (BaseInputManager iManager in GetInputManagers())
                {
                    ButtonMapping bMapping = iManager.GetButtonMapping(i);
                    if (bMapping._keyCode != BaseInputManager.NULL_MAPPING)
                    {
                        maps.Add(new InputMap(iManager, bMapping._keyCode, InputMap.Type.Button));
                    }
                    if (bMapping._altKeyCode != BaseInputManager.NULL_MAPPING)
                    {
                        maps.Add(new InputMap(iManager, bMapping._altKeyCode, InputMap.Type.Button));
                    }
                }

                _validButtonsMapping.Add(maps);
            }
        }

        protected virtual void Reset()
        {
            name = this.GetType().FullName;

            CreateDefaultAxes();
            CreateDefaultButtons();

            CreateDefaultImplementation<InputManagerKeyboard>();
            CreateDefaultImplementation<InputManagerMouse>();
            CreateDefaultImplementation<InputManagerGamepad>();
            CreateDefaultImplementation<InputManagerVR>();

            _inputActionsDefault.Enable();  //Make sure the InputActionsDefault are enabled
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

        public virtual T CreateDefaultImplementation<T>() where T : BaseInputManager
        {
            T iManager = GetComponentInChildren<T>();
            if (!iManager)
            {
                iManager = transform.CreateChild("").GetOrCreateComponent<T>();
                iManager.Reset();
            }

            if (!_inputManagers.Contains(iManager))
            {
                _inputManagers.Add(iManager);
            }

            return iManager;
        }

        protected virtual void OnEnable()
        {
            QuickVRManager.OnPostCalibrate += CheckHandsSwapped;
        }

        protected virtual void OnDisable()
        {
            QuickVRManager.OnPostCalibrate -= CheckHandsSwapped;
        }

        protected virtual void CheckHandsSwapped()
        {
            if (_xrControllerLeft != null && _xrControllerRight != null)
            {
                if (QuickSingletonManager.GetInstance<QuickVRPlayArea>().IsHandsSwapped())
                {
                    InputSystem.SetDeviceUsage(_xrControllerLeft, CommonUsages.RightHand);
                    InputSystem.SetDeviceUsage(_xrControllerRight, CommonUsages.LeftHand);
                }
                else
                {
                    InputSystem.SetDeviceUsage(_xrControllerLeft, CommonUsages.LeftHand);
                    InputSystem.SetDeviceUsage(_xrControllerRight, CommonUsages.RightHand);
                }
            }
        }

        #endregion

        #region GET AND SET

        public virtual int ToAxisID(string virtualAxisName)
        {
            int id = 0;
            if (Application.isPlaying)
            {
                id = _axisToID.ContainsKey(virtualAxisName) ? _axisToID[virtualAxisName] : -1;
            }
            else
            {
                for (; id < _virtualAxes.Count && _virtualAxes[id] != virtualAxisName; id++);
            }

            return id;
        }

        public virtual int ToButtonID(string virtualButtonName)
        {
            int id = 0;
            if (Application.isPlaying)
            {
                id = _buttonToID.ContainsKey(virtualButtonName) ? _buttonToID[virtualButtonName] : -1;
            }
            else
            {
                for (; id < _virtualButtons.Count && _virtualButtons[id] != virtualButtonName; id++) ;
            }

            return id;
        }

        public static InputActionAsset GetInputActionsDefault()
        {
            return _inputActionsDefault;
        }

        public List<BaseInputManager> GetInputManagers()
        {
            if (_inputManagers.Count != transform.childCount)
            {
                _inputManagers = new List<BaseInputManager>(GetComponentsInChildren<BaseInputManager>());
            }

            return _inputManagers;
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

            foreach (BaseInputManager manager in GetInputManagers())
            {
                manager.AddAxisMapping();
            }
        }

        [ButtonMethod]
        public void RemoveLastAxis()
        {
            if (_virtualAxes.Count == 0) return;
            _virtualAxes.RemoveAt(_virtualAxes.Count - 1);

            foreach (BaseInputManager manager in GetInputManagers())
            {
                manager.RemoveLastAxisMapping();
            }
        }

        [ButtonMethod]
        public void AddNewButton()
        {
            _virtualButtons.Add("New Button");

            foreach (BaseInputManager manager in GetInputManagers())
            {
                manager.AddButtonMapping();
            }
        }

        [ButtonMethod]
        public void RemoveLastButton()
        {
            if (_virtualButtons.Count == 0) return;
            _virtualButtons.RemoveAt(_virtualButtons.Count - 1);

            foreach (BaseInputManager manager in GetInputManagers())
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
            return _inputManager._virtualAxesState[axis];
        }

        public static bool GetButton(string button)
        {
            return _inputManager.IsVirtualButton(button)? _inputManager._virtualButtonsState[button] == VirtualButtonState.Pressed : false;
        }

        public static bool GetButtonDown(string button)
        {
            return _inputManager.IsVirtualButton(button) ? _inputManager._virtualButtonsState[button] == VirtualButtonState.Triggered : false;
        }

        public static bool GetButtonUp(string button)
        {
            return _inputManager.IsVirtualButton(button) ? _inputManager._virtualButtonsState[button] == VirtualButtonState.Released : false;
        }

        #endregion

        #region UPDATE

        public virtual void UpdateState()
        {
            UpdateStateVirtualAxes();
            UpdateStateVirtualButtons();
        }

        protected virtual void UpdateStateVirtualAxes()
        {
            for (int i = 0; i < _virtualAxes.Count; i++)
            {
                float aValue = 0;
                List<InputMap> inputMaps = _validAxesMapping[i];

                for (int j = 0; j < inputMaps.Count && aValue == 0; j++)
                {
                    InputMap iMap = inputMaps[j];
                    if (iMap._type == InputMap.Type.Axis)
                    {
                        aValue = iMap._inputManager.GetAxis(iMap._inputCode);
                    }
                    else
                    {
                        //The axis input is defined by a button. Check if the button mapped to the axis
                        //is pressed or not and return the corresponding value. 
                        bool bPressed = iMap._inputManager.GetButton(iMap._inputCode);
                        if (bPressed)
                        {
                            aValue = iMap._scale;
                        }
                    }
                }

                _virtualAxesState[_virtualAxes[i]] = aValue;
            }
        }

        protected virtual void UpdateStateVirtualButtons()
        {
            for (int i = 0; i < _virtualButtons.Count; i++)
            {
                string vButtonName = _virtualButtons[i];
                bool bPressed = false;
                List<InputMap> inputMaps = _validButtonsMapping[i];

                for (int j = 0; j < inputMaps.Count && !bPressed; j++)
                {
                    InputMap iMap = inputMaps[j];
                    bPressed = iMap._inputManager.GetButton(iMap._inputCode);
                }

                VirtualButtonState currentState = _virtualButtonsState[vButtonName];
                if (bPressed)
                {
                    if (currentState == VirtualButtonState.Idle)
                    {
                        _virtualButtonsState[vButtonName] = VirtualButtonState.Triggered;
                    }
                    else if (currentState == VirtualButtonState.Triggered)
                    {
                        _virtualButtonsState[vButtonName] = VirtualButtonState.Pressed;
                    }
                }
                else
                {
                    if (currentState == VirtualButtonState.Triggered || currentState == VirtualButtonState.Pressed)
                    {
                        _virtualButtonsState[vButtonName] = VirtualButtonState.Released;
                    }
                    else if (currentState == VirtualButtonState.Released)
                    {
                        _virtualButtonsState[vButtonName] = VirtualButtonState.Idle;
                    }
                }
            }
        }

        #endregion

    }

}


