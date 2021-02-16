using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;

namespace QuickVR
{
    ///<summary>
    ///T is The type of the InputDevice; 
    ///U is The enum type defining the Axis for this InputDevice; 
    ///V is The enum type defining the Buttons for this InputDevice;
    /// </summary>

    public abstract class InputManagerGeneric<T, U, V> : BaseInputManager
    where T : InputDevice   
    where U : struct
    where V : struct
    {

        #region PROTECTED ATTRIBUTES

        protected T _inputDevice
        {
            get
            {
                if (m_InputDevice == null)
                {
                    m_InputDevice = GetInputDevice();
                }

                return m_InputDevice;
            }
            set
            {
                m_InputDevice = value;
            }
        }
        private T m_InputDevice = null;
        
        protected Dictionary<string, U> _stringToAxis = new Dictionary<string, U>();
        protected Dictionary<string, V> _stringToButton = new Dictionary<string, V>();

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Awake()
        {
            foreach (U u in QuickUtils.GetEnumValues<U>())
            {
                _stringToAxis[u.ToString()] = u;
            }

            foreach (V v in QuickUtils.GetEnumValues<V>())
            {
                _stringToButton[v.ToString()] = v;
            }

            base.Awake();
        }

        #endregion

        #region GET AND SET

        protected abstract T GetInputDevice();

        protected virtual void SetInputDevice(U axis)
        {
            
        }

        protected virtual void SetInputDevice(V button)
        {
            
        }

        public override string[] GetAxisCodes()
        {
            return GetCodes<U>();
        }

        public override string[] GetButtonCodes()
        {
            return GetCodes<V>();
        }

        protected override float ImpGetAxis(string axisName)
        {
            float result = 0;

            U axis = _stringToAxis[axisName];
            SetInputDevice(axis);

            if (_inputDevice != null && _inputDevice.added)
            {
                result = ImpGetAxis(axis);
            }

            return result;
        }

        protected abstract float ImpGetAxis(U axis);

        protected override bool ImpGetButton(string buttonName)
        {
            bool result = false;

            V button = _stringToButton[buttonName];
            SetInputDevice(button);

            if (_inputDevice != null && _inputDevice.added)
            {
                result = ImpGetButton(button);
            }

            return result;
        }

        protected abstract bool ImpGetButton(V button);

        #endregion

    }
}


