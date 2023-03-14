using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickIKSolver : MonoBehaviour
    {

        #region PUBLIC PARAMETERS

        public bool _enableIK = true;

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
                if (m_boneLimb == null)
                {
                    _initialLocalRotationLimb = value.localRotation;
                    _targetLimb.GetChild(0).rotation = value.rotation;
                }

                m_boneLimb = value;
            }
        }

        public virtual Transform _targetLimb
        {
            get
            {
                if (!m_targetLimb)
                {
                    m_targetLimb = transform.CreateChild(QuickIKManager.IK_TARGET_PREFIX + "_Limb");
                    m_targetLimb.CreateChild("__BoneRotation__");
                }

                return m_targetLimb;
            }
        }

        public virtual Transform _targetHint
        {
            get
            {
                if (!m_targetHint)
                {
                    m_targetHint = transform.CreateChild(QuickIKManager.IK_TARGET_PREFIX + "_Hint");
                }
                return m_targetHint;
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

        protected QuickIKManager _ikManager = null;

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
        protected Vector3 _initialLocalPositionTargetLimb = Vector3.zero;

        [SerializeField, HideInInspector]
        protected Quaternion _initialLocalRotationTargetLimb = Quaternion.identity;

        [SerializeField, HideInInspector]
        protected Vector3 _initialLocalPositionTargetHint = Vector3.zero;

        protected float _lengthUpper = 0;
        protected float _lengthMid = 0;

        [SerializeField, Range(0.0f, 1.0f)]
        protected float m_weightIKPos = 1.0f;

        [SerializeField, Range(0.0f, 1.0f)]
        protected float m_weightIKRot = 1.0f;

        protected Animator _animator
        {
            get
            {
                if (!m_Animator)
                {
                    m_Animator = GetComponentInParent<Animator>();
                }

                return m_Animator;
            }
        }
        protected Animator m_Animator;

        protected virtual Transform _upperBoneReference
        {
            get
            {
                if (!m_UpperBoneReference)
                {
                    m_UpperBoneReference = _boneUpper.CreateChild("__BoneUpperReference__");
                    Vector3 fwdHint;
                    if (
                        _boneLimb == _animator.GetBoneTransform(HumanBodyBones.LeftHand) || 
                        _boneLimb == _animator.GetBoneTransform(HumanBodyBones.RightHand)
                        )
                    {
                        //fwdHint = -_animator.transform.forward;
                        fwdHint = Vector3.Lerp(-_animator.transform.forward, -_animator.transform.up, 0.25f);
                    }
                    else if 
                        (
                        _boneLimb == _animator.GetBoneTransform(HumanBodyBones.LeftFoot) || 
                        _boneLimb == _animator.GetBoneTransform(HumanBodyBones.RightFoot)
                        )
                    {
                        fwdHint = _animator.transform.forward;
                    }
                    else if 
                        (
                        _boneLimb == _animator.GetBoneTransform(HumanBodyBones.LeftThumbDistal)
                        )
                    {
                        fwdHint = _animator.transform.right;
                    }
                    else if 
                        (
                        _boneLimb == _animator.GetBoneTransform(HumanBodyBones.RightThumbDistal)
                        )
                    {
                        fwdHint = -_animator.transform.right;
                    }
                    else
                    {
                        fwdHint = _animator.transform.up;
                    }

                    m_UpperBoneReference.LookAt(_boneMid, fwdHint);
                }

                return m_UpperBoneReference;
            }
        }
        protected Transform m_UpperBoneReference = null;

        #endregion

        #region EVENTS

        public delegate void OnPostUpdateIKCallback(QuickIKSolver ikSolver);

        public static OnPostUpdateIKCallback OnPostUpdateIK;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            SavePose();

            _ikManager = GetComponentInParent<QuickIKManager>();
        }

        #endregion

        #region GET AND SET

        public virtual void SavePose()
        {
            if (_targetLimb)
            {
                _initialLocalPositionTargetLimb = _targetLimb.localPosition;
                _initialLocalRotationTargetLimb = _targetLimb.localRotation;
            }
            if (_targetHint) _initialLocalPositionTargetHint = _targetHint.localPosition;
        }

        public virtual void LoadPose()
        {
            if (_targetLimb)
            {
                _targetLimb.localPosition = _initialLocalPositionTargetLimb;
                _targetLimb.localRotation = _initialLocalRotationTargetLimb;
            }
            if (_targetHint) _targetHint.localPosition = _initialLocalPositionTargetHint;
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

        protected virtual Vector3 GetIKTargetHintPosition(float ikAngle)
        {
            //float r = GetChainLength();
            //float maxY = _boneUpper.position.y + r;

            //float t = Mathf.Clamp01((maxY - _targetLimb.position.y) / (r));

            ////return Vector3.Lerp(_targetLimb.position - Vector3.up * (GetMidLength() + QuickIKManager.DEFAULT_TARGET_HINT_DISTANCE), _targetHint.position, t);
            ////return Vector3.Lerp(_boneUpper.position + Vector3.Lerp(_animator.transform.forward, Vector3.down, 0.5f).normalized * (QuickIKManager.DEFAULT_TARGET_HINT_DISTANCE), _targetHint.position, t);
            //return Vector3.Lerp(_boneMid.position - Vector3.up * (GetMidLength() + QuickIKManager.DEFAULT_TARGET_HINT_DISTANCE), _targetHint.position, t);

            float uLength = GetUpperLength();
            //int sign = _isLeft ? -1 : 1;
            Vector3 hintPos = _boneUpper.position + _upperBoneReference.forward * (uLength * Mathf.Cos(ikAngle)) + _upperBoneReference.up * (uLength * Mathf.Sin(ikAngle));

            _targetHint.position = hintPos;

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
            //If we have an IKManager on the parent, the update order is determined by the IKManager. Otherwise, update it at lateUpdate.
            if (!_ikManager)
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
            _targetLimb.position = _boneUpper.position + (_targetLimb.position - _boneUpper.position).normalized * CheckIKTargetDistance();

            float upperLength = GetUpperLength();
            float midLength = GetMidLength();
            float chainLength = GetChainLength() * 0.9999f;

            //Align the bone with the target limb
            Vector3 ikTargetLimbPos = _targetLimb.position;
            Vector3 bUpperPos = _boneUpper.position;

            Vector3 u = (_boneMid.position - bUpperPos);
            Vector3 v = (ikTargetLimbPos - bUpperPos);
            float rotAngle = Vector3.Angle(u, v);
            _boneUpper.Rotate(Vector3.Cross(u, v), rotAngle, Space.World);

            if (_targetHint)
            {
                //Apply the ikAngle to the boneUpper. The ikAngle is computed using the cosine rule
                //float targetDistance = CheckIKTargetDistance(v);
                float targetDistance = Mathf.Min(Vector3.Distance(bUpperPos, ikTargetLimbPos), chainLength);

                float cos = (Mathf.Pow(midLength, 2) - Mathf.Pow(upperLength, 2) - Mathf.Pow(targetDistance, 2)) / (-2 * upperLength * targetDistance);
                float ikAngle = Mathf.Acos(cos);    //The IKAngle in radians. 
                v = _boneMid.position - bUpperPos;
                Vector3 w = GetIKTargetHintPosition(ikAngle) - bUpperPos;
                _boneUpper.Rotate(Vector3.Cross(v, w), ikAngle * Mathf.Rad2Deg, Space.World);
            }

            //Rotate the mid limb towards the target position. 
            Vector3 currentMidDir = (_boneLimb.position - _boneMid.position);
            Vector3 targetMidDir = (ikTargetLimbPos - _boneMid.position);
            rotAngle = Vector3.Angle(currentMidDir, targetMidDir);
            _boneMid.Rotate(Vector3.Cross(currentMidDir, targetMidDir), rotAngle, Space.World);

            boneUpperRot = _boneUpper.rotation;
            boneMidRot = _boneMid.rotation;
        }

        protected virtual float CheckIKTargetDistance()
        {
            return Mathf.Min(Vector3.Distance(_boneUpper.position, _targetLimb.position), GetChainLength());
        }

        protected virtual void ComputeIKRotation(out Quaternion boneLimbRot)
        {
            boneLimbRot = _targetLimb.childCount > 0 ? _targetLimb.GetChild(0).rotation : _targetLimb.rotation;
        }

        #endregion

    }

}
