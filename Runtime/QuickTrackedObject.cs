using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickTrackedObject : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        public bool _trackData = false;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected Vector3 _lastPosition = Vector3.zero;
        
        [SerializeField, ReadOnly]
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

        #region CREATION AND DESTRUCTION

        protected virtual void OnEnable()
        {
            QuickVRManager.OnPostUpdateTracking += UpdateTrackedData;
            StartCoroutine(CoUpdateAcceleration());
        }

        protected virtual void OnDisable()
        {
            QuickVRManager.OnPostUpdateTracking -= UpdateTrackedData;
            StopAllCoroutines();
        }

        public virtual void Reset()
        {
            _lastPosition = transform.position;

            _velocity = Vector3.zero;
            _lastVelocity = Vector3.zero;

            _accelerationFull = Vector4.zero;

            _rotationOffset = Quaternion.identity;
            _lastRotation = transform.rotation;
        }

        #endregion

        #region GET AND SET

        public Vector3 GetDisplacement()
        {
            return transform.position - _lastPosition;
            //Vector3 wDisp = transform.position - _lastPosition;
            //return QuickSingletonManager.GetInstance<QuickVRPlayArea>().transform.InverseTransformVector(wDisp);
        }

        public Quaternion GetRotationOffset()
        {
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
            if (_trackData)
            {
                float dt = Time.deltaTime;
                if (!Mathf.Approximately(dt, 0))
                {
                    _velocity = GetDisplacement() / dt;
                    _speed = _velocity.magnitude;

                    //_accelerationFull.x = _velocity.x - _lastVelocity.x;
                    //_accelerationFull.y = _velocity.y - _lastVelocity.y;
                    //_accelerationFull.z = _velocity.z - _lastVelocity.z;
                    //_accelerationFull.w = _speed - _lastVelocity.magnitude;
                    //_accelerationFull /= dt;

                    //_acceleration = _accelerationFull.w;
                    ////_acceleration = _accelerationFull.y;
                }

                _rotationOffset = Quaternion.Inverse(_lastRotation) * transform.rotation;

                _lastRotation = transform.rotation;
                _lastPosition = transform.position;
                _lastVelocity = _velocity;
            }
        }

        protected virtual IEnumerator CoUpdateAcceleration()
        {
            while (true)
            {
                float timeBegin = Time.time;
                int numSamples = 0;
                Vector3 velocityChange = Vector3.zero;

                while (numSamples < 5)
                {
                    Vector3 velocityBefore = _velocity;

                    yield return null;

                    velocityChange += _velocity - velocityBefore;

                    numSamples++;
                }

                velocityChange /= numSamples;

                _accelerationFull.x = velocityChange.x;
                _accelerationFull.y = velocityChange.y;
                _accelerationFull.z = velocityChange.z;
                _accelerationFull.w = velocityChange.magnitude;
                _accelerationFull /= Time.time - timeBegin;

                _acceleration = _accelerationFull.y;
            }
            
        }

        #endregion

    }

}
