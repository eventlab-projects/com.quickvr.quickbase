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
        //protected OVRProjectConfig _projectConfig = null;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            _target = (QuickUnityVR)target;
            //_projectConfig = OVRProjectConfig.GetProjectConfig();
            UpdateHandTrackingSupport();
        }

        #endregion

        #region GET AND SET

        //protected OVRProjectConfig.HandTrackingSupport ToOVR(QuickUnityVR.HandTrackingMode hMode)
        //{

        //    //if (hMode == QuickUnityVR.HandTrackingMode.Hands) return OVRProjectConfig.HandTrackingSupport.HandsOnly;
        //    //return OVRProjectConfig.HandTrackingSupport.ControllersOnly;

        //    return OVRProjectConfig.HandTrackingSupport.ControllersAndHands;
        //}

        #endregion

        #region UPDATE

        protected override void DrawIKControls()
        {
            _target._applyHeadRotation = EditorGUILayout.Toggle("Apply Head Rotation", _target._applyHeadRotation);
            _target._applyHeadPosition = EditorGUILayout.Toggle("Apply Head Position", _target._applyHeadPosition);

            base.DrawIKControls();
        }

        protected virtual void UpdateHandTrackingSupport()
        {
            //OVRProjectConfig.HandTrackingSupport hMode = ToOVR(_target._handTrackingMode);
            //if (hMode != _projectConfig.handTrackingSupport)
            //{
            //    _projectConfig.handTrackingSupport = hMode;
            //    OVRProjectConfig.CommitProjectConfig(_projectConfig);
            //}
        }

        protected override void DrawIKSolverProperties(IKBone ikBone)
        {
            _target.SetIKControl(ikBone, (QuickUnityVR.ControlType)EditorGUILayout.EnumPopup(ikBone.ToString(), _target.GetIKControl(ikBone)));
            QuickUnityVR.ControlType cType = _target.GetIKControl(ikBone);
            if (cType != QuickUnityVR.ControlType.Animation)
            {
                if (cType == QuickUnityVR.ControlType.Tracking)
                {
                    //DrawIKTrackingOffsetProperty(ikBone);
                    Vector3 offset = EditorGUILayout.Vector3Field("Tracking Offset", _target.GetIKTrackingOffset(ikBone));
                    _target.SetIKTrackingOffset(ikBone, offset);
                }
                DrawIKSolverPropertiesBase(_target.GetIKSolver(ikBone));
            }
        }

        #endregion

    }

}
