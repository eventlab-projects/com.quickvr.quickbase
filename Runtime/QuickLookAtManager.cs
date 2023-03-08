using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickLookAtManager : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        public float _lookAtSpeed = 1;

        #endregion

        #region PROTECTED ATTRIUBTES

        protected QuickIKManager _ikManager = null;

        protected Dictionary<HumanBodyBones, Vector3> _initialForward = new Dictionary<HumanBodyBones, Vector3>();

        protected Dictionary<HumanBodyBones, Vector3> _currentForward
        {
            get
            {
                if (m_CurrentForward == null)
                {
                    m_CurrentForward = new Dictionary<HumanBodyBones, Vector3>();

                    foreach (HumanBodyBones boneID in LOOK_AT_BONES)
                    {
                        m_CurrentForward[boneID] = _ikManager.GetIKSolver(boneID)._targetLimb.forward;
                    }
                }

                return m_CurrentForward;
            }
            set
            {
                m_CurrentForward = value;
            }
        }
        protected Dictionary<HumanBodyBones, Vector3> m_CurrentForward = null;

        protected enum State
        {
            LookAtNone,
            LookAtTransform, 
            LookAtPoint, 
            LookAtDefault, 
        }
        protected State _state = State.LookAtNone;

        protected Transform _lookAtTransform = null;    //The transform to look at when the mode is LookAtTransform
        protected Vector3 _lookAtPoint = Vector3.zero;  //The fixed point to look at when the mode is LookAtPoint

        #endregion

        #region CONSTANTS

        protected HumanBodyBones[] LOOK_AT_BONES = { HumanBodyBones.Head, HumanBodyBones.LeftEye, HumanBodyBones.RightEye };

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Start()
        {
            _ikManager = gameObject.GetOrCreateComponent<QuickIKManager>();
            foreach (HumanBodyBones boneID in LOOK_AT_BONES)
            {
                QuickIKSolver ikSolver = _ikManager.GetIKSolver(boneID);
                _initialForward[boneID] = ikSolver._targetLimb.forward;
                ikSolver._enableIK = true;
            }
        }

        #endregion

        #region GET AND SET

        [ButtonMethod]
        public virtual void LookAtCamera()
        {
            if (Camera.main)
            {
                LookAtTransform(Camera.main.transform);
            }
        }

        public virtual void LookAtTransform(Transform t)
        {
            if (t)
            {
                _lookAtTransform = t;

                _state = State.LookAtTransform;
            }
        }

        public virtual void LookAtPoint(Vector3 p)
        {
            _lookAtPoint = p;

            _state = State.LookAtPoint;
        }

        [ButtonMethod]
        public virtual void LookAtDefault()
        {
            _state = State.LookAtDefault;
        }

        [ButtonMethod]
        public virtual void StopLookAt()
        {
            _currentForward = null;
            _state = State.LookAtNone;
        }

        [ButtonMethod]
        public virtual void ResetLookAt()
        {
            StopLookAt();

            foreach (HumanBodyBones boneID in LOOK_AT_BONES)
            {
                _ikManager.GetIKSolver(boneID)._targetLimb.forward = _initialForward[boneID];
            }
        }

        protected virtual Vector3 GetTargetForward(HumanBodyBones boneID)
        {
            Vector3 result = Vector3.zero;
            Transform tLimb = _ikManager.GetIKSolver(boneID)._targetLimb;

            if (_state == State.LookAtTransform && _lookAtTransform)
            {
                result = _lookAtTransform.position - tLimb.position;
            }
            else if (_state == State.LookAtPoint)
            {
                result = _lookAtPoint - tLimb.position;
            }
            else
            {
                result = _initialForward[boneID];
            }

            return result;
        }

        #endregion

        #region UPDATE

        protected virtual void LateUpdate()
        {
            UpdateLookAt(Time.deltaTime);
        }

        public virtual void UpdateLookAt(float dt)
        {
            if (_state != State.LookAtNone)
            {
                foreach (HumanBodyBones boneID in LOOK_AT_BONES)
                {
                    Vector3 targetFwd = GetTargetForward(boneID);

                    QuickIKSolver ikSolver = _ikManager.GetIKSolver(boneID);
                    Vector3 fwd = Vector3.Lerp(_currentForward[boneID], targetFwd, dt * _lookAtSpeed);
                    ikSolver._targetLimb.forward = fwd;
                    ikSolver.UpdateIK();

                    _currentForward[boneID] = fwd;
                }
            }
        }

        #endregion

    }

}
