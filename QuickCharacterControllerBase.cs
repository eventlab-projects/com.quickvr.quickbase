using UnityEngine;
using System.Collections;

namespace QuickVR {

	public abstract class QuickCharacterControllerBase : MonoBehaviour {

		#region PUBLIC PARAMETERS

		public bool _move = true;
        public bool _hasPriority = false;

		public float _radius = 0.25f;				//Character radius
		public float _height = 2.0f;				//Character height

		public float _maxLinearSpeed = 2.5f;		//Max linear speed in m/s
		public float _linearAcceleration = 1.0f;	//Max linear acceleration in m/s^2
		public float _linearDrag = 4.0f;			//The drag applied to the linear speed when no input is applied

		public float _maxAngularSpeed = 45.0f;		//Max angular speed in degrees/second
		public float _angularAcceleration = 45.0f;	//Angular acceleration in degrees/second^2
		public float _angularDrag = 4.0f;			//The drag applied to the angular speed when no input is applied. 

		public float _maxStepHeight = 0.3f;			//The step offset used for stepping stairs and so on. 

		public bool _canJump = true;
		public float _jumpHeight = 2.0f;

		#endregion

		#region PROTECTED PARAMETERS

		protected bool _grounded = false;
        protected bool _stepping = false;
		protected Rigidbody _rigidBody = null;
		protected CapsuleCollider _collider = null;

		protected Vector3 _targetLinearVelocity = Vector3.zero;
		protected Vector3 _targetAngularVelocity = Vector3.zero;

		protected float _gravity = 0.0f;

		protected int _layerPlayer = 0;
		protected int _layerAutonomousAgent = 0;

		protected static PhysicMaterial _physicMaterial = null;

		#endregion

		#region CREATION AND DESTRUCTION

		protected virtual void Awake() {
			CreatePhysicMaterial();

			InitCollider();
			InitRigidBody();

			_gravity = Physics.gravity.magnitude;
			_layerPlayer = LayerMask.NameToLayer("Player");
			_layerAutonomousAgent = LayerMask.NameToLayer("AutonomousAgent");
		}

		protected virtual void InitCollider() {
			_collider = gameObject.GetOrCreateComponent<CapsuleCollider>();
			_collider.radius = _radius;
			_collider.height = _height;
            _collider.center = new Vector3(0.0f, _height * 0.5f, 0.0f);
            _collider.material = _physicMaterial;
		}

		protected virtual void CreatePhysicMaterial() {
			if (!_physicMaterial) {
				_physicMaterial = new PhysicMaterial("__CharacterControllerPhysicMaterial__");
				_physicMaterial.dynamicFriction = 0.0f;
				_physicMaterial.staticFriction = 0.0f;
				_physicMaterial.bounciness = 0.0f;
				_physicMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
				_physicMaterial.bounceCombine = PhysicMaterialCombine.Minimum;
			}
		}

		protected virtual void InitRigidBody() {
			_rigidBody = gameObject.GetOrCreateComponent<Rigidbody>();
			_rigidBody.freezeRotation = true;
			_rigidBody.maxAngularVelocity = _maxAngularSpeed * Mathf.Deg2Rad;
			_rigidBody.useGravity = true;
		}

		#endregion

		#region GET AND SET

		public virtual float GetMaxLinearSpeed() {
			return _maxLinearSpeed;
		}

		protected virtual float GetJumpVerticalSpeed(float jumpHeight) {
			// From the jump height and gravity we deduce the upwards speed 
			// for the character to reach at the apex.
			return Mathf.Sqrt(2.0f * jumpHeight * _gravity);
		}

		protected abstract void ComputeTargetLinearVelocity();
		protected abstract void ComputeTargetAngularVelocity();

		protected virtual void ClampLinearVelocity() {
			Vector2 vHor = new Vector2(_rigidBody.velocity.x, _rigidBody.velocity.z);
			float mSpeed = GetMaxLinearSpeed();
			if (vHor.sqrMagnitude > (mSpeed * mSpeed)) {
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

		protected virtual void FixedUpdate() {
            if (_stepping) return;

            if (CanMove())
            {
                ComputeTargetLinearVelocity();
                ComputeTargetAngularVelocity();

                UpdateLinearVelocity();
                UpdateJump();
                UpdateAngularVelocity();
            }
            else _rigidBody.velocity = Vector3.zero;
		}

		protected virtual void UpdateLinearVelocity() {
			//We are moving in the desired direction. 
			if (_targetLinearVelocity == Vector3.zero) _rigidBody.drag = _linearDrag;
			else {
				_rigidBody.drag = 0.0f;

				//Apply a force that attempts to reach our target velocity
				Vector3 offset = (_targetLinearVelocity - _rigidBody.velocity);
				Vector2 v = new Vector2(offset.x, offset.z);
				v.Normalize();
				_rigidBody.velocity += new Vector3(v.x, 0.0f, v.y) * _linearAcceleration * Time.deltaTime;
			}
			
			ClampLinearVelocity();
		}

		protected virtual void UpdateJump() {
			// Jump
			if (_grounded && _canJump && Input.GetButton("Jump")) {
				_rigidBody.velocity = new Vector3(_rigidBody.velocity.x, GetJumpVerticalSpeed(_jumpHeight), _rigidBody.velocity.z);
				_grounded = false;
			}
		}

		protected virtual void UpdateAngularVelocity() {
			_rigidBody.angularDrag = (_targetAngularVelocity == Vector3.zero)? _angularDrag : 0.0f;
			_rigidBody.AddTorque(_targetAngularVelocity, ForceMode.Acceleration);
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


        protected virtual void OnCollisionStay(Collision collision)
        {
            if (_stepping) return;

            //Allow this character to overcome step stairs according to the defined maxStepHeight. 
            //Ignore other agents.

            if ((collision.gameObject.layer == _layerPlayer) || (collision.gameObject.layer == _layerAutonomousAgent)) return;

            //Look for the contact point with the higher y
            Vector3 stepOffset = Vector3.zero;
            foreach (ContactPoint contact in collision.contacts)
            {
                //We are only interested on those contact points pointing on the same direction
                //that the horizontal velocity and in a higher elevation than current character's
                //position.

                Vector3 offset = contact.point - transform.position;
                if ((offset.y > stepOffset.y) && (Vector3.Dot(offset, _targetLinearVelocity) > 0))
                {
                    stepOffset = offset;
                }
            }

            if ((stepOffset.y > 0) && (stepOffset.y <= _maxStepHeight))
            {
                StartCoroutine(CoUpdateStepping(stepOffset));
            }
        }

        protected virtual IEnumerator CoUpdateStepping(Vector3 stepOffset)
        {
            _stepping = true;
            _rigidBody.isKinematic = true;

            //Move the rigid body upwards until the step height is reached. 
            float stepHeight = stepOffset.y;
            float targetHeight = transform.position.y + stepHeight;
            float vSpeed = GetJumpVerticalSpeed(stepHeight);
            while (transform.position.y < targetHeight)
            {
                transform.Translate(Vector3.up * vSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = new Vector3(transform.position.x, targetHeight, transform.position.z);

            //Add some horizontal movement
            _rigidBody.velocity = Vector3.Scale(_targetLinearVelocity, new Vector3(1, 0, 1));

            _rigidBody.isKinematic = false;
            _stepping = false;
        }

        #endregion

    }

}
