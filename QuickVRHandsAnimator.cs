using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickVRHandsAnimator : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        [Header("Left Hand")]
        [Range(0.0f, 1.0f)]
        public float _leftThumbClose = 0;

        [Range(0.0f, 1.0f)]
        public float _leftIndexClose = 0;

        [Range(0.0f, 1.0f)]
        public float _leftMiddleClose = 0;

        [Range(0.0f, 1.0f)]
        public float _leftRingClose = 0;

        [Range(0.0f, 1.0f)]
        public float _leftLittleClose = 0;

        [Header("Right Hand")]
        [Range(0.0f, 1.0f)]
        public float _rightThumbClose = 0;

        [Range(0.0f, 1.0f)]
        public float _rightIndexClose = 0;

        [Range(0.0f, 1.0f)]
        public float _rightMiddleClose = 0;

        [Range(0.0f, 1.0f)]
        public float _rightRingClose = 0;

        [Range(0.0f, 1.0f)]
        public float _rightLittleClose = 0;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected Animator _animator = null;

        protected Transform _axisLeftHand = null;
        protected Transform _axisRightHand = null;

        #endregion

        #region CONSTANTS

        protected const string AXIS_LEFT_HAND_NAME = "__AxisLeftHand__";
        protected const string AXIS_RIGHT_HAND_NAME = "__AxisRightHand__";
        protected const string AXIS_LEFT_THUMB_NAME = "__AxisLeftThumb__";
        protected const string AXIS_RIGHT_THUMB_NAME = "__AxisRightThumb__";

        protected const float MAX_CLOSE_ANGLE = 90.0f;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Start()
        {
            _animator = GetComponent<Animator>();

            _axisLeftHand = CreateAxisHand(true);
            _axisRightHand = CreateAxisHand(false);
        }

        protected virtual Transform CreateAxisHand(bool isLeft)
        {
            HumanBodyBones boneHandID = isLeft ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand;

            Transform tAxis = _animator.GetBoneTransform(boneHandID).CreateChild(isLeft ? AXIS_LEFT_HAND_NAME : AXIS_RIGHT_HAND_NAME);
            QuickIKManager iKManager = GetComponent<QuickIKManager>();
            if (iKManager)
            {
                tAxis.rotation = iKManager.GetIKSolver(boneHandID)._targetLimb.rotation;
            }
            else
            {
                float sign = isLeft ? -1.0f : 1.0f;
                tAxis.rotation = transform.rotation;
                tAxis.Rotate(transform.up, sign * 90.0f, Space.World);
            }

            return tAxis;
        }

        #endregion

        #region UPDATE

        protected virtual void Update()
        {
            QuickHumanFingers[] fingers = QuickHumanTrait.GetHumanFingers();
            float[] fingersLeftClose = new float[] { _leftThumbClose, _leftIndexClose, _leftMiddleClose, _leftRingClose, _leftLittleClose };
            for (int i = 0; i < fingers.Length; i++)
            {
                QuickHumanFingers f = fingers[i];
                ResetFingerRotation(f, true);
                UpdateFinger(f, fingersLeftClose[i], true);
            }

            float[] fingersRightClose = new float[] { _rightThumbClose, _rightIndexClose, _rightMiddleClose, _rightRingClose, _rightLittleClose };
            for (int i = 0; i < fingers.Length; i++)
            {
                QuickHumanFingers f = fingers[i];
                ResetFingerRotation(f, false);
                UpdateFinger(f, fingersRightClose[i], false);
            }
        }

        protected virtual void UpdateFinger(QuickHumanFingers finger, float fingerClose, bool isLeft)
        {
            Vector3 rotAxis = isLeft ? _axisLeftHand.right : _axisRightHand.right;
            float rotAngle = Mathf.Lerp(0.0f, MAX_CLOSE_ANGLE, fingerClose);

            List<QuickHumanBodyBones> fingerBones = QuickHumanTrait.GetBonesFromFinger(finger, isLeft);
            for (int i = 0; i < 3; i++)
            {
                Transform tBone = _animator.GetBoneTransform(fingerBones[i]);
                tBone.Rotate(rotAxis, rotAngle, Space.World);
            }
        }

        protected virtual void ResetFingerRotation(QuickHumanFingers finger, bool isLeft)
        {
            Vector3 targetDir = isLeft ? _axisLeftHand.forward : _axisRightHand.forward;

            List<QuickHumanBodyBones> fingerBones = QuickHumanTrait.GetBonesFromFinger(finger, isLeft);
            for (int i = 0; i < 3; i++)
            {
                Transform tBone = _animator.GetBoneTransform(fingerBones[i]);
                Transform tBoneNext = _animator.GetBoneTransform(fingerBones[i + 1]);

                Vector3 currentDir = tBoneNext.position - tBone.position;
                Vector3 rotAxis = Vector3.Cross(currentDir, targetDir);
                float rotAngle = Vector3.Angle(currentDir, targetDir);
                
                tBone.Rotate(rotAxis, rotAngle, Space.World);
            }
        }

        #endregion

    }

}


