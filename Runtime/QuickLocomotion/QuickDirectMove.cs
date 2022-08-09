using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace QuickVR
{
    
    public class QuickDirectMove : ContinuousMoveProviderBase
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

        protected Vector3 _lastPos = Vector3.zero;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void OnEnable()
        {
            if (_nodeHead.IsTracked())
            {
                _lastPos = GetCurrentPos();
            }
        }

        protected virtual void OnDisable()
        {
            _lastPos = Vector3.zero;
        }

        #endregion

        #region GET AND SET

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

        protected virtual Vector3 GetCurrentPos()
        {
            return _nodeHead.GetTrackedObject().transform.position;
        }

        /// <summary>
        /// Determines how much to slide the rig due to <paramref name="input"/> vector.
        /// </summary>
        /// <param name="input">Input vector, such as from a thumbstick.</param>
        /// <returns>Returns the translation amount in world space to move the rig.</returns>
        protected override Vector3 ComputeDesiredMove(Vector2 input)
        {
            //The input is the horizontal displacement in the QuickVRPlayArea's origin system. 
            //So we have to translate it to target avatar's system. 
            Quaternion q = Quaternion.FromToRotation(_vrPlayArea._origin.forward, system.xrOrigin.Origin.transform.forward);
            Vector3 result = q * new Vector3(input.x, 0, input.y);

            return result;
        }

        /// <summary>
        /// Creates a locomotion event to move the rig by <paramref name="translationInWorldSpace"/>,
        /// and optionally applies gravity.
        /// </summary>
        /// <param name="translationInWorldSpace">The translation amount in world space to move the rig (pre-gravity).</param>
        protected override void MoveRig(Vector3 translationInWorldSpace)
        {
            base.MoveRig(translationInWorldSpace);

            _lastPos = GetCurrentPos();
        }

        #endregion

    }

}


