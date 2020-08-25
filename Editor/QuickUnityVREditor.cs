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
            base.DrawGUI();

            UpdateHandTrackingSupport();
        }

        #endregion

    }

}
