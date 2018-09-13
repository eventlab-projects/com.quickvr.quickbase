using UnityEngine;
using System.Collections;

namespace QuickVR {

	public abstract class QuickCharacterControllerBase : MonoBehaviour {

		#region PUBLIC PARAMETERS

		public bool _move = true;
        public bool _hasPriority = false;

		public float _maxLinearSpeed = 2.5f;		//Max linear speed in m/s
		public float _linearAcceleration = 1.0f;	//Max linear acceleration in m/s^2
		public float _linearDrag = 4.0f;			//The drag applied to the linear speed when no input is applied

		public float _maxAngularSpeed = 45.0f;		//Max angular speed in degrees/second
		public float _angularAcceleration = 45.0f;	//Angular acceleration in degrees/second^2
		public float _angularDrag = 4.0f;			//The drag applied to the angular speed when no input is applied. 

		public bool _canJump = true;
		public float _jumpHeight = 2.0f;

        #endregion

        #region PROTECTED PARAMETERS

        protected QuickCharacterControllerManager _characterControllerManager = null;

		protected bool _grounded = false;
        
		protected Vector3 _targetLinearVelocity = Vector3.zero;
        protected Vector3 _currentLinearVelocity = Vector3.zero;
		protected Vector3 _targetAngularVelocity = Vector3.zero;

		protected float _gravity = 0.0f;

		#endregion

		#region CREATION AND DESTRUCTION

		protected virtual void Awake() {
            _characterControllerManager = gameObject.GetOrCreateComponent<QuickCharacterControllerManager>();
			_gravity = Physics.gravity.magnitude;
		}

        protected virtual void OnEnable()
        {
            _characterControllerManager.AddCharacterController(this);
        }

        protected virtual void OnDisable()
        {
            _characterControllerManager.RemoveCharacterController(this);
        }

		#endregion

		#region GET AND SET

        public virtual Vector3 GetCurrentLinearVelocity()
        {
            return _currentLinearVelocity;
        }

		public virtual float GetMaxLinearSpeed() {
			return _maxLinearSpeed;
		}

		protected virtual float GetJumpVerticalSpeed(float jumpHeight) {
			// From the jump height and gravity we deduce the upwards speed 
			// for the character to reach at the apex.
			return Mathf.Sqrt(2.0f * jumpHeight * _gravity);
		}

        protected virtual void ComputeTargetLinearVelocity() { }
        protected virtual void ComputeTargetAngularVelocity() { }

		protected virtual void ClampLinearVelocity() {
			Vector2 vHor = new Vector2(_currentLinearVelocity.x, _currentLinearVelocity.z);
			float mSpeed = GetMaxLinearSpeed();
			if (vHor.sqrMagnitude > (mSpeed * mSpeed)) {
				vHor.Normalize();
				vHor *= mSpeed;
                _currentLinearVelocity = new Vector3(vHor.x, _currentLinearVelocity.y, vHor.y);
			}
		}

        protected virtual bool CanMove()
        {
            return _move;
        }

		#endregion

		#region UPDATE

		public virtual void UpdateMovement() {
            if (CanMove())
            {
                UpdateLinearVelocity();
                UpdateAngularVelocity();
                UpdateJump();
            }
            else _currentLinearVelocity = Vector3.zero;
		}

		protected virtual void UpdateLinearVelocity() {
            ComputeTargetLinearVelocity();

            //We are moving in the desired direction. 
            if (_targetLinearVelocity == Vector3.zero)
            {
                _currentLinearVelocity = Vector3.zero;
            }
            else
            {
                //Apply a force that attempts to reach our target velocity
                Vector3 offset = (_targetLinearVelocity - _currentLinearVelocity);
                Vector2 v = new Vector2(offset.x, offset.z);
                v.Normalize();
                _currentLinearVelocity += new Vector3(v.x, 0.0f, v.y) * _linearAcceleration * Time.deltaTime;
            }
			
			ClampLinearVelocity();
		}

		protected virtual void UpdateAngularVelocity() {
            ComputeTargetAngularVelocity();

   //         _rigidBody.angularDrag = (_targetAngularVelocity == Vector3.zero)? _angularDrag : 0.0f;
			//_rigidBody.AddTorque(_targetAngularVelocity, ForceMode.Acceleration);
		}

        protected virtual void UpdateJump()
        {
            // Jump
            //if (_grounded && _canJump && Input.GetButton("Jump"))
            //{
            //    _rigidBody.velocity = new Vector3(_rigidBody.velocity.x, GetJumpVerticalSpeed(_jumpHeight), _rigidBody.velocity.z);
            //    _grounded = false;
            //}
        }

        #endregion

        #region PHYSICS MANAGEMENT

        //protected virtual void OnCollisionStay(Collision collision)
        //{
        //    //Allow this character to overcome step stairs according to the defined maxStepHeight. 
        //    //Ignore other agents. 
        //    if (_hasPriority && (collision.gameObject.layer == _layerAutonomousAgent))
        //    {
        //        Vector3 offset = (collision.transform.position - transform.position).normalized;
        //        collision.gameObject.GetComponent<Rigidbody>().AddForce(offset * 10.0f, ForceMode.Impulse);
        //    }
        //}

        #endregion

    }

}
