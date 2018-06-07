using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace QuickVR
{
    
    [CustomPropertyDrawer(typeof(FloatRangeAttribute))]
    public class FloatRangeDrawer : PropertyDrawer
    {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            //var r = new Rect(position.xMin, position.yMin, position.width, EditorGUIUtility.singleLineHeight);
            //var maxProp = property.FindPropertyRelative("_maxValue");
            //EditorGUI.PropertyField(r, maxProp, "MaxValue");

            //r = new Rect(r.xMin, r.yMax, r.width, r.height);
            //var valueProp = property.FindPropertyRelative("_value");
            //valueProp.intValue = Mathf.Clamp(EditorGUI.IntField(r, "Value", valueProp.intValue), 0, maxProp.intValue);

            //EditorGUI.PropertyField(position, property, label, true);
            FloatRangeAttribute fRangeAtt = (FloatRangeAttribute)attribute;
            EditorGUI.Slider(position, property, 0, fRangeAtt._maxValue);
        }
    }


    [CustomPropertyDrawer(typeof(BitMaskAttribute))]
    public class EnumBitMaskPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            // Add the actual int value behind the field name
            //label.text = label.text + "("+prop.intValue+")";
            prop.intValue = DrawBitMaskField(position, prop.intValue, label);
        }

        protected virtual int DrawBitMaskField(Rect position, int aMask, GUIContent label)
        {
            BitMaskAttribute bMaskAtt = (BitMaskAttribute)attribute;
            var allEnumNames = System.Enum.GetNames(bMaskAtt._type);
            List<string> enumNames = new List<string>();
            for (int i = bMaskAtt._startID; i <= bMaskAtt._endID; i++)
            {
                enumNames.Add(allEnumNames[i]);
            }

            return EditorGUI.MaskField(position, label, aMask, enumNames.ToArray());
        }

    }

    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }

}
