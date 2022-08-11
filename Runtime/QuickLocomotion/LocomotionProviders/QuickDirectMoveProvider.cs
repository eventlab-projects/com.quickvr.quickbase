using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;

namespace QuickVR
{
    
    public class QuickDirectMoveProvider : ContinuousMoveProviderBase
    {

        #region PROTECTED ATTRIBUTES

        protected QuickVRPlayArea _vrPlayArea
        {
            get
            {
                if (!m_VRPlayArea)
                {
                    m_VRPlayArea = QuickSingletonManager.GetInstance<QuickVRPlayArea>();
                }

                return m_VRPlayArea;
            }
        }
        protected QuickVRPlayArea m_VRPlayArea = null;

        protected QuickVRNode _nodeHead
        {
            get
            {
                if (!m_NodeHead)
                {
                    m_NodeHead = _vrPlayArea.GetVRNode(HumanBodyBones.Head);
                }

                return m_NodeHead;
            }
        }
        protected QuickVRNode m_NodeHead = null;

        protected QuickIKManager _ikManager
        {
            get
            {
                if (!m_IKManager)
                {
                    m_IKManager = QuickSingletonManager.GetInstance<QuickVRManager>().GetAnimatorSource().GetComponent<QuickIKManager>();
                }

                return m_IKManager;
            }
        }
        protected QuickIKManager m_IKManager = null;

        protected Vector3 _lastPos = Vector3.zero;

        //_headToHips will be static because we want to avoid the value to be lost when creating new instances of QuickDirectMove
        protected static Vector3 _headToHips = Vector3.zero;
        protected static Vector3 _originToHeadOffset = Vector3.zero;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Awake()
        {
            base.Awake();

            QuickVRManager.OnPostCalibrate += OnPostCalibrateAction;
        }

        protected virtual void OnDestroy()
        {
            QuickVRManager.OnPostCalibrate -= OnPostCalibrateAction;
        }

        protected virtual void OnEnable()
        {
            if (_nodeHead.IsTracked())
            {
                _lastPos = GetCurrentPos();

                //Reposition the _vrOrigin according to the new user's head position. 
                Vector3 currentOffset = GetCurrentOriginToHeadOffset();
                Vector3 t = currentOffset - _originToHeadOffset;
                _vrPlayArea._origin.position += _vrPlayArea._origin.TransformDirection(t);
            }

            QuickVRManager.OnPostUpdateIKTargets += UpdateHipsIKTarget;
        }

        protected virtual void OnDisable()
        {
            _lastPos = Vector3.zero;

            QuickVRManager.OnPostUpdateIKTargets -= UpdateHipsIKTarget;
        }

        #endregion

        #region GET AND SET

        protected virtual void OnPostCalibrateAction()
        {
            _headToHips = _ikManager.GetIKSolver(HumanBodyBones.Hips)._targetLimb.position - _ikManager.GetIKSolver(HumanBodyBones.Head)._targetLimb.position;
            _originToHeadOffset = GetCurrentOriginToHeadOffset();
        }

        /// <summary>
        /// Reads the current value of the move input.
        /// </summary>
        /// <returns>Returns the input vector, such as from a thumbstick.</returns>
        protected override Vector2 ReadInput()
        {
            Vector2 result = Vector2.zero;

            if (_nodeHead.IsTracked())
            {
                if (_lastPos == Vector3.zero)
                {
                    _lastPos = GetCurrentPos();
                }

                Vector3 offset = GetCurrentPos() - _lastPos;
                result = new Vector2(offset.x, offset.z);
            }

            return result;
        }

        protected virtual Vector3 GetCurrentOriginToHeadOffset()
        {
            Vector3 offsetWS = Vector3.ProjectOnPlane(GetCurrentPos() - _vrPlayArea._origin.position, _vrPlayArea._origin.up);
            return _vrPlayArea._origin.InverseTransformDirection(offsetWS);
        }

        protected virtual Vector3 GetCurrentPos()
        {
            return _nodeHead.GetTrackedObject().transform.position;
        }

        #endregion

        #region UPDATE

        /// <summary>
        /// Determines how much to slide the rig due to <paramref name="input"/> vector.
        /// </summary>
        /// <param name="input">Input vector, such as from a thumbstick.</param>
        /// <returns>Returns the translation amount in world space to move the rig.</returns>
        protected override Vector3 ComputeDesiredMove(Vector2 input)
        {
            return new Vector3(input.x, 0, input.y);
        }

        /// <summary>
        /// Creates a locomotion event to move the rig by <paramref name="translationInWorldSpace"/>,
        /// and optionally applies gravity.
        /// </summary>
        /// <param name="translationInWorldSpace">The translation amount in world space to move the rig (pre-gravity).</param>
        protected override void MoveRig(Vector3 translationInWorldSpace)
        {
            //The input is the horizontal displacement in the QuickVRPlayArea's origin system. 
            //So we have to translate it to target avatar's system. 
            _vrPlayArea._origin.position += translationInWorldSpace;
            Quaternion q = Quaternion.FromToRotation(_vrPlayArea._origin.forward, system.xrOrigin.Origin.transform.forward);
            translationInWorldSpace = q * translationInWorldSpace;

            base.MoveRig(translationInWorldSpace);

            _lastPos = GetCurrentPos();
        }

        protected virtual void UpdateHipsIKTarget()
        {
            if (!_vrPlayArea.GetVRNode(HumanBodyBones.Hips).IsTracked())
            {
                Transform targetHead = _ikManager.GetIKSolver(HumanBodyBones.Head)._targetLimb;
                Transform targetHips = _ikManager.GetIKSolver(HumanBodyBones.Hips)._targetLimb;

                targetHips.position = targetHead.position + _headToHips;
            }
        }

        #endregion

    }

}


