using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickTrackedObject : MonoBehaviour
    {

        #region PROTECTED ATTRIBUTES

        protected Vector3 _lastPosition = Vector3.zero;
        protected Vector3 _displacement = Vector3.zero;

        protected Vector3 _velocity = Vector3.zero;
        protected Vector3 _lastVelocity = Vector3.zero;

        protected Vector4 _accelerationFull = Vector4.zero;

        [SerializeField, ReadOnly]
        protected float _speed = 0.0f;

        [SerializeField, ReadOnly]
        protected float _acceleration = 0.0f;

        protected Quaternion _lastRotation = Quaternion.identity;
        protected Quaternion _rotationOffset = Quaternion.identity;

        #endregion

        #region GET AND SET

        protected virtual void OnEnable()
        {
            QuickVRManager.OnPostUpdateTracking += UpdateTrackedData;
        }

        protected virtual void OnDisable()
        {
            QuickVRManager.OnPostUpdateTracking -= UpdateTrackedData;
        }

        public virtual void Reset()
        {
            _lastPosition = transform.position;
            _displacement = Vector3.zero;
            
            _velocity = Vector3.zero;
            _lastVelocity = Vector3.zero;

            _accelerationFull = Vector4.zero;

            _rotationOffset = Quaternion.identity;
            _lastRotation = transform.rotation;
        }

        public Vector3 GetDisplacement()
        {
            //return IsTracked() ? _displacement : Vector3.zero;
            return _displacement;
        }

        public Quaternion GetRotationOffset()
        {
            //return IsTracked() ? _rotationOffset : Quaternion.identity;
            return _rotationOffset;
        }

        public Vector3 GetVelocity()
        {
            return _velocity;
        }

        public float GetSpeed()
        {
            return _speed;
        }

        public float GetAcceleration()
        {
            return _accelerationFull.w;
        }

        public Vector4 GetAccelerationFull()
        {
            return _accelerationFull;
        }

        #endregion

        #region UPDATE

        public virtual void UpdateTrackedData()
        {
            _displacement = transform.position - _lastPosition;
            float dt = Time.deltaTime;
            if (!Mathf.Approximately(dt, 0))
            {
                _velocity = _displacement / dt;
                _speed = _velocity.magnitude;

                _accelerationFull.x = _velocity.x - _lastVelocity.x;
                _accelerationFull.y = _velocity.y - _lastVelocity.y;
                _accelerationFull.z = _velocity.z - _lastVelocity.z;
                _accelerationFull.w = _speed - _lastVelocity.magnitude;
                _accelerationFull /= dt;

                _acceleration = _accelerationFull.w;
            }

            _rotationOffset = Quaternion.Inverse(_lastRotation) * transform.rotation;
            
            _lastRotation = transform.rotation;
            _lastPosition = transform.position;
            _lastVelocity = _velocity;
        }

        #endregion

    }

}
