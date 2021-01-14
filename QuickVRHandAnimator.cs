using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickVRHandAnimator : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        public bool _isLeftHand = true;

        [System.Serializable]
        public class FingerPose
        {

            #region PUBLIC ATTRIBUTES

            public QuickHumanFingers _finger;

            [Range(0.0f, 1.0f)]
            public float _close = 0;

            [Range(-1.0f, 1.0f)]
            public float _separation = 0;

            #endregion

            #region CREATION AND DESTRUCTION

            public FingerPose(QuickHumanFingers finger)
            {
                _finger = finger;
            }

            #endregion

            #region PROTECTED ATTRIBUTES

            protected Transform _axisFinger = null;

            #endregion

            #region GET AND SET

            public virtual Transform GetAxisFinger()
            {
                return _axisFinger;
            }

            public virtual void SetAxisFinger(Transform aFinger)
            {
                _axisFinger = aFinger;
            }

            public virtual Vector3 GetRotAxisClose()
            {
                return _finger == QuickHumanFingers.Thumb? -_axisFinger.up : _axisFinger.right;
            }

            public virtual Vector3 GetRotAxisSeparation()
            {
                return _finger == QuickHumanFingers.Thumb? _axisFinger.right : _axisFinger.up;
            }

            #endregion

        }

        public FingerPose[] _fingerPoses = null;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected Animator _animator = null;

        protected Transform _axisHand = null;

        #endregion

        #region CONSTANTS

        protected const string AXIS_HAND_NAME = "__AxisHand__";
        protected const string AXIS_FINGER_NAME = "__AxisFinger__";
        
        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Reset()
        {
            QuickHumanFingers[] fingers = QuickHumanTrait.GetHumanFingers();

            int numFingers = fingers.Length;
            _fingerPoses = new FingerPose[numFingers];
            
            for (int i = 0; i < numFingers; i++)
            {
                _fingerPoses[i] = new FingerPose(fingers[i]);
            }
        }

        protected virtual void Start()
        {
            if (_fingerPoses == null || _fingerPoses.Length != 5) Reset();

            _animator = GetComponent<Animator>();

            CreateAxisHand();

            QuickHumanFingers[] fingers = QuickHumanTrait.GetHumanFingers();
            for (int i = 0; i < fingers.Length; i++)
            {
                List<QuickHumanBodyBones> boneFingers = QuickHumanTrait.GetBonesFromFinger(fingers[i], _isLeftHand);
                Transform tBoneProximal = _animator.GetBoneTransform(boneFingers[0]);
                Transform tBoneIntermediate = _animator.GetBoneTransform(boneFingers[1]);
                Transform axisFinger = tBoneProximal.CreateChild(AXIS_FINGER_NAME);
                axisFinger.LookAt(tBoneIntermediate.position, _axisHand.up);

                _fingerPoses[i].SetAxisFinger(axisFinger);
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

        protected virtual float GetMaxAngleClose(QuickHumanFingers finger)
        {
            return finger == QuickHumanFingers.Thumb? 45.0f : 90.0f;
        }

        protected virtual float GetMaxAngleSeparation(QuickHumanFingers finger)
        {
            return finger == QuickHumanFingers.Thumb ? 60.0f : 10.0f;
        }

        #endregion

        #region UPDATE

        protected virtual void Update()
        {
            foreach (FingerPose fPose in _fingerPoses)
            {
                ResetFingerRotation(fPose);
                UpdateFingerClose(fPose);
                UpdateFingerSeparation(fPose);
            }
        }

        protected virtual void ResetFingerRotation(FingerPose fPose)
        {
            //Align each finger's Axis with AxisHand
            Transform axisFinger = fPose.GetAxisFinger();

            List<QuickHumanBodyBones> fingerBones = QuickHumanTrait.GetBonesFromFinger(fPose._finger, _isLeftHand);
            AlignFingerBone(axisFinger.up, _axisHand.up, _animator.GetBoneTransform(fingerBones[0]));

            Vector3 targetDir = _axisHand.forward;
            if (fPose._finger == QuickHumanFingers.Thumb)
            {
                targetDir = Vector3.Lerp(_axisHand.right, _axisHand.forward, 0.5f);
            }

            for (int i = 0; i < 3; i++)
            {
                Transform tBone = _animator.GetBoneTransform(fingerBones[i]);
                Transform tBoneNext = _animator.GetBoneTransform(fingerBones[i + 1]);

                AlignFingerBone(tBoneNext.position - tBone.position, targetDir, _animator.GetBoneTransform(fingerBones[i]));
            }
        }

        protected virtual void AlignFingerBone(Vector3 currentDir, Vector3 targetDir, Transform tBone)
        {
            Vector3 rotAxis = Vector3.Cross(currentDir, targetDir);
            float rotAngle = Vector3.Angle(currentDir, targetDir);

            tBone.Rotate(rotAxis, rotAngle, Space.World);
        }

        protected virtual void UpdateFingerClose(FingerPose fPose)
        {
            Vector3 rotAxis = fPose.GetRotAxisClose();
            float maxAngle = GetMaxAngleClose(fPose._finger);
            
            List<QuickHumanBodyBones> fingerBones = QuickHumanTrait.GetBonesFromFinger(fPose._finger, _isLeftHand);
            for (int i = 0; i < 3; i++)
            {
                float rotAngle = Mathf.Lerp(0.0f, maxAngle, fPose._close);
                if (i == 0 && fPose._finger == QuickHumanFingers.Thumb)
                {
                    rotAngle = Mathf.Min(25.0f, rotAngle);
                }

                Transform tBone = _animator.GetBoneTransform(fingerBones[i]);
                tBone.Rotate(rotAxis, rotAngle, Space.World);
            }
        }

        protected virtual void UpdateFingerSeparation(FingerPose fPose)
        {
            Vector3 rotAxis = fPose.GetRotAxisSeparation();
            float t = (fPose._separation + 1) / 2.0f;
            float maxAngle = GetMaxAngleSeparation(fPose._finger);            
            float rotAngle = Mathf.Lerp(-maxAngle, maxAngle, t);

            List<QuickHumanBodyBones> fingerBones = QuickHumanTrait.GetBonesFromFinger(fPose._finger, _isLeftHand);
            Transform tBone = _animator.GetBoneTransform(fingerBones[0]);

            tBone.Rotate(rotAxis, rotAngle, Space.World);
        }

        #endregion

    }

}


