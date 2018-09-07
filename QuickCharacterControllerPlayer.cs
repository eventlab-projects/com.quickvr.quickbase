using UnityEngine;
using System.Collections;

namespace QuickVR {

	public class QuickCharacterControllerPlayer : QuickCharacterControllerBase {

        #region PROTECTED ATTRIBUTES

        protected Vector3 _playerLinearVelocity = Vector3.zero;

        #endregion
        
        #region GET AND SET

        //protected override void ComputeTargetLinearVelocity() {
        //	// Calculate how fast we should be moving
        //	float hAxis = InputManager.GetAxis(InputManager.DEFAULT_AXIS_HORIZONTAL);
        //	float vAxis = InputManager.GetAxis(InputManager.DEFAULT_AXIS_VERTICAL);
        //	_targetLinearVelocity = new Vector3(hAxis, 0, vAxis);
        //	_targetLinearVelocity.Normalize();
        //	_targetLinearVelocity = transform.TransformDirection(_targetLinearVelocity);
        //	_targetLinearVelocity *= _maxLinearSpeed;
        //}

        //protected override void ComputeTargetAngularVelocity() {
        //          //float cAXis = InputManager.GetAxis(InputManager.DEFAULT_AXIS_HORIZONTAL_RIGHT);
        //          float cAXis = InputManager.GetAxis(InputManager.DEFAULT_AXIS_HORIZONTAL);
        //          _targetAngularVelocity = transform.up * cAXis * _angularAcceleration * Mathf.Deg2Rad;
        //}

        public virtual void SetPlayerLinearVelocity(Vector3 v)
        {
            _playerLinearVelocity = v;
        }

        #endregion

        #region UPDATE

        protected override void UpdateLinearVelocity()
        {
            _rigidBody.velocity = _playerLinearVelocity;
        }

        protected override void UpdateAngularVelocity()
        {
            
        }

        #endregion

    }

}
