//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//using UnityEditor;

//namespace QuickVR
//{
//    [CustomEditor(typeof(QuickOVRHandsInitializer), true)]
//    public class QuickOVRHandsInitializerEditor : QuickBaseEditor
//    {

//        #region PROTECTED ATTRIBUTES

//        protected QuickUnityVR _hTracking = null;
//        protected QuickOVRHandsInitializer _target = null;
//        protected OVRProjectConfig _projectConfig = null;

//        #endregion

//        #region CREATION AND DESTRUCTIOn

//        protected virtual void Awake()
//        {
//            _target = (QuickOVRHandsInitializer)target;
//            _hTracking = _target.GetComponent<QuickUnityVR>();
//            _projectConfig = OVRProjectConfig.GetProjectConfig();
//        }

//        #endregion
        
//        protected override void DrawGUI()
//        {
//            base.DrawGUI();

//            OVRProjectConfig.HandTrackingSupport hMode = ToOVR(_hTracking._handTrackingMode);
//            if (hMode != _projectConfig.handTrackingSupport)
//            {
//                _projectConfig.handTrackingSupport = hMode;
//                OVRProjectConfig.CommitProjectConfig(_projectConfig);
//            }
//        }

//        #region GET AND SET

//        protected OVRProjectConfig.HandTrackingSupport ToOVR(QuickUnityVR.HandTrackingMode hMode)
//        {
//            if (hMode == QuickUnityVR.HandTrackingMode.Hands) return OVRProjectConfig.HandTrackingSupport.HandsOnly;
//            return OVRProjectConfig.HandTrackingSupport.ControllersOnly;
//        }

//        #endregion

//    }

//}

