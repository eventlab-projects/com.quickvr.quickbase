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

        protected Vector3 _lastForward = Vector3.zero;
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
            _displacement = Vector3.zero;
            _lastPosition = transform.position;

            _rotationOffset = Quaternion.identity;
            _lastRotation = transform.rotation;
            _lastForward = transform.forward;
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

        public Vector3 GetLastForward()
        {
            return _lastForward;
        }

        public Vector3 GetLastPosition()
        {
            return _lastPosition;
        }

        #endregion

        #region UPDATE

        public virtual void UpdateTrackedData()
        {
            UpdateTrackedPosition();
            UpdateTrackedRotation();
        }

        protected virtual void UpdateTrackedPosition()
        {
            _displacement = transform.position - _lastPosition;
            _lastPosition = transform.position;
        }

        protected virtual void UpdateTrackedRotation()
        {
            _rotationOffset = Quaternion.Inverse(_lastRotation) * transform.rotation;
            _lastRotation = transform.rotation;
            _lastForward = transform.forward;
        }

        #endregion

    }

}
