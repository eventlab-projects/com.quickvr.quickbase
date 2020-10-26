using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public interface IQuickIKSolver
    {
        HumanBodyBones _boneID { get; set; }

        //The bone chain hierarchy
        Transform _boneUpper { get; set; }
        Transform _boneMid { get; set; }
        Transform _boneLimb { get; set; }

        //The IK parameters
        Transform _targetLimb { get; set; }
        Transform _targetHint { get; set; }

        float _weight { get; set; }

        float _weightIKPos { get; set; }
        float _weightIKRot { get; set; }
        float _weightIKHint { get; set; }

    }

    public class QuickIKSolver : MonoBehaviour, IQuickIKSolver
    {

        #region PUBLIC PARAMETERS

        public bool _enableIK = true;

        public Vector3 _offsetTargetLimbPos = Vector3.zero;

        public virtual HumanBodyBones _boneID
        {
            get
            {
                return m_boneID;
            }
            set
            {
                m_boneID = value;
            }
        }

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
                return m_targetHint;
            }
            set
            {
                m_targetHint = value;
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

        [SerializeField, HideInInspector]
        protected Quaternion _initialLocalRotationUpper = Quaternion.identity;
        
        [SerializeField, HideInInspector]
        protected Quaternion _initialLocalRotationMid = Quaternion.identity;
        
        [SerializeField, HideInInspector]
        protected Quaternion _initialLocalRotationLimb = Quaternion.identity;

        [SerializeField, HideInInspector]
        protected float _lengthUpper = 0;

        [SerializeField, HideInInspector]
        protected float _lengthMid = 0;

        [SerializeField, HideInInspector]
        protected HumanBodyBones m_boneID = HumanBodyBones.LastBone;

        [SerializeField, Range(0.0f, 1.0f)]
        protected float m_weightIKPos = 1.0f;

        [SerializeField, Range(0.0f, 1.0f)]
        protected float m_weightIKRot = 1.0f;

        #endregion

        #region GET AND SET

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
            Vector3 v = (_targetLimb.position + _offsetTargetLimbPos) - _boneUpper.position;
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
            //_boneLimb.localRotation = _initialLocalRotationLimb;
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

        #region DEBUG

        //protected virtual void OnDrawGizmos()
        //{
        //    if (_boneUpper && _boneMid)
        //    {
        //        Gizmos.color = Color.magenta;
        //        Gizmos.DrawLine(_boneUpper.position, _boneMid.position);
        //    }
        //    if (_boneMid && _boneLimb)
        //    {
        //        Gizmos.color = Color.magenta;
        //        Gizmos.DrawLine(_boneMid.position, _boneLimb.position);
        //    }
        //    if (_boneMid && _targetHint)
        //    {
        //        Gizmos.color = Color.yellow;
        //        Gizmos.DrawLine(_boneMid.position, _targetHint.position);
        //    }
        //    if (_boneUpper && _targetLimb)
        //    {
        //        Gizmos.color = Color.cyan;
        //        Gizmos.DrawLine(_boneUpper.position, GetIKTargetLimbPosition());
        //    }
        //}

        #endregion


    }

}
