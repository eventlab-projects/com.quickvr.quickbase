using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace QuickVR
{

    [InitializeOnLoad]
    public class QuickInputManagerInitializerEditor
    {

        #region PROTECTED ATTRIBUTES

        protected static SerializedProperty _pAxes = null;
        protected static SerializedObject _inputManager = null;

        protected static Dictionary<string, SerializedProperty> _axes = new Dictionary<string, SerializedProperty>();
        
        #endregion

        #region CREATION AND DESTRUCTION

        static QuickInputManagerInitializerEditor()
        {
            _inputManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
            _pAxes = _inputManager.FindProperty("m_Axes");
            for (int i = 0; i < _pAxes.arraySize; i++)
            {
                SerializedProperty axis = _pAxes.GetArrayElementAtIndex(i);

                string name = axis.FindPropertyRelative("m_Name").stringValue;
                if (name.Contains("QuickVR_") && !_axes.ContainsKey(name))
                {
                    _axes[name] = axis;
                }
            }

            CreateVRAxes();

            _inputManager.ApplyModifiedProperties();
        }

        protected static void CreateVRAxes()
        {
            List<InputManagerVR.AxisCodes> aCodes = QuickUtils.GetEnumValues<InputManagerVR.AxisCodes>();
            foreach (InputManagerVR.AxisCodes c in aCodes)
            {
                CreateVRAxis(c);
            }
        }

        protected static void CreateVRAxis(InputManagerVR.AxisCodes c)
        {
            string aName = "QuickVR_" + c.ToString();
            SerializedProperty p = GetOrCreateSerializedAxis(aName);
            
            p.FindPropertyRelative("positiveButton").stringValue = "";
            p.FindPropertyRelative("gravity").floatValue = 0;
            p.FindPropertyRelative("sensitivity").floatValue = 1;
            p.FindPropertyRelative("type").intValue = 2;
            p.FindPropertyRelative("axis").intValue = (int)c - 1;
        }

        #endregion

        #region GET AND SET

        protected static SerializedProperty GetOrCreateSerializedAxis(string aName)
        {
            if (!_axes.ContainsKey(aName))
            {
                _pAxes.arraySize++;
                _inputManager.ApplyModifiedProperties();
                _axes[aName] = _pAxes.GetArrayElementAtIndex(_pAxes.arraySize - 1);
            }

            SerializedProperty result = _axes[aName];

            result.FindPropertyRelative("m_Name").stringValue = aName;
            result.FindPropertyRelative("descriptiveName").stringValue = "";
            result.FindPropertyRelative("descriptiveNegativeName").stringValue = "";
            result.FindPropertyRelative("negativeButton").stringValue = "";
            result.FindPropertyRelative("altNegativeButton").stringValue = "";
            result.FindPropertyRelative("altPositiveButton").stringValue = "";
            result.FindPropertyRelative("dead").floatValue = 0.001f;
            result.FindPropertyRelative("snap").boolValue = false;
            result.FindPropertyRelative("invert").boolValue = false;
            result.FindPropertyRelative("joyNum").intValue = 0;

            return result;
        }

        #endregion

    }
}
