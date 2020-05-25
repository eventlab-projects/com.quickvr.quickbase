using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WebXR;

namespace QuickVR
{
    
    public class QuickWebXRHandlerHead : QuickWebXRHandlerBase
    {

        #region PROTECTED ATTRIBUTES

        protected Transform _leftEye = null;
        protected Transform _rightEye = null;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Reset()
        {
            _role = HumanBodyBones.Head;
        }

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Awake()
        {
            base.Awake();

            _leftEye = transform.parent.CreateChild("LeftEye");
            _rightEye = transform.parent.CreateChild("RightEye");
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            WebXRManager.Instance.OnHeadsetUpdate += OnHMDUpdate;
        }

        #endregion

        #region UPDATE

        private void OnHMDUpdate(
            Matrix4x4 leftProjectionMatrix,
            Matrix4x4 rightProjectionMatrix,
            Matrix4x4 leftViewMatrix,
            Matrix4x4 rightViewMatrix,
            Matrix4x4 sitStandMatrix)
        {
            WebXRMatrixUtil.SetTransformFromViewMatrix(_leftEye.transform, leftViewMatrix * sitStandMatrix.inverse);
            WebXRMatrixUtil.SetTransformFromViewMatrix(_rightEye.transform, rightViewMatrix * sitStandMatrix.inverse);
        }

        protected override void UpdateVRNode()
        {
            transform.position = (_leftEye.position + _rightEye.position) * 0.5f;
            transform.rotation = _leftEye.rotation;

            base.UpdateVRNode();
        }

        #endregion

    }

}


