using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickIKSolverEye : QuickIKSolver
    {

        [System.Serializable]
        public class BlinkData
        {
            public SkinnedMeshRenderer _renderer = null;
            public int _blendshapeID = -1;

#if UNITY_EDITOR
            public bool _showInInspector = true;
#endif
        }

        #region PUBLIC PARAMETERS

        public override Transform _targetHint
        {
            get
            {
                return null;
            }
        }

        public virtual float _weightBlink
        {
            get
            {
                return m_weightBlink;
            }
            set
            {
                m_weightBlink = value;
            }
        }

        [SerializeField, Range(0.0f, 1.0f)]
        protected float m_weightBlink = 0;

#if UNITY_EDITOR

        [SerializeField, HideInInspector]
        public bool _showAngleLimits = false;

        [SerializeField, HideInInspector]
        public bool _showBlinking = false;

#endif

        [SerializeField, HideInInspector]
        protected Vector4 _angleLimits = new Vector4(-35, 35, -15, 15); //Left, Right, Down, Up

        public virtual float _angleLimitLeft
        {
            get
            {
                return _angleLimits.x;
            }
            set
            {
                _angleLimits.x = -1 * Mathf.Abs(value);
            }
        }

        public virtual float _angleLimitRight
        {
            get
            {
                return _angleLimits.y;
            }
            set
            {
                _angleLimits.y = Mathf.Abs(value);
            }
        }

        public virtual float _angleLimitDown
        {
            get
            {
                return _angleLimits.z;
            }
            set
            {
                _angleLimits.z = -1 * Mathf.Abs(value);
            }
        }

        public virtual float _angleLimitUp
        {
            get
            {
                return _angleLimits.w;
            }
            set
            {
                _angleLimits.w = Mathf.Abs(value);
            }
        }

        public virtual float _leftRight
        {
            get
            {
                return m_leftRight;
            }
            set
            {
                m_leftRight = Mathf.Clamp(value, _angleLimitLeft, _angleLimitRight);
                _targetLimb.localRotation = Quaternion.identity;
                _targetLimb.Rotate(_targetLimb.parent.up, m_leftRight, Space.World);
                _targetLimb.Rotate(-_targetLimb.parent.right, m_downUp, Space.World);
                
            }
        }

        public virtual float _downUp
        {
            get
            {
                return m_downUp;
            }
            set
            {
                m_downUp = Mathf.Clamp(value, _angleLimitDown, _angleLimitUp);
                _targetLimb.localRotation = Quaternion.identity;
                _targetLimb.Rotate(_targetLimb.parent.up, m_leftRight, Space.World);
                _targetLimb.Rotate(-_targetLimb.parent.right, m_downUp, Space.World);
            }
        }

        public List<BlinkData> _blinking = new List<BlinkData>();
        
        #endregion

        #region PROTECTED PARAMETERS

        [SerializeField, Range(-1.0f, 1.0f)]
        protected float m_leftRight = 0;

        [SerializeField, Range(-1.0f, 1.0f)]
        protected float m_downUp = 0;

        #endregion

        #region UPDATE

        public override void UpdateIK()
        {
            //base.UpdateIK();
            //if (_enableIK && _boneLimb && _targetLimb) return;
            if (_enableIK)
            {
                _targetLimb.localPosition = _initialLocalPositionTargetLimb;
                Quaternion animBoneUpperRot = _boneUpper.rotation;
                Quaternion animBoneMidRot = _boneMid.rotation;
                Quaternion animBoneLimbRot = _boneLimb.rotation;

                ResetIKChain();

                Transform tRef = _targetLimb.parent;
                ComputeEyeballRotation(tRef.up, _angleLimitLeft, _angleLimitRight);
                ComputeEyeballRotation(tRef.right, _angleLimitDown, _angleLimitUp);
                
                _boneUpper.rotation = animBoneUpperRot; 
                _boneMid.rotation = animBoneMidRot;

                foreach (BlinkData bData in _blinking)
                {
                    if (bData._blendshapeID >= 0)
                    {
                        bData._renderer.SetBlendShapeWeight(bData._blendshapeID, _weightBlink * 100.0f);
                    }
                    
                }
                //_boneLimb.rotation = Quaternion.Lerp(animBoneLimbRot, ikBoneLimbRot, _weightIKRot);

                ////Compute Down-Up rotation
                //Vector3 w = Vector3.ProjectOnPlane(_targetLimb.forward, tRef.right);
                //if (w.sqrMagnitude < 0.001f)
                //{
                //    w = Vector3.zero;
                //}
                //if (Vector3.Dot(tRef.forward, w) < 0)
                //{
                //    w = Vector3.Scale(w, new Vector3(1, 1, -1));
                //}

                //float angleX = Mathf.Clamp(Vector3.SignedAngle(tRef.forward, w, tRef.right), _angleLimitDown, _angleLimitUp);
                //_boneLimb.Rotate(tRef.right, angleX, Space.World);

                ////Compute Left-Right rotation
                //Vector3 v = Vector3.ProjectOnPlane(_targetLimb.forward, tRef.up);
                //if (v.sqrMagnitude < 0.001f)
                //{
                //    v = Vector3.zero;
                //}
                //if (Vector3.Dot(tRef.forward, v) < 0)
                //{
                //    v = Vector3.Scale(v, new Vector3(1, 1, -1));
                //}

                //float angleY = Mathf.Clamp(Vector3.SignedAngle(tRef.forward, v, tRef.up), _angleLimitLeft, _angleLimitRight);
                //_boneLimb.Rotate(tRef.up, angleY, Space.World);

                //Debug.DrawRay(_targetLimb.position, tRef.forward, Color.blue);
                //Debug.DrawRay(_targetLimb.position, v, Color.cyan);
                //Debug.DrawRay(_targetLimb.position, w, Color.magenta);
            }
        }

        protected virtual void ComputeEyeballRotation(Vector3 planeNormal, float limitMin, float limitMax)
        {
            Vector3 v = Vector3.ProjectOnPlane(_targetLimb.forward, planeNormal);
            if (v.sqrMagnitude < 0.001f)
            {
                v = Vector3.zero;
            }

            Vector3 fwd = _targetLimb.parent.forward;
            if (Vector3.Dot(fwd, v) < 0)
            {
                v = Vector3.Scale(v, new Vector3(1, 1, -1));
            }

            float angleX = Mathf.Clamp(Vector3.SignedAngle(fwd, v, planeNormal), limitMin, limitMax);
            _boneLimb.Rotate(planeNormal, angleX, Space.World);
        }

        //public virtual void UpdateIK()
        //{
        //    if (!_enableIK || !_boneUpper || !_boneMid || !_boneLimb || !_targetLimb) return;

        //    Quaternion animBoneUpperRot = _boneUpper.rotation;
        //    Quaternion animBoneMidRot = _boneMid.rotation;
        //    Quaternion animBoneLimbRot = _boneLimb.rotation;

        //    ResetIKChain();

        //    Quaternion ikBoneUpperRot, ikBoneMidRot, ikBoneLimbRot;

        //    ComputeIKPosition(out ikBoneUpperRot, out ikBoneMidRot);
        //    ComputeIKRotation(out ikBoneLimbRot);

        //    _boneUpper.rotation = Quaternion.Lerp(animBoneUpperRot, ikBoneUpperRot, _weightIKPos);
        //    _boneMid.rotation = Quaternion.Lerp(animBoneMidRot, ikBoneMidRot, _weightIKPos);
        //    _boneLimb.rotation = Quaternion.Lerp(animBoneLimbRot, ikBoneLimbRot, _weightIKRot);
        //}

        #endregion

    }

}
