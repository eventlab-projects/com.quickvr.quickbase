using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

namespace QuickVR
{

    [System.Serializable]
    public class InputManager : MonoBehaviour
    {

        #region PROTECTED PARAMETERS

        protected static InputActionMap _actionMapDefault = null;

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
            float value = 0.0f;
            if (_inputManager.IsActiveVirtualAxis(axis))
            {
                foreach (BaseInputManager iManager in _inputManager.GetInputManagers())
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
            if (_inputManager.IsActiveVirtualButton(button))
            {
                List<BaseInputManager> inputManagers = _inputManager.GetInputManagers();
                for (int i = 0; !inState && (i < inputManagers.Count); i++)
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
            foreach (BaseInputManager iManager in GetInputManagers())
            {
                iManager.UpdateMappingState();
            }
        }

        #endregion

    }

}


