using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

namespace QuickVR
{
    [CustomEditor(typeof(QuickUnityVR), true)]
    public class QuickUnityVREditor : QuickIKManagerEditor
    {

        #region PROTECTED ATTRIBUTES

        protected QuickUnityVR _target = null;
        protected OVRProjectConfig _projectConfig = null;

        #endregion

        #region CREATION AND DESTRUCTION

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

        protected override void DrawIKSolverProperties(HumanBodyBones boneID)
        {
            _target.SetControlBody(boneID, (QuickUnityVR.ControlType)EditorGUILayout.EnumPopup(boneID.ToString(), _target.GetControlBody(boneID)));
            if (_target.GetControlBody(boneID) == QuickUnityVR.ControlType.IK)
            {
                DrawIKSolverPropertiesBase(_target.GetIKSolver(boneID));
            }
        }

        protected override void DrawIKSolverProperties(QuickHumanFingers f, bool isLeft)
        {
            _target.SetControlFinger(f, isLeft, (QuickUnityVR.ControlType)EditorGUILayout.EnumPopup(f.ToString(), _target.GetControlFinger(f, isLeft)));
            if (_target.GetControlFinger(f, isLeft) == QuickUnityVR.ControlType.IK)
            {
                DrawIKSolverPropertiesBase(_target.GetIKSolver(f, isLeft));
            }            
        }

        #endregion

    }

}
