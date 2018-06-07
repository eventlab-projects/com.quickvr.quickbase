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

        #endregion

        #region CREATION AND DESTRUCTION

        public override void Reset()
        {
            base.Reset();

            //Configure the default axes
            ConfigureDefaultAxis(InputManager.DEFAULT_AXIS_HORIZONTAL, KeyCode.D, KeyCode.A);
            ConfigureDefaultAxis(InputManager.DEFAULT_AXIS_VERTICAL, KeyCode.W, KeyCode.S);
            
            //Configure the default buttons
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_CONTINUE, KeyCode.Return, KeyCode.JoystickButton0);
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_CANCEL, KeyCode.Backspace, KeyCode.JoystickButton1);
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_EXIT, KeyCode.Escape);
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_CALIBRATE, KeyCode.R, KeyCode.JoystickButton3);
        }

        public virtual void ConfigureDefaultAxis(string virtualAxisName, KeyCode kPositive, KeyCode kNegative)
        {
            AxisMapping aMapping = GetAxisMapping(virtualAxisName);
            if (aMapping == null) return;

            aMapping._axisCode = virtualAxisName;
            if (aMapping.GetPositiveButton()._keyCode == NULL_MAPPING) aMapping.GetPositiveButton()._keyCode = kPositive.ToString();
            if (aMapping.GetNegativeButton()._keyCode == NULL_MAPPING) aMapping.GetNegativeButton()._keyCode = kNegative.ToString();
        }

        public virtual void ConfigureDefaultButton(string virtualButtonName, KeyCode key, KeyCode altKey = KeyCode.None)
        {
            ButtonMapping bMapping = GetButtonMapping(virtualButtonName);
            if (bMapping == null) return;

            if (bMapping._keyCode == NULL_MAPPING) bMapping._keyCode = key.ToString();
            if (bMapping._altKeyCode == NULL_MAPPING) bMapping._altKeyCode = altKey.ToString();
        }

        #endregion

        #region GET AND SET

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
            if ((axis == AxisCode.Mouse_X.ToString()) || (axis == AxisCode.Mouse_Y.ToString()))
            {
                axis = axis.Replace('_', ' ');			//This is to transform from Mouse_X/Y to Mouse X/Y
            }
            float aValue = Input.GetAxis(axis);
            return aValue;
        }

        protected override bool ImpGetButton(string button)
        {
            KeyCode key = (KeyCode)System.Enum.Parse(typeof(KeyCode), button);
            return Input.GetKey(key);
        }

        #endregion

    }

}
