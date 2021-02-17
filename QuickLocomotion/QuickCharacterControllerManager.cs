using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickCharacterControllerManager : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        public float _radius = 0.25f;               //Character radius
        public float _height = 2.0f;                //Character height

        public float _maxStepHeight = 0.3f;			//The step offset used for stepping stairs and so on. 

        public float _defaultMaxLinearSpeed = 2.5f;        //Max linear speed in m/s
        public float _linearAcceleration = 1.0f;    //Max linear acceleration in m/s^2
        public float _linearDrag = 4.0f;            //The drag applied to the linear speed when no input is applied

        public float _defaultMaxAngularSpeed = 45.0f;      //Max angular speed in degrees/second
        public float _angularAcceleration = 45.0f;  //Angular acceleration in degrees/second^2
        public float _angularDrag = 4.0f;			//The drag applied to the angular speed when no input is applied. 

        public bool _hasPriority = false;

        public bool _canJump = true;
        public float _jumpHeight = 2.0f;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected Rigidbody _rigidBody = null;
        protected CapsuleCollider _collider = null;
        protected static PhysicMaterial _physicMaterial = null;

        protected bool _stepping = false;
        protected Vector3 _stepVelocity = Vector3.zero;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            CreatePhysicMaterial();

            InitCollider();
            InitRigidBody();
        }

        protected virtual void CreatePhysicMaterial()
        {
            if (!_physicMaterial)
            {
                _physicMaterial = new PhysicMaterial("__CharacterControllerPhysicMaterial__");
                _physicMaterial.dynamicFriction = 0.0f;
                _physicMaterial.staticFriction = 0.0f;
                _physicMaterial.bounciness = 0.0f;
                _physicMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
                _physicMaterial.bounceCombine = PhysicMaterialCombine.Minimum;
            }
        }

        protected virtual void InitCollider()
        {
            _collider = gameObject.GetOrCreateComponent<CapsuleCollider>();
            _collider.radius = _radius;
            _collider.height = _height;
            _collider.center = new Vector3(0.0f, _height * 0.5f, 0.0f);
            _collider.material = _physicMaterial;
        }

        protected virtual void InitRigidBody()
        {
            _rigidBody = gameObject.GetOrCreateComponent<Rigidbody>();
            _rigidBody.freezeRotation = true;
            //_rigidBody.maxAngularVelocity = _maxAngularSpeed * Mathf.Deg2Rad;
            _rigidBody.useGravity = true;
            _rigidBody.drag = 0.0f;

           // _rigidBody.isKinematic = true;
        }

        #endregion

        #region GET AND SET

        public virtual bool IsStepping()
        {
            return _stepping;
        }

        public static int GetLayerPlayer()
        {
            return LayerMask.NameToLayer("Player");
        }

        public static int GetLayerAutonomousAgent()
        {
            return LayerMask.NameToLayer("AutonomousAgent");
        }

        public virtual void SetStepVelocity(Vector3 v)
        {
            _stepVelocity = v;
        }

        protected virtual Vector3 GetTotalLinearVelocity()
        {
            return _stepVelocity + _rigidBody.velocity;
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

            if ((collision.gameObject.layer == GetLayerPlayer()) || (collision.gameObject.layer == GetLayerAutonomousAgent())) return;

            //Check if the current speed is significant enough to consider that we are, at least, walking
            Vector3 totalLinearVelocity = GetTotalLinearVelocity();
            float minSpeed = 0.25f;
            float speed2 = totalLinearVelocity.sqrMagnitude;
            if (speed2 < minSpeed * minSpeed) return;

            //If we arrive here, we consider that we are walking and we have collided with an obstacle. Let's check if
            //this is an step that we can overcome. 
            //Look for the contact point with the higher y
            Vector3 stepOffset = Vector3.zero;
            foreach (ContactPoint contact in collision.contacts)
            {
                //We are only interested on those contact points pointing on the same direction
                //that the linear velocity and in a higher elevation than current character's position.
                Vector3 offset = contact.point - transform.position;
                if ((offset.y > stepOffset.y) && (Vector3.Dot(offset, totalLinearVelocity) > 0))
                {
                    stepOffset = offset;
                }
            }

            float minStepHeight = 0.05f;
            if ((stepOffset.y > minStepHeight) && (stepOffset.y <= _maxStepHeight))
            {
                StartCoroutine(CoUpdateStepping(stepOffset));
            }
        }

        protected virtual IEnumerator CoUpdateStepping(Vector3 stepOffset)
        {
            _stepping = true;
          //  _rigidBody.isKinematic = true;

            Vector3 totalLinearVelocity = GetTotalLinearVelocity();
            float speed = totalLinearVelocity.magnitude; //0.5f;

            //Move upwards until the step height is reached. 
            yield return StartCoroutine(CoUpdateStepping(transform.position + Vector3.up * stepOffset.y, stepOffset.y, speed));

            //Move in the direction of the velocity in order to ensure that the 
            float d = _collider.radius * 1.1f;    //The horizontal distance
            yield return StartCoroutine(CoUpdateStepping(transform.position + totalLinearVelocity.normalized * d, d, speed));

            _rigidBody.isKinematic = false;
            _stepping = false;
        }

        protected virtual IEnumerator CoUpdateStepping(Vector3 targetPos, float distance, float speed)
        {
            float totalTime = distance / speed;
            float elapsedTime = 0.0f;
            Vector3 initialPos = transform.position;
            while (elapsedTime < totalTime)
            {
                elapsedTime += Time.deltaTime;
                transform.position = Vector3.Lerp(initialPos, targetPos, elapsedTime / totalTime);
                yield return null;
            }
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


    }

}
