using UnityEngine;
using UnityEngine.InputSystem;

using System.Collections;
using System.Collections.Generic;

namespace QuickVR
{

    [System.Serializable]
    public class InputManagerKeyboard : BaseInputManager
    {

        #region PROTECTED PARAMETERS

        protected static Dictionary<string, Key> _stringToKey = new Dictionary<string, Key>();
        
        protected Keyboard _keyboard
        {
            get
            {
                return Keyboard.current;
            }
        }

        #endregion

        #region CREATION AND DESTRUCTION

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        protected static void Init()
        {
            foreach (Key k in QuickUtils.GetEnumValues<Key>())
            {
                _stringToKey[k.ToString()] = k;
            }
        }

        public override void Reset()
        {
            base.Reset();

            //Configure the default axes
            ConfigureDefaultAxis(InputManager.DEFAULT_AXIS_HORIZONTAL, Key.D.ToString(), Key.A.ToString());
            ConfigureDefaultAxis(InputManager.DEFAULT_AXIS_VERTICAL, Key.W.ToString(), Key.S.ToString());
            
            //Configure the default buttons
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_CONTINUE, Key.Enter.ToString());
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_CANCEL, Key.Backspace.ToString());
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_EXIT, Key.Escape.ToString());
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_CALIBRATE, Key.R.ToString());
        }

        #endregion

        #region GET AND SET

        public override string[] GetButtonCodes()
        {
            return GetCodes<Key>();
        }

        protected override float ImpGetAxis(string axis)
        {
            return 0;
        }

        protected override bool ImpGetButton(string button)
        {
            bool result = false; 
            if (_keyboard != null)
            {
                Key key = _stringToKey[button];
                result = _keyboard[key].isPressed;
            }
            
            return result;
        }

        #endregion

    }

}
