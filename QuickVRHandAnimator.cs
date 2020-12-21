using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickVRHandAnimator : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        public bool _isLeftHand = true;

        [Range(0.0f, 1.0f)]
        public float _thumbClose = 0;

        [Range(0.0f, 1.0f)]
        public float _indexClose = 0;

        [Range(0.0f, 1.0f)]
        public float _middleClose = 0;

        [Range(0.0f, 1.0f)]
        public float _ringClose = 0;

        [Range(0.0f, 1.0f)]
        public float _littleClose = 0;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected Animator _animator = null;

        protected Transform _axisHand = null;
        
        protected Dictionary<QuickHumanFingers, Transform> _axisFingers = new Dictionary<QuickHumanFingers, Transform>();

        #endregion

        #region CONSTANTS

        protected const string AXIS_HAND_NAME = "__AxisHand__";
        protected const string AXIS_FINGER_NAME = "__AxisFinger__";
        
        protected const float MAX_CLOSE_ANGLE = 90.0f;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Start()
        {
            _animator = GetComponent<Animator>();

            CreateAxisHand();

            foreach (QuickHumanFingers f in QuickHumanTrait.GetHumanFingers())
            {
                List<QuickHumanBodyBones> boneFingers = QuickHumanTrait.GetBonesFromFinger(f, _isLeftHand);
                Transform tBoneProximal = _animator.GetBoneTransform(boneFingers[0]);
                Transform tBoneIntermediate = _animator.GetBoneTransform(boneFingers[1]);
                Transform axisFinger = tBoneProximal.CreateChild(AXIS_FINGER_NAME);
                axisFinger.LookAt(tBoneIntermediate.position, _axisHand.up);

                _axisFingers[f] = axisFinger;
            }
        }

        protected virtual void CreateAxisHand()
        {
            HumanBodyBones boneHandID = _isLeftHand ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand;

            _axisHand = _animator.GetBoneTransform(boneHandID).CreateChild(AXIS_HAND_NAME);
            QuickIKManager iKManager = GetComponent<QuickIKManager>();
            if (iKManager)
            {
                _axisHand.rotation = iKManager.GetIKSolver(boneHandID)._targetLimb.rotation;
            }
            else
            {
                float sign = _isLeftHand ? -1.0f : 1.0f;
                _axisHand.rotation = transform.rotation;
                _axisHand.Rotate(transform.up, sign * 90.0f, Space.World);
            }
        }

        #endregion

        #region UPDATE

        protected virtual void Update()
        {
            QuickHumanFingers[] fingers = QuickHumanTrait.GetHumanFingers();
            float[] fingersLeftClose = new float[] { _thumbClose, _indexClose, _middleClose, _ringClose, _littleClose };
            for (int i = 0; i < fingers.Length; i++)
            {
                QuickHumanFingers f = fingers[i];
                ResetFingerRotation(f);
                UpdateFinger(f, fingersLeftClose[i]);
            }
        }

        protected virtual void UpdateFinger(QuickHumanFingers finger, float fingerClose)
        {
            Vector3 rotAxis = _axisHand.right;
            float rotAngle = Mathf.Lerp(0.0f, MAX_CLOSE_ANGLE, fingerClose);

            List<QuickHumanBodyBones> fingerBones = QuickHumanTrait.GetBonesFromFinger(finger, _isLeftHand);
            for (int i = 0; i < 3; i++)
            {
                Transform tBone = _animator.GetBoneTransform(fingerBones[i]);
                tBone.Rotate(rotAxis, rotAngle, Space.World);
            }
        }

        protected virtual void ResetFingerRotation(QuickHumanFingers finger)
        {
            Vector3 targetDir = _axisHand.forward;

            List<QuickHumanBodyBones> fingerBones = QuickHumanTrait.GetBonesFromFinger(finger, _isLeftHand);
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


