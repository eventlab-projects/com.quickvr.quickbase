using UnityEngine;
using System.Collections;

namespace QuickVR
{

    // Class to track the motion parameters of an object
    public class QuickLocomotionTracker : MonoBehaviour
    {

        #region PUBLIC PARAMETERS

        public bool _debug = false;
        
        public enum UpdateMode
        {
            Update, 
            LateUpdate, 
            FixedUpdate, 
        }
        public UpdateMode _updateMode = UpdateMode.Update;

        #endregion

        #region PROTECTED PARAMETERS

        //protected Vector3 _position;
        //protected Vector3 _lastPosition;

        //protected Vector3 _forward;
        protected Vector3 _lastForward;
        //protected float _orientationAngle; // the orientation angle of the forward vector with respect to the world forward axis
        protected float _lastOrientationAngle;

        //protected Vector3 _displacement;
        protected Vector3 _lastDisplacement;
        //protected float _displacementAngle; // the angle between the displacement vector and the world forward axis
        protected float _lastDisplacementAngle;

        //protected Vector3 _localDisplacement;
        public Vector3 _lastLocalDisplacement
        {
            get; protected set;
        }

        //protected float _localDisplacementAngle; // the local angle between the localDisplacement vector and the forward vector
        protected float _lastLocalDisplacementAngle;

        //protected float _speed;
        public float _lastSpeed 
        {
            get; protected set;
        }

        //protected float _acceleration;

        //protected float _angularSpeed;
        protected float _lastAngularSpeed;

        //protected float _angularAcceleration;

        #endregion

        #region CREATION AND DESTRUCTION
        
        protected virtual void Awake()
        {
            Reset();
        }

        public virtual void Reset()
        {
            _position = transform.position;
            _lastPosition = _position;

            _forward = transform.forward;
            _lastForward = _forward;
            _orientationAngle = Vector3.Angle(Vector3.forward, _forward);
            _lastOrientationAngle = _orientationAngle;

            _displacement = Vector3.zero;
            _lastDisplacement = _displacement;
            _displacementAngle = 0;
            _lastDisplacementAngle = _displacementAngle;
            _localDisplacement = Vector3.zero;
            _lastLocalDisplacement = _localDisplacement;
            _localDisplacementAngle = 0;
            _lastLocalDisplacementAngle = _localDisplacementAngle;

            _speed = 0;
            _lastSpeed = _speed;
            _acceleration = 0;
            _angularSpeed = 0;
            _lastAngularSpeed = _angularSpeed;
            _angularAcceleration = 0;
        }

        #endregion

        #region GET AND SET

        public Vector3 _position
        {
            get; protected set;
        }

        public Vector3 _lastPosition
        {
            get; protected set;
        }

        public Vector3 _forward
        {
            get; protected set;
        }

        
        public float _orientationAngle
        {
            get; protected set;
        }

        public Vector3 _displacement
        {
            get; protected set;
        }

        
        public float _displacementAngle
        {
            get; protected set;
        }

        public Vector3 _localDisplacement
        {
            get; protected set;
        }

        public float _localDisplacementAngle
        {
            get; protected set;
        }

        public float _speed
        {
            get; protected set;
        }

        public float _acceleration
        {
            get; protected set;
        }

        public float _angularSpeed
        {
            get; protected set;
        }
        
        public float _angularAcceleration
        {
            get; protected set;
        }
        
        public Vector3 _horizontalDisplacement
        {
            get
            {
                return new Vector3(_displacement.x, 0, _displacement.z);
            }
        }

        public float _verticalDisplacement
        {
            get
            {
                return _displacement.y;
            }
        }

        public float _displacementInclinationAngle()
        {
            Vector3 horizontalAux = Vector3.right * _horizontalDisplacement.magnitude;
            Vector3 displacementAux = horizontalAux + _verticalDisplacement * Vector3.up;

            float sign = 1.0f;
            if (Vector3.Cross(horizontalAux, displacementAux).z < 0)
                sign = -1;

            return Vector3.Angle(horizontalAux, displacementAux) * sign;
        }

        #endregion

        #region UPDATE

        protected virtual void FixedUpdate()
        {
            if (_updateMode == UpdateMode.FixedUpdate)
            {
                UpdateData();
            }
        }

        protected virtual void LateUpdate()
        {
            if (_updateMode == UpdateMode.LateUpdate)
            {
                UpdateData();
            }
        }

        protected virtual void Update()
        {
            if (_updateMode == UpdateMode.Update)
            {
                UpdateData();
            }
        }

        protected virtual void UpdateData()
        {
            float dt = Time.deltaTime;
            TrackRotation(dt);
            TrackPosition(dt);
        }

        public virtual void TrackPosition(float dt)
        {

            //if (dt < 0.04f)
            //   return;

            _position = transform.position;

            _displacement = (_position - _lastPosition) / dt;
            //_displacement.y = 0.0f;
            _displacementAngle = Vector3.Angle(Vector3.forward, _displacement);

            _localDisplacement = transform.worldToLocalMatrix * _displacement;
            _localDisplacementAngle = Vector3.Angle(Vector3.forward, _localDisplacement);

            _speed = _displacement.magnitude;
            _acceleration = (_speed - _lastSpeed) / dt;

            // Storing new parameters values
            _lastPosition = _position;
            _lastDisplacement = _displacement;
            _lastDisplacementAngle = _displacementAngle;
            _lastLocalDisplacement = _localDisplacement;
            _lastLocalDisplacementAngle = _localDisplacementAngle;
            _lastSpeed = _speed;
        }

        public virtual void TrackRotation(float dt)
        {

            // if (dt < 0.04f)
            //   return;

            _forward = transform.forward;
            _orientationAngle = Vector3.Angle(Vector3.forward, _forward);

            _angularSpeed = (_orientationAngle - _lastOrientationAngle) / dt;
            _angularAcceleration = (_angularSpeed - _lastOrientationAngle) / dt;

            // Storing new parameters values
            _lastForward = _forward;
            _lastOrientationAngle = _orientationAngle;
            _lastAngularSpeed = _angularSpeed;
        }

        #endregion

        #region DEBUG

        protected virtual void OnDrawGizmos()
        {
            if (!_debug) return;

            Gizmos.color = Color.white;
            Gizmos.DrawSphere(_position, 0.1f);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(_position, _position + _forward);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(_position, _position + _displacement.normalized * _speed);
        }


        #endregion
    }

}