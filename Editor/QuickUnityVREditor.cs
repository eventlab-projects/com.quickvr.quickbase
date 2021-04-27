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

            DrawControlsBody();
            DrawControlsHand(true);
            DrawControlsHand(false);

            DrawPropertyField("_useFootprints", "Use Footprints");
            DrawPropertyField("_handTrackingMode", "Hand Tracking Mode");


            UpdateHandTrackingSupport();

            if (EditorGUI.EndChangeCheck())
            {
                //serializedObject.ApplyModifiedProperties();
                QuickUtilsEditor.MarkSceneDirty();
            }
        }

        protected virtual void DrawControlsBody()
        {
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
                        QuickIKSolver ikSolver = _target.GetIKSolver(boneID);

                        EditorGUILayout.BeginHorizontal();
                        GUI.enabled = false;
                        EditorGUILayout.ObjectField("IKTarget", ikSolver._targetLimb, typeof(Transform), true);
                        GUI.enabled = true;
                        DrawButton("Reset", GUILayout.Width(52));
                        EditorGUILayout.EndHorizontal();

                        ikSolver._weightIKPos = EditorGUILayout.Slider("IKPosWeight", ikSolver._weightIKPos, 0, 1);
                        ikSolver._weightIKRot = EditorGUILayout.Slider("IKRotWeight", ikSolver._weightIKRot, 0, 1);
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUI.indentLevel--;
            }
        }

        protected virtual void DrawControlsHand(bool isLeft)
        {
            GUIStyle style = EditorStyles.foldout;
            FontStyle previousStyle = style.fontStyle;
            style.fontStyle = FontStyle.Bold;
            if (isLeft)
            {
                _target._showControlsFingersLeftHand = EditorGUILayout.Foldout(_target._showControlsFingersLeftHand, "Left Hand Fingers Controls");
            }
            else
            {
                _target._showControlsFingersRightHand = EditorGUILayout.Foldout(_target._showControlsFingersRightHand, "Right Hand Fingers Controls");
            }
            
            style.fontStyle = previousStyle;

            if (isLeft && _target._showControlsFingersLeftHand || !isLeft && _target._showControlsFingersRightHand)
            {
                EditorGUI.indentLevel++;
                foreach (QuickHumanFingers f in QuickHumanTrait.GetHumanFingers())
                {
                    EditorGUILayout.BeginVertical("box");
                    _target.SetControlFinger(f, isLeft, (QuickUnityVR.ControlType)EditorGUILayout.EnumPopup(f.ToString(), _target.GetControlFinger(f, isLeft)));
                    if (_target.GetControlFinger(f, isLeft) == QuickUnityVR.ControlType.IK)
                    {
                        GUI.enabled = false;
                        EditorGUILayout.ObjectField("IKTarget", _target.GetIKSolver(f, isLeft)._targetLimb, typeof(Transform), true);
                        GUI.enabled = true;
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUI.indentLevel--;
            }
        }

        #endregion

    }

}
