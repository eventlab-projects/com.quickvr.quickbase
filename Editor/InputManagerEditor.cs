using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace QuickVR
{

    [CustomEditor(typeof(InputManager), true)]
    public class InputManagerEditor : QuickBaseEditor
    {

        protected InputManager _inputManager
        {
            get
            {
                if (!m_InputManager)
                {
                    m_InputManager = target as InputManager;
                }

                return m_InputManager;
            }
        }
        private InputManager m_InputManager = null;

        protected bool _showVirtualAxes = true;
        protected bool _showVirtualButtons = true;

        protected override void DrawGUI()
        {
            EditorGUI.BeginChangeCheck();
            DrawVirtualInput(ref _showVirtualAxes, true);
            DrawVirtualInput(ref _showVirtualButtons, false);
            if (EditorGUI.EndChangeCheck())
            {
                QuickUtilsEditor.MarkSceneDirty();
            }
        }

        protected virtual void DrawVirtualInput(ref bool foldout, bool isAxis)
        {
            GUIStyle style = EditorStyles.foldout;
            FontStyle previousStyle = style.fontStyle;
            style.fontStyle = FontStyle.Bold;
            foldout = EditorGUILayout.Foldout(foldout, isAxis? "Virtual Axes" : "Virtual Buttons", style);
            style.fontStyle = previousStyle;

            if (foldout)
            {
                DrawControlButtons(isAxis);

                EditorGUILayout.BeginVertical("box");
                SerializedProperty virtualInput = serializedObject.FindProperty(isAxis ? "_virtualAxes" : "_virtualButtons"); 
                for (int i = 0; i < virtualInput.arraySize; i++)
                {
                    GUI.enabled = i >= (isAxis ? InputManager.NUM_DEFAULT_AXES : InputManager.NUM_DEFAULT_BUTTONS);
                    
                    EditorGUI.BeginChangeCheck();
                    SerializedProperty p = virtualInput.GetArrayElementAtIndex(i);
                    p.stringValue = EditorGUILayout.TextField((isAxis? "Axis " : "Button ") + i.ToString(), p.stringValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                    }

                    GUI.enabled = true;
                }
                EditorGUILayout.EndVertical();
            }
        }

        protected virtual void DrawControlButtons(bool isAxis)
        {
            EditorGUILayout.BeginHorizontal();
            if (DrawButton("Add New"))
            {
                if (isAxis)
                {
                    _inputManager.AddVirtualAxis("New Axis");
                }
                else
                {
                    _inputManager.AddVirtualButton("New Button");
                }
            }
            if (DrawButton("Remove Last"))
            {
                if (isAxis)
                {
                    _inputManager.RemoveLastVirtualAxis();
                }
                else
                {
                    _inputManager.RemoveLastVirtualButton();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

    }

}


