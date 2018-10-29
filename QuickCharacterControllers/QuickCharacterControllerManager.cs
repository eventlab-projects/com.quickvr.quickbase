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

        #endregion

        #region PROTECTED ATTRIBUTES

        protected Rigidbody _rigidBody = null;
        protected CapsuleCollider _collider = null;
        protected static PhysicMaterial _physicMaterial = null;

        protected HashSet<QuickCharacterControllerBase> _characterControllers = new HashSet<QuickCharacterControllerBase>();

        protected Vector3 _preLinearVelocity = Vector3.zero;    //The linear velocity the object had before Unity's internal physics update

        protected bool _stepping = false;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            CreatePhysicMaterial();

            InitCollider();
            InitRigidBody();
        }

        protected virtual void InitCollider()
        {
            _collider = gameObject.GetOrCreateComponent<CapsuleCollider>();
            _collider.radius = _radius;
            _collider.height = _height;
            _collider.center = new Vector3(0.0f, _height * 0.5f, 0.0f);
            _collider.material = _physicMaterial;
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

        protected virtual void InitRigidBody()
        {
            _rigidBody = gameObject.GetOrCreateComponent<Rigidbody>();
            _rigidBody.freezeRotation = true;
            //_rigidBody.maxAngularVelocity = _maxAngularSpeed * Mathf.Deg2Rad;
            _rigidBody.useGravity = true;
            _rigidBody.drag = 0.0f;
        }

        #endregion

        #region GET AND SET

        public virtual void AddCharacterController(QuickCharacterControllerBase characterController)
        {
            _characterControllers.Add(characterController);
        }

        public virtual void RemoveCharacterController(QuickCharacterControllerBase characterController)
        {
            _characterControllers.Remove(characterController);
        }

        public static int GetLayerPlayer()
        {
            return LayerMask.NameToLayer("Player");
        }

        public static int GetLayerAutonomousAgent()
        {
            return LayerMask.NameToLayer("AutonomousAgent");
        }

        #endregion

        #region UPDATE

        protected virtual void FixedUpdate()
        {
            _rigidBody.velocity = Vector3.Scale(_rigidBody.velocity, Vector3.up);

            foreach (QuickCharacterControllerBase characterController in _characterControllers)
            {
                //characterController.UpdateMovement();
                _rigidBody.velocity += characterController.GetCurrentLinearVelocity();
            }

            _preLinearVelocity = _rigidBody.velocity;
        }

        #endregion

        #region PHYSICS MANAGEMENT

        protected virtual void OnCollisionStay(Collision collision)
        {
            if (_stepping) return;

            //Allow this character to overcome step stairs according to the defined maxStepHeight. 
            //Ignore other agents.

            if ((collision.gameObject.layer == GetLayerPlayer()) || (collision.gameObject.layer == GetLayerAutonomousAgent())) return;

            //Check if the current speed is significant enough to consider that we are, at least, walking
            float minSpeed = 0.25f;
            float speed2 = Vector3.Scale(_preLinearVelocity, new Vector3(1, 0, 1)).sqrMagnitude;
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
                if ((offset.y > stepOffset.y) && (Vector3.Dot(offset, _preLinearVelocity) > 0))
                {
                    stepOffset = offset;
                }
            }

            float minStepHeight = 0.05f;
            if ((stepOffset.y > minStepHeight) && (stepOffset.y <= _maxStepHeight))
            {
                StartCoroutine(CoUpdateStepping(stepOffset, _preLinearVelocity));
            }
        }

        protected virtual IEnumerator CoUpdateStepping(Vector3 stepOffset, Vector3 linearVelocity)
        {
            _stepping = true;
            _rigidBody.isKinematic = true;

            float speed = _preLinearVelocity.magnitude; //0.5f;

            //Move upwards until the step height is reached. 
            yield return StartCoroutine(CoUpdateStepping(transform.position + Vector3.up * stepOffset.y, stepOffset.y, speed));

            //Move in the direction of the velocity in order to ensure that the 
            float d = _collider.radius * 1.01f;    //The horizontal distance
            yield return StartCoroutine(CoUpdateStepping(transform.position + linearVelocity.normalized * d, d, speed));

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

        #endregion


    }

}
