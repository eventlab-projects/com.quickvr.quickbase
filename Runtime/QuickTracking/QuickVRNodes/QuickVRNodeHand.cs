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

        protected OVRTouchSample.Hand _handAnimator = null;
        
        protected const int NUM_BONES_PER_FINGER = 4;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void OnEnable()
        {
            QuickVRManager.OnPostUpdateVRNodes += UpdateVRNodeFingers;
        }

        protected virtual void OnDisable()
        {
            QuickVRManager.OnPostUpdateVRNodes -= UpdateVRNodeFingers;
        }

        #endregion

        #region GET AND SET

        public override void SetRole(QuickHumanBodyBones role)
        {
            base.SetRole(role);

            _handAnimator = Instantiate(Resources.Load<OVRTouchSample.Hand>("Prefabs/" + (role == QuickHumanBodyBones.LeftHand ? "pf_HandLeft" : "pf_HandRight")), transform);
            _handAnimator.transform.localPosition = Vector3.zero;

            foreach (SkinnedMeshRenderer r in _handAnimator.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                r.sharedMesh = null;
            }
        }

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

        protected override void UpdateTracking()
        {
            if (QuickVRManager._handTrackingMode == QuickVRManager.HandTrackingMode.Controllers)
            {
                foreach (SkinnedMeshRenderer r in _handAnimator.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    r.enabled = true;
                }

                base.UpdateTracking();
            }
        }

        protected override void UpdateTrackedPosition(Vector3 localPos)
        {
            base.UpdateTrackedPosition(localPos);

            _trackedObject.transform.position = _handAnimator._handOrigin.position;
        }

        protected override void UpdateTrackedRotation(Quaternion localRot)
        {
            base.UpdateTrackedRotation(localRot);

            _trackedObject.transform.rotation = _handAnimator._handOrigin.rotation;
        }

        protected virtual void UpdateVRNodeFingers()
        {
            //Update the nodes of the fingers
            const int numBonesPerFinger = 4;
            foreach (QuickHumanFingers f in QuickHumanTrait.GetHumanFingers())
            {
                List<QuickHumanBodyBones> fingerBones = QuickHumanTrait.GetBonesFromFinger(f, _isLeft);
                for (int i = 0; i < numBonesPerFinger; i++)
                {
                    int boneID = ((int)f) * numBonesPerFinger + i;
                    QuickVRNode nFinger = QuickSingletonManager.GetInstance<QuickVRPlayArea>().GetVRNode(fingerBones[i]);

                    //The finger is tracked.
                    Transform t = _handAnimator.GetBoneFingerTransform(f, i);
                    nFinger.transform.position = t.position;
                    nFinger.transform.rotation = t.rotation;

                    //Correct the rotation
                    //if (IsLeft())
                    //{
                    //    nFinger.transform.Rotate(Vector3.right, 180, Space.Self);
                    //    nFinger.transform.Rotate(Vector3.up, -90, Space.Self);
                    //}
                    //else
                    //{
                    //    nFinger.transform.Rotate(Vector3.up, 90, Space.Self);
                    //}

                    nFinger.SetTracked(true);
                }
            }
        }

        #endregion


    }

}


