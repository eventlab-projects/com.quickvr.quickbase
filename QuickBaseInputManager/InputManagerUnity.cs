using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace QuickVR
{

    [System.Serializable]
    public class InputManagerUnity : BaseInputManager
    {

        #region PROTECTED PARAMETERS

        protected enum AxisCode
        {
            Horizontal,
            Vertical,
            Mouse_X,
            Mouse_Y,
        };

        protected static Dictionary<string, KeyCode> _stringToKeyCode = new Dictionary<string, KeyCode>();
        protected static Dictionary<KeyCode, string> _keyCodeToString = new Dictionary<KeyCode, string>();
        protected static Dictionary<AxisCode, string> _axisCodeToString = new Dictionary<AxisCode, string>();

        #endregion

        #region CREATION AND DESTRUCTION

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        protected static void Init()
        {
            foreach (KeyCode k in QuickUtils.GetEnumValues<KeyCode>())
            {
                _stringToKeyCode[k.ToString()] = k;
                _keyCodeToString[k] = k.ToString();
            }

            foreach (AxisCode c in QuickUtils.GetEnumValues<AxisCode>())
            {
                _axisCodeToString[c] = c.ToString();
            }
        }

        public override void Reset()
        {
            base.Reset();

            //Configure the default axes
            ConfigureDefaultAxis(InputManager.DEFAULT_AXIS_HORIZONTAL, KeyCode.D.ToString(), KeyCode.A.ToString());
            ConfigureDefaultAxis(InputManager.DEFAULT_AXIS_VERTICAL, KeyCode.W.ToString(), KeyCode.S.ToString());
            
            //Configure the default buttons
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_CONTINUE, KeyCode.Return.ToString(), KeyCode.JoystickButton0.ToString());
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_CANCEL, KeyCode.Backspace.ToString(), KeyCode.JoystickButton1.ToString());
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_EXIT, KeyCode.Escape.ToString());
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_CALIBRATE, KeyCode.R.ToString(), KeyCode.JoystickButton3.ToString());
        }

        #endregion

        #region GET AND SET

        public static KeyCode ToKeyCode(string keyName)
        {
            return _stringToKeyCode[keyName];
        }

        public static string ToString(KeyCode k)
        {
            return _keyCodeToString[k];
        }

        protected static string ToString(AxisCode c)
        {
            return _axisCodeToString[c];
        }

        public override string[] GetAxisCodes()
        {
            List<string> virtualAxes = new List<string>();
            virtualAxes.Add(BaseInputManager.NULL_MAPPING);

#if UNITY_EDITOR
            UnityEditor.SerializedObject inputManager = new UnityEditor.SerializedObject(UnityEditor.AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
            UnityEditor.SerializedProperty pAxes = inputManager.FindProperty("m_Axes");
            for (int i = 0; i < pAxes.arraySize; i++)
            {
                var axis = pAxes.GetArrayElementAtIndex(i);

                string name = axis.FindPropertyRelative("m_Name").stringValue;
                if (!name.Contains("QuickVR_") && !virtualAxes.Contains(name) && axis.FindPropertyRelative("type").intValue == 2) virtualAxes.Add(name);
            }
#endif

            return virtualAxes.ToArray();
        }

        public override string[] GetButtonCodes()
        {
            return GetCodes<KeyCode>();
        }

        protected override float ImpGetAxis(string axis)
        {
            if (axis == ToString(AxisCode.Mouse_X) || axis == ToString(AxisCode.Mouse_Y))
            {
                axis = axis.Replace('_', ' ');			//This is to transform from Mouse_X/Y to Mouse X/Y
            }
            float aValue = Input.GetAxis(axis);
            return aValue;
        }

        protected override bool ImpGetButton(string button)
        {
            KeyCode key = ToKeyCode(button);
#if UNITY_WEBGL 
            if ((int)key >= (int)KeyCode.JoystickButton0)
            {
                return false;
            }
#endif
            return Input.GetKey(key);
        }

        #endregion

    }

}
