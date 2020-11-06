using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR;

namespace QuickVR
{

    public class QuickVRHand : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        public Transform _handBone = null;
        public Transform _handBoneIndexDistal = null;
        public string _axisAnim = "";
        public float _axisAnimThreshold = 0.9f; //The axis value from where we consider the hand has changed its state

        public string _parameterState = "State";
        public string _parameterPressValue = "PressValue";

        public int _handStateIdle = (int)HAND_STATE.IDLE;
        public int _handStatePointing = (int)HAND_STATE.POINTING;
        public int _handStateGrab = (int)HAND_STATE.GRAB;

        public enum HAND_STATE
        {
            IDLE = 0,
            POINTING = 8,
            GRAB = 9
        };
        public HAND_STATE _state = HAND_STATE.IDLE;
    
        #endregion
        
        #region PROTECTED ATTRIBUTES

        protected Animator _animator = null;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            _animator = GetComponent<Animator>();        
        }

        #endregion

        #region GET AND SET

        public virtual float GetAnimationTime()
        {
            return Mathf.Clamp01(InputManager.GetAxis(_axisAnim));
        }

        public virtual bool IsPointing()
        {
            return IsHandState(HAND_STATE.POINTING, _axisAnimThreshold);
        }

        public virtual bool IsGrabbing()
        {
            return IsHandState(HAND_STATE.GRAB, _axisAnimThreshold);
        }

        protected virtual bool IsHandState(HAND_STATE state, float axisThreshold)
        {
            if (!IsAnimatorParameter(_parameterState) || !IsAnimatorParameter(_parameterPressValue)) return false;

            return (gameObject.activeSelf && (_animator.GetInteger(_parameterState) == (int)state) && (_animator.GetFloat(_parameterPressValue) > axisThreshold));
        }

        protected virtual void SetAnimatorParameter(string paramName, object paramValue)
        {
            if (IsAnimatorParameter(paramName))
            {
                if (paramValue.GetType() == typeof(int)) _animator.SetInteger(paramName, (int)paramValue);
                else if (paramValue.GetType() == typeof(float)) _animator.SetFloat(paramName, (float)paramValue);
            }
        }

        protected virtual bool IsAnimatorParameter(string paramName)
        {
            if (_animator.runtimeAnimatorController)
            {
                foreach (AnimatorControllerParameter param in _animator.parameters)
                {
                    if (param.name == paramName) return true;
                }
            }

            return false;
        }

        #endregion

        #region UPDATE

        protected virtual void Update()
        {
            //Update the animator state
            //SetAnimatorParameter(_parameterState, (int)(_state));
            //SetAnimatorParameter(_parameterPressValue, GetAnimationTime());
        }

        #endregion

    }

}
