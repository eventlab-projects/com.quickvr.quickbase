﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickIKSolver : MonoBehaviour
    {

        #region PUBLIC PARAMETERS

        public bool _enableIK = true;

        public HumanBodyBones _boneID
        {
            get
            {
                if (m_BoneID == HumanBodyBones.LastBone)
                {
                    if (_boneLimb)
                    {
                        Animator animator = GetComponentInParent<Animator>();
                        foreach (HumanBodyBones boneID in QuickHumanTrait.GetHumanBodyBones())
                        {
                            if (_boneLimb == animator.GetBoneTransform(boneID))
                            {
                                m_BoneID = boneID;
                                break;
                            }
                        }
                    }
                }

                return m_BoneID;
            }
        }
        protected HumanBodyBones m_BoneID = HumanBodyBones.LastBone;

        public virtual Transform _boneUpper
        {
            get
            {
                return m_boneUpper;
            }
            set
            {
                if (m_boneUpper == null)
                {
                    _initialLocalRotationUpper = value.localRotation;
                }

                m_boneUpper = value;
            }
        }

        public virtual Transform _boneMid
        {
            get
            {
                return m_boneMid;
            }
            set
            {
                if (m_boneMid == null)
                {
                    _initialLocalRotationMid = value.localRotation;
                }

                m_boneMid = value;
            }
        }

        public virtual Transform _boneLimb
        {
            get
            {
                return m_boneLimb;
            }
            set
            {
                if (_boneLimb == null)
                {
                    _initialLocalRotationLimb = value.localRotation;
                }

                m_boneLimb = value;
            }
        }

        public virtual Transform _targetLimb
        {
            get
            {
                return m_targetLimb;
            }
            set
            {
                m_targetLimb = value;
            }
        }

        public virtual Transform _targetHint
        {
            get
            {
                if (!m_targetHint && _boneUpper)
                {
                    foreach (Transform t in _boneUpper)
                    {
                        if (t.name.Contains(QuickIKManager.IK_TARGET_PREFIX))
                        {
                            m_targetHint = t;
                            break;
                        }
                    }
                }
                return m_targetHint;
            }
            set
            {
                m_targetHint = value;
                m_targetHint.parent = _boneUpper;
            }
        }

        public virtual float _weight
        {
            get
            {
                return 1.0f;
            }
            set
            {
                
            }
        }

        public virtual float _weightIKPos
        {
            get
            {
                return m_weightIKPos;
            }
            set
            {
                m_weightIKPos = value;
            }
        }

        public virtual float _weightIKRot
        {
            get
            {
                return m_weightIKRot;
            }
            set
            {
                m_weightIKRot = value;
            }
        }

        public virtual float _weightIKHint
        {
            get
            {
                return _targetHint ? 1.0f : 0.0f;
            }
            set
            {

            }
        }

        #endregion

        #region PROTECTED PARAMETERS

        //The bone chain hierarchy
        [SerializeField, ReadOnly]
        protected Transform m_boneUpper = null;

        [SerializeField, ReadOnly]
        protected Transform m_boneMid = null;

        [SerializeField, ReadOnly]
        protected Transform m_boneLimb = null;

        //The IK parameters
        [SerializeField, ReadOnly]
        protected Transform m_targetLimb = null;

        [SerializeField, ReadOnly]
        protected Transform m_targetHint = null;

        [SerializeField, ReadOnly]
        protected Quaternion _initialLocalRotationUpper = Quaternion.identity;

        [SerializeField, ReadOnly]
        protected Quaternion _initialLocalRotationMid = Quaternion.identity;

        [SerializeField, ReadOnly]
        protected Quaternion _initialLocalRotationLimb = Quaternion.identity;

        [SerializeField, ReadOnly]
        protected Vector3 _initialLocalPositionTargetLimb = Vector3.zero;

        [SerializeField, ReadOnly]
        protected Quaternion _initialLocalRotationTargetLimb = Quaternion.identity;

        [SerializeField, ReadOnly]
        protected Vector3 _initialLocalPositionTargetHint = Vector3.zero;

        protected float _lengthUpper = 0;
        protected float _lengthMid = 0;

        [SerializeField, Range(0.0f, 1.0f)]
        protected float m_weightIKPos = 1.0f;

        [SerializeField, Range(0.0f, 1.0f)]
        protected float m_weightIKRot = 1.0f;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            if (_boneUpper) _initialLocalRotationUpper = _boneUpper.localRotation;
            if (_boneMid) _initialLocalRotationMid = _boneMid.localRotation;
            if (_boneLimb) _initialLocalRotationLimb = _boneLimb.localRotation;

            if (_targetLimb)
            {
                _initialLocalPositionTargetLimb = _targetLimb.localPosition;
                _initialLocalRotationTargetLimb = _targetLimb.localRotation;
            }
            if (_targetHint) _initialLocalPositionTargetHint = _targetHint.localPosition;
        }

        #endregion

        #region GET AND SET

        public virtual void SaveCurrentPose()
        {
            _initialLocalPositionTargetLimb = _targetLimb.localPosition;
            _initialLocalRotationTargetLimb = _targetLimb.localRotation;
            if (_targetHint) _initialLocalPositionTargetHint = _targetHint.localPosition;
        }

        public virtual float GetUpperLength()
        {
            if (_lengthUpper == 0)
            {
                _lengthUpper = Vector3.Distance(_boneUpper.position, _boneMid.position);
            }

            return _lengthUpper;
        }

        public virtual float GetMidLength()
        {
            if (_lengthMid == 0)
            {
                _lengthMid = Vector3.Distance(_boneMid.position, _boneLimb.position);
            }

            return _lengthMid;
        }

        public virtual float GetChainLength()
        {
            return GetUpperLength() + GetMidLength();
        }

        protected virtual Vector3 GetIKTargetLimbPosition()
        {
            Vector3 v = _targetLimb.position - _boneUpper.position;
            return _boneUpper.position + (v.normalized * Mathf.Min(v.magnitude, GetChainLength()));
        }

        protected virtual Vector3 GetIKTargetHintPosition()
        {
            return _targetHint.position;
        }

        public virtual void ResetIKChain()
        {
            _boneUpper.localRotation = _initialLocalRotationUpper;
            _boneMid.localRotation = _initialLocalRotationMid;
            _boneLimb.localRotation = _initialLocalRotationLimb;
        }

        public virtual void Calibrate()
        {
            ResetIKChain();

            _targetLimb.localPosition = _initialLocalPositionTargetLimb;
            _targetLimb.localRotation = _initialLocalRotationTargetLimb;
            if (_targetHint) _targetHint.localPosition = _initialLocalPositionTargetHint;
        }

        public virtual Vector3 GetInitialLocalPosTargetLimb()
        {
            return _initialLocalPositionTargetLimb;
        }

        public virtual Quaternion GetInitialLocalRotTargetLimb()
        {
            return _initialLocalRotationTargetLimb;
        }

        public virtual Vector3 GetInitialLocalPosTargetHint()
        {
            return _initialLocalPositionTargetHint;
        }

        #endregion


        #region UPDATE

        protected virtual void LateUpdate()
        {
            if (!GetComponentInParent<QuickIKManager>())
            {
                UpdateIK();
            }
        }

        public virtual void UpdateIK()
        {
            if (!_enableIK || !_boneUpper || !_boneMid || !_boneLimb || !_targetLimb) return;

            Quaternion animBoneUpperRot = _boneUpper.rotation;
            Quaternion animBoneMidRot = _boneMid.rotation;
            Quaternion animBoneLimbRot = _boneLimb.rotation;

            ResetIKChain();

            Quaternion ikBoneUpperRot, ikBoneMidRot, ikBoneLimbRot;

            ComputeIKPosition(out ikBoneUpperRot, out ikBoneMidRot);
            ComputeIKRotation(out ikBoneLimbRot);

            _boneUpper.rotation = Quaternion.Lerp(animBoneUpperRot, ikBoneUpperRot, _weightIKPos);
            _boneMid.rotation = Quaternion.Lerp(animBoneMidRot, ikBoneMidRot, _weightIKPos);
            _boneLimb.rotation = Quaternion.Lerp(animBoneLimbRot, ikBoneLimbRot, _weightIKRot);
        }

        protected virtual void ComputeIKPosition(out Quaternion boneUpperRot, out Quaternion boneMidRot)
        {
            float upperLength = GetUpperLength();
            float midLength = GetMidLength();
            float chainLength = GetChainLength() * 0.9999f;

            //Align the bone with the target limb
            Vector3 ikTargetLimbPos = GetIKTargetLimbPosition();
            Vector3 u = (_boneMid.position - _boneUpper.position);
            Vector3 v = (ikTargetLimbPos - _boneUpper.position);
            float rotAngle = Vector3.Angle(u, v);
            _boneUpper.Rotate(Vector3.Cross(u, v).normalized, rotAngle, Space.World);

            if (_targetHint && _boneLimb)
            {
                //Apply the ikAngle to the boneUpper. The ikAngle is computed using the cosine rule
                float targetDistance = Mathf.Min(Vector3.Distance(_boneUpper.position, ikTargetLimbPos), chainLength);
                float cos = (Mathf.Pow(midLength, 2) - Mathf.Pow(upperLength, 2) - Mathf.Pow(targetDistance, 2)) / (-2 * upperLength * targetDistance);
                float ikAngle = Mathf.Acos(cos) * Mathf.Rad2Deg;
                v = _boneMid.position - _boneUpper.position;
                Vector3 w = GetIKTargetHintPosition() - _boneUpper.position;
                _boneUpper.Rotate(Vector3.Cross(v, w).normalized, ikAngle, Space.World);
            }

            //Rotate the mid limb towards the target position. 
            Vector3 currentMidDir = (_boneLimb.position - _boneMid.position).normalized;
            Vector3 targetMidDir = (ikTargetLimbPos - _boneMid.position).normalized;
            rotAngle = Vector3.Angle(currentMidDir, targetMidDir);
            _boneMid.Rotate(Vector3.Cross(currentMidDir, targetMidDir).normalized, rotAngle, Space.World);

            boneUpperRot = _boneUpper.rotation;
            boneMidRot = _boneMid.rotation;
        }

        protected virtual void ComputeIKRotation(out Quaternion boneLimbRot)
        {
            boneLimbRot = _targetLimb.childCount > 0 ? _targetLimb.GetChild(0).rotation : _targetLimb.rotation;
        }

        #endregion

    }

}