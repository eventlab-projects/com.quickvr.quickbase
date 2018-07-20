using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

namespace QuickVR
{

    [CustomEditor(typeof(QuickSpeedCurveAsset))]
    public class QuickSpeedCurveEditor : QuickBaseEditor
    {

        #region PROTECTED ATTRIBUTES

        protected SerializedProperty _propSpeedCurve = null;

        protected float _newKey = 0.0f;
        protected float _newValue = 0.0f;

        protected int _idKeyToRemove = 0;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void OnEnable()
        {
            _propSpeedCurve = serializedObject.FindProperty("_animationCurve");
        }

        #endregion

        protected override void DrawGUI()
        {
            base.DrawGUI();

            DrawAddKeyGUI();
            DrawRemoveKeyGUI();
        }

        protected virtual void DrawAddKeyGUI()
        {
            EditorGUILayout.Space();
            DrawHorizontalLine();

            _newKey = EditorGUILayout.FloatField("Frequency", _newKey);
            _newValue = EditorGUILayout.FloatField("Speed", _newValue);

            if (DrawButton("Add Key"))
            {
                AnimationCurve speedCurve = _propSpeedCurve.animationCurveValue;
                speedCurve.AddKey(_newKey, _newValue);
                _propSpeedCurve.animationCurveValue = speedCurve;

                

                _newKey = _newValue = 0;
            }
        }


        protected virtual void DrawRemoveKeyGUI()
        {
            EditorGUILayout.Space();
            DrawHorizontalLine();

            _idKeyToRemove = EditorGUILayout.IntField("ID Key to Remove", _idKeyToRemove);

            if (DrawButton("Remove Key"))
            {
                AnimationCurve speedCurve = _propSpeedCurve.animationCurveValue;
                if (_idKeyToRemove < speedCurve.length)
                {
                    speedCurve.RemoveKey(_idKeyToRemove);
                    _propSpeedCurve.animationCurveValue = speedCurve;

                    _idKeyToRemove = 0;
                }
            }
        }

    }

}
