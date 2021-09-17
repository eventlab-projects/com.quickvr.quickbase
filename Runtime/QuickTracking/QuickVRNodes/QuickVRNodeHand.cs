using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR;

namespace QuickVR
{
    
    public class QuickVRNodeHand : QuickVRNode
    {

        #region PUBLIC ATTRIBUTES

        public bool _isLeft = true;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected QuickVRHandAnimator _handAnimator = null;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Awake()
        {
            base.Awake();

            //_showModel = true;
        }

        protected virtual void OnEnable()
        {
            QuickVRManager.OnSourceAnimatorSet += OnSourceAnimatorSet;
            QuickVRManager.OnPostUpdateTracking += OnPostUpdateTracking;
        }

        private void OnPostUpdateTracking()
        {
            if (!_isLeft)
            {
                _handAnimator._fingerPoses[0]._close = 0.25f;
                _handAnimator._fingerPoses[0]._separation = 0.5f;

                _handAnimator._fingerPoses[1]._close = InputManagerVR.GetKey(InputManagerVR.ButtonCodes.RightTriggerTouch)? 0.25f : 0f;
                //_handAnimator._fingerPoses[1]._separation = 0;

                _handAnimator._fingerPoses[2]._close = 0.65f;
                //_handAnimator._fingerPoses[2]._separation = 0;

                _handAnimator._fingerPoses[3]._close = 0.8f;
                //_handAnimator._fingerPoses[3]._separation = 0;

                _handAnimator._fingerPoses[4]._close = 0.8f;
                //_handAnimator._fingerPoses[4]._separation = 0;
                //Debug.Log(InputManagerVR.GetKey(InputManagerVR.ButtonCodes.RightTriggerTouch));
            }

            _handAnimator.Update();

            
        }

        protected virtual void OnDisable()
        {
            QuickVRManager.OnSourceAnimatorSet -= OnSourceAnimatorSet;
        }

        private void OnSourceAnimatorSet(Animator animator)
        {
            if (animator)
            {
                _handAnimator = animator.gameObject.AddComponent<QuickVRHandAnimator>(); 
                _handAnimator._isLeftHand = _isLeft;
                _handAnimator.enabled = false;
                _handAnimator.Init();
            }
        }

        #endregion

        #region GET AND SET

        protected override string GetVRModelName()
        {
            string modelName = "";
            QuickVRManager.HMDModel hmdModel = QuickVRManager._hmdModel;

            if (hmdModel == QuickVRManager.HMDModel.HTCVive)
            {
                modelName = "pf_VIVE_Controller";
            }
            else if (hmdModel == QuickVRManager.HMDModel.OculusQuest)
            {
                modelName = _isLeft? "pf_OculusCV1_Controller_Left" : "pf_OculusCV1_Controller_Right";
            }
            else if (hmdModel == QuickVRManager.HMDModel.OculusQuest2)
            {
                modelName = _isLeft ? "pf_Oculus_Quest2_Controller_Left" : "pf_Oculus_Quest2_Controller_Right";
            }

            return modelName;
        }

        protected override bool GetDevicePosition(out Vector3 pos)
        {
            return _inputDevice.TryGetFeatureValue(QuickVRUsages.pointerPosition, out pos) || base.GetDevicePosition(out pos);
        }

        protected override bool GetDeviceRotation(out Quaternion rot)
        {
            return _inputDevice.TryGetFeatureValue(QuickVRUsages.pointerRotation, out rot) || base.GetDeviceRotation(out rot);
        }

        #endregion

        #region UPDATE

        protected override InputDevice CheckDevice()
        {
            return InputDevices.GetDeviceAtXRNode(_isLeft? XRNode.LeftHand : XRNode.RightHand);
        }

        protected override void UpdateTrackedPosition(Vector3 localPos)
        {
            base.UpdateTrackedPosition(localPos);

            if (_model && _model.Find("HandBone"))
            {
                _trackedObject.transform.position = _model.Find("HandBone").position;
            }
        }

        protected override void UpdateTrackedRotation(Quaternion localRot)
        {
            base.UpdateTrackedRotation(localRot);

            if (_model && _model.Find("HandBone"))
            {
                _trackedObject.transform.rotation = _model.Find("HandBone").rotation;
            }
        }

        #endregion


    }

}


