﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickIKSolver : MonoBehaviour, IQuickIKSolver
    {

        #region PUBLIC PARAMETERS

        public bool _enableIK = true;

        //The bone chain hierarchy
        public Transform m_boneUpper = null;
        public Transform m_boneMid = null;
        public Transform m_boneLimb = null;

        //The IK parameters
        public Transform m_targetLimb = null;
        public Transform m_targetHint = null;

        [Range(0.0f, 1.0f)]
        public float m_weightIKPos = 1.0f;

        [Range(0.0f, 1.0f)]
        public float m_weightIKRot = 1.0f;

        public Vector3 _offsetTargetLimbPos = Vector3.zero;

        public Transform _boneUpper
        {
            get
            {
                return m_boneUpper;
            }
            set
            {
                m_boneUpper = value;
            }
        }

        public Transform _boneMid
        {
            get
            {
                return m_boneMid;
            }
            set
            {
                m_boneMid = value;
            }
        }

        public Transform _boneLimb
        {
            get
            {
                return m_boneLimb;
            }
            set
            {
                m_boneLimb = value;
            }
        }

        public Transform _targetLimb
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

        public Transform _targetHint
        {
            get
            {
                return m_targetHint;
            }
            set
            {
                m_targetHint = value;
            }
        }

        public float _weight
        {
            get
            {
                return 1.0f;
            }
            set
            {
                
            }
        }

        public float _weightIKPos
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

        public float _weightIKRot
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

        #endregion

        #region PROTECTED PARAMETERS

        protected bool _initialized = false;

        protected Quaternion _initialLocalRotationUpper = Quaternion.identity;
        protected Quaternion _initialLocalRotationMid = Quaternion.identity;
        protected Quaternion _initialLocalRotationLimb = Quaternion.identity;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Start()
        {
            Calibrate();

            _initialized = true;
        }

        #endregion

        #region GET AND SET

        public virtual void Calibrate()
        {
            _initialLocalRotationUpper = _boneUpper.localRotation;
            _initialLocalRotationMid = _boneMid.localRotation;
            _initialLocalRotationLimb = _boneLimb.localRotation;
        }

        public virtual float GetUpperLength()
        {
            return Vector3.Distance(_boneUpper.position, _boneMid.position);
        }

        public virtual float GetMidLength()
        {
            return Vector3.Distance(_boneMid.position, _boneLimb.position);
        }

        public virtual float GetChainLength()
        {
            return GetUpperLength() + GetMidLength();
        }

        protected virtual Vector3 GetIKTargetLimbPosition()
        {
            Vector3 v = (_targetLimb.position + _offsetTargetLimbPos) - _boneUpper.position;
            return _boneUpper.position + (v.normalized * Mathf.Min(v.magnitude, GetChainLength()));
        }

        public virtual void ResetIKChain()
        {
            if (!_initialized) return;

            _boneUpper.localRotation = _initialLocalRotationUpper;
            _boneMid.localRotation = _initialLocalRotationMid;
            _boneLimb.localRotation = _initialLocalRotationLimb;
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
                Vector3 w = _targetHint.position - _boneUpper.position;
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

        #region DEBUG

        protected virtual void OnDrawGizmos()
        {
            if (_boneUpper && _boneMid) Debug.DrawLine(_boneUpper.position, _boneMid.position, Color.magenta);
            if (_boneMid && _boneLimb) Debug.DrawLine(_boneMid.position, _boneLimb.position, Color.magenta);
            if (_boneMid && _targetHint) Debug.DrawLine(_boneMid.position, _targetHint.position, Color.yellow);
            if (_boneUpper && _targetLimb) Debug.DrawLine(_boneUpper.position, GetIKTargetLimbPosition(), Color.cyan);
        }

        #endregion


    }

}
