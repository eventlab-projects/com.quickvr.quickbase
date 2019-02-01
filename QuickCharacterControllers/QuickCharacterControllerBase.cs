using UnityEngine;
using System.Collections;

namespace QuickVR
{

    public abstract class QuickCharacterControllerBase : MonoBehaviour
    {

        #region PUBLIC PARAMETERS

        public bool _move = true;
        public bool _hasPriority = false;

        #endregion

        #region PROTECTED PARAMETERS

        protected QuickCharacterControllerManager _characterControllerManager = null;
        protected Rigidbody _rigidBody = null;

        protected bool _grounded = false;
        
        protected Vector3 _targetLinearVelocity = Vector3.zero;
        protected Vector3 _targetAngularVelocity = Vector3.zero;
        protected Vector3 _preLinearVelocity = Vector3.zero;    //The linear velocity the object had before Unity's internal physics update

        protected float _gravity = 0.0f;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            _characterControllerManager = gameObject.GetOrCreateComponent<QuickCharacterControllerManager>();
            _gravity = Physics.gravity.magnitude;
        }

        protected virtual void Start()
        {
            _rigidBody = GetComponent<Rigidbody>();
        }

        #endregion

        #region GET AND SET

        public virtual float GetMaxLinearSpeed()
        {
            return _characterControllerManager._defaultMaxLinearSpeed;
        }

        protected virtual float GetJumpVerticalSpeed(float jumpHeight)
        {
            // From the jump height and gravity we deduce the upwards speed 
            // for the character to reach at the apex.
            return Mathf.Sqrt(2.0f * jumpHeight * _gravity);
        }

        protected virtual void ComputeTargetLinearVelocity() { }
        protected virtual void ComputeTargetAngularVelocity() { }

        protected virtual void ClampLinearVelocity()
        {
            Vector2 vHor = new Vector2(_rigidBody.velocity.x, _rigidBody.velocity.z);
            float mSpeed = GetMaxLinearSpeed();
            if (vHor.sqrMagnitude > (mSpeed * mSpeed))
            {
                vHor.Normalize();
                vHor *= mSpeed;
                _rigidBody.velocity = new Vector3(vHor.x, _rigidBody.velocity.y, vHor.y);
            }
        }

        protected virtual bool CanMove()
        {
            return _move;
        }

        #endregion

        #region UPDATE

        protected virtual void FixedUpdate()
        {
            if (_characterControllerManager.IsStepping()) return;

            if (CanMove())
            {
                UpdateLinearVelocity();
                UpdateAngularVelocity();
                UpdateJump();
            }
            else _rigidBody.velocity = Vector3.zero;

            _preLinearVelocity = _rigidBody.velocity;
        }

        protected virtual void UpdateLinearVelocity()
        {
            ComputeTargetLinearVelocity();

            //We are moving in the desired direction. 
            if (_targetLinearVelocity == Vector3.zero) _rigidBody.drag = _characterControllerManager._linearDrag;
            else
            {
                _rigidBody.drag = 0.0f;

                //Apply a force that attempts to reach our target velocity
                Vector3 offset = (_targetLinearVelocity - _rigidBody.velocity);
                Vector2 v = new Vector2(offset.x, offset.z);
                v.Normalize();
                _rigidBody.velocity += new Vector3(v.x, 0.0f, v.y) * _characterControllerManager._linearAcceleration * Time.deltaTime;
            }

            ClampLinearVelocity();
        }

        protected virtual void UpdateAngularVelocity()
        {
            ComputeTargetAngularVelocity();

            _rigidBody.angularDrag = (_targetAngularVelocity == Vector3.zero) ? _characterControllerManager._angularDrag : 0.0f;
            _rigidBody.AddTorque(_targetAngularVelocity, ForceMode.Acceleration);
        }

        protected virtual void UpdateJump()
        {
            // Jump
            if (_grounded && _characterControllerManager._canJump && Input.GetButton("Jump"))
            {
                _rigidBody.velocity = new Vector3(_rigidBody.velocity.x, GetJumpVerticalSpeed(_characterControllerManager._jumpHeight), _rigidBody.velocity.z);
                _grounded = false;
            }
        }

        #endregion

    }

}