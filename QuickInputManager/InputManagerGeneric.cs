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

        public override string[] GetAxisCodes()
        {
            return GetCodes<U>();
        }

        public override string[] GetButtonCodes()
        {
            return GetCodes<V>();
        }

        protected override float ImpGetAxis(string axis)
        {
            float result = 0;

            if (_inputDevice != null)
            {
                result = ImpGetAxis(_stringToAxis[axis]);
            }

            return result;
        }

        protected abstract float ImpGetAxis(U axis);

        protected override bool ImpGetButton(string button)
        {
            bool result = false;

            if (_inputDevice != null)
            {
                result = ImpGetButton(_stringToButton[button]);
            }

            return result;
        }

        protected abstract bool ImpGetButton(V button);

        #endregion

    }
}


