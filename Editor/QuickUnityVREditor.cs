using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

namespace QuickVR
{
    [CustomEditor(typeof(QuickUnityVR), true)]
    public class QuickUnityVREditor : QuickBaseEditor
    {

        #region PROTECTED ATTRIBUTES

        protected QuickUnityVR _target = null;
        protected OVRProjectConfig _projectConfig = null;

        #endregion

        #region CREATION AND DESTRUCTIOn

        protected virtual void Awake()
        {
            _target = (QuickUnityVR)target;
            _projectConfig = OVRProjectConfig.GetProjectConfig();
            UpdateHandTrackingSupport();
        }

        #endregion

        #region GET AND SET

        protected OVRProjectConfig.HandTrackingSupport ToOVR(QuickUnityVR.HandTrackingMode hMode)
        {

            //if (hMode == QuickUnityVR.HandTrackingMode.Hands) return OVRProjectConfig.HandTrackingSupport.HandsOnly;
            //return OVRProjectConfig.HandTrackingSupport.ControllersOnly;

            return OVRProjectConfig.HandTrackingSupport.ControllersAndHands;
        }

        #endregion

        #region UPDATE

        protected virtual void UpdateHandTrackingSupport()
        {
            OVRProjectConfig.HandTrackingSupport hMode = ToOVR(_target._handTrackingMode);
            if (hMode != _projectConfig.handTrackingSupport)
            {
                _projectConfig.handTrackingSupport = hMode;
                OVRProjectConfig.CommitProjectConfig(_projectConfig);
            }
        }

        protected override void DrawGUI()
        {
            //base.DrawGUI();

            GUI.enabled = false;
            DrawPropertyField("m_Script", "Script");
            GUI.enabled = true;

            EditorGUI.BeginChangeCheck();

            GUIStyle style = EditorStyles.foldout;
            FontStyle previousStyle = style.fontStyle;
            style.fontStyle = FontStyle.Bold;
            _target._showControlsBody = EditorGUILayout.Foldout(_target._showControlsBody, "Body Controls");
            style.fontStyle = previousStyle;

            if (_target._showControlsBody)
            {
                EditorGUI.indentLevel++;
                foreach (HumanBodyBones boneID in QuickIKManager.GetIKLimbBones())
                {
                    EditorGUILayout.BeginVertical("box");
                    _target.SetControlBody(boneID, (QuickUnityVR.ControlType)EditorGUILayout.EnumPopup(boneID.ToString(), _target.GetControlBody(boneID)));
                    if (_target.GetControlBody(boneID) == QuickUnityVR.ControlType.IK)
                    {
                        GUI.enabled = false;
                        EditorGUILayout.ObjectField("IKTarget", _target.GetIKSolver(boneID)._targetLimb, typeof(Transform), true);
                        GUI.enabled = true;
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUI.indentLevel--;
            }

            DrawPropertyField("_useFootprints", "Use Footprints");
            DrawPropertyField("_handTrackingMode", "Hand Tracking Mode");


            UpdateHandTrackingSupport();

            if (EditorGUI.EndChangeCheck())
            {
                //serializedObject.ApplyModifiedProperties();
                QuickUtilsEditor.MarkSceneDirty();
            }
        }

        #endregion

    }

}
